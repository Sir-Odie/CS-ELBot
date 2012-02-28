// Eternal Lands Bot
// Copyright (C) 2006  Artem Makhutov
// artem@makhutov.org
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

using System;
using System.Timers;
using System.IO;

namespace cs_elbot.AdvancedCommunication
{
    /// <summary>
    /// description of RulesHandler.
    /// </summary>
    public class RulesHandler
    {
        private TCPWrapper TheTCPWrapper;
        private MySqlManager TheMySqlManager;
        private Logger TheLogger;
        private TradeHandler TheTradeHandler;
        private System.Timers.Timer RulesTimer = new System.Timers.Timer();

        private int Channel = 23456;
        public int nextline=0;
 /*
        public int nextlineset
        {
            get
            {
                return nextline;
            }
            set
            {
                nextline = value;
            }
        }
*/
        public RulesHandler(TCPWrapper MyTCPWrapper, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler)
        {
            if (Settings.botid != 1)
                return;
            this.TheLogger = MyLogger;
            this.TheMySqlManager = MyMySqlManager;
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheTradeHandler = MyTradeHandler;

            TheTCPWrapper = MyTCPWrapper;
            TheMySqlManager = MyMySqlManager;
            this.RulesTimer.Interval = 1000;

            RulesTimer.Elapsed += new ElapsedEventHandler(RulesTimer_Tick);
            TheTCPWrapper.GotConnected += new TCPWrapper.GotConnectedEventHandler(OnGotConnected);
            TheTCPWrapper.GotDisconnected += new TCPWrapper.GotDisconnectedEventHandler(OnGotDisconnected);
        }

        private void RulesTimer_Tick(object sender, ElapsedEventArgs e)
        {
            TheLogger.Debug("------Rules timer triggered\n");
            this.RulesTimer.Interval = 1000;
            int i=0,found=0;
            string line = null;
            StreamReader s = File.OpenText("../rules.txt");
            found = 0;
            while ((line = s.ReadLine()) != null)
            {
                if (i == nextline)
                {
                    found = 1;
                    nextline++;
                    TheLogger.Debug("---nextline " + nextline.ToString() + "\n");
                    break;
                }
                i++;
            }
            s.Close();
            if (found == 0)
            {
                nextline = 0;
            }
            else
            {
                TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#jc " + Channel.ToString()));
                TheTCPWrapper.Send(CommandCreator.RAW_TEXT("@@" + Channel.ToString() + line));
                TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#lc " + Channel.ToString()));
            }
        }

        private void OnGotConnected(object sender, EventArgs e)
        {
            if (0 == 0 /*|| Settings.botid != 1*/)
                return;
            //else
            //{
            //    RulesTimer.Start();
            //    TheLogger.Debug("------Rules timer activated\n");
            //}
        }

        private void OnGotDisconnected(object sender, EventArgs e)
        {
            RulesTimer.Stop();
            //RulesTimer2.Stop();
        }
    }
}
