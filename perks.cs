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

namespace cs_elbot
{
    /// <summary>
    /// description of Inventory.
    /// </summary>

    public class Perks
    {
        // OnGotNewStatsList
        public delegate void GotPerksEventHandler(object sender, GotPerksEventArgs e);

        public class GotPerksEventArgs : EventArgs
        {
        }

        private TCPWrapper TheTCPWrapper;
        private Logger TheLogger;
        private MySqlManager TheMySqlManager;


        public Perks(TCPWrapper MyTCPWrapper, Logger MyLogger, MySqlManager MyMySqlManager)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
            this.TheLogger = MyLogger;
            this.TheMySqlManager = MyMySqlManager;
            //TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "I'm too far away from storage to open it, move me closer please!"));
        }

        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            if (e.CommandBuffer[0] == 0x00 && e.CommandBuffer[4] == 130 && MainClass.gettingPerks)  //this means raw text
            {
                string MessageBuffer = System.Text.ASCIIEncoding.ASCII.GetString(e.CommandBuffer, 5, e.CommandBuffer.Length - 5);
                if (!MessageBuffer.Contains("You are") && !MessageBuffer.Contains("Welcome") &&!MessageBuffer.Contains("Game Time"))
                {
                    TheMySqlManager.updatePerks(MessageBuffer);
                    MainClass.gettingPerks = false;
                }
            }
        }
    }
}
