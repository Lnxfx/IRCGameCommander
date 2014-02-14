/*
 * Copyright (c) <2014> LNXFX
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using vJoyInterfaceWrap;

namespace WindowsFormsApplication1
{
    class TwitchClient
    {
        private Socket twitchsock;

        private byte[] channel;
        private byte[] user;
        private byte[] nick;
        private byte[] pass;
        private string[] buttons;
        private Dictionary<string, uint> dictionary;
        static private vJoy joystick;
        static private vJoy.JoystickState iReport;
        static public uint device = 1;

        public TwitchClient() 
        {
            int index = 0;
            dictionary = new Dictionary<string, uint>();
            twitchsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            buttons = new string[] {"up", "down", "left", "right", "select", "start", "a", "b", "x", "y", "l", "r", "r2", "l2", "r3", "l3", "home"};
            foreach (string item in buttons)
                dictionary.Add(item, (uint)index++);
            initJoystick();
        }

        public bool connect(string server, int port, string channel, string user, string pass)
        {
            this.pass = Encoding.Default.GetBytes("PASS " + pass + "\r\n");
            this.nick = Encoding.Default.GetBytes("NICK " + user + "\r\n");
            this.user = Encoding.Default.GetBytes("USER " + user + "\r\n");
            this.channel = Encoding.Default.GetBytes("JOIN " + channel + "\r\n");
            IPEndPoint dir = new IPEndPoint(IPAddress.Parse(server), port);
            try
            {
                twitchsock.Connect(dir);
                twitchsock.Send(this.pass, 0, this.pass.Length,0);
                twitchsock.Send(this.nick, 0, this.nick.Length, 0);
                twitchsock.Send(this.user, 0, this.user.Length, 0);
                twitchsock.Send(this.channel, 0, this.channel.Length, 0);

            }
            catch(Exception e) 
            {
                Console.WriteLine("Can't connect with irc Server: {0} ", e.ToString());
            }
            return true;
        }

        public string readComment() 
        {
            Match match = null;
            string msg;
            string com;
            byte[] buffer = new byte[2040];
            string[] command = new string[2];
            twitchsock.Receive(buffer);
            msg = Encoding.UTF8.GetString(buffer);
            if (msg.Contains("PING"))
            {
                try
                {
                    byte[] pong = Encoding.Default.GetBytes("PONG " + msg.Split(' ')[1] + "\r\n");
                    twitchsock.Send(pong, 0, pong.Length, 0);
                }
                catch (Exception e) 
                {
                    Console.WriteLine("Error PINGING server {0}", e.ToString());
                }
            }
            match = Regex.Match(msg, @":(?<name>\w+)![a-zA-Z0-9_]*@[a-zA-Z0-9_]*.[a-zA-Z0-9_]*.[a-zA-Z0-9_]* [a-zA-Z0-9_]* #[a-zA-Z0-9_]* :");
            if (match.Success)
            {
                com = msg.Split(':')[2];
                com = com.Trim().ToLower();
                foreach(string word in buttons)
                {
                    if(com.Equals(word))
                    {
                        Console.WriteLine(match.Groups["name"].Value + ": " + com);
                        for (uint i = 0; i < buttons.Length; i++)
                            joystick.SetBtn(false, device, i);
                        joystick.SetBtn(true, device, dictionary[com]);
                        return match.Groups["name"].Value + ": " + com;
                    }
                }
            }
            return "";
        }

        public void close() 
        {
            twitchsock.Close();
        }
        private void initJoystick()
        {
            joystick = new vJoy();
            iReport = new vJoy.JoystickState();
            Console.WriteLine("Detecting Joystick ...");
            if (device <= 0 || device > 16)
            {
                Console.WriteLine("Illegal device ID {0}\nExit!", device);
                return;
            }
            
            if (!joystick.vJoyEnabled())
            {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }
            else
                Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString());

            
            VjdStat status = joystick.GetVJDStatus(device);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    Console.WriteLine("vJoy Device {0} is already owned by this feeder\n", device);
                    break;
                case VjdStat.VJD_STAT_FREE:
                    Console.WriteLine("vJoy Device {0} is free\n", device);
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", device);
                    return;
                case VjdStat.VJD_STAT_MISS:
                    Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", device);
                    return;
                default:
                    Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", device);
                    return;
            };
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(device))))
            {
                Console.WriteLine("Failed to acquire vJoy device number {0}.\n", device);
                return;
            }
            else
                Console.WriteLine("Acquired: vJoy device number {0}.\n", device);

        }
    }
}
