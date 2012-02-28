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
	/// description of PMCommandHandler.
	/// </summary>
	public class PMCommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private AdminHelpCommandHandler TheAdminHelpCommandHandler;
		private MySqlManager TheMySqlManager;
		//private bool CommandIsDisabled;
		
		public PMCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser,AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
            TheAdminHelpCommandHandler.AddCommand("#pm - send a pm to somebody");
            TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
            //this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#pm",Settings.botid);
			
			//if (CommandIsDisabled == false)
            //{
            //    TheAdminHelpCommandHandler.AddCommand("#pm - send a pm to somebody");
            //    TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
            //}
		}
		
		private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
		{
			string Message = e.Message;
			
			if (Message[0]!='#')
			{
				Message = "#" + Message;
			}
			
			string[] CommandArray = Message.Split(' ');					
			
			if (CommandArray[0] == "#pm")
			{
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#pm", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#pm", Settings.botid))
			    {
			    	TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"You are not authorized to use this command!"));
			    	return;
			    }
				
				int i;
				Message="";
				
				if (CommandArray.Length<3)
					goto WrongArguments;
				
				for (i=2;i<CommandArray.Length;i++)
				{
					Message += CommandArray[i] + " ";
				}
                Message = Message.Trim();
				TheTCPWrapper.Send(CommandCreator.SEND_PM(CommandArray[1],Message));
			}
			
			return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "-------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Here is the usage of the #pm command:"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "#pm <username> <message>             "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "-------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Example: #pm artem Hi, how are You?  "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "-------------------------------------"));
            return;
		}
	}
}
