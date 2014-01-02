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
	/// description of WearCommandHandler.
	/// </summary>
	public class WearCommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		////private bool CommandIsDisabled;
		private AdminHelpCommandHandler TheAdminHelpCommandHandler;
		private Logger TheLogger;
		private TradeHandler TheTradeHandler;
		private Inventory TheInventory;
        	private AdvancedCommunication.ActorHandler TheActorHandler;
        	private string wearAction = "";
        	private string username = "";
        	int SQLID = 0;

        	public WearCommandHandler(AdvancedCommunication.ActorHandler MyActorHandler, TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler, Inventory MyInventory)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheLogger = MyLogger;
			this.TheTradeHandler = MyTradeHandler;
			this.TheInventory = MyInventory;
           		 this.TheActorHandler = MyActorHandler;
			//this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#drop",Settings.botid);
			
			//if (CommandIsDisabled == false)
			{
				TheAdminHelpCommandHandler.AddCommand("#wear - make me wear an item");
                		TheAdminHelpCommandHandler.AddCommand("#remove - make me remove an item");
                		TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
                		TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
            		}
		}
        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            if (wearAction == "")
            {
                return;
            }
            if (e.CommandBuffer[0] == 52 || e.CommandBuffer[0] == 53) // ACTOR_WEAR_ITEM||UNWEAR
            {
                Int16 actorId = System.BitConverter.ToInt16(e.CommandBuffer,3);
                //byte pos = e.CommandBuffer[6];
                if (actorId == TheActorHandler.MyActorID && TheInventory.GettingInventoryItems == false)
                {
                    TradeHandler.TradeLogItem MyTradeLogItem;
                    MyTradeLogItem.action = wearAction;
                    MyTradeLogItem.quantity = 1;
                    MyTradeLogItem.price = 0;
                    MyTradeLogItem.KnownItemsSqlID = SQLID;
                    MyTradeLogItem.categoryNum = -1;
                    this.TheMySqlManager.LogTrade(MyTradeLogItem, username, Settings.botid, true);
                    wearAction = "";
                }

            }
	    if (e.CommandBuffer[0] == 20) 
	    {
		string Text=System.Text.ASCIIEncoding.ASCII.GetString(e.CommandBuffer,4,e.CommandBuffer.Length-4).Trim();
		Text = Text.Replace((char)10, ' ');
		TheLogger.Log(Text);
		TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I can't wear that item!"));
		wearAction = "";
	    }

            if (e.CommandBuffer[0] == 21 && (wearAction != "")) // GET_INVENTORY_ITEM
            {
                TheInventory.requestInventory();
            }

        }
		private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
		{
            int nameToID = -1;
            username = e.username;

			string Message = e.Message.ToLower().Replace("\'","\\\'").Replace("\"","\\\"");
			
			if (Message[0]!='#')
			{
				Message = "#" + Message;
			}
			
			string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#wear" || CommandArray[0] == "#remove")
			{
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled(CommandArray[0], Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }
                if (CommandArray[0] == "#wear")
                {
                    if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#wear", Settings.botid))
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                        return;
                    }
                }

                if (CommandArray[0] == "#remove")
                {
                    if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#remove", Settings.botid))
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                        return;
                    }
                }
                if(TheTradeHandler.AmITrading())
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I am currently trading, please retry shortly."));
                    return;
                }

                if (TheInventory.GettingInventoryItems == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Please wait a moment for me to finish getting my inventory..."));
                    return;
                }

		try
		{
                    if (CommandArray.Length < 2)
                    {
                        goto WrongArguments;
                    }
                    string str = "";
                    int i;
                    for (i = 1; i < CommandArray.Length; i++)
                    {
                        str += CommandArray[i] + " ";
                    }
                    str = str.Trim();
		    if (str == "")
		    {
			goto WrongArguments;
		    }
                    nameToID = TheMySqlManager.GetItemID(str, Settings.botid, false);
                    if (nameToID == -1)
                    {
                        SQLID = int.Parse(CommandArray[1]);
                    }
                    else
                    {
                        SQLID = nameToID;
                    }
		}
		catch
		{
			goto WrongArguments;
		}
                if (CommandArray[0] == "#wear")
                { 
                    wearAction = "wore"; 
                }
                else
                {
                    wearAction = "removed";
                }
                if (!WearItem(SQLID, wearAction))
		{
			wearAction = "";
		}
			}
			return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Usage of the #wear/#remove command:    "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|#wear <item>                           "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|#remove <item>                         "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #wear warm fur gloves         "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #remove warm fur gloves       "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
	    wearAction = "";
            return;
		}
		
		private bool WearItem(int SQLID, string wearAction)
		{
			System.Collections.ArrayList InventorySnapshop = TheInventory.GetInventoryList();

            bool[] slotfull = new bool[44];
            byte fromSlot = 128;
            byte toSlot = 128;

            for (int i = 0; i < 44; i++)
            {
                slotfull[i] = false;
            }
            if (wearAction == "wore")
            {
                for (int i = 0; i < InventorySnapshop.Count; i++)
                {
                    Inventory.inventory_item MyInventoryItem = (Inventory.inventory_item)InventorySnapshop[i];
                    //find the item in non-worn inventory
                    //if found, find an open wear slot and try to wear it
                    //take notes of which slots are full....
                    slotfull[MyInventoryItem.pos] = true;
                    if (MyInventoryItem.SqlID == SQLID && MyInventoryItem.pos < 36)
                    {
                        //item is in inventory, try to wear it?
                        fromSlot = (byte)MyInventoryItem.pos;
                    }
                }
                //find a wear slot that's open
                for (int i = 36; i < 44; i++)
                {
                    if (slotfull[i] == false)
                    {
                        //put the item in this slot (slot i)
                        toSlot = (byte)i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < InventorySnapshop.Count; i++)
                {
                    Inventory.inventory_item MyInventoryItem = (Inventory.inventory_item)InventorySnapshop[i];
                    //find the item in the wear slots
                    //if found, find an open inventory slot and move it there
                    slotfull[MyInventoryItem.pos] = true;
                    if (MyInventoryItem.SqlID == SQLID && MyInventoryItem.pos > 35)
                    {
                        fromSlot = (byte)MyInventoryItem.pos;
                    }
                }
                //find an inventory slot that's open
                for (int i = 0; i < 36; i++)
                {
                    if (slotfull[i] == false)
                    {
                        //put the item in this slot (slot i)
                        toSlot = (byte)i;
                        break;
                    }
                }
            }
	    if(fromSlot != 128 && toSlot != 128)
	    {
            	TheTCPWrapper.Send(CommandCreator.MOVE_INVENTORY_ITEM(fromSlot, toSlot));
		return true;
	    }
	    else
	    {
		wearAction = "";
		TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Wrong item or no free slot!"));
		return false;
	    }

		}
	}
}
