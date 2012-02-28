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
	/// description of SAYCommandHandler.
	/// </summary>
	public class SAYCommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
        private PMHandler ThePMHandler;
		////private bool CommandIsDisabled;
		private AdminHelpCommandHandler TheAdminHelpCommandHandler;
		private Logger TheLogger;
		
		public SAYCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser,AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, PMHandler MyPMHandler)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheLogger = MyLogger;
            this.ThePMHandler = MyPMHandler;
			//this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#say",Settings.botid);
			
			//if (CommandIsDisabled == false)
			{
                TheAdminHelpCommandHandler.AddCommand("#say / #s - say something in local channel");
                //TheAdminHelpCommandHandler.AddCommand("#s - null");
                ThePMHandler.AddCommand("#s");
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

            if (CommandArray[0].ToLower() == "#say" || CommandArray[0].ToLower() == "#s")
			{
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#say", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#say", Settings.botid))
			    {
			    	TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"You are not authorized to use this command!"));
			    	return;
			    }
				    
				int i;
				Message="";
				
				if (CommandArray.Length<2)
					goto WrongArguments;
				
				for (i=1;i<CommandArray.Length;i++)
				{
                    Message += CommandArray[i];
                    if (i < CommandArray.Length - 1)
                    {
                        Message += " ";
                    }
				}
                if (Message.ToLower().Contains("#killme") ||
                    Message.ToLower().Contains("#reset") ||
                    Message.ToLower().Contains("#abuse") ||
                    Message.ToLower().Contains("#help_me") ||
                    Message.ToLower().Contains("#suicide") ||
                    Message.ToLower().Contains("#destroy_guild") ||
                    (Message.ToLower().Contains("#beam") && TheMySqlManager.GetUserRank(e.username, Settings.botid)<100) ||
                    (Message.ToLower().Contains("#change_owner") && TheMySqlManager.GetUserRank(e.username, Settings.botid) < 100))
                {
                    TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#GM " + System.Convert.ToChar(141) + "ALERT " + e.username + " tried to get me to send a prohibited command"));
                    TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#GM " + System.Convert.ToChar(136) + Message));
                }
                else
                {
                    TheTCPWrapper.Send(CommandCreator.RAW_TEXT(Message));
                }
			}
			
			return;
					
			WrongArguments:
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Here is the usage of the #say command:"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"#say <message>"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"--------------------------------------"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Example: #say Hi, how are You?"));
				return;
		}
	}
}
