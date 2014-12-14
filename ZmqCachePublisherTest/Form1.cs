using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZmqCachePublisher;

namespace ZmqCachePublisherTest
{
    public partial class Form1 : Form
    {
        private ZmqPublisher publisher;
        public Form1()
        {
            InitializeComponent();
            publisher = new ZmqPublisher();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            publisher.Notify(button1.GetType(),textBox1.Text.Trim());
        }
    }
}
