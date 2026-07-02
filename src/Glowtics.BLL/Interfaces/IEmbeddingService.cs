using System.Threading;
using System.Threading.Tasks;

namespace Glowtics.BLL.Interfaces
{
    public interface IEmbeddingService
    {
        /// <summary>Embed a document for storage (Cohere input_type=search_document). Returns a 384-d vector.</summary>
        Task<float[]> EmbedDocumentAsync(string text, CancellationToken cancellationToken = default);
    }
}
