using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using Google.Apis.YouTube.v3.Data;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using YouTube.Base;
using YouTube.Base.Clients;

namespace War2Streaming
{
    public partial class Form1 : Form
    {
        public static readonly List<OAuthClientScopeEnum> scopes = new List<OAuthClientScopeEnum>()
        {
            //OAuthClientScopeEnum.ChannelMemberships,
            //OAuthClientScopeEnum.ManageAccount,
            OAuthClientScopeEnum.ManageData,
            //OAuthClientScopeEnum.ManagePartner,
            //OAuthClientScopeEnum.ManagePartnerAudit,
            //OAuthClientScopeEnum.ManageVideos,
            //OAuthClientScopeEnum.ReadOnlyAccount,
            //OAuthClientScopeEnum.ViewAnalytics,
            //OAuthClientScopeEnum.ViewMonetaryAnalytics
        };

        private Bot b = new Bot();

        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (b.con == 0) label2.Text = "Twitch NOT connected";
            if (b.con == 1) label2.Text = "Twitch connected";
            if (b.ycon == 1)
            {
                label8.Text = "Youtube connected";
                textBox10.Enabled = false;
            }
            int read = MemoryRead.ReadPointers(textBox1.Text);
            if (read != 1)
            {
                if (read == 0) label1.Text = "War2 process not found!";
                if (read == 2) label1.Text = "War2Streaming plugin not found!";
            }
            else
            {
                if ((label1.Text == "War2 process not found!")
                    ||(label1.Text == "War2Streaming plugin not found!")) 
                    label1.Text = "War2 OK";
                MemoryRead.SendNames(listBox1, textBox1.Text, checkBox7.Checked);
                if (b.text == "")
                {
                    if (b.msgs.Count != 0)
                    {
                        b.text = b.msgs[0];
                        b.msgs.RemoveAt(0);
                        if (checkBox4.Checked)
                        {
                            string[] splt = b.text.Split('\x03');
                            bool sub = splt[0][0] == '\x04';
                            string user = splt[0].Remove(0, 1);
                            user = user.Remove(user.Length - 1, 1);
                            string msgg = splt[1].Remove(0, 1);
                            if (user.Length > 30)
                                user = user.Remove(user.Length - 1 - (user.Length - 30), user.Length - 30);
                            user += '\x0';
                            if (msgg == textBox5.Text)
                            {
                                if ((checkBox3.Checked && sub) || !checkBox3.Checked)
                                {
                                    int indx = listBox1.Items.IndexOf(user);
                                    if (indx == -1)
                                    {
                                        indx = listBox1.Items.IndexOf(" ");
                                        if (indx != -1) listBox1.Items[indx] = user;
                                        else listBox1.Items.Add(user);
                                    }
                                    b.text = "";
                                }
                            }
                            if (msgg == textBox6.Text)
                            {
                                if ((checkBox3.Checked && sub) || !checkBox3.Checked)
                                {
                                    int indx = listBox1.Items.IndexOf(user);
                                    if (indx != -1) listBox1.Items[indx] = " ";
                                    b.text = "";
                                }
                            }
                        }
                    }
                }
                if (b.text != "")
                {
                    if (checkBox1.Checked)
                    {
                        int r = MemoryRead.SendMsg(b.text, textBox1.Text);
                        if (r == 1)
                        {
                            label1.Text = "Message was send to War2 successfully";
                            b.text = "";
                        }
                        if (r == 2)
                            label1.Text = "Waiting for War2 to receive message...";
                        if (r == 0)
                            label1.Text = "War2 process not found!";
                        if (r == 3)
                            label1.Text = "War2Streaming plugin not found!";
                    }
                    else
                        b.text = "";
                }

                MemoryRead.RecMsg(textBox1.Text);
                if (MemoryRead.msgs.Count != 0)
                {
                    for (int i = 0; i < MemoryRead.msgs.Count; i++)
                    {
                        bool msg_command = false;
                        if (MemoryRead.msgs[i] == textBox11.Text)//clear
                        {
                            listBox1.Items.Clear();
                            msg_command = true;
                        }
                        if (MemoryRead.msgs[i] == textBox12.Text)//names
                        {
                            checkBox4.Checked = !checkBox4.Checked;
                            if (checkBox4.Checked)
                            {
                                listBox1.Enabled = true;
                                textBox4.Enabled = true;
                                textBox5.Enabled = true;
                                textBox6.Enabled = true;
                                button2.Enabled = true;
                                button3.Enabled = true;
                                button4.Enabled = true;
                                checkBox3.Enabled = true;
                            }
                            else
                            {
                                listBox1.Enabled = false;
                                textBox4.Enabled = false;
                                textBox5.Enabled = false;
                                textBox6.Enabled = false;
                                button2.Enabled = false;
                                button3.Enabled = false;
                                button4.Enabled = false;
                                checkBox3.Enabled = false;
                                listBox1.Items.Clear();
                            }
                            msg_command = true;
                        }
                        if (MemoryRead.msgs[i] == textBox13.Text)//draw
                        {
                            checkBox7.Checked = !checkBox7.Checked;
                            msg_command = true;
                        }

                        if (MemoryRead.msgs[i].Length > (textBox14.Text.Length + 1))
                        {
                            string sstart = string.Copy(MemoryRead.msgs[i]);
                            try { sstart = sstart.Remove(textBox14.Text.Length, sstart.Length - textBox14.Text.Length); }
                            catch (Exception) { };
                            if (sstart == textBox14.Text)//color
                            {
                                string sc = MemoryRead.msgs[i].Remove(0, textBox14.Text.Length + 1);
                                MemoryRead.col = 251;//default yellow
                                try { MemoryRead.col = Convert.ToByte(sc); }
                                catch (Exception) { }
                                if (MemoryRead.col == 0)
                                {
                                    Random rrr = new Random();
                                    MemoryRead.col = (byte)((rrr.Next(0, 255) + 1) % 256);
                                }
                                label17.Text = string.Format("Names color: {0}", MemoryRead.col);
                                msg_command = true;
                            }
                        }
                        if (msg_command)
                        {
                            MemoryRead.msgs.RemoveAt(i);
                            break;
                        }
                    }
                    if (MemoryRead.msgs.Count != 0)
                    {
                        label1.Text = "Received message from War2:\n" + MemoryRead.msgs[0];
                        bool msg_sended = false;
                        if (b.client.IsConnected)
                        {
                            if (b.client.JoinedChannels.Count != 0)
                            {
                                if (checkBox2.Checked)
                                {
                                    if (MemoryRead.msgs.Count != 0)
                                    {
                                        b.client.SendMessage(b.chan, MemoryRead.msgs[0]);
                                        msg_sended = true;
                                    }
                                }
                            }
                        }
                        if (b.ycon != 0)
                        {
                            if (b.yclient != null)
                            {
                                if (checkBox4.Checked)
                                {
                                    if (MemoryRead.msgs.Count != 0)
                                    {
                                        b.yclient.SendMessage(MemoryRead.msgs[0]);
                                        msg_sended = true;
                                    }
                                }
                            }
                        }
                        if (msg_sended) MemoryRead.msgs.RemoveAt(0);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox2.Text.Length >= 8) && (textBox3.Text.Length >= 3))
            {
                label2.Text = "Connecting to twitch...";
                if (b.client.IsConnected)
                    b.client.Disconnect();
                ConnectionCredentials credentials = new ConnectionCredentials("War2Streaming", textBox2.Text);
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
            }
            else
                label2.Text = "Twitch Error! Write correct data first!";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            b.prog_start = DateTime.Now;
            if (File.Exists("record.save"))
            {
                StreamReader F = File.OpenText("record.save");
                while (!F.EndOfStream)
                {
                    string S = F.ReadLine();
                    textBox1.Text = S;
                    S = F.ReadLine();
                    textBox2.Text = S;
                    S = F.ReadLine();
                    textBox3.Text = S;
                    S = F.ReadLine();
                    if (!((S == "") || (S == null))) textBox5.Text = S;
                    else textBox5.Text = "!i_want_to_play_with_u";
                    S = F.ReadLine();
                    if (!((S == "") || (S == null))) textBox6.Text = S;
                    else textBox6.Text = "!bye_bye";
                    S = F.ReadLine();
                    if (S != null) checkBox1.Checked = Convert.ToBoolean(S);
                    else checkBox1.Checked = true;
                    S = F.ReadLine();
                    if (S != null) checkBox2.Checked = Convert.ToBoolean(S);
                    else checkBox2.Checked = true;
                    S = F.ReadLine();
                    if (S != null) checkBox3.Checked = Convert.ToBoolean(S);
                    else checkBox3.Checked = false;
                    S = F.ReadLine();
                    if (S != null) checkBox4.Checked = Convert.ToBoolean(S);
                    else checkBox4.Checked = true;
                    S = F.ReadLine();
                    textBox7.Text = S;
                    S = F.ReadLine();
                    textBox8.Text = S;
                    S = F.ReadLine();
                    if (S != null) checkBox5.Checked = Convert.ToBoolean(S);
                    else checkBox5.Checked = true;
                    S = F.ReadLine();
                    if (S != null) checkBox6.Checked = Convert.ToBoolean(S);
                    else checkBox6.Checked = true;
                    S = F.ReadLine();
                    if (S != null) checkBox7.Checked = Convert.ToBoolean(S);
                    else checkBox7.Checked = true;
                    S = F.ReadLine();
                    textBox9.Text = S;
                    S = F.ReadLine();
                    if (!((S == "") || (S == null))) textBox10.Text = S;
                    else textBox10.Text = "60";
                    S = F.ReadLine();
                    if (!((S == "") || (S == null))) textBox11.Text = S;
                    else textBox11.Text = "!clear";
                    S = F.ReadLine();
                    if (!((S == "") || (S == null))) textBox12.Text = S;
                    else textBox12.Text = "!names";
                    S = F.ReadLine();
                    if (!((S == "") || (S == null))) textBox13.Text = S;
                    else textBox13.Text = "!draw";
                    S = F.ReadLine();
                    if (!((S == "") || (S == null))) textBox14.Text = S;
                    else textBox14.Text = "!color";
                    S = F.ReadLine();
                    MemoryRead.col = 251;
                    if (!((S == "") || (S == null)))try {MemoryRead.col = Convert.ToByte(S);}catch(Exception){};
                    label17.Text = string.Format("Names color: {0}", MemoryRead.col);
                }
                F.Close();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (File.Exists("record.save"))
                File.Delete("record.save");
            FileStream F = File.OpenWrite("record.save");
            byte[] info = new UTF8Encoding(true).GetBytes(textBox1.Text + "\n");
            F.Write(info, 0, info.Length);
            byte[] info2 = new UTF8Encoding(true).GetBytes(textBox2.Text + "\n");
            F.Write(info2, 0, info2.Length);
            byte[] info3 = new UTF8Encoding(true).GetBytes(textBox3.Text + "\n");
            F.Write(info3, 0, info3.Length);
            byte[] info4 = new UTF8Encoding(true).GetBytes(textBox5.Text + "\n");
            F.Write(info4, 0, info4.Length);
            byte[] info5 = new UTF8Encoding(true).GetBytes(textBox6.Text + "\n");
            F.Write(info5, 0, info5.Length);
            byte[] info6 = new UTF8Encoding(true).GetBytes(checkBox1.Checked.ToString() + "\n");
            F.Write(info6, 0, info6.Length);
            byte[] info7 = new UTF8Encoding(true).GetBytes(checkBox2.Checked.ToString() + "\n");
            F.Write(info7, 0, info7.Length);
            byte[] info8 = new UTF8Encoding(true).GetBytes(checkBox3.Checked.ToString() + "\n");
            F.Write(info8, 0, info8.Length);
            byte[] info9 = new UTF8Encoding(true).GetBytes(checkBox4.Checked.ToString() + "\n");
            F.Write(info9, 0, info9.Length);
            byte[] info10 = new UTF8Encoding(true).GetBytes(textBox7.Text + "\n");
            F.Write(info10, 0, info10.Length);
            byte[] info11 = new UTF8Encoding(true).GetBytes(textBox8.Text + "\n");
            F.Write(info11, 0, info11.Length);
            byte[] info12 = new UTF8Encoding(true).GetBytes(checkBox5.Checked.ToString() + "\n");
            F.Write(info12, 0, info12.Length);
            byte[] info13 = new UTF8Encoding(true).GetBytes(checkBox6.Checked.ToString() + "\n");
            F.Write(info13, 0, info13.Length);
            byte[] info14 = new UTF8Encoding(true).GetBytes(checkBox7.Checked.ToString() + "\n");
            F.Write(info14, 0, info14.Length);
            byte[] info15 = new UTF8Encoding(true).GetBytes(textBox9.Text + "\n");
            F.Write(info15, 0, info15.Length);
            byte[] info16 = new UTF8Encoding(true).GetBytes(textBox10.Text + "\n");
            F.Write(info16, 0, info16.Length);
            byte[] info17 = new UTF8Encoding(true).GetBytes(textBox11.Text + "\n");
            F.Write(info17, 0, info17.Length);
            byte[] info18 = new UTF8Encoding(true).GetBytes(textBox12.Text + "\n");
            F.Write(info18, 0, info18.Length);
            byte[] info19 = new UTF8Encoding(true).GetBytes(textBox13.Text + "\n");
            F.Write(info19, 0, info19.Length);
            byte[] info20 = new UTF8Encoding(true).GetBytes(textBox14.Text + "\n");
            F.Write(info20, 0, info20.Length);
            byte[] info21 = new UTF8Encoding(true).GetBytes(MemoryRead.col.ToString() + "\n");
            F.Write(info21, 0, info21.Length);
            F.Close();
            listBox1.Items.Clear();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("http://en.war2.ru");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel2.LinkVisited = true;
            System.Diagnostics.Process.Start("https://github.com/Mistral-war2ru/War2Streaming");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox1.Items.Count; i++)
                if (listBox1.GetSelected(i))
                {
                    if (listBox1.Items[i] == " ") listBox1.Items.RemoveAt(i);
                    else listBox1.Items[i] = " ";
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                if (listBox1.Items.IndexOf(textBox4.Text) == -1)
                {
                    if (textBox4.Text.Length < 30)listBox1.Items.Add(textBox4.Text);
                }
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                listBox1.Enabled = true;
                textBox4.Enabled = true;
                textBox5.Enabled = true;
                textBox6.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                checkBox3.Enabled = true;
            }
            else
            {
                listBox1.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                checkBox3.Enabled = false;
                listBox1.Items.Clear();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if ((textBox7.Text.Length >= 8) && (textBox8.Text.Length >= 8))
            {
                string clientID = textBox7.Text;
                string clientSecret = textBox8.Text;
                label8.Text = "Connecting to youtube...";
                b.ycon = 0;
                textBox10.Enabled = true;
                Task.Run(async () =>
                {
                    try
                    {
                        YouTubeConnection connection = await YouTubeConnection.ConnectViaLocalhostOAuthBrowser(clientID, clientSecret, scopes);
                        if (connection != null)
                        {
                            Channel channel = null;
                            if (textBox9.Text != "") channel = await connection.Channels.GetChannelByID(textBox9.Text);
                            else channel = await connection.Channels.GetMyChannel();
                            if (channel != null) 
                            {
                                //var broadcast = await connection.LiveBroadcasts.GetMyActiveBroadcast();
                                var broadcast = await connection.LiveBroadcasts.GetChannelActiveBroadcast(channel);
                                b.yclient = new ChatClient(connection);
                                b.yclient.OnMessagesReceived += b.YClient_OnMessagesReceived;
                                if (broadcast != null)
                                {
                                    int secs = 60;
                                    try
                                    {
                                        secs = Convert.ToInt32(textBox10.Text);
                                    }
                                    catch (Exception) { textBox10.Text = "60"; }
                                    if (secs < 1)
                                    {
                                        secs = 1;
                                        textBox10.Text = "60";
                                    }
                                    if (await b.yclient.Connect(broadcast, true, 1000 * secs))//60 sec default
                                    {
                                        //await b.yclient.SendMessage("War2Streaming connected!");
                                        label8.Text = "Youtube Connected!";
                                        textBox10.Enabled = false;
                                        b.ycon = 1;
                                    }
                                    else
                                        label8.Text = "Youtube Error! No Broadcast!";
                                }
                                else
                                    label8.Text = "Youtube Error! No Broadcast!";
                            }
                            else label8.Text = "Youtube Error! No channel!";
                        }
                        else
                            label8.Text = "Youtube NOT connected";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error! " + ex.Message);
                        b.ycon = 0;
                    }
                });
            }
            else
                label8.Text = "Youtube Error! Write correct data first!";
        }
    }
}
