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
        private Form1 _this;
        public Form1()
        {
            InitializeComponent();
            _this = this;
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

        private async void button1_Click(object sender, EventArgs e)
        {

            var amnt = numericUpDown1.Value * 1000000;
            console.AppendText(string.Format("Preparing to load {0} records in cache in\n", amnt));
            console.AppendText("=================================================\n");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await FillCache(amnt);
            sw.Stop();
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

        private Cache<SimpleCachedItem> _itemCache;
        private async Task FillCache(decimal amnt)
        {
            await Task.Factory.StartNew(() =>
            {
                if (_itemCache == null)
                    _itemCache = new Cache<SimpleCachedItem>(x => x.CorrelationId);
                _itemCache.Clear();
                for (var i = 0; i < amnt; i++)
                {
                    _itemCache.Add(new SimpleCachedItem(i));
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

        private async Task RandomlyChangeStuff(int amnt)
        {
            var random = new Random(DateTime.Now.Millisecond);
            await Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < amnt; i++)
                {
                    var s = random.Next(amnt);
                    var item = _itemCache.Get(s);
                    item.Modified = DateTime.Now;
                    if (i % 10000 == 0)
                        InvokeOnUiThread(() =>
                            { 
                                console.AppendText(string.Format("Modified {0} records in cache\n", i)); 
                            });
                }
            });
        }
    }
}
