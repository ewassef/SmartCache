using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoDiscovery;
using Cache;
using ZMQ;

namespace ZmqCachePublisher
{
    public class ZmqPublisher : IPublisher
    {
        private static readonly Context Context;
        private static readonly Socket Socket;
        private const string Service = "ZmqCachePublisher";
        private static readonly Listener Listener;
        private static readonly int Port;
        static ZmqPublisher()
        {
            Context = new Context(10);
            Socket = Context.Socket(SocketType.PUB);
            Port = Networking.GetOpenPort();
            Socket.SndBuf = 1024 * 1024 * 50;
            Socket.RcvBuf = 1024 * 1024 * 50;
            Socket.Bind(string.Format("tcp://*:{0}", Port));
            Networking.GetMachineIps().ForEach(ip => Announcer.BroadcastAvailability(Service, ip, Port));
            Listener = new Listener(Service, NewClientArrived);
            StartListening();
        }

        private static void NewClientArrived(string serviceName, IPAddress ipAddress, int port)
        {
            if (serviceName != Service)
                return;

            Listener.StopListening();
            Networking.GetMachineIps().ForEach(ip => Announcer.BroadcastAvailability(Service, ip, Port));
            StartListening();
        }

        private static void StartListening()
        {
            if (Listener.IsRunning)
                Listener.StopListening();
            Task.Factory.StartNew(() => Listener.Start(Direction.Inbound));
        }

        public string PublishingServiceName { get { return Service; } }

        public void Notify(Type type, object key)
        {
            Socket.Send(Encoding.Unicode.GetBytes(string.Format("{0}|{1}", type, key)));
        }


    }
}
