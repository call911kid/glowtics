using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.DTOs;

namespace Glowtics.BLL.Interfaces
{
    public interface IAdvancedLangflowService
    {
        /// <summary>Validation flow: is the photo a usable single front-facing face?</summary>
        Task<ValidationResult> ValidateAsync(byte[] photoBytes, string fileName, CancellationToken cancellationToken = default);

        /// <summary>Analysis flow (only for valid photos): photo -> skin profile / diagnosis text.</summary>
        Task<string> AnalyzeSkinProfileAsync(byte[] photoBytes, string fileName, string collectionName, CancellationToken cancellationToken = default);

        /// <summary>Build-routine flow: skin profile text -> RAG + rerank -> product routine + external product ids.</summary>
        Task<LangflowDiagnosisResult> BuildRoutineAsync(string skinProfile, string collectionName, CancellationToken cancellationToken = default);

        /// <summary>Agent flow: a free-text skincare question -> Agent that calls catalog tools -> grounded answer.</summary>
        Task<string> AgentChatAsync(string message, CancellationToken cancellationToken = default);
    }
}
