using System.Net.Http;
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
    public class LangflowService : ILangflowService
    {
        private readonly HttpClient _httpClient;
        private readonly LangflowSettings _settings;
        private readonly LangflowEmbeddingSettings _embeddingSettings;

        public LangflowService(HttpClient httpClient, IOptions<LangflowSettings> settings, IOptions<LangflowEmbeddingSettings> embeddingSettings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _embeddingSettings = embeddingSettings.Value;
        }

        public async Task ProcessProductEmbeddingsAsync(LangflowEmbeddingDto request, CancellationToken cancellationToken = default)
        {
            var activeIngredientsStr = request.ActiveIngredients != null ? string.Join(", ", request.ActiveIngredients) : "";
            var targetConditionsStr = request.TargetConditions != null ? string.Join(", ", request.TargetConditions) : "";
            var conflictsStr = request.Conflicts != null ? string.Join(", ", request.Conflicts) : "None";
            
            var inputValue = $"Product ID: {request.ExternalProductId}. Product Name: {request.Name}. Category: . Targets: {targetConditionsStr}. Active ingredients: {activeIngredientsStr}. Conflicts: {conflictsStr}. Availability: {request.IsAvailable.ToString().ToLower()}.";

            var payload = new
            {
                input_value = inputValue,
                input_type = "chat",
                output_type = "chat",
                tweaks = new Dictionary<string, object>
                {
                    {
                        "HuggingFaceInferenceAPIEmbeddings-VFHJc", new 
                        {
                            model_name = "sentence-transformers/all-MiniLM-L6-v2",
                            api_key = _embeddingSettings.HfApiKey
                        }
                    },
                    {
                        "MongoDBAtlasVector-DKKMz", new 
                        {
                            db_name = _embeddingSettings.DbName,
                            collection_name = _embeddingSettings.CollectionName,
                            index_name = "vector_index",
                            mongodb_atlas_cluster_uri = _embeddingSettings.MongoUri,
                            quantization = "none"
                        }
                    }
                }
            };
            
            var requestUri = new Uri(new Uri(_embeddingSettings.BaseUrl), $"api/v1/run/{_embeddingSettings.WorkflowId}?stream=false");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            requestMessage.Content = JsonContent.Create(payload);
            
            // Clear the default API key to avoid conflicts, and add the embedding specific one
            _httpClient.DefaultRequestHeaders.Remove("x-api-key");
            requestMessage.Headers.Add("x-api-key", _embeddingSettings.ApiKey);

            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(ErrorCodes.EmbeddingGenerationFailed); 
            }
        }

        public async Task<LangflowDiagnosisResult> DiagnoseAsync(byte[] photoBytes, string fileName, string contentType, string collectionName, CancellationToken cancellationToken = default)
        {
            // 1. Upload photo to Langflow
            var boundary = Guid.NewGuid().ToString("N");
            using var content = new MultipartFormDataContent(boundary);
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);

            var fileContent = new ByteArrayContent(photoBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", fileName);

            var uploadResponse = await _httpClient.PostAsync($"files/upload/{_settings.FlowId}", content, cancellationToken);
            if (!uploadResponse.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(ErrorCodes.InternalServerError, await uploadResponse.Content.ReadAsStringAsync(cancellationToken));
            }

            var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var filePath = uploadResult.GetProperty("file_path").GetString();

            // 2. Call /api/v1/run to execute pipeline
            var runPayload = new
            {
                output_type = "chat",
                input_type = "chat",
                tweaks = new Dictionary<string, object>
                {
                    {
                        _settings.ChatInputNodeId, new 
                        {
                            files = new[] { filePath },
                            input_value = ""
                        }
                    }
                    /*,
                    {
                        "MongoDBAtlasVector-BIfTP", new 
                        {
                            collection_name = collectionName
                        }
                    }*/
                }
            };

            var runResponse = await _httpClient.PostAsJsonAsync($"run/{_settings.FlowId}?stream=false", runPayload, cancellationToken);
            if (!runResponse.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(ErrorCodes.InternalServerError, "Failed to execute Langflow pipeline.");
            }

            var runResult = await runResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            
            // 3. Parse Response
            string? routineJson = null;
            string? rejectionText = null;

            var outputs = runResult.GetProperty("outputs")[0].GetProperty("outputs");
            foreach (var output in outputs.EnumerateArray())
            {
                if (output.TryGetProperty("results", out var results) && 
                    results.TryGetProperty("message", out var message) && 
                    message.TryGetProperty("text", out var textElement))
                {
                    var text = textElement.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (text.TrimStart().StartsWith("{"))
                        {
                            routineJson = text;
                        }
                        else
                        {
                            rejectionText = text;
                        }
                    }
                }
            }

            if (routineJson == null)
            {
                // Must be a rejection (e.g. "Image rejected...")
                throw new ExternalServiceException(ErrorCodes.BusinessRuleViolation, rejectionText ?? "Analysis failed with unknown error.");
            }

            var routineObj = JsonSerializer.Deserialize<LangflowRoutineResponse>(routineJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new LangflowDiagnosisResult
            {
                SkinProfileResult = routineJson,
                ExternalProductIds = routineObj?.Routine?.Select(r => r.ProductId).ToList() ?? new List<string>(),
                RoutineItems = routineObj?.Routine ?? new List<LangflowRoutineItem>()
            };
        }
    }
}
