using System;
using System.Timers;
namespace cs_elbot
{
    public class Storage
    {
        // OnGotNewStorageList
        public delegate void GotNewStorageListEventHandler(object sender, GotNewStorageListEventArgs e);
        public event GotNewStorageListEventHandler GotNewStorageList;

        public class GotNewStorageListEventArgs : EventArgs
        {
            public System.Collections.ArrayList TheStorage;
            public GotNewStorageListEventArgs(System.Collections.ArrayList TheStorage)
            {
                this.TheStorage = TheStorage;
            }
        }

        protected void OnGotNewStorageList(GotNewStorageListEventArgs e)
        {
            if (GotNewStorageList != null)
                GotNewStorageList(this, e);
        }
        private System.Collections.ArrayList TheStorage = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList());

        public struct StorageItem
        {
            //public byte pos;
            public UInt16 pos;
            public int imageid;
            public UInt32 quantity;
            public string name;
            public int category_num;
            public int knownItemsID;
            public UInt32 reservedQuantity;
        }

        public struct StorageCategory
        {
            public int num;
            public string name;
        }

        private TCPWrapper TheTCPWrapper;
        private Logger TheLogger;
        private MySqlManager TheMySqlManager;

        public System.Collections.ArrayList TheStorageCategories = new System.Collections.ArrayList();

        public Storage(TCPWrapper MyTCPWrapper, Logger MyLogger, MySqlManager MyMySqlManager)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            //this.TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
            this.TheLogger = MyLogger;
            this.TheMySqlManager = MyMySqlManager;
        }

        public System.Collections.ArrayList GetStorageList()
        {
            return (System.Collections.ArrayList)TheStorage.Clone();
        }

        public void AddItem(StorageItem MyStorageItem)
        {
            TheStorage.Add(MyStorageItem);
            //add the row to the database
            return;
        }
        public void updateItem(StorageItem myStorageItem, int quantity, bool replaceQuantity)
        {
            bool itemFound = false;
            StorageItem tempStorageItem = new StorageItem();;
            for (int count = 0; count < TheStorage.Count; count++)
            {
                tempStorageItem = (StorageItem)TheStorage[count];
                if (tempStorageItem.pos == myStorageItem.pos && tempStorageItem.knownItemsID == myStorageItem.knownItemsID)
                {
                    TradeHandler.TradeLogItem myItem = new TradeHandler.TradeLogItem();
                    myItem.KnownItemsSqlID = myStorageItem.knownItemsID;
                    myItem.quantity = (uint)Math.Abs(quantity);
                    myItem.categoryNum = myStorageItem.category_num;
                    if (quantity < 0)
                    {
                        myItem.action = "deposited";
                        if (replaceQuantity)
                        {
                            tempStorageItem.quantity = myItem.quantity;
                        }
                        else
                        {
                            tempStorageItem.quantity += myItem.quantity;
                        }
                    }
                    else
                    {
                        myItem.action = "withdrew";
                        if (replaceQuantity)
                        {
                            tempStorageItem.quantity = myItem.quantity;
                        }
                        else
                        {
                            tempStorageItem.quantity -= myItem.quantity;
                        }
                    }
                    if (tempStorageItem.quantity > 0)
                    {
                        TheStorage[count] = tempStorageItem;
                        TheMySqlManager.UpdateStorageItem(myItem, replaceQuantity);
                    }
                    else
                    {
                        TheStorage.RemoveAt(count);
                        TheMySqlManager.DeleteStorageItem(myStorageItem);
                    }
                    itemFound = true;
                }
            }
            if (!itemFound)
            {
                TradeHandler.TradeLogItem myItem = new TradeHandler.TradeLogItem();
                myItem.action = "deposited";
                myItem.KnownItemsSqlID = myStorageItem.knownItemsID;
                myItem.price = 0;
                myItem.quantity = (uint)Math.Abs(quantity);
                myItem.categoryNum = myStorageItem.category_num;
                TheMySqlManager.UpdateStorageItem(myItem, false);
                myStorageItem.quantity = myItem.quantity;
                TheStorage.Add(myStorageItem);
            }
        }
        public void ClearList()
        {
            TheStorage.Clear();
            //delete the stroage in the database
            TheMySqlManager.ClearStorage();
            return;
        }
        public int Count()
        {
            return TheStorage.Count;
        }

        public void UpdateText(string nameText)
        {
            int idx;
            nameText = nameText.Replace((char)10, ' ');
            lock (TheStorage.SyncRoot)
            {
                for (idx = 0; idx < TheStorage.Count; idx++)
                {
                    StorageItem MyStorageItem = (StorageItem)TheStorage[idx];
                    if (MyStorageItem.name == "")
                    {
                        MyStorageItem.name = nameText;
                        TradeHandler.TradeItem MyTradeItem = new TradeHandler.TradeItem();
                        MyTradeItem.name = MyStorageItem.name;
                        MyTradeItem.imageid = MyStorageItem.imageid;
                        MyTradeItem.weight = -1;
                        TradeHandler.TradeItem MyTempTradeItem = new TradeHandler.TradeItem();
                        MyTempTradeItem = MyTradeItem;
                        MyStorageItem.knownItemsID = TheMySqlManager.GetStorageSQLID(MyTempTradeItem);
                        //get the reserved amount
                        MyStorageItem.reservedQuantity = TheMySqlManager.ReservedAmount(MyStorageItem.knownItemsID);
                        TheStorage[idx] = MyStorageItem;
                        //what we wanna do here is only insert on the last one, then insert them all...
                        if (idx == TheStorage.Count - 1)
                        {
                            //insert all of the rows
                            TheMySqlManager.InsertStorageItems(TheStorage);
                            TradeHandler.openingStorage = false;
                        }
                        //TheMySqlManager.InsertStorageItem(MyStorageItem);
                        break;
                    }
                }
            }
        }

        public void ClearCategories()
        {
            TheStorageCategories.Clear();
            return;
        }

        public void AddCategory( string category_name, int category_number)
        {
            string nameText = category_name;
            nameText = nameText.Replace((char)10, ' ');
            StorageCategory MyStorageCategory = new StorageCategory();
            MyStorageCategory.name = category_name.Trim();
            MyStorageCategory.num = category_number;
            TheStorageCategories.Add( MyStorageCategory );
            TheMySqlManager.InsertStorageCategory(MyStorageCategory);
            //insert the categories into the table
            return;
        }
    }
}