using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cache
{
    public class BusinessLogicCache<T>
        where T : class
    {
        private static Cache<T> _cache;
        private static readonly ConcurrentDictionary<object, List<object>> Storage = new ConcurrentDictionary<object, List<object>>();

        public BusinessLogicCache()
        {
            if (_cache == null)
            {
                _cache = new Cache<T>();
            }
        }

        public static void Add(object key, Collection<T> value)
        {
            var ids = value.Select(x => _cache.Selector.Invoke(x)).ToList();
            Storage.AddOrUpdate(key, ids, (x, y) => Storage.TryUpdate(key, ids, ids) ? ids : null);
            value.ToList().ForEach(x => _cache.Add(x));
        }

        public static Collection<T> Get(object key)
        {
            List<object> keys;
            Storage.TryGetValue(key, out keys);
            var collection = new Collection<T>();
            keys.Select(x => _cache.Get(x)).ToList().ForEach(collection.Add);
            return collection;
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