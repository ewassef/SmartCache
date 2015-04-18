using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Cache
{
    public class CollectionCache<T>
        where T : class
    {
        private static Cache<T> _cache;
        private static readonly ConcurrentDictionary<object, List<object>> Storage = new ConcurrentDictionary<object, List<object>>();
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(10);
        public CollectionCache(Func<T, object> singleItemKeySelector)
        {
            if (_cache == null)
            {
                _cache = new Cache<T>(singleItemKeySelector);
            }
        }

        public const string DEFAULTKEY = "__ALL__";
        public void Add(object key, IEnumerable<T> value, Action<IEnumerable<T>> collectionWritethrough = null, TimeSpan? expiration = null,Action<object> expirationCallback = null)
        {
            var ids = value.Select(x => _cache.Selector.Invoke(x)).ToList();
            Storage[key ?? DEFAULTKEY] = ids;
            value.ToList().ForEach(x => _cache.Add(x,null,expiration));
            if (collectionWritethrough != null)
            {
                collectionWritethrough.Invoke(value);
            }

            ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), (a, b) =>
            {
                ClearKey(a, b);
                if (expirationCallback != null)
                    expirationCallback(key);
            }, key, expiration ?? _defaultTimeout,
                                                   true);
        }

        private void ClearKey(object key, bool timedOut)
        {
            Clear(key);
        }

        public IEnumerable<T> Get(object key, Func<object, IEnumerable<T>> collectionReadthrough = null, TimeSpan? expirationInCaseOfReadThrough = null)
        {

            if (key == null)
            {

                if (collectionReadthrough != null && _cache.ItemsInCache == 0)
                {
                    var items = collectionReadthrough.Invoke(key).ToList();
                    items.ForEach(x => _cache.Add(x));
                    return items;
                }
                return Cache<T>.Storage.Select(x => x.Value);
            }

            List<object> keys;
            if (Storage.TryGetValue(key, out keys))
            {
                var collection = new Collection<T>();
                keys.Select(x => _cache.Get(x)).ToList().ForEach(collection.Add);
                return collection.Where(x=>x!=null);
            }
            //get them and put them in the cache
            if (collectionReadthrough != null)
            {
                var items = collectionReadthrough.Invoke(key).ToList();
                Add(key, items, null, expirationInCaseOfReadThrough ?? _defaultTimeout);
                return items;
            }
            return null;
        }

        public long ItemsInCache
        {
            get { return Storage.Count; }
        }

        public void Clear()
        {
            Storage.Clear();
        }

        public void Clear(object key)
        {
            List<object> dummy;
            Storage.TryRemove(key ?? DEFAULTKEY, out dummy);
        }

        public Cache<T> ItemCache { get { return _cache; } }

        public void Add(T item, Action<T> writethrough = null)
        {
            _cache.Add(item,writethrough);
            //add this to the "all" key for later
            List<object> keys;
            if (!Storage.TryGetValue(DEFAULTKEY, out keys)) return;
            if (keys.All(x => !x.Equals(_cache.Selector(item))))
                keys.Add(_cache.Selector(item));
            Storage[DEFAULTKEY] = keys;
        }

        public void FlushCollectionCacheLeaveItems()
        {
            var keys = Storage.Keys.ToList();
            foreach (var key in keys)
                Clear(key);
        }

        public IEnumerable<object> FindKeysReferencing(object itemKey)
        {
            var pairs = Storage.Where(x => x.Value == itemKey);
            return pairs.Select(x => x.Key);
        }
    }
}