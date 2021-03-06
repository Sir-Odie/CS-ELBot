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
	/// description of WithdrawCommandHandler.
	/// </summary>
	public class WithdrawCommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		//private bool CommandIsDisabled;
		private AdvHelpCommandHandler TheAdvHelpCommandHandler;
		private Storage TheStorage;
        private TradeHandler TheTradeHandler;
        private Stats TheStats;
        private Inventory TheInventory;

        public WithdrawCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdvHelpCommandHandler MyAdvHelpCommandHandler, MySqlManager MyMySqlManager, Storage MyStorage, TradeHandler MyTradeHandler, Stats MyStats, Inventory MyInventory)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheAdvHelpCommandHandler = MyAdvHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheStorage = MyStorage;
            this.TheTradeHandler = MyTradeHandler;
            this.TheStats = MyStats;
            this.TheInventory = MyInventory;

			{
                TheAdvHelpCommandHandler.AddCommand("#withdraw - withdraw an item from storage");
                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
                this.TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
                TheMessageParser.Get_GlobalMessage += new cs_elbot.BasicCommunication.MessageParser.Get_GlobalMessageEventHandler(TheMessageParser_Get_GlobalMessage);
            }
		}

        void TheMessageParser_Get_GlobalMessage(object sender, cs_elbot.BasicCommunication.MessageParser.Get_GlobalMessageEventArgs e)
        {
            string message = e.Message;
            if (message.Contains("You do not have enough room in your inventory"))
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I do not have enough room in my inventory!"));
                TheTradeHandler.makingWithdraw = false;
                TheTradeHandler.withdrawMade = false;
            }
            if (message.Contains("You cannot withdraw/deposite from your storage while on trade!"))
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "You cannot withdraw/deposit while trading (use #give or put items in the trade window.)"));
                TheTradeHandler.makingWithdraw = false;
                TheTradeHandler.withdrawMade = false;
            }
        }
        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            if (TheTradeHandler.makingWithdraw)
            {
                if (e.CommandBuffer[0] == 68) // STORAGE_ITEMS (per category)
                {
                    STORAGE_ITEMS(e.CommandBuffer);
                }
            }
            if (TheTradeHandler.withdrawMade)
            {
                if (e.CommandBuffer[0] == 68) // STORAGE_ITEMS (per category)
                {
                    STORAGE_ITEMS(e.CommandBuffer);
                }
                if (e.CommandBuffer[0] == 21)
                {
                    GET_NEW_INVENTORY_ITEM(e.CommandBuffer);
                }

                if (e.CommandBuffer[0] == 49)
                {
                    SEND_PARTIAL_STAT(e.CommandBuffer);
                }
            }
        }
        public int totalWithdrawn;
        private void GET_NEW_INVENTORY_ITEM(byte[] data)
        {
            if (TheTradeHandler.withdrawMade)
            {
                byte pos = data[9];
                byte flags = data[10];
                uint quantity = System.BitConverter.ToUInt32(data, 5);
                int image_id = System.BitConverter.ToInt16(data, 3);
                int quantityWithdrawn = 1;
                if ((flags & 4) > 0)
                {
                    quantityWithdrawn = (int)(quantity - TheInventory.Quantity(MyStorageItem.knownItemsID));
                }
                //Console.WriteLine("quantity: " + quantity);
                //Console.WriteLine("inv quantity: " + TheInventory.Quantity(MyStorageItem.knownItemsID));
                //Console.WriteLine("quantity withdrawn: " + quantityWithdrawn);
                TheTradeHandler.AddTrade(MyStorageItem.knownItemsID, 0, (uint)(quantityWithdrawn), "Withdrew");
                MyStorageItem.quantity -= (uint)quantityWithdrawn;
                totalWithdrawn += quantityWithdrawn;
                TheTradeHandler.makingWithdraw = false;
                TheInventory.requestInventory();
            }
        }
        private void SEND_PARTIAL_STAT(byte[] data)
        {
            if (TheTradeHandler.withdrawMade == true)
            {
                TheTradeHandler.SentThanks = false;
                TradeHandler.username = username;
                TheTradeHandler.LogTrade();
                TheStorage.updateItem(MyStorageItem, quantityRemaining, true);
                TheTradeHandler.withdrawMade = false;
            }
        }
        private int quantityRemaining;
        private void STORAGE_ITEMS(byte[] data)
        {
            if (TheTradeHandler.withdrawMade)
            {
                uint item_count = data[3]; // should be in this byte but isn't yet :P
                UInt16 data_length = System.BitConverter.ToUInt16(data, 1);
                data_length += 2;
                uint category_num = data[4];
                UInt16 pos;
                int imageid;
                uint localquantity;
                //Storage.StorageItem MyStorageItem = new Storage.StorageItem();
                // so, we're calculating the number of items with the size of the packet
                item_count = (uint)(data_length - 5) / 8;
                //Console.WriteLine(data_length + "|" + item_count);
                if (item_count == 1)
                {
                    for (int i = 0; i < item_count; i++)
                    {
                        imageid = System.BitConverter.ToUInt16(data, i * 8 + 5);
                        localquantity = System.BitConverter.ToUInt32(data, i * 8 + 5 + 2);
                        pos = System.BitConverter.ToUInt16(data, i * 8 + 5 + 6);
                        quantityRemaining = (int)localquantity;
                    }
                }
            }
            else if (TheTradeHandler.makingWithdraw)
            {
                TheTCPWrapper.Send(CommandCreator.WITHDRAW_ITEM(MyStorageItem.pos, (UInt16)quantity));
                TheTradeHandler.withdrawMade = true;
            }
        }
        public Storage.StorageItem MyStorageItem = new Storage.StorageItem();
        public UInt32 quantity = 0;
        public string username = "";
        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {
            string Message = e.Message.ToLower().Replace("\'", "\\\'").Replace("\"", "\\\"");
            string[] Inv=new string[64];

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');
            if (CommandArray[0] == "#withdraw")
            {
                username = e.username;
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#withdraw", Settings.botid);


                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                int rank = TheMySqlManager.GetUserRank(e.username, Settings.botid);

                if (rank < TheMySqlManager.GetCommandRank("#withdraw", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }

                if (CommandArray.Length < 1)
                    goto WrongArguments;

                if (TheStorage.TheStorageCategories.Count == 0)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You must open storage first!"));
                    return;
                }

                if (TradeHandler.openingStorage)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Please wait a moment for storage to finish opening!"));
                    return;
                }

                if (TheInventory.GettingInventoryItems == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Please wait a moment for me to finish getting my inventory..."));
                    return;
                }

                //ok, let's do a withdraw
                totalWithdrawn = 0;
                string itemName = "";
                int SQLID = -1;
                int nameToID = -1;
                string str = "";
                try
                {
                    if (CommandArray.Length < 2)
                    {
                        goto WrongArguments;
                    }
                    if (CommandArray.Length < 3)
                    {
                        quantity = 1;
                        try
                        {
                            SQLID = int.Parse(CommandArray[1]);
                            nameToID = SQLID;
                        }
                        catch
                        {
                            itemName = CommandArray[1] + " ";
                            nameToID = TheMySqlManager.GetItemID(CommandArray[1], Settings.botid, true);
                        }
                    }
                    if (CommandArray.Length > 2)
                    {
                        int i;
                        try
                        {
                            quantity = uint.Parse(CommandArray[1]);
                            for (i = 2; i < CommandArray.Length; i++)
                            {
                                str += CommandArray[i] + " ";
                            }
                        }
                        catch
                        {
                            quantity = 1;
                            for (i = 1; i < CommandArray.Length; i++)
                            {
                                str += CommandArray[i] + " ";
                            }
                        }
                        finally
                        {
                            str = str.Trim();
                            nameToID = TheMySqlManager.GetItemID(str, Settings.botid, true);
                            itemName = str + " ";
                        }
                    }
                }
                catch
                {
                    goto WrongArguments;
                }
                finally
                {
                    try
                    {
                        SQLID = int.Parse(str);
                    }
                    catch
                    {
                        SQLID = nameToID;
                    }
                }
                //Console.WriteLine(quantity + " " + SQLID);
                //try
                //{
                //    quantity = UInt32.Parse(CommandArray[1]);
                //    try
                //    {
                //        SQLID = int.Parse(CommandArray[2]);
                //    }
                //    catch
                //    {
                //        for (int i = 2; i < CommandArray.Length; i++)
                //        {
                //            itemName += CommandArray[i] + " ";
                //        }
                //        SQLID = TheMySqlManager.GetItemID(itemName, Settings.botid, true);
                //    }
                //}
                //catch
                //{
                //    quantity = 1;
                //    try
                //    {
                //        SQLID = int.Parse(CommandArray[1]);
                //    }
                //    catch
                //    {
                //        for (int i = 1; i < CommandArray.Length; i++)
                //        {
                //            itemName += CommandArray[i] + " ";
                //        }
                //        SQLID = TheMySqlManager.GetItemID(itemName,Settings.botid, true);
                //    }
                //}
                System.Collections.ArrayList StorageSnapshot = new System.Collections.ArrayList();
                StorageSnapshot = TheStorage.GetStorageList();
                int total_items = StorageSnapshot.Count;

                //Console.WriteLine(quantity + "|" + itemName + "|" + SQLID);
                //Console.WriteLine("total items: " + total_items);
                for (int i = 0; i < total_items; i++)
                {
                    MyStorageItem = (Storage.StorageItem)StorageSnapshot[i];
                    if (MyStorageItem.knownItemsID == SQLID)
                    {
                        TheTradeHandler.makingWithdraw = true;
                        //Console.WriteLine(MyStorageItem.pos + MyStorageItem.name + MyStorageItem.knownItemsID + "|" + MyStorageItem.quantity);
                        TheTCPWrapper.Send(CommandCreator.GET_STORAGE_CATEGORY((byte)MyStorageItem.category_num));
                        break;
                    }
                }
                if (!TheTradeHandler.makingWithdraw)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I don't seem to have any " + itemName + "in storage!"));
                }
            }
            return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[-------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Here is the usage of the #withdraw command:"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[#withdraw amt item                         "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[-------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #withdraw 1 silver ore            "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[-------------------------------------------"));
            return;
		}
	}
}
