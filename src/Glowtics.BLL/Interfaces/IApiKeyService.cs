using Glowtics.BLL.Responses;

namespace Glowtics.BLL.Interfaces
{
    public interface IApiKeyService
    {
        ApiKeyGenerationResponse GenerateApiKey();
        string HashApiKey(string rawKey);
    }
}
