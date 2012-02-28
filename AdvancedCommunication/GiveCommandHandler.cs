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
	/// description of GiveCommandHandler.
	/// </summary>
	public class GiveCommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		////private bool CommandIsDisabled;
		private AdvHelpCommandHandler TheAdvHelpCommandHandler;
		private Logger TheLogger;
		private TradeHandler TheTradeHandler;
        private PMHandler ThePMHandler;
		
		public GiveCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser,AdvHelpCommandHandler MyAdvHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler, PMHandler MyPMHandler)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheAdvHelpCommandHandler = MyAdvHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheLogger = MyLogger;
			this.TheTradeHandler = MyTradeHandler;
            this.ThePMHandler = MyPMHandler;
			//this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#give",Settings.botid);
			
			//if (CommandIsDisabled == false)
			{
                TheAdvHelpCommandHandler.AddCommand("#give / #g - give you an item");
                ThePMHandler.AddCommand("#g");
                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
			}
		}
		
		private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
		{
			uint quantity = 0;
			int SQLID = 0;
            //int nameToID = -1;
            //bool fromInventory = true;
            bool fromInventory = false;
            string itemName = "";
			
			string Message = e.Message.ToLower();
			
			if (Message[0]!='#')
			{
				Message = "#" + Message;
			}
			
			string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#give" || CommandArray[0] == "#g")
			{
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#give", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#give", Settings.botid))
			    {
			    	TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"You are not authorized to use this command!"));
			    	return;
			    }

                if (TheTradeHandler.Trading == false || TradeHandler.username.ToLower() != e.username.ToLower())
				{
					TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Please trade with me first!"));
					return;
				}
                if (TradeHandler.openingStorage)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Please wait a moment for storage to finish opening!"));
                    return;
                }

                if (CommandArray.Length < 2)
                {
                    goto WrongArguments;
                }

                int i = 0;
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
                    itemName += CommandArray[i] + " ";
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
                    TheTradeHandler.Donating = true;
                    TheTradeHandler.itemTraded = true;
                    if (fromInventory == true)
                    {
                        TheTradeHandler.AddTrade(SQLID, 0, ItemsPlacedOnTrade, "gave from inv");
                    }
                    else
                    {
                        TheTradeHandler.AddTrade(SQLID, 0, ItemsPlacedOnTrade, "gave from sto");
                    }
                }
				
				if (ItemsPlacedOnTrade<quantity)
				{
					if (ItemsPlacedOnTrade==0 && !fromInventory)
					{
						TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Sorry, I don't have this item in storage!"));
					}
                    else if (ItemsPlacedOnTrade == 0)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I don't have this item in inventory!"));
                    }
                    else
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I only have " + ItemsPlacedOnTrade.ToString() + " of this item!"));
                    }
				}
			}
			
			return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Here is the usage of the #give command:"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|#give <quantity> <name or itemid>      "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #give 100 2                   "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #give 1 Pickaxe               "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #give 1 Pickaxe inv           "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            return;
		}
	}
}
