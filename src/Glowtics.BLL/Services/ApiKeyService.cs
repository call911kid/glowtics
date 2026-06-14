using System;
using System.Security.Cryptography;
using System.Text;
using Glowtics.BLL.Interfaces;
using Glowtics.BLL.Responses;
using Glowtics.BLL.Settings;
using Microsoft.Extensions.Options;

namespace Glowtics.BLL.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly ApiKeySettings _settings;

        public ApiKeyService(IOptions<ApiKeySettings> options)
        {
            _settings = options.Value;
        }

        public ApiKeyGenerationResponse GenerateApiKey()
        {
            var bytes = new byte[_settings.KeyLengthBytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var base64 = Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            var rawKey = $"{_settings.Prefix}{base64}";
            var hash = HashApiKey(rawKey);
            
            var hint = rawKey.Substring(rawKey.Length - 4);

            return new ApiKeyGenerationResponse 
            { 
                RawKey = rawKey, 
                Hash = hash, 
                Hint = hint 
            };
        }

        public string HashApiKey(string rawKey)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawKey));
            
            var builder = new StringBuilder();
            foreach (var b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
