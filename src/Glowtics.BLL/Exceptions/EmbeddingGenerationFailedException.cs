using System;

namespace Glowtics.BLL.Exceptions
{
    public class EmbeddingGenerationFailedException : ExternalServiceException
    {
        public EmbeddingGenerationFailedException() : base("Embedding generation failed.") { }
        public EmbeddingGenerationFailedException(string message) : base(message) { }
        public EmbeddingGenerationFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
