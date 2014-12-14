﻿using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Runtime.Caching;

namespace Cache
{
    public class Cache<T> where T : class
    {
        internal static readonly ConcurrentDictionary<object, T> Storage = new ConcurrentDictionary<object, T>();
        internal static readonly ObjectCache cache = new MemoryCache(typeof(T).Name,new NameValueCollection());
        public Func<T, object> Selector { get; set; }
        public Action<T> Writethrough;
        public Func<object, T> Readthrough;
        public Cache(Func<T, object> keySelector = null)
        {
            if (keySelector != null)
                Selector = keySelector;
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
                    Storage[Selector.Invoke(item)] =  item;
            }

            return item;
        }
        public void Add(T value, Action<T> writethroughOverride = null)
        {
            if (Selector == null)
            {
                throw new InvalidOperationException(
                    "You havent specified a Key Selector on this object, therefore, you cannot insert anything into the cache");
            }
            Storage[Selector.Invoke(value)] =value;
            if (writethroughOverride != null)
            {
                writethroughOverride.Invoke(value);
            }
            else
                if (Writethrough != null)
                {
                    Writethrough.Invoke(value);
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
    }
}