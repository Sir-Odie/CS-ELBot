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
using System.Timers;

namespace cs_elbot
{
    /// <summary>
    /// Description of Inventory.
    /// </summary>

    public class Inventory
    {
        // OnGotNewInventoryList
        public delegate void GotNewInventoryListEventHandler(object sender, GotNewInventoryListEventArgs e);
        public event GotNewInventoryListEventHandler GotNewInventoryList;

        public class GotNewInventoryListEventArgs : EventArgs
        {
            public System.Collections.ArrayList TheInventory;
            public GotNewInventoryListEventArgs(System.Collections.ArrayList TheInventory)
            {
                this.TheInventory = TheInventory;
            }
        }

        protected void OnGotNewInventoryList(GotNewInventoryListEventArgs e)
        {
            inventoryRequested = false;
            if (GotNewInventoryList != null)
                GotNewInventoryList(this, e);
        }

        private System.Collections.Hashtable KnownItems = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
        private System.Collections.ArrayList TheInventory = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList());
        //        public bool GettingInventoryItems = false;
        //jg change here
        public bool GettingInventoryItems = false;
        public bool justGambled = false;
        public bool Gambling = false;
        public bool inventoryRequested = false;
        private bool firstTime = true;

        public static int sizeOfPacket = 0;


        public struct inventory_item
        {
            public byte pos;
            public string name;
            public string description;
            //            public uint quantity;
            public UInt32 quantity;
            public int SqlID;
            public int imageid;
            public int weight;
            public bool is_resource;
            public bool is_reagent;
            public bool is_stackable;
            public bool use_with_inventory;
            public UInt32 reservedQuantity;
        }

        private TCPWrapper TheTCPWrapper;
        private Logger TheLogger;
        private MySqlManager TheMySqlManager;

        public Inventory(TCPWrapper MyTCPWrapper, Logger MyLogger, MySqlManager MyMySqlManager)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
            this.TheLogger = MyLogger;
            this.TheMySqlManager = MyMySqlManager;
            TheTCPWrapper.GotDisconnected += new TCPWrapper.GotDisconnectedEventHandler(GotServerDisconnected);
        }
        private void GotServerDisconnected(object Sender, EventArgs e)
        {
            GettingInventoryItems = false;
            inventoryRequested = false;
	    // dirty hack: belongs into the TCPWrapper but we have no MySql access there
	    TheMySqlManager.ImStarted(Settings.botid);
        }
        public bool isStackable(int SQLID)
        {
            bool isStackable = false;
            foreach (inventory_item MyInventoryItem in GetInventoryList())
            {
                if (MyInventoryItem.SqlID == SQLID)
                {
                    isStackable = MyInventoryItem.is_stackable;
                    break;
                }
            }
            return isStackable;
        }
        public int GetMoneySlotID()
        {
            int SlotID = -1;
            int positionInArray = 0;
            System.Collections.ArrayList InventoryPosList = new System.Collections.ArrayList();

            foreach (inventory_item MyInventoryItem in GetInventoryList())
            {
                if (MyInventoryItem.name == "Gold Coins")
                {
                    SlotID = positionInArray;
                    break;
                    //                    return MyInventoryItem.pos;
                }
                positionInArray++;
            }

            return SlotID;
        }

        public uint GetMoneyAmount()
        {
            System.Collections.ArrayList InventoryPosList = new System.Collections.ArrayList();

            foreach (inventory_item MyInventoryItem in GetInventoryList())
            {
                if (MyInventoryItem.name == "Gold Coins")
                {
                    return MyInventoryItem.quantity;
                }
            }

            return 0;
        }

        public System.Collections.ArrayList GetInventoryListBySQLID(int SqlID, System.Collections.ArrayList InventoryList)
        {
            System.Collections.ArrayList InventorySQLIDList = new System.Collections.ArrayList();

            foreach (inventory_item MyInventoryItem in InventoryList)
            {
                if (MyInventoryItem.SqlID == SqlID)
                {
                    InventorySQLIDList.Add(MyInventoryItem);
                }
            }

            return (System.Collections.ArrayList)InventorySQLIDList.Clone();
        }

        public System.Collections.ArrayList GetInventoryListBySQLID(int SqlID)
        {
            System.Collections.ArrayList InventorySQLIDList = new System.Collections.ArrayList();

            foreach (inventory_item MyInventoryItem in GetInventoryList())
            {
                if (MyInventoryItem.SqlID == SqlID)
                {
                    InventorySQLIDList.Add(MyInventoryItem);
                }
            }

            return (System.Collections.ArrayList)InventorySQLIDList.Clone();
        }

        public System.Collections.ArrayList GetInventoryList()
        {
            return (System.Collections.ArrayList)TheInventory.Clone();
        }

        public void requestInventory()
        {
            Console.WriteLine("inventory requested: " + inventoryRequested);
            if (!inventoryRequested)
            {
                inventoryRequested = true;
                TheTCPWrapper.Send(CommandCreator.SEND_MY_INVENTORY());
            }
        }

        private void HERE_YOUR_INVENTORY(byte[] data)
        {
            //ITEM_REAGENT           = 1, /*!< can be used in magic */
            //ITEM_RESOURCE          = 2, /*!< can be used to manufacture */
            //ITEM_STACKABLE         = 4, /*!< the item is stackable */
            //ITEM_INVENTORY_USABLE  = 8, /*!< item can be used with inventory */
            //ITEM_TILE_USABLE       = 16,
            //ITEM_PLAYER_USABLE     = 32, /*!< item is usable by players */
            //ITEM_OBJECT_USABLE     = 64,
            //ITEM_ON_OFF            = 128,
            if (GettingInventoryItems)
            {
                return;
            }
            TheLogger.Log("Getting inventory item names!");
            TheInventory.Clear();
            TheMySqlManager.ClearInventoryList(Settings.botid);
            GettingInventoryItems = true;

            int total_items = data[3];

            if (total_items == 0)
            {
                OnGotNewInventoryList(new GotNewInventoryListEventArgs(TheInventory));
                TheLogger.Log("Got all inventory item names!");
                inventoryRequested = false;
                GettingInventoryItems = false;
                if (firstTime)
                {
                    MainClass.gettingPerks = true;
                    TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#list_perks"));
                    firstTime = false;
                }
                sizeOfPacket = 8;
                //TheTCPWrapper.Send(CommandCreator.SIT_DOWN(false));
            }
            else
            {
                sizeOfPacket = (data.Length - 4) / total_items;
                Console.WriteLine(data.Length + " " + total_items + ":size of packets: " + sizeOfPacket);
                if (sizeOfPacket != 8)
                {
                    for (int i = 0; i < total_items; i++)
                    {
                        inventory_item MyInventoryItem = new inventory_item();

                        byte pos = data[i * sizeOfPacket + 1 + 6 + 3];

                        MyInventoryItem.name = "";
                        MyInventoryItem.description = "";
                        MyInventoryItem.weight = 0;
                        MyInventoryItem.SqlID = 0;

                        MyInventoryItem.imageid = System.BitConverter.ToInt16(data, i * sizeOfPacket + 1 + 3);
                        MyInventoryItem.quantity = System.BitConverter.ToUInt32(data, i * sizeOfPacket + 1 + 2 + 3);
                        MyInventoryItem.pos = pos;

                        int flags = data[i * sizeOfPacket + 1 + 7 + 3];

                        MyInventoryItem.is_resource = ((flags & 2) > 0);
                        MyInventoryItem.is_reagent = ((flags & 1) > 0);
                        MyInventoryItem.use_with_inventory = ((flags & 8) > 0);
                        MyInventoryItem.is_stackable = ((flags & 4) > 0);

                        TheInventory.Add(MyInventoryItem);

                        TheTCPWrapper.Send(CommandCreator.LOOK_AT_INVENTORY_ITEM((byte)MyInventoryItem.pos));
                    }
                }
                else
                {
                    TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#item_uid"));
                    inventoryRequested = false;
                    GettingInventoryItems = false;
                    requestInventory();
                }
            }

            //for (int i = 0; i < total_items; i++)
            //{
            //    inventory_item MyInventoryItem = new inventory_item();

            //    byte pos = data[i * 8 + 1 + 6 + 3];

            //    MyInventoryItem.name = "";
            //    MyInventoryItem.description = "";
            //    MyInventoryItem.weight = 0;
            //    MyInventoryItem.SqlID = 0;

            //    MyInventoryItem.imageid = System.BitConverter.ToInt16(data, i * 8 + 1 + 3);
            //    MyInventoryItem.quantity = System.BitConverter.ToUInt32(data, i * 8 + 1 + 2 + 3);
            //    MyInventoryItem.pos = pos;

            //    int flags = data[i * 8 + 1 + 7 + 3];

            //    MyInventoryItem.is_resource = ((flags & 2) > 0);
            //    MyInventoryItem.is_reagent = ((flags & 1) > 0);
            //    MyInventoryItem.use_with_inventory = ((flags & 8) > 0);
            //    MyInventoryItem.is_stackable = ((flags & 4) > 0);

            //    TheInventory.Add(MyInventoryItem);

            //    TheTCPWrapper.Send(CommandCreator.LOOK_AT_INVENTORY_ITEM((byte)MyInventoryItem.pos));
            //    System.Threading.Thread.Sleep(10);
            //}
        }

        private inventory_item SetSQLID(inventory_item MyInventoryItem)
        {
            MyInventoryItem.SqlID = TheMySqlManager.SetSQLID(MyInventoryItem);
            return MyInventoryItem;
        }

        public int HaveItem(int MySQLID)
        {
            return HaveItem(MySQLID, "");
        }
        public int HaveItem(string MyItemName)
        {
            return HaveItem(0, MyItemName);
        }
        public int HaveItem(int MySQLID, string MyItemName)
        {
            int pos = -1;
            System.Collections.ArrayList MyInventoryList = GetInventoryList();
            foreach (Inventory.inventory_item MyInventoryItem in MyInventoryList)
            {
                if (MyInventoryItem.SqlID == MySQLID || MyItemName == MyInventoryItem.name)
                {
                    if (MyInventoryItem.pos < 36)
                    {
                        pos = MyInventoryItem.pos;
                    }
                }
            }
            return pos;
        }

        public int Quantity(string MyItemName)
        {
            return Quantity(0, MyItemName);
        }
        public int Quantity(int MySQLID)
        {
            return Quantity(MySQLID, "");
        }
        public int Quantity(int MySQLID, string MyItemName)
        {
            int ItemQuantity = 0;
            System.Collections.ArrayList MyInventoryList = GetInventoryList();
            foreach (Inventory.inventory_item MyInventoryItem in MyInventoryList)
            {
                if (MyInventoryItem.SqlID == MySQLID || MyInventoryItem.name == MyItemName)
                {
                    if (MyInventoryItem.pos < 36)//draavell add to not count worn inventory in wanted items
                    {
                        ItemQuantity += (int)MyInventoryItem.quantity;
                    }
                }
            }
            return ItemQuantity;
        }

        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            if (e.CommandBuffer[0] == 19)
            {
                HERE_YOUR_INVENTORY(e.CommandBuffer);
                return;
            }
            if (e.CommandBuffer[0] == 20)
            {
                INVENTORY_ITEM_TEXT(e.CommandBuffer);
                return;
            }

            if (e.CommandBuffer[0] == 21)
            {
                GET_NEW_INVENTORY_ITEM(e.CommandBuffer);
                return;
            }
        }

        private void REMOVE_ITEM_FROM_INVENTORY(byte[] data)
        {
            byte pos = data[3];
            string sql;
            System.Collections.ArrayList InventoryPosList = new System.Collections.ArrayList();

            foreach (inventory_item MyInventoryItem in GetInventoryList())
            {
                if (MyInventoryItem.pos == pos)
                {
                    sql = "DELETE FROM inventory WHERE `pos` = '" + pos.ToString() + "' AND `botid` = '" + Settings.botid + "' LIMIT 1;#" + MyInventoryItem.name;
                    TheMySqlManager.raw_sql(sql);
                }
            }
            return;

        }

        private void GET_NEW_INVENTORY_ITEM(byte[] data)
        {
            byte pos = data[9];
            byte flags = data[10];
            uint quantity = System.BitConverter.ToUInt32(data, 5);
            int image_id = System.BitConverter.ToInt16(data, 3);
            string sql;
            System.Collections.ArrayList InventoryPosList = new System.Collections.ArrayList();

            foreach (inventory_item MyInventoryItem in GetInventoryList())
            {
                if (MyInventoryItem.pos == pos)
                {
                    sql = "UPDATE inventory SET quantity = '" + quantity.ToString() + "' WHERE `pos` = '" + pos.ToString() + "' AND `botid` = '" + Settings.botid + "' LIMIT 1;";
                    TheMySqlManager.raw_sql(sql);
                }
            }
            return;
        }

        private void INVENTORY_ITEM_TEXT(byte[] data)
        {
            if (GettingInventoryItems == false||Gambling)
            {
                return;
            }

            int i = 0;
            string ItemDescription = "";
            string TempItemDescription = "";

            TempItemDescription = System.Text.ASCIIEncoding.ASCII.GetString(data, 4, data.Length - 4).Trim();
            TempItemDescription = TempItemDescription.Replace((char)10, ' ');

            // remove bad some chars (eg color tags)
            for (i = 0; i < TempItemDescription.Length; i++)
            {
                if (!(TempItemDescription[i] < 32 || TempItemDescription[i] > 126))
                {
                    ItemDescription = ItemDescription + TempItemDescription[i];
                }
            }

            lock (TheInventory.SyncRoot)
            {
                for (i = 0; i < TheInventory.Count; i++)
                {
                    inventory_item MyInventoryItem = (inventory_item)TheInventory[i];
                    if (MyInventoryItem.description == "")
                    {
                        ItemDescription = ItemDescription.Trim();
                        MyInventoryItem.description = ItemDescription;
                        if (MyInventoryItem.description.ToLower().Contains("extract"))
                        {
                            MyInventoryItem.name = ItemDescription.Substring(0, ItemDescription.LastIndexOf(" - ")).Trim();
                        }
                        else
                        {
                            MyInventoryItem.name = ItemDescription.Substring(0, ItemDescription.IndexOf(" - ")).Trim();
                        }
                        MyInventoryItem.weight = int.Parse(ItemDescription.Substring(ItemDescription.IndexOf("Weight:") + 8, ItemDescription.Length - (ItemDescription.IndexOf("Weight:") + 8 + 4)));
                        MyInventoryItem.SqlID = TheMySqlManager.SetSQLID(MyInventoryItem);
                        //get the reserved amount
                        MyInventoryItem.reservedQuantity = TheMySqlManager.ReservedAmount(MyInventoryItem.SqlID);
                        TheInventory[i] = MyInventoryItem;
                        // get the knownitems id (setsqlid)
                        //                        TheInventory[i] = SetSQLID((inventory_item)TheInventory[i]);
                        //update the known item info on the table mostly for weight and imageid
                        TheMySqlManager.updateknownitems(MyInventoryItem, MyInventoryItem.SqlID);
                        // insert the row may only need this for debugging
                        string dump = "INSERT INTO inventory (pos,quantity, knownitemsid, botid) VALUES ";
                        dump += "(" +
                        MyInventoryItem.pos + "," +
                        MyInventoryItem.quantity + "," +
                        MyInventoryItem.SqlID.ToString() + "," +
                        Settings.botid.ToString() + ")";
                        dump += ";";
                        TheMySqlManager.raw_sql(dump);

                        if (i + 1 < TheInventory.Count) //if this isn't the last one jump out of the loop
                        {
                            break;
                        }
                        //                        else
                        {
                            OnGotNewInventoryList(new GotNewInventoryListEventArgs(TheInventory));
                            TheLogger.Log("Got all inventory item names!");
                            inventoryRequested = false;
                            GettingInventoryItems = false;
                            if (firstTime)
                            {
                                MainClass.gettingPerks = true;
                                TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#list_perks"));
                                firstTime = false;
                            }

                            //TheTCPWrapper.Send(CommandCreator.SIT_DOWN(false));
                        }
                    }
                }
            }
        }

        internal void updateCoins(uint priceOfTicket)
        {
            int slotID = GetMoneySlotID();
            inventory_item myInventoryItem = (inventory_item)TheInventory[slotID];
            myInventoryItem.quantity += priceOfTicket;
            TheInventory[slotID] = myInventoryItem;
            TheMySqlManager.updateCoins(priceOfTicket);
        }
    }
}

