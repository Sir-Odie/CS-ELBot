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
	/// description of DropCommandHandler.
	/// </summary>
	public class DropCommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		////private bool CommandIsDisabled;
		private AdminHelpCommandHandler TheAdminHelpCommandHandler;
		private Logger TheLogger;
		private TradeHandler TheTradeHandler;
		private Inventory TheInventory;
		
		public DropCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser,AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger,TradeHandler MyTradeHandler, Inventory MyInventory)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheLogger = MyLogger;
			this.TheTradeHandler = MyTradeHandler;
			this.TheInventory = MyInventory;
			//this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#drop",Settings.botid);
			
			//if (CommandIsDisabled == false)
			{
				TheAdminHelpCommandHandler.AddCommand("#drop - make me drop a item from my inventory");
				TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
            }
		}

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
		{
			UInt32 quantity = 0;
			int SQLID = 0;
            int nameToID = -1;

			string Message = e.Message.ToLower().Replace("\'","\\\'").Replace("\"","\\\"");
			
			if (Message[0]!='#')
			{
				Message = "#" + Message;
			}
			
			string[] CommandArray = Message.Split(' ');
			
			if (CommandArray[0] == "#drop")
			{
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#drop", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#drop", Settings.botid))
			    {
			    	TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"You are not authorized to use this command!"));
			    	return;
			    }
                if(TheTradeHandler.AmITrading())
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I am currently trading, please retry shortly."));
                    return;
                }

                try
                {
                    if (CommandArray.Length < 2)
                    {
                        goto WrongArguments;
                    }
                    if (CommandArray.Length < 3)
                    {
                        quantity = 1;
                        nameToID = TheMySqlManager.GetItemID(CommandArray[1],false);
                        if (nameToID != -1)
                        {
                            SQLID = nameToID;
                        }
                        if (nameToID == -1)
                        {
                            SQLID = int.Parse(CommandArray[1]);
                        }
                    }
                    if (CommandArray.Length > 2)
                    {
                        quantity = UInt32.Parse(CommandArray[1]);
                        string str = "";
                        int i;
                        for (i = 2; i < CommandArray.Length; i++)
                        {
                            str += CommandArray[i] + " ";
                        }
                        str = str.Trim();
                        nameToID = TheMySqlManager.GetItemID(str,false);
                        if (nameToID == -1)
                        {
                            SQLID = int.Parse(CommandArray[2]);
                        }
                        if (nameToID != -1)
                        {
                            SQLID = nameToID;
                        }
                    }
                }
                catch
				{
					goto WrongArguments;
				}
					
				UInt32 ItemsDropped = DropItems(SQLID,quantity);
                TheInventory.GettingInventoryItems = true;
				
				if (ItemsDropped>0)
				{
					TradeHandler.TradeLogItem MyTradeLogItem;
					MyTradeLogItem.action = "dropped";
					MyTradeLogItem.quantity = ItemsDropped;
					MyTradeLogItem.price = 0;
					MyTradeLogItem.KnownItemsSqlID = SQLID;
                    MyTradeLogItem.categoryNum = -1;
					this.TheMySqlManager.LogTrade(MyTradeLogItem,e.username,Settings.botid, true);
				}
				
				if (ItemsDropped<quantity)
				{
					if (ItemsDropped==0)
					{
						TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Sorry, I don't have this item!"));
					}
					else
					{
						TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"I dropped only " + ItemsDropped.ToString() +" of this item!"));
					}
				}
				else
				{
					TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"I dropped " + ItemsDropped.ToString() +" of this item!"));
				}
                TheInventory.requestInventory();
			}
			
			return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Here is the usage of the #drop command:"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|#drop <quantity> <name or itemid>      "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #drop 100 Fruits              "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            return;
		}
		
		private uint DropItems(int SQLID, UInt32 quantity)
		{
			UInt32 QuantityDropItems = 0;
			System.Collections.ArrayList InventorySnapshop = TheInventory.GetInventoryList();
			
			for (int i=0;i<InventorySnapshop.Count;i++)
			{
				Inventory.inventory_item MyInventoryItem = (Inventory.inventory_item)InventorySnapshop[i];
				
				if (MyInventoryItem.SqlID==SQLID && MyInventoryItem.pos<36)
				{
					if (MyInventoryItem.quantity <= (quantity-QuantityDropItems))
					{
						TheTCPWrapper.Send(CommandCreator.DROP_ITEM(MyInventoryItem.pos,MyInventoryItem.quantity));
						QuantityDropItems += MyInventoryItem.quantity;
						MyInventoryItem.quantity = 0;
						InventorySnapshop[i] = MyInventoryItem;
					}
					else
					{
						TheTCPWrapper.Send(CommandCreator.DROP_ITEM(MyInventoryItem.pos,(quantity-QuantityDropItems)));
						QuantityDropItems += quantity;
						MyInventoryItem.quantity -= quantity;
						InventorySnapshop[i] = MyInventoryItem;
					}
					
					if (QuantityDropItems >= quantity)
					{
						return QuantityDropItems;
					}
				}
			}
			
			return QuantityDropItems;
		}
	}
}
