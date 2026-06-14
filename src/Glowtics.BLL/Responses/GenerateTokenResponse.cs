using System;

namespace Glowtics.BLL.Responses
{
    public class GenerateTokenResponse
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }

    }
}
