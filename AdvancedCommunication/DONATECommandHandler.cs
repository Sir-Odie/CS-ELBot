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
	/// description of DONATECommandHandler.
	/// </summary>
	public class DONATECommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		//private bool CommandIsDisabled;
		private AdvHelpCommandHandler TheAdvHelpCommandHandler;
		private Logger TheLogger;
		private TradeHandler TheTradeHandler;
		
		public DONATECommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser,AdvHelpCommandHandler MyAdvHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheAdvHelpCommandHandler = MyAdvHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheLogger = MyLogger;
			this.TheTradeHandler = MyTradeHandler;
			//this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#donate",Settings.botid);
			
			//if (CommandIsDisabled == false)
			{
                TheAdvHelpCommandHandler.AddCommand("#donate - donate something to me");
                TheAdvHelpCommandHandler.AddCommand("#donation - null");
                TheAdvHelpCommandHandler.AddCommand("#d - null");
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

            if (CommandArray[0] == "#donate" || (CommandArray[0] == "#donation" || (CommandArray[0] == "#d" && CommandArray.Length==1))
			{
                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#donate", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }

                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#donate", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (Settings.IsTradeBot == false && TheTradeHandler.storageAvailable == false)
				{
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I am not a trade bot!"));
					return;
				}
				
				if (TheMySqlManager.GetUserRank(e.username,Settings.botid)<TheMySqlManager.GetCommandRank("#donate",Settings.botid))
			    {
			    	TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"You are not authorized to use this command!"));
			    	return;
			    }
				
				if (TheTradeHandler.Trading == false || TradeHandler.username.ToLower()!=e.username.ToLower())
				{
					TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Please trade with me first!"));
					return;
				}
				
                if (TheTradeHandler.storageAvailable == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Items will be put in my inventory."));
                    TheTradeHandler.storageAvailable = false;
                }

                try
				{
					if (CommandArray.Length!=1)
					goto WrongArguments;
					
					if (TheTradeHandler.PutOnSale==false && TheTradeHandler.Billance == 0 && TheTradeHandler.PartnerTradeItemsList.Count == 0 && TheTradeHandler.MyItemList.Count==0)
					{
						TheTradeHandler.Donating = true;
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You can donate to me now."));
//                        if (Settings.IsTradeBot == false)
//                        {
//                            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Move me away from storage if you want to donate into my inventory..."));
//                        }
					}
					else
					{
						TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Plese restart the trade. And put no items on the trade before sending #donate."));
					}
				}
				catch
				{
					goto WrongArguments;
				}
			}
			
			return;
					
			WrongArguments:
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Here is the usage of the #donate command:"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"#donate"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"------------------------------------------"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Example: #donate"));
				return;
		}
	}
}
