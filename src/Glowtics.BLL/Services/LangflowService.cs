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

        public Task<LangflowDiagnosisResult> DiagnoseAsync(byte[] photoBytes, string fileName, string collectionName, CancellationToken cancellationToken = default)
        {
            // TODO: Implement the 2-step Langflow API call:
            // 1. Upload photo to /api/v1/files/upload/{flow_id}
            // 2. Call /api/v1/run/{flow_id} with the uploaded file path
            throw new NotImplementedException("Langflow API integration for diagnosis is pending.");
        }
    }
}
