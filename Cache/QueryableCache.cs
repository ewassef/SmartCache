using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cache
{
    public class QueryableCache<T> : Cache<T> where T : class
    {
        public QueryableCache(Func<T, object> keySelector = null)
            : base(keySelector)
        {

        }
        public IEnumerable<T> Find(Func<T, bool> selector)
        {
            return Storage.Values.Where(selector);
        }
    }
}
