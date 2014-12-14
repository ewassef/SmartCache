using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AutoDiscovery
{
    public class Networking
    {
        public static int GetOpenPort()
        {
            const int portStartIndex = 10000;
            const int portEndIndex = 20000;
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var endPoints = properties.GetActiveTcpListeners().ToList();
            endPoints.AddRange(properties.GetActiveUdpListeners());
            var usedPorts = endPoints.Select(p => p.Port).ToList();
            for (var port = portStartIndex; port < portEndIndex; port++)
            {
                if (usedPorts.Contains(port))
                    continue;
                return port;
            }
            return -1;
        }
        public static List<IPAddress> GetMachineIps()
        {
            return Dns.GetHostAddresses(Dns.GetHostName()).Where(x=>x.AddressFamily==AddressFamily.InterNetwork).ToList();
        }
    }
}