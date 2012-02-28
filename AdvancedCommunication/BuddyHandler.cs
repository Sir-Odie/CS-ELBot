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

namespace cs_elbot.AdvancedCommunication
{
	/// <summary>
	/// description of BuddyHandler.
	/// </summary>
	public class BuddyHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		////private bool CommandIsDisabled;
		private AdvHelpCommandHandler TheAdvHelpCommandHandler;
		private System.Collections.Hashtable BuddyList = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
        private BasicCommunication.Login TheLogin;
		
		public struct Buddy
		{
			public string name;
			public System.DateTime LoginTime;
			public System.DateTime LogoutTime;
			public bool Online;
		}

        public BuddyHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdvHelpCommandHandler MyAdvHelpCommandHandler, MySqlManager MyMySqlManager, BasicCommunication.Login MyLogin)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheAdvHelpCommandHandler = MyAdvHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
            this.TheMessageParser.Got_LocationInfo += new cs_elbot.BasicCommunication.MessageParser.Got_LocationInfo_EventHandler(TheMessageParser_Got_LocationInfo);
            TheLogin = MyLogin;
            this.TheLogin.Got_LOG_IN_OK +=new cs_elbot.BasicCommunication.Login.Got_LOG_IN_OK_EventHandler(LOG_IN_OK);
            this.TheTCPWrapper.GotConnected += new TCPWrapper.GotConnectedEventHandler(TheTCPWrapper_GotConnected);

			
			//this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#online",Settings.botid);
			
			//if (CommandIsDisabled == false)
			{
				TheAdvHelpCommandHandler.AddCommand("#online - lists online buddies");
				TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
			}
		}


        bool firstTimeThrough = true;
        void TheTCPWrapper_GotConnected(object sender, EventArgs e)
        {
            firstTimeThrough = true;
        }
        void TheMessageParser_Got_LocationInfo(object sender, cs_elbot.BasicCommunication.MessageParser.Got_LocationInfo_EventArgs e)
        {
            //firstTimeThrough = false;
        }
		
        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            if (e.CommandBuffer[0] == 59)
            {
                string name = System.Text.ASCIIEncoding.ASCII.GetString(e.CommandBuffer, 5, e.CommandBuffer.Length - 6);
                int colorCode = e.CommandBuffer[4];
                if (colorCode == 254)
                {
                    //logged off
                    TheMySqlManager.setBuddyOffline(name);
                    if (!firstTimeThrough)
                    {
                        string tempText = "#gm " + System.Convert.ToChar(TheMySqlManager.playerloggedoffcolor(Settings.botid)) + "### " + name.ToUpper() + " LOGGED OFF ###";
                        TheTCPWrapper.Send(CommandCreator.RAW_TEXT(tempText));
                    }
                }
                else if (colorCode == 0)
                {
                    //logged on
                    TheMySqlManager.setBuddyOnline(name);
                    if (!firstTimeThrough)
                    {
                        string tempText = "#gm " + System.Convert.ToChar(TheMySqlManager.playerloggedoncolor(Settings.botid)) + "### " + name.ToUpper() + " LOGGED ON ###";
                        TheTCPWrapper.Send(CommandCreator.RAW_TEXT(tempText));
                    }
                }
                else
                {
                    name = System.Text.ASCIIEncoding.ASCII.GetString(e.CommandBuffer, 4, e.CommandBuffer.Length - 4);
                    TheMySqlManager.deleteBuddy(name);
                }
            }
        }

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {
            string Message = e.Message.ToLower().Replace("\'", "\\\'").Replace("\"", "\\\"");

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#online")
            {
                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#online", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }

                if (CommandArray.Length < 1)
                    goto WrongArguments;

                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Name".PadRight(20, ' ') + "|Lastlogin          |Lastlogout   |LoggdIn"));
                TheMySqlManager.listBuddies(e.username);


            }


            return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Here is the usage of the #online command:"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "#online"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "#online all"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Example: #online"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Example: #online all"));
            return;
        }
        private void LOG_IN_OK(object sender, EventArgs e)
        {
            firstTimeThrough = false;
        }

    }
}
