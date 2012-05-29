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

namespace cs_elbot.AdvancedCommunication
{
    /// <summary>
    /// description of HouseKeepingHandler.
    /// </summary>
    public class HouseKeepingHandler
    {
        private TCPWrapper TheTCPWrapper;
        private MySqlManager TheMySqlManager;
        private Logger TheLogger;
        private errorHandler TheErrorHandler;
        private System.Timers.Timer HouseKeepingTimer = new System.Timers.Timer();

        public HouseKeepingHandler(TCPWrapper MyTCPWrapper, MySqlManager MyMySqlManager, Logger MyLogger, errorHandler MyErrorHandler)
        {
            this.TheLogger = MyLogger;
            this.TheMySqlManager = MyMySqlManager;
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheErrorHandler = MyErrorHandler;

            TheTCPWrapper = MyTCPWrapper;
            TheMySqlManager = MyMySqlManager;
            TheLogger.Debug("New HouseKeepingTimer.Interval: " + this.HouseKeepingTimer.Interval + "\n");

            HouseKeepingTimer.Elapsed += new ElapsedEventHandler(HouseKeepingTimer_Tick);
            TheTCPWrapper.GotConnected += new TCPWrapper.GotConnectedEventHandler(OnGotConnected);
            TheTCPWrapper.GotDisconnected += new TCPWrapper.GotDisconnectedEventHandler(OnGotDisconnected);
        }

        private void HouseKeepingTimer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                GC.Collect();
                if (!MainClass.launchedByBotManager)
                {
                    TheMySqlManager.KillSleepingMySQLConnections(3000);
                }
                this.HouseKeepingTimer.Interval = MainClass.ONE_MINUTE * 3;
                TheLogger.Debug("New HouseKeepingTimer.Interval: " + this.HouseKeepingTimer.Interval + "\n");
            }
            catch (Exception exception)
            {
                TheErrorHandler.writeErrorMessage("Exception raised in the houskeeping timer..." + exception);
            }
            finally
            { }
        }

        private void OnGotConnected(object sender, EventArgs e)
        {
            HouseKeepingTimer.Start();
        }

        private void OnGotDisconnected(object sender, EventArgs e)
        {
            HouseKeepingTimer.Stop();
        }
    }
}
