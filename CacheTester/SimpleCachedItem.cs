using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheTester
{
    [Serializable]
    internal class SimpleCachedItem
    {
        private int _id;
        private int[] _values;
        public SimpleCachedItem()
        {
            _values = new int[64];
        }

        public SimpleCachedItem(int id)
        {
            _values = new int[64];
            _id = id;
        }

        public int CorrelationId
        {
            get { return _id; }
            set { _id = value; }
        }
        public DateTime Modified { get; set; }
    }
}
