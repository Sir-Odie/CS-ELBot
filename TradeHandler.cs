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
using System.Collections;

namespace cs_elbot
{
	/// <summary>
	/// description of TradeHandler.
	/// </summary>
	public class TradeHandler
	{
        private AdvancedCommunication.HelpCommandHandler TheHelpCommandHandler;
        private errorHandler TheErrorHandler;
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private Logger TheLogger;
		private MySqlManager TheMySqlManager;
		private Inventory TheInventory;
        private AdvancedCommunication.ActorHandler TheActorHandler;
        private Storage TheStorage;

		private int TradeTimeElapsed = 0;
		public System.Timers.Timer TradeTimer = new System.Timers.Timer();
		
		public int AcceptStateMe = 0;
		public int AcceptStatePartner = 0;
		
		private int MyMoneySlotID = 0;
		//private byte MyMoneyOnTradeSlotID = 0;
		private uint MyMoneyAmount = 0;
		private uint MyMoneyOnTrade = 0;
		
		private System.Collections.ArrayList InventorySnapshop = new System.Collections.ArrayList();
		private System.Collections.ArrayList InventorySnapshopLoging = new System.Collections.ArrayList();

        private System.Collections.ArrayList StorageSnapshot = new System.Collections.ArrayList();
        
        //private System.Collections.Hashtable MyTradeItemsList = new System.Collections.Hashtable();
		
		public System.Collections.Hashtable PartnerTradeItemsList = new System.Collections.Hashtable();
        //going to need this, btu it needs to be the total trade log, not juse my items
        //public System.Collections.ArrayList TradeLogItemList = new System.Collections.ArrayList();
        public System.Collections.ArrayList MyItemList = new System.Collections.ArrayList();


        public bool storageAvailable = false;
        public double Billance = 0;
		
		public static string username = "";
		public bool Trading = false;
        public bool SentThanks = false;
		public bool Donating = false;
        public bool Gambling = false;
        public bool claimingPrize = false;
        public bool Giving = false;
		public bool PutOnSale = false;

        //variables for new total command
        public bool totalCalculated = false;
        public double totalOwed = 0;
        public bool invalidItem = false;
        public int idleTradeTime = 0;
        public bool idleWarningSent = false;




        public struct TradeLogItem
		{
			public double price;
			public uint quantity;
			public int KnownItemsSqlID;
			public string action;
            public int categoryNum;
		}
		
		public struct SellingItem
		{
			public int PricelistSqlID;
			public int SellingItemsSqlID;
			public int KnownItemsSqlID;
			public bool announce;
			public double pricesale;
			public double pricesalemembers;
		}

		
		public struct WantedItem
		{
			public int PricelistSqlID;
			public int SellingItemsSqlID;
			public int KnownItemsSqlID;
			public bool announce;
            public double pricepurchase;
            public double pricepurchasemembers;
            public int maxquantity;
            public string name;
            public int weight;
        }
		
		public struct TradeItem
		{
			public byte pos;
			public int imageid;
			public uint quantity;
			public string name;
			public int KnownItemsSqlID;
            public double pricepurchase;
            public double pricepurchasemembers;
            public int weight;
            public int maxQuantity;
            public bool validTradeItem;
            public int ELServerItemID;
        }

		
		public TradeHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, MySqlManager MyMySqlManager, Inventory MyInventory, AdvancedCommunication.ActorHandler MyActorHandler, Logger MyLogger, Storage MyStorage, errorHandler MyErrorHandler, AdvancedCommunication.HelpCommandHandler MyHelpCommandHandler)
		{
            this.TheErrorHandler = MyErrorHandler;
            this.TheHelpCommandHandler = MyHelpCommandHandler;
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheMySqlManager = MyMySqlManager;
			this.TheInventory = MyInventory;
			this.TheActorHandler = MyActorHandler;
			this.TheLogger = MyLogger;
            this.TheStorage = MyStorage;
            //this.TheErrorHandler = MyErrorHandler;

			
			TradeTimer.Stop();
			TradeTimer.Interval=1000;
			TradeTimer.Elapsed += new System.Timers.ElapsedEventHandler(TradeTimer_Tick );
			
			this.TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
			this.TheMessageParser.Got_TradeRequest += new BasicCommunication.MessageParser.Got_TradeRequest_EventHandler(OnGotTradeRequest);
			this.TheMessageParser.Got_AbortTrade += new BasicCommunication.MessageParser.Got_AbortTrade_EventHandler(OnGotAbortTrade);
			this.TheInventory.GotNewInventoryList += new Inventory.GotNewInventoryListEventHandler(OnGotNewInventoryList);
            if (Settings.IsTradeBot == true)
            {
                MyHelpCommandHandler.AddCommand("#total - get a detailed total of your transaction");
            }

            TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);

		}

        public bool AmITrading()
        {
            return Trading;
        }

		private void OnGotNewInventoryList(object sender, Inventory.GotNewInventoryListEventArgs e)
		{
            if (Trading == true)
            {
                Console.WriteLine("Getting inventory list while trading!!!");
                return;
            }
            bool Member = (TheMySqlManager.CheckIfTradeMember(TradeHandler.username, Settings.botid) == true);
            //Log items got
			foreach (TradeItem MyTradeItem in PartnerTradeItemsList.Values)
			{
				if (MyTradeItem.name!="Gold Coins")
				{
					string action = "";
					//if (this.Donating==true)
					{
                        if (storageAvailable)
                        {
                            action = "received from (to sto)";
                        }
                        else
                        {
                            action = "received from (to inv)";
                        }
					}
					if (this.PutOnSale==true)
					{
						action = "putonsale";
					}
					if (this.Donating == false && this.PutOnSale==false && !storageAvailable)
					{
						action = "bought from";
					}
                    if (((!Member && MyTradeItem.pricepurchase == 0) || (!Member && MyTradeItem.pricepurchase == 0)) && action == "bought from")
					{
						action = "received from (to inv)";
					}
                    AddTrade(MyTradeItem.KnownItemsSqlID, (Member ? MyTradeItem.pricepurchasemembers : MyTradeItem.pricepurchase), MyTradeItem.quantity, action);
                }
				else
				{
					//Check if money was donated
                    string action = "";
                    if (this.Donating == true)
                    {
                        if (storageAvailable)
                        {
                            action = "received from (to sto)";
                        }
                        else
                        {
                            action = "received from (to inv)";
                        }
                    }
                    else
                    {
                        action = "transaction coins from";
                    }
                    AddTrade(MyTradeItem.KnownItemsSqlID, (Member ? MyTradeItem.pricepurchasemembers : MyTradeItem.pricepurchase), (uint)(MyTradeItem.quantity - System.Convert.ToInt32(System.Math.Ceiling(this.Billance))), action);
                }
			}

            //can't we just check the tradeloglist, if we have a trade to log, we log it?
            //Check this later!!!
            if (this.MyItemList.Count > 0)
            {
                LogTrade();
            }

            // Check if there was a change in the inventory list
            //if (e.TheInventory.Count!=InventorySnapshopLoging.Count && InventorySnapshopLoging.Count!=0)
            //{
            //    LogTrade();
            //    return;
            //}
			
            //Inventory.inventory_item[] MyInventoryItemsArrayA = (Inventory.inventory_item[])InventorySnapshopLoging.ToArray(typeof(Inventory.inventory_item));
            //Inventory.inventory_item[] MyInventoryItemsArrayB = (Inventory.inventory_item[])e.TheInventory.ToArray(typeof(Inventory.inventory_item));
			
            //for (int i=0;i<MyInventoryItemsArrayA.Length;i++)
            //{
            //    if ((MyInventoryItemsArrayA[i].name != MyInventoryItemsArrayB[i].name) || (MyInventoryItemsArrayA[i].quantity!=MyInventoryItemsArrayB[i].quantity))
            //    {
            //        LogTrade();
            //        return;
            //    }
            //}
            Trading = false;
            itemTraded = false;
            return;
		}
		
		public void LogTrade()
		{
            if (SentThanks == false)
            {
                // Log the trade
                // get the flag to say if coins should be displayed to the owner...
                bool sendOwnerCoinPMs = this.TheMySqlManager.sendCoinPMs();
                foreach (TradeLogItem MyTradeLogItem in MyItemList)
                {
                    if (MyTradeLogItem.action.ToLower().Contains("transaction coins"))
                    {
                        continue;
                    }
                    this.TheMySqlManager.LogTrade(MyTradeLogItem, TradeHandler.username, Settings.botid, sendOwnerCoinPMs);

                    if (Settings.IsTradeBot == false && !MyTradeLogItem.action.Contains("inv") && Donating == false && !withdrawMade && !depositMade)
                    {
                        TheMySqlManager.UpdateStorageItem(MyTradeLogItem, false);
                    }
                }
                foreach (TradeLogItem MyTradeLogItem in MyItemList)
                {
                    if (!MyTradeLogItem.action.ToLower().Contains("transaction coins"))
                    {
                        continue;
                    }
                    this.TheMySqlManager.LogTrade(MyTradeLogItem, TradeHandler.username, Settings.botid, sendOwnerCoinPMs);

                    if (Settings.IsTradeBot == false && !MyTradeLogItem.action.Contains("inv") && Donating == false && !withdrawMade && !depositMade)
                    {
                        TheMySqlManager.UpdateStorageItem(MyTradeLogItem, false);
                    }
                }

                this.PartnerTradeItemsList = new System.Collections.Hashtable();
                //Check this later
                this.MyItemList = new System.Collections.ArrayList();

                this.InventorySnapshop = TheInventory.GetInventoryList();
                this.InventorySnapshopLoging = (System.Collections.ArrayList)InventorySnapshop.Clone();

                this.Donating = false;
                this.PutOnSale = false;
                //this.storageAvailable = false;
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "Thank you for using my services!"));
                TheLogger.Log("Trade was successful!");
                SentThanks = true;
                openingStorage = false;
                TradeHandler.username = "";
//              TheTCPWrapper.Send(CommandCreator.SIT_DOWN(false));
            }
            this.storageAvailable = false;
        }
		
		public void AddTrade(int KnownItemsSqlID, double price, uint quantity, string action)
		{
            TradeLogItem MyTradeLogItem;
			MyTradeLogItem.action = action;
			MyTradeLogItem.quantity = quantity;
			MyTradeLogItem.price = price;
			MyTradeLogItem.KnownItemsSqlID = KnownItemsSqlID;
            MyTradeLogItem.categoryNum = -1;
            int count = 0;
            bool found = false;
            for (count = 0; count < MyItemList.Count; count++)
            {
                TradeLogItem tempItem = (TradeLogItem)MyItemList[count];
                if (tempItem.KnownItemsSqlID == MyTradeLogItem.KnownItemsSqlID && tempItem.action == MyTradeLogItem.action)
                {
                    tempItem.quantity += quantity;
                    MyItemList[count] = tempItem;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                MyItemList.Add(MyTradeLogItem);
            }
            TheLogger.Debug("AddTrade " + KnownItemsSqlID.ToString() + "\n");
		}

        private void OnGotTradeRequest(object sender, BasicCommunication.MessageParser.Got_TradeRequest_EventArgs e, bool junk)
        {
            string TradePartnerUsername = e.username;
            Int16 TradePartnerUserID = TheActorHandler.GetUserIDFromname(TradePartnerUsername);
            TheTCPWrapper.Send(CommandCreator.TRADE_WITH(TradePartnerUserID));
        }
        
        private void OnGotTradeRequest(object sender, BasicCommunication.MessageParser.Got_TradeRequest_EventArgs e)
        {
            string TempPartnerUsername = e.username;
            Int16 TempPartnerUserID = TheActorHandler.GetUserIDFromname(TempPartnerUsername);
            if (Trading == true)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TempPartnerUsername, "Sorry, I am busy. Please try the #retrade in a few seconds!"));
                return;
            }
            totalCalculated = false;
            string TradePartnerUsername = e.username;
            Int16 TradePartnerUserID = TheActorHandler.GetUserIDFromname(TradePartnerUsername);
            int playerRank = TheMySqlManager.GetUserRank(e.username, Settings.botid);
            int buyCommandRank = TheMySqlManager.GetCommandRank("#buy", Settings.botid);
            int guildRank = TheMySqlManager.CheckIfBannedGuild(e.username, Settings.botid);

            if ( playerRank < buyCommandRank )
            {
                //TheTCPWrapper.Send(CommandCreator.TRADE_WITH(TradePartnerUserID));
                TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
                if (playerRank < 0)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "I don't trade with players that are banned from using my services"));
                }
                else
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, you are not authorized to trade with me!"));
                }
                return;
            }


            if (guildRank < 0 && ((playerRank <= buyCommandRank)||playerRank == 0))
            {
                //TheTCPWrapper.Send(CommandCreator.TRADE_WITH(TradePartnerUserID));
                TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
                if (guildRank < 0)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "I don't trade with guilds that are banned from using my services"));
                }
                return;
            }

            if (MainClass.botType == 1 && !MainClass.atHome)
            {
                string owners = TheMySqlManager.botowner(Settings.botid).ToString();
                if (!owners.ToLower().Contains(TradePartnerUsername.ToLower()))
                {
                    //TheTCPWrapper.Send(CommandCreator.TRADE_WITH(TradePartnerUserID));
                    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "I may only trade with my owners at storage.  Please try me later, after I return home."));
                    return;
                }
            }
			
			// Check if I am not busy (e.g. getting inventory item names)
			if (TheInventory.GettingInventoryItems==true || TheInventory.inventoryRequested)
			{
                //TheTCPWrapper.Send(CommandCreator.TRADE_WITH(TradePartnerUserID));
                //TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
				TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername,"Sorry, I am busy. Please try #retrade in a few seconds!"));
                return;
			}

            // Check if I am a trade bot or a storage bot
            //ok, if this person isn't in range at all, we don't wanna try to trade with them
            if (TheActorHandler.ActorsHashTable.Contains(TheActorHandler.GetUserIDFromname(TradePartnerUsername)) == false)
            {
                TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "Trying to trade out of range is a no no! please don't do it :P"));
                return;
            }
            if (MainClass.botType == 3)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "Hello " + TradePartnerUsername + ", I am a guard bot"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "To give me an item, just drop it in the trade window"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "To get an item, use the give command eg give 1 pickaxe"));
            }
            else if (Settings.IsTradeBot == false)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "Hello " + TradePartnerUsername + ", I am a storage bot"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "To give me an item, just drop it in the trade window"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "To get an item, use the give command eg give 1 pickaxe"));
            }
            else
            {

                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "Hello " + TradePartnerUsername + ", I am a trade bot"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "To sell me an item, just drop it in the trade window"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "To buy an item, use the BUY command eg buy 1 pickaxe"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "At any time during the trade you may click accept or use the #total command to get the trade details."));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradePartnerUsername, "I won't put coins in the trade window until you click accept once."));
            }
            // Reset all variables for fresh trade
            idleWarningSent = false;
            idleTradeTime = 0;
            totalCalculated = false;
            totalOwed = 0;
            invalidItem = false;
            TradeHandler.username = TradePartnerUsername;
			this.MyItemList.Clear();
            this.PartnerTradeItemsList.Clear();
			this.AcceptStateMe = 0;
			this.AcceptStatePartner = 0;
			this.Billance = 0;
            SentThanks = false;
			this.InventorySnapshop = TheInventory.GetInventoryList();
//            this.StorageSnapshot = TheStorage.GetStorageList();
			this.InventorySnapshopLoging = (System.Collections.ArrayList)InventorySnapshop.Clone();
			
			this.MyMoneyAmount = TheInventory.GetMoneyAmount();
			this.MyMoneySlotID = TheInventory.GetMoneySlotID();
            this.MyMoneyOnTrade = 0;
			//this.MyMoneyOnTradeSlotID = 0;
			
			//this.Trading = false;
			//this.Donating = false;
            if (Settings.IsTradeBot == false && MainClass.botType == 2)
            {
                this.Donating = true;
            }
            this.PutOnSale = false;
            //TheTCPWrapper.Send(CommandCreator.SIT_DOWN(true));
			// Start the counter for Trade-Timeout
			if (Settings.TradeTimeOut!=0)
			{
				TradeTimeElapsed = 0;
				TradeTimer.Start();
			}
			
			//Reset the TradeItemsLists
            MyItemList = new ArrayList();
			//MyTradeItemsList = new System.Collections.Hashtable();
			PartnerTradeItemsList = new System.Collections.Hashtable();
			
			// Accept the trade
			TheTCPWrapper.Send(CommandCreator.TRADE_WITH(TradePartnerUserID));
            Trading = true;

        }
        public bool itemTraded = false;
        public void stopTimer()
        {
            TradeTimer.Stop();
            TradeTimeElapsed = 0;
        }
		private void OnGotAbortTrade(object sender, System.EventArgs e)
		{
            totalOwed = 0;
            totalCalculated = false;
            TradeTimer.Stop();
            TradeTimeElapsed = 0;
            if (TheInventory.GettingInventoryItems == false && itemTraded)
            {
                username = "";
                this.PartnerTradeItemsList.Clear();
                this.MyItemList.Clear();
                //TheInventory.GettingInventoryItems = true;
                itemTraded = false;
                TheInventory.inventoryRequested = false;
                TheInventory.requestInventory();
            }
            else if (TheInventory.GettingInventoryItems == false && itemTraded == false)
            {
                TheInventory.inventoryRequested = false;
            }
            itemTraded = false;
            SentThanks = true;
            Trading = false;
            Donating = false;
            //storageAvailable = false;
            storageOpen = false;
            openingStorage = false;
            //let's try this, see if it fixes it...
            username = "";
        }
		
		private void TradeTimer_Tick(object sender,System.Timers.ElapsedEventArgs eArgs)
		{
            try
            {
                TradeTimeElapsed++;
                idleTradeTime++;

                // Check if we have reached the Trade-Timeout
                if (TradeTimeElapsed > Settings.TradeTimeOut && Settings.TradeTimeOut != 0)
                {
                    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Sorry, your trade time is over. You had " + Settings.TradeTimeOut.ToString() + " seconds to do the trade. Please try again!"));
                    TradeTimer.Stop();
                    TradeTimeElapsed = 0;
                    Trading = false;
                    username = "";
                    SentThanks = true;
                }
                if (idleTradeTime > 15 && !idleWarningSent)
                {
                    idleWarningSent = true;
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "============= Reminder (from 15 seconds idle time) =============="));
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "At any time during the trade you may click accept or use the #total command to get the trade details."));
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I won't put coins in the trade window until you click accept once."));
                }
            }
            catch (Exception exception)
            {
                TheErrorHandler.writeErrorMessage("Exception raised in the trade handler timer..." + exception);
            }
            finally
            { }
		}
		
		private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
		{
            if (Gambling || claimingPrize)
            {
                return;
            }
            if (e.CommandBuffer[0] == 35) // GET_TRADE_OBJECT
			{
                GET_TRADE_OBJECT(e.CommandBuffer);
				return;
			}
			if (e.CommandBuffer[0] == 36)
			{
				GET_TRADE_ACCEPT(e.CommandBuffer);
				return;
			}
			if (e.CommandBuffer[0] == 37)
			{
				GET_TRADE_REJECT(e.CommandBuffer);
				return;
			}
            if (e.CommandBuffer[0] == 38)
            {
                GET_TRADE_ABORT(e.CommandBuffer);
                return;
            }
            if (e.CommandBuffer[0] == 39)
			{
				REMOVE_TRADE_OBJECT(e.CommandBuffer);
				return;
			}
			if (e.CommandBuffer[0] == 20)
			{
				INVENTORY_ITEM_TEXT(e.CommandBuffer);
				return;
			}
            // 40 = GET_YOUR_TRADE_OBJECTS, sent whenever a trade is initiated
            if (e.CommandBuffer[0] == 40) //GET_YOUR_TRADE_OBJECTS
            {
                GET_YOUR_TRADE_OBJECTS(e.CommandBuffer);
                return;
            }
            if (e.CommandBuffer[0] == 41) //GET_TRADE_PARTNER_name
            {
                int i,storagetrade;
                i = System.BitConverter.ToInt16(e.CommandBuffer, 1) - 2;
                storagetrade = System.BitConverter.ToInt16(e.CommandBuffer, 3);
                string name = System.Text.Encoding.ASCII.GetString(e.CommandBuffer, 4, i);
                return;
            }
            if (e.CommandBuffer[0] == 67) // STORAGE_LIST
            {
                STORAGE_LIST(e.CommandBuffer);
                return;
            }
            
            if (e.CommandBuffer[0] == 68) // STORAGE_ITEMS (per category)
            {
                STORAGE_ITEMS(e.CommandBuffer);
                return;
            }
        
            if (e.CommandBuffer[0] == 69) // STORAGE_TEXT
            {
                STORAGE_TEXT(e.CommandBuffer);
                return;
            }
        }

        private void GET_YOUR_TRADE_OBJECTS(byte[] data)
        {
            int itemCount = data[3];
            return;
        }

        private void STORAGE_TEXT(byte[] data)
        {
            if (TheInventory.GettingInventoryItems == true || withdrawMade || depositMade || storageOpen)
                return;

            string TempItemDescription;

            TempItemDescription = System.Text.ASCIIEncoding.ASCII.GetString(data, 4, data.Length - 4).Trim();
            TheStorage.UpdateText(TempItemDescription);
            //if (!openingStorage && storageOpen)
            if (!openingStorage)
            {
                this.StorageSnapshot = TheStorage.GetStorageList();
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Storage should be open now!"));
                storageOpen = true;
            }
            return;
        }
        public bool makingWithdraw = false;
        public bool makingDeposit = false;
        public bool withdrawMade = false;
        public bool depositMade = false;
        public static bool storageOpen = false;
        public static bool openingStorage = false;
        private void STORAGE_ITEMS(byte[] data)
        {
            if (TheInventory.GettingInventoryItems == true || makingWithdraw || withdrawMade || makingDeposit || depositMade || storageOpen)
            {
                if (storageOpen && (!depositMade && !withdrawMade && !makingDeposit && !makingWithdraw))
                {
                    storageOpen = false;
                }
                return;
            }
            uint item_count = data[3]; // should be in this byte but isn't yet :P
            UInt16 data_length = System.BitConverter.ToUInt16(data,1);
            data_length += 2;
            uint category_num = data[4];
            UInt16 pos;
            int imageid;
            uint quantity;
            Storage.StorageItem MyStorageItem = new Storage.StorageItem();
            // so, we're calculating the number of items with the size of the packet
            Console.WriteLine("Storage data length: " + data_length);
            uint remainder = (uint)((data_length - 5) % Inventory.sizeOfPacket);
            Console.WriteLine("Remainder: " + remainder);
            if (Inventory.sizeOfPacket == 8 && remainder > 0)
            {
                Inventory.sizeOfPacket = 10;
            }
            item_count = (uint)((data_length - 5) / Inventory.sizeOfPacket);
            //check to see if the above devides evenly, if not, it's possible that inventory was empty and it's really 10 and not 8
            //should be able to figure that out here and set it to the right value...
            if (item_count > 0)
            {
                itemsFoundInStorage = true;
                for (int i = 0; i < item_count; i++)
                {
                    imageid = System.BitConverter.ToUInt16(data, i * Inventory.sizeOfPacket + 5);
                    quantity = System.BitConverter.ToUInt32(data, i * Inventory.sizeOfPacket + 5 + 2);
                    pos = System.BitConverter.ToUInt16(data, i * Inventory.sizeOfPacket + 5 + 6);
                    MyStorageItem.imageid = imageid;
                    MyStorageItem.pos = pos;
                    MyStorageItem.quantity = quantity;
                    MyStorageItem.name = "";
                    MyStorageItem.category_num = (int)category_num;
                    TheStorage.AddItem(MyStorageItem);
                    TheTCPWrapper.Send(CommandCreator.LOOK_AT_STORAGE_ITEM(pos));
                }
            }
            //if (item_count > 0)
            //{
            //    itemsFoundInStorage = true;
            //    for (int i = 0; i < item_count; i++)
            //    {
            //        imageid = System.BitConverter.ToUInt16(data, i * 8 + 5);
            //        quantity = System.BitConverter.ToUInt32(data, i * 8 + 5 + 2);
            //        //pos = data[i * 8 + 5 + 6];
            //        //pos = System.BitConverter.ToUInt16(data, i * 8 + 5 + 6);
            //        pos = System.BitConverter.ToUInt16(data, i * 8 + 5 + 6);
            //        MyStorageItem.imageid = imageid;
            //        MyStorageItem.pos = pos;
            //        MyStorageItem.quantity = quantity;
            //        MyStorageItem.name = "";
            //        MyStorageItem.category_num = (int)category_num;
            //        TheStorage.AddItem(MyStorageItem);
            //        TheTCPWrapper.Send(CommandCreator.LOOK_AT_STORAGE_ITEM(pos));
            //        //System.Threading.Thread.Sleep(20);
            //    }
            //}
            categoryCount++;
            if (categoryCount == total_categories && !itemsFoundInStorage)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Storage should be open now! (although it appears to be empty)"));
                storageOpen = true;
                openingStorage = false;
            }
            return;
        }
        private int total_categories;
        private int categoryCount;
        public bool itemsFoundInStorage = false;
        private void STORAGE_LIST(byte[] data)
        {
            if (storageOpen || Settings.IsTradeBot)
            {
                return;
            }
            total_categories = 0;
            categoryCount = 0;
            itemsFoundInStorage = false;
            openingStorage = true;
            TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Opening storage, please wait..."));
            total_categories = data[3];
            int category_number = 0;
            int stringsize = 1;
            string category_name = "";

            storageAvailable = true;
            TheStorage.ClearCategories();
            Storage.StorageCategory MyStorageCategory = new Storage.StorageCategory();
            TheStorage.ClearList();

            // loop through the total categories getting each one and its id
            for (int i = 0; i < total_categories; i++)
            {
                category_number = data[i + stringsize + 3];
                category_name = "";
                // the names are seperated by a 0x00, so read each byte until we find it
                while (data[i + stringsize + 3] != 0x00)
                {
                    stringsize++;
                    if (data[i + stringsize + 3] > 32 && data[i + stringsize + 3] < 126)
                    {
                        category_name += System.Text.ASCIIEncoding.ASCII.GetString(data, i + 3 + stringsize, 1);
                    }
                }
                // add the category to the array of categories
                MyStorageCategory.name = category_name;
                MyStorageCategory.num = category_number;
                TheStorage.AddCategory(category_name, category_number);
                TheTCPWrapper.Send(CommandCreator.GET_STORAGE_CATEGORY((byte)category_number));
            }
            TheStorage.AddCategory("Temporarily Unknown", -1);

        }
		private void GET_TRADE_ACCEPT(byte [] buffer)
		{
            if (Gambling || claimingPrize)
            {
                return;
            }
			//int TheBillance = 0;

            if (!totalCalculated && !storageAvailable && !Donating)
            {
                Console.WriteLine("Calculating total!");
                doTotal(username, false);
            }
            if (invalidItem)
            {
                //TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "***** There are invalid items that must be removed before we can procede.  See the trade details above. *****"));
                invalidItem = false;
                totalCalculated = false;
                return;        
            }
            if (totalOwed > 0)
            {
                //TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "You must put the required amount of coins on trade! (" + totalOwed + "gc)"));
                return;
            }
            else if (totalOwed < 0 && (MyMoneyOnTrade != Convert.ToUInt32(Math.Abs(totalOwed))))
            {
                //put my coins on trade
                MyMoneyOnTrade = Convert.ToUInt32(Math.Abs(totalOwed));
                uint moneyPutOnTrade = PutItemsOnTrade(1, MyMoneyOnTrade);
		System.Threading.Thread.Sleep(300);
                if (moneyPutOnTrade < MyMoneyOnTrade)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "!!!!!  I DON'T HAVE ENOUGH COINS TO COVER MY SIDE OF THE TRADE. !!!!!"));
                    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
                    return;
                }
                AddTrade(1, 1, MyMoneyOnTrade, "transaction coins to");
                return;
            }
			if (buffer[3] == 0)
			{
				AcceptStateMe=1;
				TheLogger.Debug("RX: ME: GET_TRADE_ACCEPT\n");
				return;
			}
			
			TheLogger.Debug("RX: PARTNER: GET_TRADE_ACCEPT\n");

            if (PartnerTradeItemsList.Count == 0 && MyItemList.Count == 0)
            {
                TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
                TheLogger.Debug("TX: EXIT_TRADE\n");
            }
            //if ((System.Math.Floor(this.Billance) == this.MyMoneyOnTrade) || Donating)
			{
				if (AcceptStatePartner==1 && AcceptStateMe == 1)
				{
					TradeTimer.Stop();
					TradeTimeElapsed = 0;
					TheTCPWrapper.Send(CommandCreator.ACCEPT_TRADE_ENTIRE( storageAvailable ));
                    Trading = false;
                    TheInventory.inventoryRequested = true;
					TheLogger.Debug("TX: ACCEPT_TRADE_ENTIRE\n");
                }
				else
				{
					AcceptStatePartner=1;
					TheTCPWrapper.Send(CommandCreator.ACCEPT_TRADE());
					TheLogger.Debug("TX: ACCEPT_TRADE\n");
                }
            }
            //else
            //{
            //    //AcceptStatePartner=1;
            //    string tempUsername = username;
            //    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
            //    TheTCPWrapper.Send(CommandCreator.SEND_PM(tempUsername, "Sorry, I can't accept this trade!"));
            //    TheLogger.Debug("TX: EXIT_TRADE\n");
            //}
		}
		
		private void GET_TRADE_REJECT(byte [] buffer)
		{
			
			if (buffer[3] == 0)
			{
				TheLogger.Debug("RX : ME: GET_TRADE_REJECT\n");
				AcceptStateMe = 0;
			}
			else
			{
				TheLogger.Debug("RX : PARTNER: GET_TRADE_REJECT\n");
				AcceptStatePartner = 0;
				TheTCPWrapper.Send(CommandCreator.REJECT_TRADE());
                if (MyMoneyOnTrade > 0)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "!!!!!  You cannot modify the trade after I've partially accepted it !!!!!"));
                    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
                }
			}
		}

        private void GET_TRADE_ABORT(byte[] buffer)
        {
            //not sure what to do here
            TradeTimer.Stop();
            storageAvailable = false;
        }
		
        //public void ChangeBillance(double Billance)
        //{
        //    // Trying to fix floating point operations
        //    Billance = Billance * 1000;
        //    Billance = System.Math.Round(Billance);
        //    Billance = Billance / 1000;
			
        //    this.Billance = this.Billance * 1000;
        //    this.Billance = System.Math.Round(this.Billance);
        //    this.Billance = this.Billance / 1000;
			
        //    this.Billance += Billance;
			
        //    this.Billance = this.Billance * 1000;
        //    this.Billance = System.Math.Round(this.Billance);
        //    this.Billance = this.Billance / 1000;
        //    int RoundedBillance = System.Convert.ToInt32(System.Math.Floor(this.Billance));
			
        //    if (RoundedBillance>this.MyMoneyOnTrade)
        //    {
        //        this.MyMoneyOnTrade += PutItemsOnTrade(1, (System.Convert.ToUInt32(RoundedBillance - this.MyMoneyOnTrade)),true);
        //        if (this.MyMoneyOnTrade<RoundedBillance)
        //        {
        //            TheTCPWrapper.Send(CommandCreator.SEND_PM(username,"Sorry, I don't have enough change!"));
        //        }
        //    }
			
        //    if (this.MyMoneyOnTrade>RoundedBillance && this.MyMoneyOnTrade>0)
        //    {
        //        if (RoundedBillance<=0)
        //        {
        //            // Update the InventorySnapshop
        //            Inventory.inventory_item MyInventoryItem = (Inventory.inventory_item)InventorySnapshop[this.MyMoneySlotID];
        //            MyInventoryItem.quantity += this.MyMoneyOnTrade;
        //            InventorySnapshop[this.MyMoneySlotID] = MyInventoryItem;
					
        //            // Remove the items from trade
        //            TheTCPWrapper.Send(CommandCreator.REMOVE_OBJECT_FROM_TRADE(System.Convert.ToByte(this.MyMoneyOnTradeSlotID),this.MyMoneyOnTrade));
        //            this.MyMoneyOnTrade = 0;
        //        }
        //        else
        //        {
        //            // Update the InventorySnapshop
        //            Inventory.inventory_item MyInventoryItem = (Inventory.inventory_item)InventorySnapshop[this.MyMoneySlotID];
        //            MyInventoryItem.quantity += System.Convert.ToUInt32((this.MyMoneyOnTrade-RoundedBillance));
        //            InventorySnapshop[this.MyMoneySlotID] = MyInventoryItem;
					
        //            // Remove the items from trade
        //            TheTCPWrapper.Send(CommandCreator.REMOVE_OBJECT_FROM_TRADE(System.Convert.ToByte(this.MyMoneyOnTradeSlotID),System.Convert.ToUInt32((this.MyMoneyOnTrade-RoundedBillance))));
        //            this.MyMoneyOnTrade -= System.Convert.ToUInt32((this.MyMoneyOnTrade-RoundedBillance));
        //        }
        //    }
			
        //    if (RoundedBillance<this.MyMoneyOnTrade)
        //    {
        //        TheTCPWrapper.Send(CommandCreator.SEND_PM(username,"You owe me " + (-RoundedBillance).ToString() + " gc!"));
        //    }
			
        //    TheLogger.Debug("Billance:" + this.Billance.ToString() + "\n");
        //    TheLogger.Debug("RoundedBillance:" + RoundedBillance.ToString() + "\n");
        //}

        public uint PutItemsOnTrade(int SQLID, uint quantity)
        { 
            return PutItemsOnTrade( SQLID, quantity, true);
        }
        public uint PutItemsOnTrade(int SQLID, uint quantity, bool fromInventory)
        {
            //totalCalculated = false;
            itemTraded = true;
            bool itemFound = false;
            uint reservedAmount = TheMySqlManager.ReservedAmount(SQLID);
            uint reservedAmountForUser = TheMySqlManager.ReservedAmountForUser(SQLID, username);
            Inventory.inventory_item MyInventoryItem = new Inventory.inventory_item();
            Storage.StorageItem MyStorageItem = new Storage.StorageItem();
            int totalOnHand = 0;
            int itemIndex = 0;
            uint position = 0;
            if (fromInventory)
            {
                for (int i = 0; i < InventorySnapshop.Count; i++)
                {
                    MyInventoryItem = (Inventory.inventory_item)InventorySnapshop[i];
                    if (MyInventoryItem.SqlID == SQLID && MyInventoryItem.pos < 36)
                    {
                        itemFound = true;
                        totalOnHand += (int)MyInventoryItem.quantity;
                        itemIndex = i;
                        position = MyInventoryItem.pos;
                    }
                }
            }
            else
            {
                for (int i = 0; i < StorageSnapshot.Count; i++)
                {
                    MyStorageItem = (Storage.StorageItem)StorageSnapshot[i];
                    if (MyStorageItem.knownItemsID == SQLID && itemFound == false)
                    {
                        itemFound = true;
                        totalOnHand = (int)MyStorageItem.quantity;
                        itemIndex = i;
                        position = MyStorageItem.pos;
                    }
                }
            }

            totalOnHand = totalOnHand - (int)reservedAmount + (int)reservedAmountForUser;
            if (totalOnHand < 0)
            {
                totalOnHand = 0;
            }
            //Console.WriteLine("SQLID: " + SQLID);
            //Console.WriteLine("Position: " + position);
            //Console.WriteLine("Quantity: " + quantity);
            //Console.WriteLine("reservedAmount: " + reservedAmount);
            //Console.WriteLine("reservedAmountForUser: " + reservedAmountForUser);
            //Console.WriteLine("Item Found: " + itemFound);
            //Console.WriteLine("Total On Hand: " + totalOnHand);
            //Console.WriteLine("Item index: " + itemIndex);
            uint amountToTrade = 0;
            if (itemFound && totalOnHand > 0)
            {
                if (totalOnHand > quantity)
                {
                    amountToTrade = quantity;
                }
                else
                {
                    amountToTrade = (uint)totalOnHand;
                }
                uint quantityLeftOver = (uint)totalOnHand - amountToTrade;
                //Console.WriteLine("Amount to trade: " + amountToTrade);
                if (fromInventory)
                {
                    MyInventoryItem = (Inventory.inventory_item)InventorySnapshop[itemIndex];

                    MyInventoryItem.quantity = quantityLeftOver;
                    InventorySnapshop[itemIndex] = MyInventoryItem;
                }
                else //from storage
                {
                    MyStorageItem = (Storage.StorageItem)StorageSnapshot[itemIndex];
                    MyStorageItem.quantity = quantityLeftOver;
                    StorageSnapshot[itemIndex] = MyStorageItem;
                }
                if (position > 255)
                {
                    TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE((ushort)position, amountToTrade));
                }
                else
                {
                    TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE((byte)position, amountToTrade, fromInventory));
                }
            }

            Console.WriteLine("Amount to trade: " + amountToTrade);
            return amountToTrade;
        }
        //public uint PutItemsOnTrade(int SQLID, uint quantity, bool fromInventory)  //old code saved for reference...
        //{
        //    itemTraded = true;
        //    uint QuantityPutOnTrade = 0;
        //    bool itemFound = false;
        //    uint reservedAmount = TheMySqlManager.ReservedAmount(SQLID);
        //    uint reservedAmountForUser = TheMySqlManager.ReservedAmountForUser(SQLID, username);
        //    Console.WriteLine("SQLID: " + SQLID);
        //    Console.WriteLine("Quantity: " + quantity);
        //    Console.WriteLine("reservedAmount: " + reservedAmount);
        //    Console.WriteLine("reservedAmountForUser: " + reservedAmountForUser);
        //    uint totalOnHand = 0;
        //    if (fromInventory)
        //    {
        //        for (int i = 0; i < InventorySnapshop.Count; i++)
        //        {
        //            Inventory.inventory_item MyInventoryItem = (Inventory.inventory_item)InventorySnapshop[i];
        //            if (MyInventoryItem.SqlID == SQLID && MyInventoryItem.pos < 36)
        //            {
        //                itemFound = true;
        //                totalOnHand += MyInventoryItem.quantity;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Storage.StorageItem MyStorageItem = (Storage.StorageItem)StorageSnapshot[i];
        //        for (int i = 0; i < StorageSnapshot.Count; i++)
        //        {
        //            if (MyStorageItem.knownItemsID == SQLID && itemFound == false)
        //            {
        //                itemFound = true;
        //                totalOnHand = MyStorageItem.quantity;
        //            }
        //        }
        //    }

        //    if (fromInventory == true)
        //    {
        //        for (int i = 0; i < InventorySnapshop.Count; i++)
        //        {
        //            Inventory.inventory_item MyInventoryItem = (Inventory.inventory_item)InventorySnapshop[i];

        //            if (MyInventoryItem.SqlID == SQLID && MyInventoryItem.pos < 36)
        //            {
        //                Console.WriteLine("Inventory Quantity: " + MyInventoryItem.quantity);
        //                if (MyInventoryItem.is_stackable == true)
        //                {
        //                    if (MyInventoryItem.quantity > (reservedAmount + quantity))
        //                    {
        //                        QuantityPutOnTrade += (uint)(quantity);
        //                        TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE(MyInventoryItem.pos, (uint)(QuantityPutOnTrade), fromInventory));
        //                        if ((MyInventoryItem.quantity - (uint)QuantityPutOnTrade) < 0)
        //                        {
        //                            MyInventoryItem.quantity = 0;
        //                        }
        //                        else
        //                        {
        //                            MyInventoryItem.quantity -= (uint)QuantityPutOnTrade;
        //                        }
        //                        InventorySnapshop[i] = MyInventoryItem;
        //                    }
        //                    else
        //                    {
        //                        int qtyAvailableForTrade = (int)(MyInventoryItem.quantity - (reservedAmount - reservedAmountForUser));
        //                        if (qtyAvailableForTrade > 0)
        //                        {
        //                            if (qtyAvailableForTrade < MyInventoryItem.quantity)
        //                            {
        //                                if (qtyAvailableForTrade > quantity)
        //                                {
        //                                    QuantityPutOnTrade = quantity;
        //                                }
        //                                else
        //                                {
        //                                    QuantityPutOnTrade = (uint)qtyAvailableForTrade;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (qtyAvailableForTrade > quantity)
        //                                {
        //                                    QuantityPutOnTrade = quantity;
        //                                }
        //                                else
        //                                {
        //                                    QuantityPutOnTrade = (uint)qtyAvailableForTrade;
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            QuantityPutOnTrade = 0;
        //                        }
        //                        //                                QuantityPutOnTrade += (uint)(MyInventoryItem.quantity - reservedAmount);
        //                        TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE(MyInventoryItem.pos, (uint)QuantityPutOnTrade, fromInventory));
        //                        MyInventoryItem.quantity -= (uint)QuantityPutOnTrade;
        //                        InventorySnapshop[i] = MyInventoryItem;
        //                    }
        //                }
        //                else
        //                {
        //                    if (reservedAmount == 0 && QuantityPutOnTrade < quantity)
        //                    {
        //                        QuantityPutOnTrade += (uint)MyInventoryItem.quantity;
        //                        TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE(MyInventoryItem.pos, MyInventoryItem.quantity, fromInventory));
        //                        MyInventoryItem.quantity = 0;
        //                        InventorySnapshop[i] = MyInventoryItem;
        //                    }
        //                    else if (reservedAmount > 0 && reservedAmountForUser > 0 && QuantityPutOnTrade < quantity - (reservedAmount - reservedAmountForUser))
        //                    {
        //                        QuantityPutOnTrade += (uint)MyInventoryItem.quantity;
        //                        TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE(MyInventoryItem.pos, MyInventoryItem.quantity, fromInventory));
        //                        MyInventoryItem.quantity = 0;
        //                        InventorySnapshop[i] = MyInventoryItem;
        //                    }
        //                    else if ((quantity - reservedAmount) > QuantityPutOnTrade)
        //                    {
        //                        QuantityPutOnTrade += (uint)MyInventoryItem.quantity;
        //                        TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE(MyInventoryItem.pos, MyInventoryItem.quantity, fromInventory));
        //                        MyInventoryItem.quantity = 0;
        //                        InventorySnapshop[i] = MyInventoryItem;
        //                    }
        //                }

        //                if (QuantityPutOnTrade >= quantity)
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    else //from/to storage
        //    {
        //        int total_items = StorageSnapshot.Count;
        //        for (int i = 0; i < total_items; i++)
        //        {
        //            Storage.StorageItem MyStorageItem = (Storage.StorageItem)StorageSnapshot[i];
        //            if (MyStorageItem.knownItemsID == SQLID && itemFound == false)
        //            {
        //                itemFound = true;
        //                if (MyStorageItem.quantity <= (quantity - QuantityPutOnTrade))
        //                {
        //                    if (MyStorageItem.pos > 255)
        //                    {
        //                        TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE(MyStorageItem.pos, MyStorageItem.quantity, false));
        //                        QuantityPutOnTrade += (uint)MyStorageItem.quantity;
        //                        MyStorageItem.quantity = 0;
        //                        StorageSnapshot[i] = MyStorageItem;
        //                    }
        //                    else
        //                    {
        //                        TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE((byte)MyStorageItem.pos, MyStorageItem.quantity, false));
        //                        QuantityPutOnTrade += (uint)MyStorageItem.quantity;
        //                        MyStorageItem.quantity = 0;
        //                        StorageSnapshot[i] = MyStorageItem;
        //                    }
        //                }
        //                else
        //                {
        //                    if (MyStorageItem.pos > 255)
        //                    {
        //                        TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE(MyStorageItem.pos, (uint)(quantity - QuantityPutOnTrade)));
        //                        QuantityPutOnTrade += (uint)quantity;
        //                        MyStorageItem.quantity -= (uint)quantity;
        //                        StorageSnapshot[i] = MyStorageItem;
        //                    }
        //                    else
        //                    {
        //                        TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE((byte)MyStorageItem.pos, (uint)(quantity - QuantityPutOnTrade)));
        //                        QuantityPutOnTrade += (uint)quantity;
        //                        MyStorageItem.quantity -= (uint)quantity;
        //                        StorageSnapshot[i] = MyStorageItem;
        //                    }
        //                }
        //            }
        //        }

        //    }
        //    return (uint)QuantityPutOnTrade;
        //}
		
//        private void GET_TRADE_OBJECT(byte [] buffer)
//        {
//            if (Gambling || claimingPrize)
//            {
//                return;
//            }
//            TheLogger.Debug("GET_TRADE_OBJECT\n");
			
//            // buffer[11]==1 --> New trade object on the trade partner side
//            // buffer[11]==0 --> New trade object on my side
//            if (buffer[11]==1)
//            {
//                byte pos = buffer[10];

//                //here here
//                TradeItem MyTradeItem;
//                if (PartnerTradeItemsList.Contains(pos)) // is this item already in the trade window?
//                {
////                    if (Settings.IsTradeBot == true)
//                    {
//                        MyTradeItem = (TradeItem)PartnerTradeItemsList[pos];
//                        MyTradeItem.quantity += System.BitConverter.ToUInt32(buffer, 5);
//                        PartnerTradeItemsList[pos] = MyTradeItem;
//                        if (MyTradeItem.name != "")
//                        {
//                            CheckBuyingItems(MyTradeItem, System.BitConverter.ToUInt32(buffer, 5), MyTradeItem.quantity);
//                        }
//                    }
//                }
//                else
//                {
//                    MyTradeItem = new TradeItem();
//                    MyTradeItem.pos = pos;
//                    MyTradeItem.imageid = System.BitConverter.ToInt16(buffer,3);
//                    if (MyTradeItem.imageid == 3 && Billance == 0 && Donating == false)
//                    {
//                        TheLogger.Debug("MyWantedItemList.Contains complete\nItem not wanted for purchase\n");
//                        TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
//                        TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "Sorry, but I am not buying Gold Coins."));
//                        return;
//                    }
//                    MyTradeItem.quantity = System.BitConverter.ToUInt32(buffer, 5);
//                    MyTradeItem.name="";
//                    MyTradeItem.pricepurchase = 0;
//                    MyTradeItem.pricepurchasemembers = 0;
					
//                    MyTradeItem.KnownItemsSqlID=-1;
//                    //PartnerTradeItemsList.Add(pos, MyTradeItem);
//                    PartnerTradeItemsList.Add(pos,MyTradeItem);
					
//                    GettingTradListItemnameID = pos;
					
//                    TheLogger.Debug("Got new Item at pos:" + pos.ToString()+"\n");
//                    TheTCPWrapper.Send(CommandCreator.LOOK_AT_TRADE_ITEM(pos, true));
//                }
//            }
			
//            // Check the slot of my own money - old comment
//            // See if this is the money slot or not, if it is, remember it.
//            if (buffer[11]==0)
//            {
//                byte pos = buffer[10];
//                int imageid = System.BitConverter.ToInt16(buffer,3);
				
//                if (imageid==3)
//                {
//                    this.MyMoneyOnTradeSlotID = pos;
//                }
//            }
			
//            // see if they put money in to pay for the trade, if so, adjust the balance.
//            if (buffer[11]==1 && System.BitConverter.ToUInt16(buffer,3)==3)
//            {
//                if (this.Donating==false)
//                {
//                    ChangeBillance((int)System.BitConverter.ToUInt32(buffer,5));
//                }
//            }
//        }
        private void GET_TRADE_OBJECT(byte[] buffer)
        {
            if (Gambling || claimingPrize)
            {
                return;
            }
            TheLogger.Debug("GET_TRADE_OBJECT\n");
            Console.WriteLine("Size of trade object buffer: " + buffer.Length);

            idleTradeTime = 0;
            // buffer[11]==1 --> New trade object on the trade partner side
            // buffer[11]==0 --> New trade object on my side
            if (buffer[11] == 1)
            {
                totalCalculated = false;
                byte pos = buffer[10];

                //here here
                TradeItem MyTradeItem;
                if (PartnerTradeItemsList.Contains(pos)) // is this item already in the trade window?
                {
                    MyTradeItem = (TradeItem)PartnerTradeItemsList[pos];
                    MyTradeItem.quantity += System.BitConverter.ToUInt32(buffer, 5);
                    PartnerTradeItemsList[pos] = MyTradeItem;
                }
                else
                {
                    MyTradeItem = new TradeItem();
                    MyTradeItem.pos = pos;
                    MyTradeItem.imageid = System.BitConverter.ToInt16(buffer, 3);
                    MyTradeItem.quantity = System.BitConverter.ToUInt32(buffer, 5);
                    MyTradeItem.name = "";
                    MyTradeItem.pricepurchase = 0;
                    MyTradeItem.pricepurchasemembers = 0;

                    MyTradeItem.KnownItemsSqlID = -1;
                    MyTradeItem.validTradeItem = true;
                    PartnerTradeItemsList.Add(pos, MyTradeItem);

                    if (Inventory.sizeOfPacket == 10)
                    {
                        MyTradeItem.ELServerItemID = System.BitConverter.ToInt16(buffer, 12);
                        Console.WriteLine(MyTradeItem.ELServerItemID);
                        TheMySqlManager.getItemInfo(ref MyTradeItem);
                        Console.WriteLine("Item ID: " + MyTradeItem.KnownItemsSqlID + "|Item name: " + MyTradeItem.name + "|Item weight: " + MyTradeItem.weight);
                        if (MyTradeItem.KnownItemsSqlID == -1)
                        {
                            TheTCPWrapper.Send(CommandCreator.LOOK_AT_TRADE_ITEM(pos, true));
                        }
                        else
                        {
                            if (Settings.IsTradeBot == true && !(Donating || PutOnSale))
                            {
                                if (MyTradeItem.name.ToLower() != "gold coins")
                                {
                                    if (CheckBuyingItems(ref MyTradeItem))
                                    {
                                        MyTradeItem.validTradeItem = true;
                                    }
                                    else
                                    {
                                        TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "!!!I am not buying " + MyTradeItem.name + ".  You will need to remove this item before I will accept the trade!!!"));
                                        MyTradeItem.validTradeItem = false;
                                    }
                                }
                                else
                                {
                                    MyTradeItem.validTradeItem = true;
                                    MyTradeItem.pricepurchase = 1;
                                    MyTradeItem.pricepurchasemembers = 1;
                                }
                            }
                            PartnerTradeItemsList[MyTradeItem.pos] = MyTradeItem;
                        }
                    }
                    else
                    {
                        TheTCPWrapper.Send(CommandCreator.LOOK_AT_TRADE_ITEM(pos, true));
                    }
                }
            }
            else
            {
                int imageID = System.BitConverter.ToInt16(buffer, 3);
                uint quantity = System.BitConverter.ToUInt32(buffer, 5);
                if (imageID == 3 && !(Donating || PutOnSale))
                {
                    AcceptStatePartner = 1;
                    TheTCPWrapper.Send(CommandCreator.ACCEPT_TRADE());
                    TheLogger.Debug("TX: ACCEPT_TRADE\n");
                }
            }

        }
	
		private void REMOVE_TRADE_OBJECT(byte [] buffer)
		{
            //if (itemTraded)
            //{
            //    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
            //    TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "Items may not be removed from the trade window once a trade has begun (until a bug is fix!)"));
            //    return;
            //}
            idleTradeTime = 0;
            totalCalculated = false;
			if (buffer[8]==1)
			{
				byte pos = buffer[7];
				UInt32 quantity = System.BitConverter.ToUInt32(buffer,3);
				TheLogger.Debug("REMOVE_TRADE_OBJECT pos:" + pos.ToString() + " quantity:" + quantity.ToString() + "\n");
				
				if (PartnerTradeItemsList.Contains(pos))
				{
					TradeItem MyTradeItem = (TradeItem)PartnerTradeItemsList[pos];
					
					if (MyTradeItem.quantity-quantity<1)
					{
						PartnerTradeItemsList.Remove(pos);
					}
					else
					{
						MyTradeItem.quantity -= quantity;
						PartnerTradeItemsList[pos] = MyTradeItem;
					}
				}
				else
				{
					TheLogger.ErrorLog("Error while REMOVE_TRADE_OBJECT - PartnerTradeItemsList does not contain pos");
				}
			}
		}
		
		private void INVENTORY_ITEM_TEXT (byte[] data)
		{

            if (Trading == false || TheInventory.GettingInventoryItems == true || Gambling || claimingPrize)
                return;
            //if getting storage items still, pause here...
            int i=0;
			string ItemDescription = "";
			string TempItemDescription = "";
			
			TempItemDescription=System.Text.ASCIIEncoding.ASCII.GetString(data,4,data.Length-4).Trim();
            // remove bad some chars (eg color tags)
            TempItemDescription = TempItemDescription.Replace((char)10, ' ');
			
            TheLogger.Debug("Beginning FOR loop (1)\n");
			for (i=0;i<TempItemDescription.Length;i++)
			{
				if (!(TempItemDescription[i]<32 || TempItemDescription[i]>126))
				{
					ItemDescription = ItemDescription+TempItemDescription[i];
				}
			}
            TheLogger.Debug("FOR loop (1) completed\n");
            TradeItem MyTradeItem = new TradeItem();
            MyTradeItem.pos = 0;

            // Get all the keys in the hashtable and sort them
            ArrayList keys = new ArrayList(PartnerTradeItemsList.Keys);
            keys.Sort();

            foreach (object obj in keys)
            {
                MyTradeItem = (TradeItem)PartnerTradeItemsList[obj];
                if (MyTradeItem.name == "")
                {
                    break;
                }
            }
            TheLogger.Debug("Beginning TRIM(1)\n");
            ItemDescription = ItemDescription.Trim();
            MyTradeItem.name = ItemDescription;
            MyTradeItem.name = MyTradeItem.name.Replace((char)10, ' ');
            MyTradeItem.name = MyTradeItem.name.Substring(0, MyTradeItem.name.IndexOf("Weight:"));
            MyTradeItem.name = MyTradeItem.name.Trim();

            if (Settings.IsTradeBot == true && !(Donating || PutOnSale))
            {
                if (MyTradeItem.name.ToLower() != "gold coins")
                {
                    if (CheckBuyingItems(ref MyTradeItem))
                    {
                        MyTradeItem.validTradeItem = true;
                    }
                    else
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "!!!I am not buying " + MyTradeItem.name + ".  You will need to remove this item before I will accpet the trade!!!"));
                        MyTradeItem.validTradeItem = false;
                    }
                }
                else
                {
                    MyTradeItem.validTradeItem = true;
                    MyTradeItem.pricepurchase = 1;
                    MyTradeItem.pricepurchasemembers = 1;
                }
            }

            MyTradeItem.weight = int.Parse(ItemDescription.Substring(ItemDescription.IndexOf("Weight:") + 8, ItemDescription.Length - (ItemDescription.IndexOf("Weight:") + 8 + 4)));
            MyTradeItem.KnownItemsSqlID = TheMySqlManager.GetKnownItemsSQLID(MyTradeItem);
            PartnerTradeItemsList[MyTradeItem.pos] = MyTradeItem;

            //double check if this was needed or not..... testing
            Inventory.inventory_item MyInventoryItem = new Inventory.inventory_item();
            MyInventoryItem.name = MyTradeItem.name;
            MyInventoryItem.imageid = MyTradeItem.imageid;
            MyInventoryItem.name = MyTradeItem.name;
            MyInventoryItem.is_resource = false;
            MyInventoryItem.is_reagent = false;
            MyInventoryItem.is_stackable = false;
            MyInventoryItem.use_with_inventory = false;
            MyInventoryItem.description = "";
            MyInventoryItem.weight = MyTradeItem.weight;
            TheMySqlManager.updateknownitems(MyInventoryItem, MyTradeItem.KnownItemsSqlID);

		}
		
		private bool CheckBuyingItems(ref TradeItem MyTradeItem)
		{
            bool Member = (TheMySqlManager.CheckIfTradeMember(TradeHandler.username, Settings.botid) == true);
            //loop through the wanted list to see if we want it...
            bool buying = false;
            foreach (WantedItem MyWantedItem in TheMySqlManager.GetWantedItemList(0).Values)
            {
                if (MyTradeItem.name.ToLower() == MyWantedItem.name.ToLower())
                {
                    int qtyOnHand = TheInventory.Quantity(MyWantedItem.name);
                    if (qtyOnHand >= MyWantedItem.maxquantity && MyWantedItem.maxquantity > 0)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "I'm already at my maximum quantity of " + MyTradeItem.name));
                        buying = false;
                    }
                    else if (MyWantedItem.maxquantity == 0)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "I am buying an unlimited number of "  + MyTradeItem.name + " for " + (Member ? MyWantedItem.pricepurchasemembers.ToString() : MyWantedItem.pricepurchase.ToString()) + " gc each."));
                        buying = true;
                    }
                    else
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "I am buying up to " + (MyWantedItem.maxquantity - qtyOnHand) + " " + MyTradeItem.name + " for " + (Member ? MyWantedItem.pricepurchasemembers.ToString() : MyWantedItem.pricepurchase.ToString()) + " gc each."));
                        buying = true;
                    }
                    MyTradeItem.pricepurchase = MyWantedItem.pricepurchase;
                    MyTradeItem.pricepurchasemembers = MyWantedItem.pricepurchasemembers;
                    MyTradeItem.maxQuantity = (MyWantedItem.maxquantity - qtyOnHand);
                    if (MyTradeItem.maxQuantity < 0)
                    {
                        MyTradeItem.maxQuantity = 0;
                    }
                    break;
                }
            }
            return buying;

            ////TheLogger.Debug("CheckBuyingItems(1) Entered\n");
            //Console.WriteLine("CheckBuyingItems(1) Entered + Item ID found in wanted list:" + foundSQLID +  "\n" );
            //if (this.Donating == true && quantity > 0)
            //{
            //    TheLogger.Debug("Donating\n");
            //    return 0;
            //}
			
            //if (this.PutOnSale==true && quantity>0)
            //{
            //    TheLogger.Debug("Putonsale\n");
            //    return 0;
            //}

            ////TheLogger.Debug("Passed 2 IF(), entering third: MyTradeItem.imageid = " + MyTradeItem.imageid.ToString() + "\n");
            //Console.WriteLine("Passed 2 IF(), entering third: MyTradeItem.imageid = " + MyTradeItem.imageid.ToString() + "\n");
            //if (MyTradeItem.name != "Gold Coins")
            //{
            //    System.Collections.ArrayList MyWantedItemList = TheMySqlManager.GetWantedItemListIDs();
            //    //TheLogger.Debug("Beginning MyWantedItemList.Contains\n");
            //    Console.WriteLine("Beginning MyWantedItemList.Contains\n");
            //    if (MyWantedItemList.Contains(MyTradeItem.KnownItemsSqlID))
            //    {
            //        //TheLogger.Debug("MyWantedItemList.Contains IF() passed\nBeginning WantedItem\n");
            //        Console.WriteLine("MyWantedItemList.Contains IF() passed\nBeginning WantedItem\n");
            //        WantedItem MyWantedItem = (WantedItem)MyWantedItemList[MyTradeItem.KnownItemsSqlID];
            //        TheLogger.Debug("WantedItem complete\nSending PM");
            //        if (MyWantedItem.maxquantity > 0)
            //        {
            //            count = System.Convert.ToInt32(TotalQuantity);
            //            TheLogger.Debug("Max " + MyWantedItem.maxquantity.ToString() + "count " + count.ToString() + "\n");
            //            if (  TheInventory.HaveItem(MyTradeItem.name) != -1)
            //            {
            //                if (TheInventory.Quantity(MyTradeItem.name) > MyWantedItem.maxquantity)
            //                {
            //                    TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "I am already carrying more than my allowed quantity of " + MyTradeItem.name + "."));
            //                    AcceptStateMe = 0;
            //                    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
            //                    return 0;
            //                }
            //                count += TheInventory.Quantity(MyTradeItem.name);
            //            }
            //            if (MyWantedItem.maxquantity < count)
            //            {
            //                TheLogger.Debug("Too many\n");
            //                TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "I am only buying up to " + (MyWantedItem.maxquantity - TheInventory.Quantity(MyTradeItem.name)) + " " + MyTradeItem.name + "."));
            //                AcceptStateMe = 0;
            //                //if (itemTraded)
            //                {
            //                    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
            //                }
            //                return 0;
            //            }
            //        }
            //        if (quantity > 0)
            //        {
            //            TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "I am buying " + MyTradeItem.name + " for " + (Member ? MyWantedItem.pricepurchasemembers.ToString() : MyWantedItem.pricepurchase.ToString()) + " gc each."));
            //        }
            //        TheLogger.Debug("PM sent\nBegin ChangeBillance\n");
            //        //ChangeBillance((Member ? MyWantedItem.pricepurchasemembers : MyWantedItem.pricepurchase) * quantity);
            //        TheLogger.Debug("ChangeBillance complete\n");
            //        TheLogger.Debug("MyWantedItemList.Contains complete\n");
            //        return (Member ? MyWantedItem.pricepurchasemembers : MyWantedItem.pricepurchase);
            //    }
            //    TheLogger.Debug("MyWantedItemList.Contains complete\nItem not wanted for purchase\n");
            //    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
            //    TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "Sorry, but I am not buying " + MyTradeItem.name + "."));
            //}
			
            //return 0;
		}

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {
            string Message = e.Message.ToLower().Replace("\'", "\\\'").Replace("\"", "\\\"");

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#total")
            {
                if (Settings.IsTradeBot == false)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I am not a trade bot!"));
                    return;
                }
                int rank = TheMySqlManager.GetUserRank(e.username, Settings.botid);
                if (rank < TheMySqlManager.GetCommandRank("#total", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }

                if (!this.AmITrading() || e.username != TradeHandler.username)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You must be currently trading with me to use this command."));
                    return;
                }

                string str = "", str2 = "";

                if (TheInventory.GettingInventoryItems == true)
                {
                    str2 = "I am building my inventory list, please try again in a few seconds";
                    str = str.PadRight(str2.Length, '=');
                    str = "[" + str;
                    str2 = "[" + str2;
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str2));
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));

                    return;
                }
                doTotal(e.username, true);
            }
        }
        private void doTotal(string username, bool fromPM)
        {
            idleTradeTime = 0;
            invalidItem = false;
            if (fromPM)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "===== Displaying your transaction details ====="));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "==============================================="));
            }
            bool Member = (TheMySqlManager.CheckIfTradeMember(TradeHandler.username, Settings.botid) == true);

            double total = 0;
            //going to do this twice to fake a sort...
            if (fromPM)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "*** Items you put in the trade window: ***"));
            }
            if (PartnerTradeItemsList.Count > 0)
            {
                foreach (TradeHandler.TradeItem myTradeItem in PartnerTradeItemsList.Values)
                {
                    if (!myTradeItem.validTradeItem)
                    {
                        continue;
                    }
                    if (myTradeItem.quantity > myTradeItem.maxQuantity && myTradeItem.name.ToLower() != "gold coins" && myTradeItem.maxQuantity > 0)
                    {
                        uint difference = Convert.ToUInt32(myTradeItem.quantity - myTradeItem.maxQuantity);
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I'm only buying up to " + myTradeItem.maxQuantity + " " + myTradeItem.name + " " + difference + " must be removed!"));
                        invalidItem = true;
                        continue;
                    }
                    double price = (Member ? myTradeItem.pricepurchasemembers : myTradeItem.pricepurchase);
                    if (fromPM)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, myTradeItem.quantity + " " + myTradeItem.name + (myTradeItem.validTradeItem ? (" @ " + price.ToString() + "gc = " + (Convert.ToString((price * myTradeItem.quantity))) + "gc") : " must be removed!!!")));
                    }
                    total -= (price * myTradeItem.quantity);
                }
                foreach (TradeHandler.TradeItem myTradeItem in PartnerTradeItemsList.Values)
                {
                    if (myTradeItem.validTradeItem)
                    {
                        continue;
                    }
                    invalidItem = true;
                    double price = (Member ? myTradeItem.pricepurchasemembers : myTradeItem.pricepurchase);
                    //if (fromPM)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, myTradeItem.quantity + " " + myTradeItem.name + (myTradeItem.validTradeItem ? ("@" + price.ToString() + "gc = " + (Convert.ToString((price * myTradeItem.quantity))) + "gc") : " must be removed!!!")));
                    }
                }
            }
            else
            {
                if (fromPM)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "NONE"));
                }
            }
            if (fromPM)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "==============================================="));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "*** Items I put in the trade window: ***"));
            }
            if (MyItemList.Count > 0)
            {
                foreach (TradeHandler.TradeLogItem myTradeLogItem in MyItemList)
                {
                    if (fromPM)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, myTradeLogItem.quantity + " " + TheMySqlManager.GetKnownItemsname(myTradeLogItem.KnownItemsSqlID) + " @ " + myTradeLogItem.price.ToString() + "gc = " + (Convert.ToString((myTradeLogItem.price * myTradeLogItem.quantity))) + "gc"));
                    }
                    total += (myTradeLogItem.price * myTradeLogItem.quantity);
                }
            }
            else
            {
                if (fromPM)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "NONE"));
                }
            }
            if (fromPM)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "==============================================="));
            }
            if (!invalidItem)
            {
                if (total < -1)
                {
                    total = Math.Truncate(total);
                    if (fromPM)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I owe you: " + Math.Abs(total) + "gc"));
                    }
                    else
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I owe you: " + Math.Abs(total) + "gc (/" + Settings.Loginname + " #total to see the details)"));
                    }
                }
                else if (total > 0)
                {
                    double remainder = total % 1;
                    if (remainder > 0)
                    {
                        total++;
                    }
                    total = Math.Truncate(total);
                    if (fromPM)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "You owe me: " + total + "gc"));
                    }
                    else
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "You owe me: " + total + "gc (/" + Settings.Loginname + " #total to see the details)"));
                    }
                }
                else
                {
                    if (fromPM)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "All coin amounts are correct!"));
                    }
                    else
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "All coin amounts are correct! (/" + Settings.Loginname + " #total to see the details)"));
                    }
                }
            }
            if (fromPM)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "==============================================="));
            }
            totalCalculated = true;
            totalOwed = total;
            Console.WriteLine("Total: " + total.ToString());
        }
	}
}
