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
	public class UseItemCommandHandler
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
        private string username;
        int SQLID = 0;

        public UseItemCommandHandler(AdvancedCommunication.ActorHandler MyActorHandler, TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler, Inventory MyInventory)
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
				TheAdminHelpCommandHandler.AddCommand("#useitem - make me use an item in inventory");
                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
                TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
                TheInventory.GotNewInventoryList += new Inventory.GotNewInventoryListEventHandler(TheInventory_GotNewInventoryList);
            }
		}

        public int slotUsed = 0;
        public int qtyBefore = 0;
        public string cooldownMessage = "";
        void TheInventory_GotNewInventoryList(object sender, Inventory.GotNewInventoryListEventArgs e)
        {
            if (!usingItem)
            {
                return;
            }
            if (qtyBefore == TheInventory.Quantity(SQLID))
            {
                if (cooldownMessage != "")
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, cooldownMessage));
                }
            }
            else
            {
                TradeHandler.username = username;
                TheTradeHandler.SentThanks = false;
                TheTradeHandler.AddTrade(SQLID, 0, 1, "Item used");
                TheTradeHandler.LogTrade();
                TradeHandler.username = "";
            }
            qtyBefore = 0;
            slotUsed = 0;
            usingItem = false;
        }

        public bool usingItem = false;
        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            if (e.CommandBuffer[0] == 0x00 && usingItem)  //this means raw text
            {
                string MessageBuffer = System.Text.ASCIIEncoding.ASCII.GetString(e.CommandBuffer, 5, e.CommandBuffer.Length - 5);
                if (MessageBuffer.Contains("this item still has"))
                {
                    cooldownMessage = MessageBuffer;
                }
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

            if (CommandArray[0] == "#useitem")
			{
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled(CommandArray[0], Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (CommandArray[0] == "#useitem")
                {
                    if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#useitem", Settings.botid))
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
                    if (nameToID != -1)
                    {
                        SQLID = nameToID;
                    }
                }
                catch
				{
                    goto WrongArguments;
				}
                username = e.username;
                UseItem(SQLID);
			}
			
			return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Usage of the #useitem command:         "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|#useitem <item>                        "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #useitem ring of portland     "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            return;
		}
		
		private void UseItem(int SQLID)
		{
			System.Collections.ArrayList InventorySnapshop = TheInventory.GetInventoryList();

            for (int i = 0; i < InventorySnapshop.Count; i++)
            {
                Inventory.inventory_item MyInventoryItem = (Inventory.inventory_item)InventorySnapshop[i];
                if (MyInventoryItem.SqlID == SQLID && MyInventoryItem.pos < 36)
                {
                    slotUsed = MyInventoryItem.pos;
                    qtyBefore = (int)MyInventoryItem.quantity;
                    usingItem = true;
                    TheTCPWrapper.Send(CommandCreator.USE_ITEM(slotUsed));
                    TheInventory.requestInventory();
                    break;
                }
            }
		}
	}
}
