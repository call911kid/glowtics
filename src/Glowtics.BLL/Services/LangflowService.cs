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

        public LangflowService(HttpClient httpClient, IOptions<LangflowSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task ProcessProductEmbeddingsAsync(LangflowEmbeddingDto request, CancellationToken cancellationToken = default)
        {
            // 1. Translation: Map domain DTO variables to Langflow's required payload schema
            // Serialize the product data to pass as the main text input for Langflow to parse
            var productJson = JsonSerializer.Serialize(new 
            {
                external_product_id = request.ExternalProductId,
                target_conditions = request.TargetConditions,
                active_ingredients = request.ActiveIngredients,
                conflicts = request.Conflicts,
                is_available = request.IsAvailable
            });

            var payload = new
            {
                input_value = productJson,
                input_type = "chat",
                output_type = "chat",
                tweaks = new Dictionary<string, object>
                {
                    {
                        "MongoDBAtlasVector-BIfTP", new 
                        {
                            collection_name = request.CollectionName
                        }
                    }
                }
            };
            
            // 2. Execution: The HttpClient already has the BaseUrl and Auth Headers!
            var response = await _httpClient.PostAsJsonAsync($"run/{_settings.FlowId}", payload, cancellationToken);
            
            // 3. Error Handling Translation
            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(ErrorCodes.EmbeddingGenerationFailed); 
            }
        }

        public async Task<LangflowDiagnosisResult> DiagnoseAsync(byte[] photoBytes, string fileName, string collectionName, CancellationToken cancellationToken = default)
        {
            // 1. Upload photo to Langflow
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(photoBytes);
            content.Add(fileContent, "file", fileName);

            var uploadResponse = await _httpClient.PostAsync($"files/upload/{_settings.FlowId}", content, cancellationToken);
            if (!uploadResponse.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(ErrorCodes.InternalServerError, "Failed to upload image to Langflow.");
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
                        "ChatInput", new 
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
                ExternalProductIds = routineObj?.Routine?.Select(r => r.ProductId).ToList() ?? new List<string>()
            };
        }
    }
}
