using System.Text.Json;

namespace ProjectAssetAgent.Models
{
    public sealed class AgentOptions
    {
        public string ApiBaseUrl { get; set; } = "http://localhost:5000";

        public string ApiKey { get; set; } = "dev-local-api-key-change-in-prod";

        public int HeartbeatIntervalSeconds { get; set; } = 30;

        public int RequestTimeoutSeconds { get; set; } = 15;

        public int MaxRetries { get; set; } = 3;

        public static AgentOptions Load()
        {
            var options = new AgentOptions();

            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(configPath))
            {
                try
                {
                    var json = File.ReadAllText(configPath);
                    var fromFile = JsonSerializer.Deserialize<AgentOptions>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (fromFile is not null)
                    {
                        options = fromFile;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[WARN] No se pudo leer {configPath}: {ex.Message}. Usando valores por defecto.");
                }
            }

            if (Environment.GetEnvironmentVariable("ASSET_API_URL") is { Length: > 0 } envUrl)
            {
                options.ApiBaseUrl = envUrl;
            }

            if (Environment.GetEnvironmentVariable("ASSET_API_KEY") is { Length: > 0 } envKey)
            {
                options.ApiKey = envKey;
            }

            if (int.TryParse(Environment.GetEnvironmentVariable("ASSET_HEARTBEAT_SECONDS"), out var envInterval) &&
                envInterval > 0)
            {
                options.HeartbeatIntervalSeconds = envInterval;
            }

            options.HeartbeatIntervalSeconds = Math.Max(5, options.HeartbeatIntervalSeconds);
            options.RequestTimeoutSeconds = Math.Clamp(options.RequestTimeoutSeconds, 3, 120);
            options.MaxRetries = Math.Clamp(options.MaxRetries, 1, 10);

            return options;
        }
    }
}
