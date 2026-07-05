using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Constants;
using Glowtics.BLL.DTOs;
using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Interfaces;
using Glowtics.BLL.Settings;
using Microsoft.Extensions.Options;

namespace Glowtics.BLL.Services
{
    public class AdvancedLangflowService : IAdvancedLangflowService
    {
        private const string TransientStructuredOutputError = "No structured output returned";
        private const string DefaultRejectReason = "Image rejected. Please upload a clear, front-facing photo of a single face in good lighting.";
        private const int MaxRunRetries = 2;
        // The Langflow MongoDBAtlasVector component leaves `quantization` null, which Atlas rejects
        // ("Invalid quantization cannot be null. Allowed values are: none, scalar, binary."). We override
        // it to "none" via tweaks — matching the un-quantized index CreateMongoCollectionCommand provisions.
        private const string MongoQuantization = "none";

        private readonly HttpClient _httpClient;
        private readonly AdvancedLangflowSettings _settings;
        // node ids are auto-discovered from each flow (Langflow reassigns them on import).
        private readonly ConcurrentDictionary<string, FlowNodes> _nodeCache = new();

        public AdvancedLangflowService(HttpClient httpClient, IOptions<AdvancedLangflowSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<ValidationResult> ValidateAsync(byte[] photoBytes, string fileName, CancellationToken cancellationToken = default)
        {
            var nodes = await GetFlowNodesAsync(_settings.ValidationFlowId, cancellationToken);
            var filePath = await UploadPhotoAsync(_settings.ValidationFlowId, photoBytes, fileName, cancellationToken);

            var payload = new
            {
                output_type = "chat",
                input_type = "chat",
                tweaks = new Dictionary<string, object>
                {
                    [nodes.ChatInput] = new { files = new[] { filePath }, input_value = "" }
                }
            };

            var runJson = await RunFlowWithRetryAsync(_settings.ValidationFlowId, payload, cancellationToken);
            var outputs = CollectOutputs(runJson);

            // The validation flow's ChatOutput emits "True"/"False".
            var verdict = outputs.TryGetValue(nodes.ChatOutput, out var v) ? v : outputs.Values.FirstOrDefault();
            var isValid = string.Equals(verdict?.Trim(), "True", StringComparison.OrdinalIgnoreCase);

            return new ValidationResult
            {
                IsValid = isValid,
                Reason = isValid ? null : DefaultRejectReason
            };
        }

        public async Task<string> AnalyzeSkinProfileAsync(
            byte[] photoBytes, string fileName, string collectionName, CancellationToken cancellationToken = default)
        {
            var nodes = await GetFlowNodesAsync(_settings.AnalysisFlowId, cancellationToken);
            var filePath = await UploadPhotoAsync(_settings.AnalysisFlowId, photoBytes, fileName, cancellationToken);

            var tweaks = new Dictionary<string, object>
            {
                [nodes.ChatInput] = new { files = new[] { filePath }, input_value = "" }
            };
            // Harmless if the analysis flow has no Mongo node (skin-only diagnosis).
            if (!string.IsNullOrEmpty(nodes.Mongo))
            {
                tweaks[nodes.Mongo!] = new { collection_name = collectionName, quantization = MongoQuantization };
            }

            var runJson = await RunFlowWithRetryAsync(_settings.AnalysisFlowId,
                new { output_type = "chat", input_type = "chat", tweaks }, cancellationToken);
            var outputs = CollectOutputs(runJson);

            var profileText = outputs.TryGetValue(nodes.ChatOutput, out var pt) ? pt : outputs.Values.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(profileText))
            {
                throw new ExternalServiceException(ErrorCodes.DiagnosisFailed, "Langflow analysis returned no skin profile.");
            }
            return StripCodeFences(profileText!);
        }

        // The vision LLM often wraps its JSON in ```json ... ``` markdown fences; strip them so the
        // profile is parseable downstream (FE display + ExtractSkinType + the build-routine flow input).
        private static string StripCodeFences(string text)
        {
            var t = text.Trim();
            if (!t.StartsWith("```")) return t;
            var firstNewline = t.IndexOf('\n');
            if (firstNewline >= 0) t = t[(firstNewline + 1)..];   // drop the opening ```json line
            if (t.EndsWith("```")) t = t[..^3];                   // drop the closing ```
            return t.Trim();
        }

        public async Task<LangflowDiagnosisResult> BuildRoutineAsync(
            string skinProfile, string collectionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.BuildRoutineFlowId))
            {
                throw new ExternalServiceException(ErrorCodes.DiagnosisFailed,
                    "Build-routine flow is not configured. Set LangflowApiAdvanced:BuildRoutineFlowId after deploying GLOWTICS Build Routine.json.");
            }

            var nodes = await GetFlowNodesAsync(_settings.BuildRoutineFlowId, cancellationToken);

            // Text-in: the skin profile from the analysis flow feeds the build-routine flow's ChatInput.
            var tweaks = new Dictionary<string, object>
            {
                [nodes.ChatInput] = new { input_value = skinProfile }
            };
            if (!string.IsNullOrEmpty(nodes.Mongo))
            {
                tweaks[nodes.Mongo!] = new { collection_name = collectionName, quantization = MongoQuantization };
            }

            var runJson = await RunFlowWithRetryAsync(_settings.BuildRoutineFlowId,
                new { output_type = "chat", input_type = "chat", tweaks }, cancellationToken);
            var outputs = CollectOutputs(runJson);

            var routineText = outputs.TryGetValue(nodes.ChatOutput, out var rt) ? rt : outputs.Values.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(routineText))
            {
                throw new ExternalServiceException(ErrorCodes.DiagnosisFailed, "Langflow build-routine returned no routine.");
            }
            // The routine LLM also wraps its JSON in ```json fences intermittently; strip so parsing (and the
            // per-product name/rationale it yields) is reliable — otherwise we fall back to id-only and miss matches.
            routineText = StripCodeFences(routineText!);

            // Parse the routine items so per-product rationale flows to the UI (not a generic label).
            var routineItems = new List<LangflowRoutineItem>();
            try
            {
                var parsed = JsonSerializer.Deserialize<LangflowRoutineResponse>(
                    routineText!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed?.Routine != null) routineItems = parsed.Routine;
            }
            catch (JsonException) { /* non-JSON routine — fall back to id extraction only */ }

            var productIds = routineItems.Count > 0
                ? routineItems.Where(i => !string.IsNullOrWhiteSpace(i.ProductId)).Select(i => i.ProductId).Distinct().ToList()
                : ExtractProductIds(routineText!);

            return new LangflowDiagnosisResult
            {
                IsValidFace = true,
                SkinProfileResult = routineText!,
                ExternalProductIds = productIds,
                RoutineItems = routineItems
            };
        }

        public async Task<string> AgentChatAsync(string message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.AgentFlowId))
            {
                throw new ExternalServiceException(ErrorCodes.DiagnosisFailed,
                    "Agent flow is not configured. Set LangflowApi:AgentFlowId after deploying GLOWTICS-agent.json.");
            }

            var nodes = await GetFlowNodesAsync(_settings.AgentFlowId!, cancellationToken);

            // Text-in / text-out: the Agent component decides which catalog tools to call.
            var payload = new
            {
                output_type = "chat",
                input_type = "chat",
                tweaks = new Dictionary<string, object>
                {
                    [nodes.ChatInput] = new { input_value = message }
                }
            };

            var runJson = await RunFlowWithRetryAsync(_settings.AgentFlowId!, payload, cancellationToken);
            var outputs = CollectOutputs(runJson);

            var answer = outputs.TryGetValue(nodes.ChatOutput, out var a) ? a : outputs.Values.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(answer))
            {
                throw new ExternalServiceException(ErrorCodes.DiagnosisFailed, "Agent flow returned no answer.");
            }
            return answer!;
        }

        private sealed class FlowNodes
        {
            public string ChatInput = "";
            public string ChatOutput = "";
            public string? Mongo;
        }

        /// <summary>
        /// Reads a flow definition and finds its ChatInput / ChatOutput / Mongo node ids,
        /// cached per flow. Survives re-imports (Langflow reassigns node ids on import).
        /// </summary>
        private async Task<FlowNodes> GetFlowNodesAsync(string flowId, CancellationToken cancellationToken)
        {
            if (_nodeCache.TryGetValue(flowId, out var cached))
            {
                return cached;
            }

            var response = await SendGuardedAsync(
                () => _httpClient.GetAsync($"flows/{flowId}", cancellationToken), "flow load", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(ErrorCodes.DiagnosisFailed,
                    $"Could not load flow {flowId} (HTTP {(int)response.StatusCode}).");
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var nodes = doc.RootElement.GetProperty("data").GetProperty("nodes");
            var fn = new FlowNodes();
            foreach (var node in nodes.EnumerateArray())
            {
                var id = node.GetProperty("id").GetString() ?? "";
                var type = node.GetProperty("data").TryGetProperty("type", out var t) ? t.GetString() : null;
                if (type == "ChatInput") fn.ChatInput = id;
                else if (type == "ChatOutput") fn.ChatOutput = id;
                else if (type == "MongoDBAtlasVector") fn.Mongo = id;
            }

            if (string.IsNullOrEmpty(fn.ChatInput) || string.IsNullOrEmpty(fn.ChatOutput))
            {
                throw new ExternalServiceException(ErrorCodes.DiagnosisFailed,
                    $"Flow {flowId} is missing a ChatInput or ChatOutput node.");
            }

            _nodeCache[flowId] = fn;
            return fn;
        }

        private async Task<string> UploadPhotoAsync(string flowId, byte[] photoBytes, string fileName, CancellationToken cancellationToken)
        {
            using var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(photoBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(fileContent, "file", string.IsNullOrWhiteSpace(fileName) ? "face.jpg" : fileName);

            // .NET emits a QUOTED boundary; Langflow's python-multipart parser rejects
            // it with HTTP 422 "Invalid boundary format" — strip the quotes.
            foreach (var p in form.Headers.ContentType!.Parameters)
            {
                if (string.Equals(p.Name, "boundary", StringComparison.OrdinalIgnoreCase))
                {
                    p.Value = p.Value?.Trim('"');
                }
            }

            var response = await SendGuardedAsync(
                () => _httpClient.PostAsync($"files/upload/{flowId}", form, cancellationToken), "photo upload", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(ErrorCodes.DiagnosisFailed,
                    $"Photo upload to Langflow failed (HTTP {(int)response.StatusCode}).");
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            if (!doc.RootElement.TryGetProperty("file_path", out var fp) || fp.GetString() is not { Length: > 0 } filePath)
            {
                throw new ExternalServiceException(ErrorCodes.DiagnosisFailed, "Langflow upload returned no file path.");
            }
            return filePath;
        }

        private async Task<string> RunFlowWithRetryAsync(string flowId, object payload, CancellationToken cancellationToken)
        {
            string lastError = string.Empty;
            for (var attempt = 0; attempt <= MaxRunRetries; attempt++)
            {
                var response = await SendGuardedAsync(
                    () => _httpClient.PostAsJsonAsync($"run/{flowId}?stream=false", payload, cancellationToken), "flow run", cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return body;
                }

                lastError = body;
                if (!body.Contains(TransientStructuredOutputError, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            throw new ExternalServiceException(ErrorCodes.DiagnosisFailed,
                $"Langflow flow run failed: {Truncate(lastError, 300)}");
        }

        /// <summary>Run an HTTP call and translate transport-level failures (Langflow/ngrok tunnel down,
        /// backend unresponsive) into a clean ExternalServiceException (-> 502) instead of letting a raw
        /// HttpRequestException / timeout bubble up as an opaque 500. A genuine client-side cancellation
        /// (caller's token) is left alone so it isn't mistaken for a backend outage.</summary>
        private async Task<HttpResponseMessage> SendGuardedAsync(
            Func<Task<HttpResponseMessage>> send, string action, CancellationToken cancellationToken)
        {
            try
            {
                return await send();
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // HttpClient timeout (not a client disconnect): the backend is too slow / hung.
                throw new ExternalServiceException(ErrorCodes.DiagnosisFailed,
                    $"Langflow did not respond in time during {action}. The AI service may be offline.");
            }
            catch (HttpRequestException ex)
            {
                // DNS / connection failure: tunnel down or backend unreachable.
                throw new ExternalServiceException(ErrorCodes.DiagnosisFailed,
                    $"Could not reach Langflow during {action}. The AI service may be offline. ({ex.Message})");
            }
        }

        /// <summary>component_id -> output text, across the run response.</summary>
        private static Dictionary<string, string> CollectOutputs(string runJson)
        {
            var outputs = new Dictionary<string, string>();
            using var doc = JsonDocument.Parse(runJson);
            if (doc.RootElement.TryGetProperty("outputs", out var outer))
            {
                foreach (var run in outer.EnumerateArray())
                {
                    if (!run.TryGetProperty("outputs", out var inner)) continue;
                    foreach (var node in inner.EnumerateArray())
                    {
                        var id = node.TryGetProperty("component_id", out var cid) ? cid.GetString() : null;
                        var text = node.TryGetProperty("results", out var res)
                                   && res.TryGetProperty("message", out var msg)
                                   && msg.TryGetProperty("text", out var t)
                            ? t.GetString()
                            : null;
                        if (!string.IsNullOrEmpty(id) && !string.IsNullOrWhiteSpace(text))
                        {
                            outputs[id!] = text!.Trim();
                        }
                    }
                }
            }
            return outputs;
        }

        private static List<string> ExtractProductIds(string routineText)
        {
            var ids = new List<string>();
            try
            {
                using var doc = JsonDocument.Parse(routineText);
                if (doc.RootElement.TryGetProperty("routine", out var routine) && routine.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in routine.EnumerateArray())
                    {
                        if (item.TryGetProperty("productId", out var pid) && pid.GetString() is { Length: > 0 } id)
                        {
                            ids.Add(id);
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Non-JSON routine text: leave ids empty, keep the raw text.
            }
            return ids.Distinct().ToList();
        }

        private static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) || value.Length <= max ? value : value[..max];
    }
}
