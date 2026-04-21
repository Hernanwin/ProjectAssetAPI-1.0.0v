using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
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
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "Unknown";
        }

        public static string GetOperatingSystem()
        {
            return RuntimeInformation.OSDescription;
        }
    }
}
