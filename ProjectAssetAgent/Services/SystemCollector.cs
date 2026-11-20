using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ProjectAssetAgent.Services
{
    public static class SystemCollector
    {
        public static string GetHostname()
        {
            return Environment.MachineName;
        }

        public static string GetIPAddress()
        {
            try
            {
                var activeInterfaceIp = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni =>
                        ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                    .Select(ua => ua.Address)
                    .FirstOrDefault(ip =>
                        ip.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(ip));

                if (activeInterfaceIp is not null)
                {
                    return activeInterfaceIp.ToString();
                }
            }
            catch
            {
                // Fallback a resolución DNS si no se pueden consultar interfaces
            }

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch
            {
                // Ignorado: devolvemos Unknown
            }

            return "Unknown";
        }

        public static string GetOperatingSystem()
        {
            return RuntimeInformation.OSDescription.Trim();
        }
    }
}
