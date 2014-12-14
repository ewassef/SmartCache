using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace AutoDiscovery
{
    public class Announcer :IDisposable
    {
        public bool Disposing = false;

        // Fixed Port for broadcast.
        // You may change it but CLIENT and SERVER must be configured with the same port.
        private const int AutoDiscoveryPort = 20001;
        private const string AutoDiscoveryIp = "239.255.255.255";
        private const int AutoDiscoveryTimeout = 1000;
        private static readonly UdpClient Udp;
        static readonly IPEndPoint GroupEp;
        static readonly DataContractSerializer Serializer;
        static Announcer()
        {
            Udp = new UdpClient { EnableBroadcast = true, Client = { ReceiveTimeout = AutoDiscoveryTimeout } };
            GroupEp = new IPEndPoint(IPAddress.Parse("255.255.255.255"), AutoDiscoveryPort);
            Serializer = new DataContractSerializer(typeof(BasicTransportMsg));
        }


        public void Dispose()
        {
           Udp.Close();
        }

        public static void BroadcastAvailability(string serviceToBroadcast, IPAddress listeningIp, int listeningPort)
        {
            var serializer = new DataContractSerializer(typeof(BasicTransportMsg));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, new BasicTransportMsg
                {
                    Ip = listeningIp,
                    Port = listeningPort,
                    Request = Direction.Outbound,
                    ServiceType = serviceToBroadcast
                });
            var packet = stream.ToArray();
            try
            {
                Udp.Send(packet, packet.Length, GroupEp);
            }
            catch (SocketException e)
            {
            }
           
        }

        public static void BroadcastDiscovery(string serviceToDiscover)
        {
           
            
            var stream = new MemoryStream();
            Serializer.WriteObject(stream, new BasicTransportMsg
            {
                Ip = IPAddress.Any,
                Port = 0,
                Request = Direction.Inbound,
                ServiceType = serviceToDiscover
            });
            var packet = stream.ToArray();
            try
            {
                Udp.Send(packet, packet.Length, GroupEp);
            }
            catch (SocketException e)
            {
            }
            
        }
    }
}