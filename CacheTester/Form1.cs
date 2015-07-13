using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cache;

namespace CacheTester
{
    public partial class Form1 : Form
    {
        private readonly QueryableCache<SimpleCachedItem> _queryable;
        private readonly CollectionCache<SimpleCachedItem> _collectionCache;
        private readonly Cache<SimpleCachedItem> _itemCache;
        private readonly Form1 _this;
        public Form1()
        {
            InitializeComponent();
            _this = this;
            if (_itemCache == null)
                _itemCache = new Cache<SimpleCachedItem>(x => x.CorrelationId);
            _collectionCache = new CollectionCache<SimpleCachedItem>(x => x.CorrelationId);
            _collectionCache.MissingItemsReadthrough = Replenish;
            _queryable = new QueryableCache<SimpleCachedItem>();
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            var amnt = numericUpDown1.Value * 1000000;
            console.AppendText(string.Format("Preparing to load {0} records in cache in\n", amnt));
            console.AppendText("=================================================\n");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await FillCache(amnt);
            sw.Stop();
            await DoCollectionCache();
            console.AppendText(string.Format("Populated {0} records in cache in {1}\n", amnt, sw.Elapsed));
            console.AppendText(string.Format("\t rate of {0} records/sec\n", decimal.Round((amnt / sw.ElapsedMilliseconds) * 1000, 2)));
            console.AppendText(string.Format("Preparing to randomly change {0} records in cache in\n", amnt / 2));
            console.AppendText("=================================================\n");
            sw.Start();
            await RandomlyChangeStuff((int)(amnt / 2));
            sw.Stop();
            console.AppendText(string.Format("Modified {0} records in cache in {1}\n", amnt / 2, sw.Elapsed));
            console.AppendText(string.Format("\t rate of {0} records/sec\n", decimal.Round(((amnt / 2) / sw.ElapsedMilliseconds) * 1000, 2)));

        }

        private async Task DoCollectionCache()
        {
            await Task.Run(() =>
            {
                int limit = (int) (_itemCache.ItemsInCache/10000);
                _collectionCache.Add("all", _queryable.Find(x => x.CorrelationId > limit));
                _queryable.Find(x => x.CorrelationId > limit).ToList().ForEach(s =>
                {
                    _itemCache.Clear(s);
                });
                _collectionCache.Get("all");
            });
        }

        private async Task FillCache(decimal amnt)
        {
            await Task.Factory.StartNew(() =>
            {

                for (var i = 0; i < amnt; i++)
                {
                    _itemCache.Add(new SimpleCachedItem(i), null, TimeSpan.FromSeconds(1), x => InvokeOnUiThread(() =>
                    {
                        if (x.CorrelationId % 100000 == 0)
                            console.AppendText(string.Format("Removed {0} from cache\n", x.CorrelationId));
                    }));
                    if (i % 100000 == 0)
                        InvokeOnUiThread(() =>
                        {
                            lblItemsCache.Text = _itemCache.ItemsInCache.ToString();
                        });
                }
                InvokeOnUiThread(() =>
                {
                    lblItemsCache.Text = _itemCache.ItemsInCache.ToString();
                });
            });
        }

        void InvokeOnUiThread(Action action)
        {
            if (_this.InvokeRequired)
            {
                _this.Invoke(new MethodInvoker(action));
            }
            else
            {
                action();
            }
        }

        private async Task RandomlyChangeStuff(int amnt)
        {
            var random = new Random(DateTime.Now.Millisecond);
            await Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < amnt; i++)
                {
                    var s = random.Next(amnt);
                    var item = _itemCache.Get(s);
                    if (item != null)
                    {
                        item.Modified = DateTime.Now;
                        if (i % 10000 == 0)
                            InvokeOnUiThread(() =>
                            {
                                console.AppendText(string.Format("Modified {0} records in cache\n", i));
                            });
                    }
                }
            });
        }

        private IEnumerable<SimpleCachedItem> Replenish(IEnumerable<object> args)
        {
            InvokeOnUiThread(() =>
            {
                console.AppendText(string.Format("Cache is requesting the replenishing of the ids {0}",
                    string.Join(",", args)));
            });
            return args.ToList().Select(i => new SimpleCachedItem {CorrelationId = Convert.ToInt32(i)});
        }
    }
}
