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
	public class MessageLogger
	{
        private BasicCommunication.MessageParser TheMessageParser;
        private MySqlManager TheMySqlManager;
        private Logger TheLogger;

		public bool LogGotPM = true;
		public bool LogSendPM = true;
		public bool LogGlobalMessage = true;
		public bool LogGossipFeed = true;
		
		public MessageLogger(BasicCommunication.MessageParser MyMessageParser, Logger MyLogger, MySqlManager MyMySqlManager)
		{
			TheLogger = MyLogger;
			TheMessageParser = MyMessageParser;
            TheMySqlManager = MyMySqlManager;
			this.TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
			this.TheMessageParser.Send_PM += new BasicCommunication.MessageParser.Send_PM_EventHandler(OnSendPM);
			this.TheMessageParser.Get_GlobalMessage += new BasicCommunication.MessageParser.Get_GlobalMessageEventHandler(OnGlobalMessage);
		}
		
		private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
		{
			if (TheMySqlManager.loggotpms(Settings.botid)==true)
			{
				if (e.username.ToLower() == "gossip" && LogGossipFeed==false)
				{
					return;
				}
				TheLogger.PMLog("Got PM from "+ e.username + ": " + e.Message);
			}
		}
		private void OnSendPM(object sender, BasicCommunication.MessageParser.Send_PM_EventArgs e)
		{
            if (TheMySqlManager.logsendpms(Settings.botid) == true)
			{
				if (e.username.ToLower() == "gossip" && LogGossipFeed==false)
				{
					return;
				}
				TheLogger.PMLog("Send PM to "+ e.username + ": " + e.Message);
			}
		}
		private void OnGlobalMessage(object sender, BasicCommunication.MessageParser.Get_GlobalMessageEventArgs e)
		{
			if (LogGlobalMessage==true)
			{
				TheLogger.ChatLog(e.Message);
			}
		}
	}
}
