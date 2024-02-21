using System;
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
using StreamingClient.Base.Web;
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
    public static int pc = 0;
    public static int pa1 = 0;
    public static int pa2 = 0;
    public static List<string> msgs = new List<string>();
    public static byte col = 251;
    public static long a1 = 0x3FFFFFFFFFFFFFF;
    public static long a2 = 0;

    public static int ReadPointers(string name)
    {
        pp = 0;//pointer to all pointers
        pm = 0;//pointer to send message
        pb = 0;//ponter to back message
        pn = 0;//pointer to names arrays
        pd = 0;//pointer to draw names
        pc = 0;//pointer to color
        pa1 = 0;//pointer to allowed units 1
        pa2 = 0;//pointer to allowed units 2
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
            ReadProcessMemory((int)processHandle, pp + 16, buf, 4, ref bytesRead);
            pc = buf[0] + 256 * buf[1] + 256 * 256 * buf[2] + 256 * 256 * 256 * buf[3];
            ReadProcessMemory((int)processHandle, pp + 20, buf, 4, ref bytesRead);
            pa1 = buf[0] + 256 * buf[1] + 256 * 256 * buf[2] + 256 * 256 * 256 * buf[3];
            ReadProcessMemory((int)processHandle, pp + 24, buf, 4, ref bytesRead);
            pa2 = buf[0] + 256 * buf[1] + 256 * 256 * buf[2] + 256 * 256 * 256 * buf[3];
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

    public static int SendNames(ListBox box, string name, bool draw_names)
    {
        int ret = 0;
        ReadPointers(name);
        string nm = "Warcraft II BNE";
        if (name != "") nm = name;
        Process[] process = Process.GetProcessesByName(nm);
        if (process.Length == 0)
            return 0;
        IntPtr processHandle = OpenProcess(0x001F0FFF, false, process[0].Id);
        if ((pn != 0) && (pd != 0) && (pc != 0) && (pa1 != 0) && (pa2 != 0))
        {
            byte[] bufd = new byte[1];
            bufd[0] = draw_names ? (byte)1 : (byte)0;
            WriteProcessMemory((int)processHandle, pd, bufd, 1, out int byteswd);
            bufd[0] = col;
            WriteProcessMemory((int)processHandle, pc, bufd, 1, out int byteswc);

            byte[] bufa = new byte[8];
            bufa = BitConverter.GetBytes(a1);
            WriteProcessMemory((int)processHandle, pa1, bufa, 8, out int byteswa1);
            bufa = BitConverter.GetBytes(a2);
            WriteProcessMemory((int)processHandle, pa2, bufa, 8, out int byteswa2);

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

    public static List<string> unit_types;

    public static void fill_list()
    {
        unit_types = new List<string>();
        unit_types.Add("Footman");
        unit_types.Add("Grunt");
        unit_types.Add("Peasant");
        unit_types.Add("Peon");
        unit_types.Add("Ballista");
        unit_types.Add("Catapult");
        unit_types.Add("Knight");
        unit_types.Add("Ogre");
        unit_types.Add("Archer");
        unit_types.Add("Troll");
        unit_types.Add("Mage");
        unit_types.Add("Death knight");
        unit_types.Add("Paladin");
        unit_types.Add("Ogre-mage");
        unit_types.Add("Dwarwes");
        unit_types.Add("Goblins");
        unit_types.Add("Attack Peasant");
        unit_types.Add("Attack Peon");
        unit_types.Add("Ranger");
        unit_types.Add("Berserk");
        unit_types.Add("Alleria");
        unit_types.Add("Teron");
        unit_types.Add("Kurdran");
        unit_types.Add("Dentarg");
        unit_types.Add("Hadgar");
        unit_types.Add("Grom");
        unit_types.Add("Human tanker");
        unit_types.Add("Orc tanker");
        unit_types.Add("Human transport");
        unit_types.Add("Orc transport");
        unit_types.Add("Elf destroyer");
        unit_types.Add("Troll destroyer");
        unit_types.Add("Battleship");
        unit_types.Add("Juggernaut");
        unit_types.Add("TYPE_A100");
        unit_types.Add("Deathwing");
        unit_types.Add("TYPE_B36");
        unit_types.Add("TYPE_B37");
        unit_types.Add("Submarine");
        unit_types.Add("Turtle");
        unit_types.Add("Helicopter");
        unit_types.Add("Zeppelin");
        unit_types.Add("Grifon");
        unit_types.Add("Dragon");
        unit_types.Add("Tyralyon");
        unit_types.Add("Eye");
        unit_types.Add("Danath");
        unit_types.Add("Kargath");
        unit_types.Add("TYPE_A5");
        unit_types.Add("Chogal");
        unit_types.Add("Lothar");
        unit_types.Add("Guldan");
        unit_types.Add("Uter");
        unit_types.Add("Zuljin");
        unit_types.Add("TYPE_A600");
        unit_types.Add("Skeleton");
        unit_types.Add("Demon");
        unit_types.Add("Critter");
        unit_types.Add("Farm");
        unit_types.Add("Pig farm");
        unit_types.Add("Human barrack");
        unit_types.Add("Orc barrack");
        unit_types.Add("Church");
        unit_types.Add("Altar");
        unit_types.Add("Human tower");
        unit_types.Add("Orc tower");
        unit_types.Add("Stables");
        unit_types.Add("Ogremound");
        unit_types.Add("Inventor");
        unit_types.Add("Alchemist");
        unit_types.Add("Aviary");
        unit_types.Add("Dragonroost");
        unit_types.Add("Shipyard");
        unit_types.Add("Wharf");
        unit_types.Add("Town Hall");
        unit_types.Add("Great Hall");
        unit_types.Add("Elf lumbermill");
        unit_types.Add("Troll lumbermill");
        unit_types.Add("Human foundry");
        unit_types.Add("Orc foundry");
        unit_types.Add("Mage tower");
        unit_types.Add("Temple of damned");
        unit_types.Add("Human smith");
        unit_types.Add("Orc smith");
        unit_types.Add("Human refinery");
        unit_types.Add("Orc refinery");
        unit_types.Add("Human oil platform");
        unit_types.Add("Orc oil platform");
        unit_types.Add("Keep");
        unit_types.Add("Stronghold");
        unit_types.Add("Castle");
        unit_types.Add("Fortress");
        unit_types.Add("Goldmine");
        unit_types.Add("Oil");
        unit_types.Add("Hstart");
        unit_types.Add("Ostart");
        unit_types.Add("Human arrow tower");
        unit_types.Add("Orc arrow tower");
        unit_types.Add("Human canon tower");
        unit_types.Add("Orc canon tower");
        unit_types.Add("Circle");
        unit_types.Add("Portal");
        unit_types.Add("Runestone");
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

        if (user == "restreambot")
        {
            if (fm.Contains("[YouTube:"))
            {
                fm = fm.Remove(0, 10);
                int fi = fm.IndexOf(']');
                user = fm.Substring(0, fi);
                fm = fm.Remove(0, fi + 2);
                user = '\x05' + user;
                msg_rec(user, fm);
            }
        }
        else
        {
            if (e.ChatMessage.IsSubscriber) user = '\x04' + user;
            else user = '\x05' + user;
            msg_rec(user, fm);
        }
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
            MemoryRead.fill_list();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
