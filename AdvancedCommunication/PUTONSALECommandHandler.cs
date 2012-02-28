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
	/// description of PUTONSALECommandHandler.
	/// </summary>
	public class PUTONSALECommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		////private bool CommandIsDisabled;
		private AdminHelpCommandHandler TheAdminHelpCommandHandler;
		private Logger TheLogger;
		private TradeHandler TheTradeHandler;
		
		public PUTONSALECommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser,AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheLogger = MyLogger;
			this.TheTradeHandler = MyTradeHandler;
			//this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#putonsale",Settings.botid);
			
			//if (CommandIsDisabled == false)
			{
                if (Settings.IsTradeBot == true)
                {
                    TheAdminHelpCommandHandler.AddCommand("#putonsale - put something on sale");
                }
				TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
			}
		}
		
		private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
		{
			string Message = e.Message.ToLower();
			
			if (Message[0]!='#')
			{
				Message = "#" + Message;
			}
			
			string[] CommandArray = Message.Split(' ');
			
			if (CommandArray[0] == "#putonsale")
			{
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#putonsale", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (Settings.IsTradeBot == false)
				{
					TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Sorry, I am not a trade bot!"));
					return;
				}
				
				if (TheMySqlManager.GetUserRank(e.username,Settings.botid)<TheMySqlManager.GetCommandRank("#putonsale",Settings.botid))
			    {
			    	TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"You are not authorized to use this command!"));
			    	return;
			    }
				
				if (TheMySqlManager.CheckIfTradeMember(e.username,Settings.botid)==false)
			    {
			    	TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"You are not a trade alliance member!"));
			    	return;
			    }

                if (TheTradeHandler.Trading == false || TradeHandler.username.ToLower() != e.username.ToLower())
				{
					TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Please trade with me first!"));
					return;
				}
				
				try
				{
					if (CommandArray.Length<1)
					goto WrongArguments;
					
					if (TheTradeHandler.Donating==false && TheTradeHandler.Billance == 0 && TheTradeHandler.PartnerTradeItemsList.Count == 0 && TheTradeHandler.MyItemList.Count==0)
					{
						TheTradeHandler.PutOnSale = true;
						TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"You can give the items to me now."));
					}
					else
					{
						TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Plese restart the trade. And put no items on the trade before sending #putonsale."));
					}
				}
				catch
				{
					goto WrongArguments;
				}
			}
			
			return;
					
			WrongArguments:
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Here is the usage of the #putonsale command:"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"#putonsale"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"------------------------------------------"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Example: #putonsale"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Example: #putonsale 44"));
				return;
		}
	}
}
