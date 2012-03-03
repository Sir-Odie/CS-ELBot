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

namespace cs_elbot.Logging
{
	/// <summary>
	/// description of MessageLogger.
	/// </summary>
	public class LoginLogger
	{		
		private BasicCommunication.Login TheLogin;
		private Logger TheLogger;
		private TCPWrapper TheTCPWrapper;
        private MySqlManager TheMySqlManager;
		
		public LoginLogger(BasicCommunication.Login MyLogin, Logger MyLogger, TCPWrapper MyTCPWrapper, MySqlManager MyMySqlManager)
		{
			TheLogger = MyLogger;
			TheLogin = MyLogin;
			TheTCPWrapper = MyTCPWrapper;
            		TheMySqlManager = MyMySqlManager;
			this.TheLogin.Got_LOG_IN_OK += new BasicCommunication.Login.Got_LOG_IN_OK_EventHandler(LOG_IN_OK);
			this.TheLogin.Got_LOG_IN_NOT_OK += new BasicCommunication.Login.Got_LOG_IN_NOT_OK_EventHandler(LOG_IN_NOT_OK);
		}
		
		private void LOG_IN_OK(object sender, EventArgs e)
		{
			TheLogger.Log("Logged in to the account!");
			TheMySqlManager.ImLoggedIn(Settings.botid);
            string[] names = TheMySqlManager.pmmonitorlist(Settings.botid).Split('|');
            foreach (string name in names)
            {
                if (MySqlManager.playerIsOnline(name))
                {
                    //Console.WriteLine(PMArray[0] + "is not online!");
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(name.Trim(), "I'm logged in."));
                }
            }
            //MainClass.gettingPerks = true;
            //TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#list_perks"));

		}
		private void LOG_IN_NOT_OK(object sender, EventArgs e)
		{
			TheLogger.ErrorLog("Could not login! Please check the username/password!");
		}
	}
}
