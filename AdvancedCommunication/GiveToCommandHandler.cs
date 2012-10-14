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
	/// description of GiveToCommandHandler.
	/// </summary>
	public class GiveToCommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		////private bool CommandIsDisabled;
		private AdvHelpCommandHandler TheAdvHelpCommandHandler;
		private Logger TheLogger;
		private TradeHandler TheTradeHandler;
        	private PMHandler ThePMHandler;
		
		public GiveToCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser,AdvHelpCommandHandler MyAdvHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler, PMHandler MyPMHandler)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheAdvHelpCommandHandler = MyAdvHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheLogger = MyLogger;
			this.TheTradeHandler = MyTradeHandler;
            		this.ThePMHandler = MyPMHandler;
                	TheAdvHelpCommandHandler.AddCommand("#giveto - give item to person trading with bot");
                	TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
		}
		
		private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
		{
			uint quantity = 0;
			int SQLID = 0;
            		bool fromInventory = false;
			bool hasComment = false;
			string Comment = ", ";
			
			string Message = e.Message.ToLower();
			
			if (Message[0]!='#')
			{
				Message = "#" + Message;
			}
			
			string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#giveto")
			{
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#giveto", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#giveto", Settings.botid))
			    {
			    	TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"You are not authorized to use this command!"));
			    	return;
			    }

                if (TheTradeHandler.Trading == false || TradeHandler.username.ToLower() == "")
				{
					TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Someone must be trading with me first!"));
					return;
				}

                if (CommandArray.Length < 2)
                {
                    goto WrongArguments;
                }
                int i = 0;
                string itemName = "";
                try
                {
                    //there is a quantity given
                    quantity = uint.Parse(CommandArray[1]);
                    i = 2;
                }
                catch
                {
                    //no quantity given
                    quantity = 1;
                    i = 1;
                }
                if (!TradeHandler.storageOpen)
                {
                    fromInventory = true;
                }
                for (; i < CommandArray.Length; i++)
                {
                    if (CommandArray[i] == "inv" || CommandArray[i] == "inventory")
                    {
                        fromInventory = true;
                        break;
                    }
                    if (CommandArray[i] == "#")
                    {
                        hasComment = true;
                        break;
                    }
                    itemName += CommandArray[i] + " ";
                }
		if (hasComment == true)
		{
			i++;
			for (; i < CommandArray.Length; i++)
			{
				Comment += CommandArray[i];
				if (i < CommandArray.Length - 1)
				{
					Comment += " ";
				}
			}
			if (Comment.Length > 31)
				Comment = Comment.Substring(0,31);
		}
                try
                {
                    SQLID = int.Parse(itemName);
                }
                catch
                {
                    SQLID = TheMySqlManager.GetItemID(itemName, Settings.botid, !fromInventory);
                }

                uint ItemsPlacedOnTrade = TheTradeHandler.PutItemsOnTrade(SQLID,quantity, fromInventory);
                if (ItemsPlacedOnTrade>0)
				{
                    TheTradeHandler.itemTraded = true;
                    if (fromInventory == true)
                    {
                        if (hasComment == true)
			{
				TheTradeHandler.AddTrade(SQLID, 0, ItemsPlacedOnTrade, "gave from inv" + Comment);
				hasComment = false;
				Comment = ", ";
			}
			else
			{
				TheTradeHandler.AddTrade(SQLID, 0, ItemsPlacedOnTrade, "gave from inv");
			}
                    }
                    else
                    {
                        if (hasComment == true)
			{
				TheTradeHandler.AddTrade(SQLID, 0, ItemsPlacedOnTrade, "gave from sto" + Comment);
				hasComment = false;
				Comment = ", ";
			}
			else
			{
				TheTradeHandler.AddTrade(SQLID, 0, ItemsPlacedOnTrade, "gave from sto");
			}
                    }
                }
				
				if (ItemsPlacedOnTrade<quantity)
				{
					if (ItemsPlacedOnTrade==0)
					{
						TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Sorry, I don't have this item!"));
					}
					else
					{
						TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Sorry, I only have " + ItemsPlacedOnTrade.ToString() +" of this item!"));
					}
				}
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Put " + ItemsPlacedOnTrade.ToString() + " of " + TheMySqlManager.GetKnownItemsname(SQLID) + " into the trade window(" + TradeHandler.username + ")"));
			}
			
			return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|--------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Here is the usage of the #giveto command:   "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|#giveto <quantity> <name or itemid>         "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|#giveto <quantity> <name or itemid> # Reason"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|--------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #giveto 100 2                      "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #giveto 1 Pickaxe                  "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #giveto 1 Pickaxe # as a gift      "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #giveto 1 Pickaxe inv              "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|--------------------------------------------"));
            return;
		}
	}
}
