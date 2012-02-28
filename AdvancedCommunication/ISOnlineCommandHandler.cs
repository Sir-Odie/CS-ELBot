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
	/// description of LOCCommandHandler.
	/// </summary>
	public class ISOnlineCommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		//private bool CommandIsDisabled;
		private HelpCommandHandler TheHelpCommandHandler;
		private Logger TheLogger;
		
		public ISOnlineCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser,HelpCommandHandler MyHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheHelpCommandHandler = MyHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheLogger = MyLogger;
			//this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#loc",Settings.botid);
			
			//if (CommandIsDisabled == false)
			{
                TheHelpCommandHandler.AddCommand("#isonline - tells you the online status of a player");
                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
			}
		}
		
		private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
		{
			string Message = e.Message;
			
			if (Message[0]!='#')
			{
				Message = "#" + Message;
			}
			
			string[] CommandArray = Message.Split(' ');

            if (CommandArray[0].ToLower() == "#isonline")
			{
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#isonline", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#isonline", Settings.botid))
			    {
			    	TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"You are not authorized to use this command!"));
			    	return;
			    }
                if (CommandArray.Length < 2)
                {
                    goto WrongArguments;
                }
                string playerName = CommandArray[1];
                bool playerIsOnline = false;
                string serverName = "live";
                if (CommandArray.Length == 3)
                {
                    serverName = CommandArray[2];
                }
                if ((playerIsOnline = TheMySqlManager.isPlayerOnline(e.username, playerName, serverName)))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, playerName + " is online on the " + serverName + " server."));
                }
                else
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, playerName + " is not online on the " + serverName + " server."));
                }
                return;

            WrongArguments:
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[-------------------------------------------"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Here is the usage of the #isonline command:"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[#isonline <playername> <server>            "));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[-------------------------------------------"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #isonline DogBreath test          "));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[-------------------------------------------"));
                return;
            }
		}
	}
}
