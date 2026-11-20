using System.Net.Http.Json;
using ProjectAssetAgent.Models;

namespace ProjectAssetAgent.Services
{
    public sealed class ApiService : IDisposable
    {
        private const string ApiKeyHeaderName = "X-Api-Key";

        private readonly HttpClient _client;
        private readonly int _maxRetries;

        public ApiService(AgentOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            _maxRetries = options.MaxRetries;
            _client = new HttpClient
            {
                BaseAddress = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds)
            };

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                _client.DefaultRequestHeaders.Add(ApiKeyHeaderName, options.ApiKey);
            }
        }

        public async Task<bool> SendAssetAsync(VisualLab asset, CancellationToken cancellationToken = default)
        {
            for (var attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    using var response = await _client.PostAsJsonAsync(
                        "api/VisualLabs/register",
                        asset,
                        cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[{DateTime.UtcNow:O}] Heartbeat enviado correctamente ({(int)response.StatusCode}) para {asset.Hostname} ({asset.IPAddress}).");
                        return true;
                    }

                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    Console.Error.WriteLine(
                        $"[{DateTime.UtcNow:O}] Error HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) en intento {attempt}/{_maxRetries}: {body}");

                    // No reintentar errores de cliente irrecuperables (400/401/403)
                    if ((int)response.StatusCode is >= 400 and < 500 and not 408 and not 429)
                    {
                        return false;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                {
                    Console.Error.WriteLine(
                        $"[{DateTime.UtcNow:O}] Fallo de conexión en intento {attempt}/{_maxRetries}: {ex.Message}");
                }

                if (attempt < _maxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
