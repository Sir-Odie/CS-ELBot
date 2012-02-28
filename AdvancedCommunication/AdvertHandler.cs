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

namespace cs_elbot.AdvancedCommunication
{
    /// <summary>
    /// description of AdvertHandler.
    /// </summary>
    public class AdvertHandler
    {
        private TCPWrapper TheTCPWrapper;
        private MySqlManager TheMySqlManager;
        private Logger TheLogger;
        private TradeHandler TheTradeHandler;
        private Inventory TheInventory;
        private errorHandler TheErrorHandler;
        private System.Timers.Timer AdvertTimer = new System.Timers.Timer();

        public AdvertHandler(TCPWrapper MyTCPWrapper, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler, Inventory MyInventory, errorHandler MyErrorHandler)
        {
            Random RandomClass = new Random();
            int i;
            this.TheLogger = MyLogger;
            this.TheMySqlManager = MyMySqlManager;
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheTradeHandler = MyTradeHandler;
            this.TheInventory = MyInventory;
            this.TheErrorHandler = MyErrorHandler;

            TheTCPWrapper = MyTCPWrapper;
            TheMySqlManager = MyMySqlManager;
            TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
            if (TheMySqlManager.advertise())
            {
                this.AdvertTimer.Interval = 1000;
                if (TheMySqlManager.advertchannel(Settings.botid) != 99999999)
                {
                    //if (TheMySqlManager.lastadverttime(Settings.botid) + TheMySqlManager.minadverttime() <= DateTime.Now )
                    if (TheMySqlManager.lastadverttime(Settings.botid) <= TheMySqlManager.minadverttime())
                    {
                        i = TheMySqlManager.minadverttime() * 1000;
                        i += (RandomClass.Next(0, TheMySqlManager.randomadvertdelay()) * 1000);
                        if (i < 900000)
                            i = 900000;//minimum 15 minute delay
                        this.AdvertTimer.Interval = i;
                    }
                }
            }
            else
            {
                this.AdvertTimer.Interval = 900000;
            }

            TheLogger.Debug("New AdvertTimer.Interval: " + this.AdvertTimer.Interval + "\n");

            AdvertTimer.Elapsed += new ElapsedEventHandler(AdvertTimer_Tick);
            TheTCPWrapper.GotConnected += new TCPWrapper.GotConnectedEventHandler(OnGotConnected);
            TheTCPWrapper.GotDisconnected += new TCPWrapper.GotDisconnectedEventHandler(OnGotDisconnected);
        }

        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            if (e.CommandBuffer[0] == 0x05 && BasicCommunication.new_minute.getServerTime() != new DateTime()) //new minute
            {
                //will want to check the number of elapsed minutes and see if we need to advertise
            }
        }
        private void AdvertTimer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                int i = 0;
                uint advertchannel = TheMySqlManager.advertchannel(Settings.botid);
                Random RandomClass = new Random();
                string advertstr = "", str2 = "", endm = TheMySqlManager.advertendmessage().Trim();
                if (TheTradeHandler.AmITrading() || (TheInventory.GettingInventoryItems == true))
                {
                    this.AdvertTimer.Interval = (1 * 1000);// +RandomClass.Next(0, (60 * 60 * 1000)); ;//m*s*us=1 hour
                    TheLogger.Debug("Advert delayed due to current trade\n");
                    return;
                }
                if (advertchannel != 99999999)
                {
                    //this.AdvertTimer.Interval = (15 * 60 * 1000) + RandomClass.Next(0, (60 * 60 * 1000)); ;//m*s*us=1 hour
                    //this.AdvertTimer.Interval=60*

                    i = TheMySqlManager.minadverttime() * 1000;
                    i += (RandomClass.Next(0, TheMySqlManager.randomadvertdelay()) * 1000);
                    if (i < 900000)
                        i = 900000;//minimum 15 minute delay
                    this.AdvertTimer.Interval = i;
                }
                else
                {
                    if (MainClass.serverName == "test")
                    {
                        this.AdvertTimer.Interval = 1000;
                    }
                }
                TheLogger.Debug("New AdvertTimer.Interval: " + this.AdvertTimer.Interval + "\n");

                if (advertchannel != 99999999 && TheMySqlManager.lastadverttime(Settings.botid) < TheMySqlManager.minadverttime() && MainClass.serverName != "test")
                    return;

                //Start the advertising here, if it's set to true...
                //Assumptions: 140 characters max length
                //Need to get advert_rates from the database for this bot to see how the weights are distributed
                if (TheMySqlManager.advertise())
                {
                    //decide if we should do a text message or a _goods_ message
                    //get the rate for text adverts, it's a number between 0 and 1
                    //then get a random number between 0 and one, if the random number is less than the advert rate, do the advert
                    //otherwise, do a _goods_ advert (buying and selling...)
                    double advertTextRate = TheMySqlManager.getAdvertTextRate();
                    double sellRate = TheMySqlManager.getSellRate();
                    Random randomDouble = new Random();
                    double randomRate = randomDouble.NextDouble();
                    if (randomRate < advertTextRate)
                    {
                        //do a text advert
                        //get a random row from the adverts table and use it as the advertisement
                        //select * from adverts where botid = 2 order by rand() LIMIT 1;
                        advertstr = TheMySqlManager.getTextAdvert();
                    }
                    else
                    {
                        //do a goods advert (buying and selling)
                        //get the selling rate, it's between 0 and 1
                        //get a random number between 0 and 1, if the rate is less than the selling rate, do a "selling" advert
                        //otherwise, do a "buying" advert
                        System.Collections.ArrayList MyInventoryList = TheInventory.GetInventoryList();
                        System.Collections.ArrayList MySellingItemsList = TheMySqlManager.GetSellingList(1);

                        // sum up all inventory items if the items are on sale
                        System.Collections.SortedList TheInventoryList = new System.Collections.SortedList();
                        System.Collections.ArrayList MyWantedItemsList = TheMySqlManager.GetWantedList(1);

                        //add up the inventory stuff
                        foreach (Inventory.inventory_item MyInventoryItem in MyInventoryList)
                        {
                            if (TheInventoryList.Contains(MyInventoryItem.SqlID) && MyInventoryItem.pos < 36)
                            {
                                Inventory.inventory_item TempInventoryItem = (Inventory.inventory_item)TheInventoryList[MyInventoryItem.SqlID];
                                TempInventoryItem.quantity += MyInventoryItem.quantity;
                                TheInventoryList[MyInventoryItem.SqlID] = TempInventoryItem;
                            }
                            else
                            {
                                if (MyInventoryItem.pos < 36)
                                {
                                    TheInventoryList.Add(MyInventoryItem.SqlID, MyInventoryItem);
                                }
                            }
                        }
                        randomRate = randomDouble.NextDouble();
                        if (randomRate < sellRate)
                        {
                            //selling
                            advertstr = "Selling:";
                            foreach (TradeHandler.SellingItem MySellingItem in MySellingItemsList)
                            {
                                if (TheInventoryList.Contains(MySellingItem.KnownItemsSqlID))
                                {
                                    Inventory.inventory_item MyInventoryItem = (Inventory.inventory_item)TheInventoryList[MySellingItem.KnownItemsSqlID];
                                    if (MySellingItem.pricesale > 0)
                                    {
                                        uint tempAmount = 0;
                                        if ((TheMySqlManager.ReservedAmount(MyInventoryItem.SqlID)) < MyInventoryItem.quantity)
                                        {
                                            tempAmount = MyInventoryItem.quantity;
                                            tempAmount = tempAmount - (TheMySqlManager.ReservedAmount(MyInventoryItem.SqlID));
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        str2 = tempAmount.ToString() + " " + MyInventoryItem.name + ":" + MySellingItem.pricesale.ToString() + "gc";
                                        if (advertstr.Length + str2.Length + endm.Length < 140)
                                        {
                                            if (advertstr.Length > 8)
                                                advertstr += ", ";
                                            advertstr += str2;
                                        }
                                    }
                                }
                            }
                            if (advertstr.Length > 8)
                            {
                                advertstr += "|" + endm;
                            }
                            else
                            {
                                advertstr = "";
                            }
                        }
                        else
                        {
                            //buying
                            advertstr = "Buying:";
                            foreach (TradeHandler.WantedItem MyWantedItem in MyWantedItemsList)
                            {
                                if (TheInventoryList.Contains(MyWantedItem.KnownItemsSqlID))
                                {
                                    Inventory.inventory_item MyInventoryItem = (Inventory.inventory_item)TheInventoryList[MyWantedItem.KnownItemsSqlID];
                                    if (MyWantedItem.pricepurchase > 0 && MyInventoryItem.quantity < MyWantedItem.maxquantity)
                                    {
                                        str2 = TheMySqlManager.GetKnownItemsname(MyWantedItem.KnownItemsSqlID) + ":" + MyWantedItem.pricepurchase.ToString() + "gc";
                                        if (advertstr.Length + str2.Length + endm.Length < 140)
                                        {
                                            if (advertstr.Length > 7)
                                                advertstr += ", ";
                                            advertstr += str2;
                                        }
                                    }
                                }
                                else
                                {
                                    if (MyWantedItem.pricepurchase > 0)
                                    {
                                        str2 = TheMySqlManager.GetKnownItemsname(MyWantedItem.KnownItemsSqlID) + ":" + MyWantedItem.pricepurchase.ToString() + "gc";
                                        if (advertstr.Length + str2.Length + endm.Length < 140)
                                        {
                                            if (advertstr.Length > 7)
                                                advertstr += ", ";
                                            advertstr += str2;
                                        }
                                    }
                                }
                            }
                            if (advertstr.Length > 7)
                            {
                                advertstr += "|" + endm;
                            }
                            else
                            {
                                advertstr = "";
                            }
                        }
                    }
                }
                if (advertstr != "")
                {
                    TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#jc " + advertchannel.ToString()));
                    TheTCPWrapper.Send(CommandCreator.RAW_TEXT("@" + advertstr));
                    TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#lc " + advertchannel.ToString()));
                    TheMySqlManager.advertlog("@@" + advertchannel.ToString() + " " + advertstr);
                    string sql = "UPDATE bots SET lastadverttime = NOW() WHERE botid = " + Settings.botid;
                    TheMySqlManager.raw_sql(sql);
                }
                advertstr = "";
            }
            catch (Exception exception)
            {
                TheErrorHandler.writeErrorMessage("Exception raised in the advert handler..." + exception);
            }
            finally
            { 
            }
        }

        private void OnGotConnected(object sender, EventArgs e)
        {
            AdvertTimer.Start();
        }

        private void OnGotDisconnected(object sender, EventArgs e)
        {
            AdvertTimer.Stop();
        }
    }
}
