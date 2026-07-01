using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Documents;
using Glowtics.BLL.Interfaces;
using Glowtics.BLL.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Glowtics.BLL.Services
{
    /// <summary>
    /// Observability for /analyze: structured log (always) + a trace doc in MongoDB
    /// (collection "Traces") + a best-effort push to Langfuse when configured.
    /// </summary>
    public class AnalysisTracer : IAnalysisTracer
    {
        private const string CollectionName = "Traces";
        private readonly IMongoCollection<TraceDocument> _collection;
        private readonly ILogger<AnalysisTracer> _logger;
        private readonly LangfuseSettings _langfuse;
        private readonly HttpClient _httpClient;

        public AnalysisTracer(
            IMongoDatabase database,
            ILogger<AnalysisTracer> logger,
            IOptions<LangfuseSettings> langfuse,
            HttpClient httpClient)
        {
            _collection = database.GetCollection<TraceDocument>(CollectionName);
            _logger = logger;
            _langfuse = langfuse.Value;
            _httpClient = httpClient;
        }

        public async Task TraceAsync(AnalysisTrace trace, CancellationToken cancellationToken = default)
        {
            // 1. structured log line — visible in any log aggregator
            _logger.LogInformation(
                "analyze trace latency={LatencyMs}ms accepted={Accepted} products={ProductCount} collection={Collection} model={Model} error={Error}",
                trace.LatencyMs, trace.Accepted, trace.ProductCount, trace.Collection, trace.Model, trace.Error);

            // 2. persist to MongoDB for a local observability dashboard/query
            var doc = new TraceDocument
            {
                Name = trace.Name,
                LatencyMs = trace.LatencyMs,
                Accepted = trace.Accepted,
                ProductCount = trace.ProductCount,
                Collection = trace.Collection,
                Model = trace.Model,
                Error = trace.Error
            };
            try
            {
                await _collection.InsertOneAsync(doc, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "failed to persist analyze trace to Mongo");
            }

            // 3. best-effort push to Langfuse (no-op when keys are not configured)
            if (_langfuse.Enabled)
            {
                await TryPushToLangfuse(doc, cancellationToken);
            }
        }

        private async Task TryPushToLangfuse(TraceDocument doc, CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpClient;
                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_langfuse.PublicKey}:{_langfuse.SecretKey}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

                var payload = new
                {
                    batch = new object[]
                    {
                        new
                        {
                            id = Guid.NewGuid().ToString(),
                            type = "trace-create",
                            timestamp = doc.CreatedAt.ToString("o"),
                            body = new
                            {
                                id = doc.Id,
                                name = doc.Name,
                                metadata = new
                                {
                                    doc.LatencyMs,
                                    doc.Accepted,
                                    doc.ProductCount,
                                    doc.Collection,
                                    doc.Model,
                                    doc.Error
                                }
                            }
                        }
                    }
                };

                var host = _langfuse.Host.TrimEnd('/');
                await client.PostAsJsonAsync($"{host}/api/public/ingestion", payload, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "failed to push trace to Langfuse");
            }
        }
    }
}
