using System.Threading;
using System.Threading.Tasks;

namespace Glowtics.BLL.Interfaces
{
    /// <summary>One observability record for an /analyze request (latency, outcome, cost signals).</summary>
    public class AnalysisTrace
    {
        public string Name { get; set; } = "analyze";
        public long LatencyMs { get; set; }
        public bool Accepted { get; set; }
        public int ProductCount { get; set; }
        public string Collection { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    public interface IAnalysisTracer
    {
        Task TraceAsync(AnalysisTrace trace, CancellationToken cancellationToken = default);
    }
}
