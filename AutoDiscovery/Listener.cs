using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDiscovery
{
    public class Listener
    {
        // In the following example code we reply to incoming client an IP Address that
        // Client must use as server for any purpose. (TCP Server not implemented)
        public IPAddress AddrDaemonListenIp;

        // Which port we will broadcast as TCP Server (not implemented).
        public int BroadCastDaemonPort = 0;

        // Port the UDP server will listen to broadcast packets from UDP Clients.
        private const int AutoDiscoveryPort = 20001;

        private const int AutoDiscoveryTimeout = 10000;
        private static readonly UdpClient Newsock = null;
        private readonly DataContractSerializer _deserializer;
        private readonly Action<string, IPAddress, int> _notificationMethod;
        private readonly string _serviceToListenFor;
        private Task _listener;
        private bool _running;
        static Listener()
        {
            Newsock = new UdpClient();

            var group = new IPEndPoint(IPAddress.Any, AutoDiscoveryPort);

            Newsock.AllowNatTraversal(true);
            Newsock.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Newsock.Client.Bind(group);
        }

        public Listener(string serviceToListenFor, Action<string, IPAddress, int> notificationMethod)
        {
            _deserializer = new DataContractSerializer(typeof(BasicTransportMsg));
            _notificationMethod = notificationMethod;
            _serviceToListenFor = serviceToListenFor;
        }

        public bool IsRunning
        {
            get { return _running; }
        }

        public void LookForPublishersAndListen()
        {
            //send once to indicate that youre up
            Announcer.BroadcastDiscovery(_serviceToListenFor);
            _listener = Task.Factory.StartNew(() => Start());
        }
        public void Start(Direction messageDirection = Direction.Outbound)
        {
            _running = true;
            while (_running)
            {
                try
                {
                    do
                    {
                        var local = new IPEndPoint(IPAddress.Any, 0);
                        var receivedData = Newsock.Receive(ref local);
                        var msg = _deserializer.ReadObject(new MemoryStream(receivedData));
                        if (!(msg is BasicTransportMsg))
                            continue;
                        var btMsg = msg as BasicTransportMsg;
                        if (btMsg.ServiceType == _serviceToListenFor && btMsg.Request == messageDirection)
                            _notificationMethod.Invoke(btMsg.ServiceType, btMsg.Ip, btMsg.Port);
                    } while (_running);
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.TimedOut)
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(1000);
                }
                finally
                {
                    if (Newsock != null)
                        Newsock.Close();
                }
            }
        }

        public void StopListening()
        {
            _running = false;
            _listener.Wait(AutoDiscoveryTimeout);
            //force it to stop
            _listener = null;
        }
    }
}