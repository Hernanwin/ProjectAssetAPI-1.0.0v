using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ProjectAssetAPI.Options;

namespace ProjectAssetAPI.Security
{
    public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "ApiKey";
        public const string HeaderName = "X-Api-Key";

        private readonly IOptionsMonitor<SecurityOptions> _securityOptions;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IOptionsMonitor<SecurityOptions> securityOptions)
            : base(options, logger, encoder)
        {
            _securityOptions = securityOptions;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var configuredKey = _securityOptions.CurrentValue.ApiKey;
            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("API Key no configurada en el servidor."));
            }

            if (!Request.Headers.TryGetValue(HeaderName, out var providedKeyValues))
            {
                return Task.FromResult(AuthenticateResult.Fail($"Falta el encabezado {HeaderName}."));
            }

            var providedKey = providedKeyValues.ToString();
            if (string.IsNullOrWhiteSpace(providedKey))
            {
                return Task.FromResult(AuthenticateResult.Fail($"El encabezado {HeaderName} está vacío."));
            }

            var expectedBytes = Encoding.UTF8.GetBytes(configuredKey);
            var providedBytes = Encoding.UTF8.GetBytes(providedKey);

            if (expectedBytes.Length != providedBytes.Length ||
                !CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes))
            {
                return Task.FromResult(AuthenticateResult.Fail("API Key inválida."));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "api-key-client"),
                new Claim(ClaimTypes.Name, "AuthorizedClient")
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
