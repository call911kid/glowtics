using System.Security.Claims;
using System.Text.Encodings.Web;
using Glowtics.BLL.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Glowtics.Api.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IMediator _mediator;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IMediator mediator)
            : base(options, logger, encoder)
        {
            _mediator = mediator;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Glowtics-Key", out var apiKeyValues))
            {
                return AuthenticateResult.NoResult();
            }

            var rawKey = apiKeyValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(rawKey))
            {
                return AuthenticateResult.NoResult();
            }

            var response = await _mediator.Send(new ValidateApiKeyQuery(rawKey));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, response.UserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
