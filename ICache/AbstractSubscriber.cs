using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoDiscovery;

namespace Cache
{
    public abstract class AbstractSubscriber
    {
        protected string SubscribingServiceName = null;
        private Action<string, object> _methodToCall; 
        public void Subscribe(string typeFullname, object key,Action<string, object> methodToCall)
        {
            // simply override the old key for that type if its there
            var mainkey=subscriptions.GetOrAdd(typeFullname, new ConcurrentDictionary<object, object>());
            mainkey.AddOrUpdate(key, key,(o, o1) => o);
            _methodToCall = methodToCall;
        }

        private ConcurrentDictionary<string, ConcurrentDictionary<object,object>> subscriptions;

        private ConcurrentQueue<KeyValuePair<object, string>> messageQueue;

        /// <summary>
        /// Implement your external listener here then call 
        /// EnqueueMsg(object key, Type msg)
        /// </summary>
        protected abstract void ListenForExternalMessages();

        public abstract void Start();
        public abstract void Stop();

        protected AbstractSubscriber()
        {
            subscriptions = new ConcurrentDictionary<string, ConcurrentDictionary<object,object>>();
            messageQueue = new ConcurrentQueue<KeyValuePair<object, string>>();
            Task.Factory.StartNew(CheckForSubscribedMsgs, TaskCreationOptions.LongRunning);
            
        }

        protected void EnqueueMsg(object key, string typeFullname)
        {
            lock (SubscribingServiceName)
            {
                messageQueue.Enqueue(new KeyValuePair<object, string>(key, typeFullname));
                Monitor.Pulse(SubscribingServiceName);
            }
        }

        private void CheckForSubscribedMsgs()
        {
            while (true) //figure out a way to exit
            {
                if (string.IsNullOrWhiteSpace(SubscribingServiceName))
                {
                    Thread.Sleep(1000);
                    continue;
                }
                lock (SubscribingServiceName)
                {
                    KeyValuePair<object, string> msg;
                    while (messageQueue.TryDequeue(out msg))
                    {
                        ConcurrentDictionary<object, object> keys;
                        if (subscriptions.TryGetValue(msg.Value, out keys))
                        {
                            object trash;
                            if (keys.TryRemove(msg.Key, out trash))
                            {
                                // We remove it from the subscriptions under the assumption 
                                // that you will implement the code correctly and kill the item
                                _methodToCall.DynamicInvoke(msg.Value, msg.Key);

                            }
                        }
                    }
                    Monitor.Wait(SubscribingServiceName);
                }
            }
        }

    }
}
