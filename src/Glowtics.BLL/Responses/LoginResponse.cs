using System;

namespace Glowtics.BLL.Responses
{
    public record LoginResponse(string AccessToken, int ExpiresIn);
}
