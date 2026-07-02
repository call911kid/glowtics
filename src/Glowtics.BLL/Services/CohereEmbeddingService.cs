using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Interfaces;
using Glowtics.BLL.Settings;
using Microsoft.Extensions.Options;

namespace Glowtics.BLL.Services
{
    /// <summary>
    /// Embeds text with Cohere's REST API (embed-english-light-v3.0, input_type=search_document)
    /// so backend-written product vectors match the ones the analyze flow's RAG step queries.
    /// </summary>
    public class CohereEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly CohereSettings _settings;

        public CohereEmbeddingService(HttpClient httpClient, IOptions<CohereSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<float[]> EmbedDocumentAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                throw new ExternalServiceException(ErrorCodes.EmbeddingGenerationFailed,
                    "Cohere API key is not configured. Set Cohere:ApiKey.");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.cohere.com/v1/embed")
            {
                Content = JsonContent.Create(new
                {
                    texts = new[] { text },
                    model = _settings.Model,
                    input_type = "search_document"
                })
            };
            request.Headers.Add("Authorization", $"Bearer {_settings.ApiKey}");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(ErrorCodes.EmbeddingGenerationFailed,
                    $"Cohere embed failed (HTTP {(int)response.StatusCode}).");
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            if (!doc.RootElement.TryGetProperty("embeddings", out var embs) || embs.GetArrayLength() == 0)
            {
                throw new ExternalServiceException(ErrorCodes.EmbeddingGenerationFailed, "Cohere returned no embedding.");
            }

            var first = embs[0];
            var vector = new List<float>(first.GetArrayLength());
            foreach (var v in first.EnumerateArray())
            {
                vector.Add(v.GetSingle());
            }

            if (vector.Count != _settings.Dimensions)
            {
                throw new ExternalServiceException(ErrorCodes.EmbeddingGenerationFailed,
                    $"Cohere embedding dim {vector.Count} != expected {_settings.Dimensions}.");
            }
            return vector.ToArray();
        }
    }
}
