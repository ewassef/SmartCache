using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoDiscovery;
using Cache;
using ZMQ;


namespace ZmqCacheSubscriber
{
    public class ZmqSubscriber : AbstractSubscriber
    {
        List<Context> _zmqContexts = new List<Context>();
        List<Socket> _zmqSockets = new List<Socket>(); 
        private static Listener _listener;
        private static bool _running;

        public ZmqSubscriber()
        {
            SubscribingServiceName = "ZmqCachePublisher";
        }
        
        private  void NotificationMethod(string serviceName,IPAddress ipAddress, int port)
        {
            if (serviceName != SubscribingServiceName)
                return;
            
            var context = new Context(10);
            var soc = context.Socket(SocketType.SUB);
            soc.RcvBuf = 1024 * 50;
            soc.Connect(string.Format("tcp://{0}:{1}",ipAddress,port));
            soc.Subscribe(string.Empty,Encoding.Unicode);
            Task.Factory.StartNew(() =>
            {
                while (_running)
                {
                    try
                    {
                        var msg = soc.Recv(Encoding.Unicode).Split('|');
                        //string.Format("{0}|{1}", type, key)
                        if (msg.Length!=2)continue;

                        var type = msg.First();
                        var key = msg.Last();
                        EnqueueMsg(key, type);

                    }
                    catch (System.Exception e)
                    { }
                }
            });
            _zmqContexts.Add(context);
            _zmqSockets.Add(soc);
        }


        /// <summary>
        /// Implement your external listener here then call 
        /// EnqueueMsg(object key, Type msg)
        /// </summary>
        protected override void ListenForExternalMessages()
        {
            //This is a multithreaded multi-endpoint call, so we are doing it ourselves
        }

        public override void Start()
        {
            if (_listener == null || !_listener.IsRunning)
            {
                _running = true;
                _listener = new Listener(SubscribingServiceName, NotificationMethod);
                Task.Factory.StartNew(() => _listener.Start(), TaskCreationOptions.LongRunning);
                Announcer.BroadcastDiscovery(SubscribingServiceName);
            }
        }

        public override void Stop()
        {
            _running = false;
        }
    }
}
