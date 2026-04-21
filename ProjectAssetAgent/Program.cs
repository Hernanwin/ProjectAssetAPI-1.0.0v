using ProjectAssetAgent.Models;
using ProjectAssetAgent.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("iniciado.");

        var api = new ApiService();

        var asset = new VisualLab
        {
            Hostname = SystemCollector.GetHostname(),
            IPAddress = SystemCollector.GetIPAddress(),
            OperatingSystem = SystemCollector.GetOperatingSystem(),
            Status = "Online"
        };

        await api.SendAssetAsync(asset);

        Console.WriteLine("Datos enviados.");
    }
}
