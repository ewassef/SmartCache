using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoDiscovery;

namespace AutodiscoveryTester
{
    public partial class Form1 : Form
    {
        private Form1 _this;
        public Form1()
        {
            InitializeComponent();
            
            _this = this;
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
            dataGridView1.DataSource = _messagesRecvieved.ToList();
            dataGridView1.Refresh();
            dataGridView1.Invalidate();
        }

        private readonly List<object> _messagesRecvieved = new List<object>();

        private void MessageCameIn(string servicename,IPAddress ipAddress, int port)
        {
            _messagesRecvieved.Add(new {Service = servicename,IP= ipAddress,Port = port});
            InvokeOnUiThread(refresh);
        }

        private Listener _listener;
        

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (string.IsNullOrEmpty(textBox1.Text))
                return;
            if (button1.Text == "Start")
            {
                if (_listener != null)
                {
                    _listener.StopListening();
                    _listener = null;
                }
            }
            _listener = new Listener(textBox1.Text, MessageCameIn);
            _listener.LookForPublishersAndListen();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Announcer.BroadcastAvailability(textBox2.Text.Trim(),IPAddress.Any,-1);
        }
    }
}
