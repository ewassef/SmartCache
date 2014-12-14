using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoDiscovery;
using Cache;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.Owin.Hosting;
using Owin;

namespace SignalRPublisher
{
    public class SignalR : PersistentConnection,IPublisher
    {
        private const string Service = "SignalRCachePublisher";
        public string PublishingServiceName { get { return Service; } }
        private static int _port;
        private static readonly Listener Listener;
        public ConcurrentDictionary<string,string> connections = new ConcurrentDictionary<string, string>(); 

        public void Notify(Type type, object key)
        {
            connections.ToList().ForEach(c => Connection.Send(c.Key,new{type.FullName, Key = key.ToString()}));
            
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            connections[connectionId] = connectionId;
            return base.OnConnected(request, connectionId);
        }

        protected override Task OnDisconnected(IRequest request, string connectionId)
        {
            var outc = connectionId;
            connections.TryRemove(connectionId, out outc);
            return base.OnDisconnected(request, connectionId);
        }
        
        public SignalR()
        {
            _port = Networking.GetOpenPort();
            
            using (WebApp.Start<Startup>(string.Format("http://*:{0}/",_port)))
            {
                Console.WriteLine("Server running at http://localhost:/" +_port);
                Console.ReadLine();
            }
        }
        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                // Turn cross domain on 
                //var config = new HubConfiguration { EnableCrossDomain = true };
                // This will map out to http://localhost:X/signalr by default
                var x = new ConnectionConfiguration() {EnableCrossDomain = true};
                //app.MapHubs(config);
                
                app.MapConnection<SignalR>(string.Format("http://localhost:{0}/signalr",_port), x);
            }
        }
       
    }
}
