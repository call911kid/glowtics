using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.DTOs;

namespace Glowtics.BLL.Interfaces
{
    public interface ILangflowService
    {
        /// <summary>
        /// Sends product data to Langflow for embedding generation and MongoDB insertion.
        /// </summary>
        Task ProcessProductEmbeddingsAsync(LangflowEmbeddingDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a photo and domain to Langflow for diagnosis, returning the skin profile and product IDs.
        /// </summary>
        Task<LangflowDiagnosisResult> DiagnoseAsync(byte[] photoBytes, string fileName, string contentType, string collectionName, CancellationToken cancellationToken = default);
    }
}
