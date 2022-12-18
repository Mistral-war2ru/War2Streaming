﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using Google.Apis.YouTube.v3.Data;
using StreamingClient.Base.Util;
using YouTube.Base;
using YouTube.Base.Clients;

public class MemoryRead
{
    const int PROCESS_WM_READ = 0x0010;

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(int hProcess,
      int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(int hProcess, 
        int lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    public static extern Int32 CloseHandle(IntPtr hProcess);

    public static int pm = 0;
    public static int pb = 0;
    public static int pp = 0;
    public static int pn = 0;
    public static int pd = 0;
    public static List<string> msgs = new List<string>();

    public static int ReadPointers(string name)
    {
        pp = 0;//pointer to all pointers
        pm = 0;//pointer to send message
        pb = 0;//ponter to back message
        pn = 0;//pointer to names arrays
        pd = 0;//pointer to draw names
        string nm = "Warcraft II BNE";
        if (name != "") nm = name;
        Process[] process = Process.GetProcessesByName(nm);
        if (process.Length == 0)
            return 0;
        IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process[0].Id);
        int bytesRead = 0;
        byte[] buf = new byte[4];
        ReadProcessMemory((int)processHandle, 0x0048FFF0, buf, 4, ref bytesRead);
        pp = buf[0] + 256 * buf[1] + 256 * 256 * buf[2] + 256 * 256 * 256 * buf[3];
        if (pp != 0)
        {
            ReadProcessMemory((int)processHandle, pp, buf, 4, ref bytesRead);
            pm = buf[0] + 256 * buf[1] + 256 * 256 * buf[2] + 256 * 256 * 256 * buf[3];
            ReadProcessMemory((int)processHandle, pp + 4, buf, 4, ref bytesRead);
            pb = buf[0] + 256 * buf[1] + 256 * 256 * buf[2] + 256 * 256 * 256 * buf[3];
            ReadProcessMemory((int)processHandle, pp + 8, buf, 4, ref bytesRead);
            pn = buf[0] + 256 * buf[1] + 256 * 256 * buf[2] + 256 * 256 * 256 * buf[3];
            ReadProcessMemory((int)processHandle, pp + 12, buf, 4, ref bytesRead);
            pd = buf[0] + 256 * buf[1] + 256 * 256 * buf[2] + 256 * 256 * 256 * buf[3];
        }
        else return 2;
        CloseHandle(processHandle);
        return 1;
    }

    public static int SendMsg(string msg, string name)
    {
        int ret = 0;
        ReadPointers(name);
        string nm = "Warcraft II BNE";
        if (name != "") nm = name;
        Process[] process = Process.GetProcessesByName(nm);
        if (process.Length == 0)
            return 0;
        IntPtr processHandle = OpenProcess(0x001F0FFF, false, process[0].Id);
        if (pm != 0)
        {
            int bytesRead = 0;
            byte[] buf = new byte[1];
            ReadProcessMemory((int)processHandle, pm, buf, 1, ref bytesRead);
            if (buf[0] == 0)
            {
                byte[] buf2 = new byte[64];
                buf2[0] = 0;
                char[] buf3 = msg.ToCharArray();
                for (int i = 0; (i < 55) && (i < msg.Length); i++)
                {
                    buf2[i] = (byte)buf3[i];
                }
                WriteProcessMemory((int)processHandle, pm, buf2, 55, out bytesRead);
                ret = 1;
            }
            else
                ret = 2; 
        }
        else
            ret = 3;
        CloseHandle(processHandle);
        return ret;
    }

    public static int RecMsg(string name)
    {
        int ret = 0;
        ReadPointers(name);
        string nm = "Warcraft II BNE";
        if (name != "") nm = name;
        Process[] process = Process.GetProcessesByName(nm);
        if (process.Length == 0)
            return 0;
        IntPtr processHandle = OpenProcess(0x001F0FFF, false, process[0].Id);
        if (pb != 0)
        {
            int bytesRead = 0;
            byte[] buf = new byte[1];
            ReadProcessMemory((int)processHandle, pb, buf, 1, ref bytesRead);
            if (buf[0] != 0)
            {
                byte[] buf2 = new byte[64];
                buf2[0] = 0;
                ReadProcessMemory((int)processHandle, pb, buf2, 60, ref bytesRead);
                string s = "";
                for (int i = 0; (i < 60) && (buf2[i] != 0); i++)
                {
                    s += (char)buf2[i];
                }
                msgs.Add(s);
                ret = 1;
                buf[0] = 0;
                WriteProcessMemory((int)processHandle, pb, buf, 1, out bytesRead);
            }
            else
                ret = 2;  
        }
        else
            ret = 3;
        CloseHandle(processHandle);
        return ret;
    }

    public static int SendNames(ListBox box, string name,bool draw_names)
    {
        int ret = 0;
        ReadPointers(name);
        string nm = "Warcraft II BNE";
        if (name != "") nm = name;
        Process[] process = Process.GetProcessesByName(nm);
        if (process.Length == 0)
            return 0;
        IntPtr processHandle = OpenProcess(0x001F0FFF, false, process[0].Id);
        if ((pn != 0) && (pd != 0))
        {
            byte[] bufd = new byte[1];
            bufd[0] = draw_names ? (byte)1 : (byte)0;
            WriteProcessMemory((int)processHandle, pd, bufd, 1, out int byteswd);
            byte[] buf = new byte[1000 * 32];
            for (int i = 0; i < 1000 * 32; i++) buf[i] = 0;
            for (int k = 0; (k < box.Items.Count) && (k < 1000); k++)
            {
                string msg = (string)box.Items[k];
                char[] buf2 = msg.ToCharArray();
                for (int i = 0; (i < 32) && (i < msg.Length); i++)
                {
                    buf[k * 32 + i] = (byte)buf2[i];
                }
            }
            WriteProcessMemory((int)processHandle, pn, buf, 1000 * 32, out int bytesw);
            if (bytesw != 0) ret = 1;
        }
        else
            ret = 3;
        CloseHandle(processHandle);
        return ret;
    }
}

class Bot
{
    public TwitchClient client;
    public ChatClient yclient;

    public DateTime prog_start;
    public string text = "";
    public string ytext = "";
    public List<string> msgs = new List<string>();
    public string chan = "";
    public int con = 0;
    public int ycon = 0;

    public Bot()
    {
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        WebSocketClient customClient = new WebSocketClient(clientOptions);
        client = new TwitchClient(customClient);
    }

    List<string> Divider(string str, int blockLength)
    {
        List<string> Blocks = new List<string>(str.Length / blockLength + 1);
        for (int i = 0; i < str.Length; i += blockLength)
        {
            if (str.Length - i > blockLength)
                Blocks.Add(str.Substring(i, blockLength));
            else
                Blocks.Add(str.Substring(i, str.Length - i) + new String('\0', blockLength - (str.Length - i)));
        }
        return Blocks;
    }

    public void msg_rec(string user, string fm)
    {
        if (user.Length + 4 < 55)
        {
            if (user.Length + fm.Length + 4 > 55)
            {
                List<string> ms = Divider(fm, 55 - user.Length - 4);
                foreach (string s in ms)
                {
                    msgs.Add(user + ":" + '\x03' + " " + s);
                }
            }
            else
                msgs.Add(user + ":" + '\x03' + " " + fm);
        }
    }

    public void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)//twitch
    {
        string fm = e.ChatMessage.Message;
        string user = e.ChatMessage.Username;
        if (e.ChatMessage.IsSubscriber) user = '\x04' + user;
        else user = '\x05' + user;
        msg_rec(user, fm);
    }

    public void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)//twitch
    {
        con = 1;
    }

    public void YClient_OnMessagesReceived(object sender, IEnumerable<LiveChatMessage> messages)
    {
        foreach (LiveChatMessage message in messages)
        {
            try
            {
                if (message.Snippet.HasDisplayContent.GetValueOrDefault())
                {
                    if (message.Snippet.PublishedAt > prog_start)
                    {
                        string start = message.Snippet.DisplayMessage.Substring(0, 5);
                        if ((!(bool)message.AuthorDetails.IsChatOwner) || (start == "TEST "))
                        {
                            string fm = message.Snippet.DisplayMessage;
                            string user = message.AuthorDetails.DisplayName;
                            if ((bool)message.AuthorDetails.IsChatSponsor) user = '\x04' + user;
                            else user = '\x05' + user;
                            msg_rec(user, fm);
                        }
                    }
                }

            }
            catch (Exception){}
        }
    }
}

namespace War2Streaming
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}