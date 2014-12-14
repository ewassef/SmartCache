using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZmqCacheSubscriber;

namespace ZmqCacheSubscriberTest
{
    public partial class Form1 : Form
    {
        private ZmqSubscriber subscriber;
        private Form1 _this;
        public Form1()
        {
            InitializeComponent();
            subscriber = new ZmqSubscriber();
            _this = this;
        }

        List<object> messages  = new List<object>();

        private void button1_Click(object sender, EventArgs e)
        {
            subscriber.Subscribe(typeof(Button).FullName,textBox1.Text.ToString(),MethodToCall);
            subscriber.Start();
        }

        private void MethodToCall(string type, object o)
        {
            messages.Add(new{Type=type,Key=o});
            InvokeOnUiThread(refresh);
        }

        private void InvokeOnUiThread(Action action)
        {
            if (_this.InvokeRequired)
                _this.Invoke(new MethodInvoker(action.Invoke));
            else
            {
                action.Invoke();
            }
        }

        void refresh()
        {
            dataGridView1.DataSource = messages.ToList();
            dataGridView1.Refresh();
            dataGridView1.Invalidate();
        }
    }
}
