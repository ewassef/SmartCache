using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cache
{
    public class Cache<T> where T : class
    {
        internal static readonly ConcurrentDictionary<object, T> Storage = new ConcurrentDictionary<object, T>();

        internal static readonly List<CacheRecord> Timeouts = new List<CacheRecord>();

        private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(10);

        public Func<T, object> Selector { get; set; }

        public Action<T> Writethrough;
        public Func<object, T> Readthrough;

        public Cache(Func<T, object> keySelector = null)
        {
            if (keySelector != null)
                Selector = keySelector;

            ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), (i, g) =>
            {
                ClearKeys();
            }, null, TimeSpan.FromSeconds(10), false);
        }

        public T Get(object key, Func<object, T> readthroughOverride = null)
        {
            T item;
            if (!Storage.TryGetValue(key, out item))
            {
                if (readthroughOverride != null)
                {
                    item = readthroughOverride.Invoke(key);
                }
                else if (Readthrough != null)
                {
                    item = Readthrough.Invoke(key);
                }
                if (item != null)
                    Storage[Selector.Invoke(item)] = item;
            }

            return item;
        }

        public void Add(T value, Action<T> writethroughOverride = null, TimeSpan? expiration = null, Action<T> expiryCallback = null)
        {
            if (Selector == null)
            {
                throw new InvalidOperationException(
                    "You havent specified a Key Selector on this object, therefore, you cannot insert anything into the cache");
            }
            var key = Selector.Invoke(value);
            Storage[key] = value;
            if (writethroughOverride != null)
            {
                writethroughOverride.Invoke(value);
            }
            else
                if (Writethrough != null)
                {
                    Writethrough.Invoke(value);
                }

            lock (Timeouts)
            {
                Timeouts.Add(new CacheRecord
                {
                    ExpirationTime = DateTime.Now + (expiration ?? _defaultTimeout),
                    Key = key,
                    ExpiryCallback = expiryCallback
                });
            }
        }

        private void ClearKeys()
        {
            var items = Cleanup();
            foreach (var item in items)
            {
                T value;
                if (!Storage.TryRemove(item.Key, out value)) continue;
                if (item.ExpiryCallback != null)
                    item.ExpiryCallback(value);
            }
        }

        public long ItemsInCache
        {
            get { return Storage.Count; }
        }

        public void Clear()
        {
            Storage.Clear();
        }

        public void Clear(T item)
        {
            Storage.TryRemove(Selector.Invoke(item), out item);
        }

        private List<CacheRecord> Cleanup()
        {
            var value = DateTime.Now;
            lock (Timeouts)
            {
                var records = Timeouts.Where(x => x.ExpirationTime < value).ToList();
                Timeouts.RemoveAll(x => x.ExpirationTime < value);
                return records;
            }
        }

        internal class CacheRecord
        {
            public object Key { get; set; }

            public Action<T> ExpiryCallback { get; set; }

            public DateTime ExpirationTime { get; set; }
        }
    }
}