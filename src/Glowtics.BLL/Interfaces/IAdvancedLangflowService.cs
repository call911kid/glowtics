using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.DTOs;

namespace Glowtics.BLL.Interfaces
{
    public interface IAdvancedLangflowService
    {
        /// <summary>Validation flow: is the photo a usable single front-facing face?</summary>
        Task<ValidationResult> ValidateAsync(byte[] photoBytes, string fileName, CancellationToken cancellationToken = default);

        /// <summary>Analysis flow (only called for valid photos): diagnosis + translation + RAG -> routine.</summary>
        Task<LangflowDiagnosisResult> AnalyzeRoutineAsync(byte[] photoBytes, string fileName, string collectionName, CancellationToken cancellationToken = default);

        /// <summary>Agent flow: a free-text skincare question -> Agent that calls catalog tools -> grounded answer.</summary>
        Task<string> AgentChatAsync(string message, CancellationToken cancellationToken = default);
    }
}
