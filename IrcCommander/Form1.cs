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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private TwitchClient twitchclient;
        private System.Threading.Timer timer;
        private System.Threading.TimerCallback timerCallback; 
        private bool connected;
        public Form1()
        {
            InitializeComponent();
            timerCallback = new TimerCallback(timer_tick);
            timer = null;
            connected = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (connected) 
            {
                timer.Dispose();
                twitchclient.disconnect();
                connected = false;
                btnConnect.Text = "Connect";
            }
            else if (twitchclient.connect(txtServer.Text, int.Parse(txtPort.Text), txtChannel.Text, txtUser.Text, txtPass.Text))
            {
                btnConnect.Text = "Disconnect";
                connected = true;
                timer = new System.Threading.Timer(timerCallback, null, 100, 100);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            twitchclient = new TwitchClient();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(timer != null)
                timer.Dispose();
            twitchclient.disconnect();
            twitchclient.close();
        }

        private void timer_tick(object sender)
        {
            twitchclient.readComment();
        }
    }
}
