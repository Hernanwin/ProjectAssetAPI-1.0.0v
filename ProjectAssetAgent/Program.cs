using ProjectAssetAgent.Models;
using ProjectAssetAgent.Services;

var runOnce = args.Any(a =>
    string.Equals(a, "--once", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(a, "-1", StringComparison.OrdinalIgnoreCase));

var options = AgentOptions.Load();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.WriteLine($"[{DateTime.UtcNow:O}] Señal de detención recibida. Cerrando agente...");
    cts.Cancel();
};

Console.WriteLine($"[{DateTime.UtcNow:O}] ProjectAssetAgent iniciado -> API: {options.ApiBaseUrl} | Intervalo: {options.HeartbeatIntervalSeconds}s | Modo: {(runOnce ? "único (--once)" : "heartbeat continuo")}");

using var api = new ApiService(options);

async Task SendHeartbeatAsync(CancellationToken token)
{
    var asset = new VisualLab
    {
        Hostname = SystemCollector.GetHostname(),
        IPAddress = SystemCollector.GetIPAddress(),
        OperatingSystem = SystemCollector.GetOperatingSystem(),
        Status = "Online"
    };

    await api.SendAssetAsync(asset, token);
}

await SendHeartbeatAsync(cts.Token);

if (runOnce || cts.IsCancellationRequested)
{
    Console.WriteLine($"[{DateTime.UtcNow:O}] ProjectAssetAgent finalizado.");
    return;
}

using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.HeartbeatIntervalSeconds));

try
{
    while (await timer.WaitForNextTickAsync(cts.Token))
    {
        await SendHeartbeatAsync(cts.Token);
    }
}
catch (OperationCanceledException)
{
    // Cierre limpio
}

Console.WriteLine($"[{DateTime.UtcNow:O}] ProjectAssetAgent detenido correctamente.");
