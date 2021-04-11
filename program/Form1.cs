using System;
using System.Windows.Forms;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace War2Twitch
{
    public partial class Form1 : Form
    {
        private Bot b = new Bot();

        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (b.con==1)
            {
                label2.Text = "Twitch connected";
            }
            if (b.text == "")
            {
                if (b.msgs.Count != 0)
                {
                    b.text = b.msgs[0];
                    b.msgs.RemoveAt(0);
                }
            }
            if (b.text!="")
            {
                if (checkBox1.Checked)
                {
                    int r = MemoryRead.SendMsg(b.text);
                    if (r == 1)
                    {
                        label1.Text = "Message was send to War2 successfully";
                        b.text = "";
                    }
                    if (r == 2)
                        label1.Text = "Waiting for War2 to receive message...";
                    if (r == 0)
                        label1.Text = "War2 process not found";
                    if (r == 3)
                        label1.Text = "War2Twitch plugin not found";
                }
                else
                    b.text = "";
            }
            if (b.client.IsConnected) 
            {
                if (b.client.JoinedChannels.Count != 0) 
                {
                    if (checkBox2.Checked)
                    {
                        MemoryRead.RecMsg();
                        if (MemoryRead.msgs.Count != 0)
                        {
                            label1.Text = "Received message from War2: " + MemoryRead.msgs[0];
                            //if (checkBox2.Checked)
                            b.client.SendMessage(b.chan, MemoryRead.msgs[0]);
                            MemoryRead.msgs.RemoveAt(0);
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox2.Text.Length >= 8) && (textBox3.Text.Length >= 5))
            {
                label2.Text = "Connecting to twitch...";
                if (b.client.IsConnected)
                    b.client.Disconnect();
                ConnectionCredentials credentials = new ConnectionCredentials("War2Twitch", textBox2.Text);
                var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 750,
                    ThrottlingPeriod = TimeSpan.FromSeconds(30)
                };
                WebSocketClient customClient = new WebSocketClient(clientOptions);
                b.client = new TwitchClient(customClient);
                b.chan = "";
                b.con = 0;

                b.client.Initialize(credentials, textBox3.Text);

                b.client.OnMessageReceived += b.Client_OnMessageReceived;
                b.client.OnJoinedChannel += b.Client_OnJoinedChannel;

                b.client.Connect();
                if (b.client.IsConnected)
                {
                        b.chan = textBox3.Text;
                }
                timer2.Interval = 10000;
                timer2.Start();
            }
            else
                label2.Text = "Twitch Error! Write correct data first!";
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (b.con == 0)
                label2.Text = "Twitch NOT connected";
        }
    }
}
