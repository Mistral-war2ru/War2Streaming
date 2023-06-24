using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace War2Streaming
{
    public partial class Allowed_units : Form
    {
        public Allowed_units()
        {
            InitializeComponent();
        }

        private void Allowed_units_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 102; i++)
            {
                if ((((i < 64) ? MemoryRead.a1 : MemoryRead.a2) & ((long)1 << i)) != 0)
                    listBox1.Items.Add(MemoryRead.unit_types[i]);
                else listBox2.Items.Add(MemoryRead.unit_types[i]);
            }
        }

        int find_k(string s)
        {
            for (int i = 0; i < 102; i++)
            {
                if (MemoryRead.unit_types[i] == s) return i;
            }
            return -1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> removed = new List<string>();
            foreach (string s in listBox1.SelectedItems)
            {
                removed.Add(s);
                listBox2.Items.Add(s);
                int k = find_k(s);
                if (k != -1)
                {
                    if (k<64)
                    {
                        MemoryRead.a1 &= ~((long)1 << k);
                    }
                    else
                    {
                        MemoryRead.a2 &= ~((long)1 << k);
                    }
                }
            }
            foreach(string s in removed)
            {
                listBox1.Items.Remove(s);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<string> removed = new List<string>();
            foreach (string s in listBox2.SelectedItems)
            {
                removed.Add(s);
                listBox1.Items.Add(s);
                int k = find_k(s);
                if (k != -1)
                {
                    if (k < 64)
                    {
                        MemoryRead.a1 |= ((long)1 << k);
                    }
                    else
                    {
                        MemoryRead.a2 |= ((long)1 << k);
                    }
                }
            }
            foreach (string s in removed)
            {
                listBox2.Items.Remove(s);
            }
        }
    }
}
