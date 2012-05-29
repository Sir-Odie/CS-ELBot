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
    /// description of AltAdvertHandler.
    /// </summary>
    public class AltAdvertHandler
    {
        private TCPWrapper TheTCPWrapper;
        private MySqlManager TheMySqlManager;
        private Logger TheLogger;
        private TradeHandler TheTradeHandler;
        private Inventory TheInventory;
        private System.Timers.Timer AltAdvertTimer = new System.Timers.Timer();
        private errorHandler TheErrorHandler;

        public AltAdvertHandler(TCPWrapper MyTCPWrapper, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler, Inventory MyInventory, errorHandler MyErrorHandler)
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
            uint altadvertchannel = TheMySqlManager.altadvertchannel(Settings.botid);
            this.AltAdvertTimer.Interval = 900000;
            if (TheMySqlManager.altadvertise())
            {
                this.AltAdvertTimer.Interval = 1000;
                if (altadvertchannel != 99999999)
                {
                    if (TheMySqlManager.lastaltadverttime(Settings.botid) <= TheMySqlManager.minaltadverttime())
                    {
                        i = TheMySqlManager.minaltadverttime() * 1000;
                        i += (RandomClass.Next(0, TheMySqlManager.randomaltadvertdelay()) * 1000);
                        if (i < 900000)
                            i = 900000;//minimum 15 minute delay
                        this.AltAdvertTimer.Interval = i;
                    }
                }
            }

            TheLogger.Debug("New AltAdvertTimer.Interval: " + this.AltAdvertTimer.Interval + "\n");

            if (altadvertchannel != 99999999 && TheMySqlManager.lastaltadverttime(Settings.botid) < TheMySqlManager.minaltadverttime() && MainClass.serverName != "test")
                return;

            AltAdvertTimer.Elapsed += new ElapsedEventHandler(AltAdvertTimer_Tick);
            TheTCPWrapper.GotConnected += new TCPWrapper.GotConnectedEventHandler(OnGotConnected);
            TheTCPWrapper.GotDisconnected += new TCPWrapper.GotDisconnectedEventHandler(OnGotDisconnected);
        }

        private void AltAdvertTimer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (TheInventory.GettingInventoryItems == true)
                {
                    return;
                }
                //int Channel = 32768;
                int i = 0, RandomAdvertNumber, ii;
                Random RandomClass = new Random();
                string str = "";

                if (TheMySqlManager.altadvertchannel(Settings.botid) != 99999999)
                {
                    //this.AdvertTimer.Interval = (15 * 60 * 1000) + RandomClass.Next(0, (60 * 60 * 1000)); ;//m*s*us=1 hour
                    //this.AdvertTimer.Interval=60*

                    i = TheMySqlManager.minaltadverttime() * 1000;
                    i += (RandomClass.Next(0, TheMySqlManager.randomaltadvertdelay()) * 1000);
                    if (i < 900000)
                        i = 900000;//minimum 15 minute delay
                    this.AltAdvertTimer.Interval = i;
                }
                else
                {
                    if (MainClass.serverName == "test")
                    {
                        this.AltAdvertTimer.Interval = 1000;

                    }
                }
                TheLogger.Debug("New AltAdvertTimer.Interval: " + this.AltAdvertTimer.Interval + "\n");

                uint altadverchannel = TheMySqlManager.altadvertchannel(Settings.botid);
                if (altadverchannel < 1)
                    return;
                if (altadverchannel < 32768 && TheMySqlManager.lastaltadverttime(Settings.botid) < TheMySqlManager.minaltadverttime())
                    return;
                if (TheMySqlManager.altadvertise())
                {
                    try
                    {
                        System.Collections.ArrayList AdvertsArrayList = TheMySqlManager.GetBotAltAdverts(Settings.botid);
                        TheLogger.Debug("Arraylist count is " + AdvertsArrayList.Count.ToString() + "\n");

                        if (AdvertsArrayList.Count > 0)
                        {
                            RandomAdvertNumber = RandomClass.Next(0, AdvertsArrayList.Count);
                            TheLogger.Debug("Attempting alt advert #" + RandomAdvertNumber.ToString() + " of " + AdvertsArrayList.Count.ToString() + " in total\n");
                            string Advert, Advert2 = "";
                            Advert = (string)AdvertsArrayList[RandomAdvertNumber];
                            ii = 0;
                            for (RandomAdvertNumber = 0; RandomAdvertNumber < Advert.Length; RandomAdvertNumber++)
                            {
                                //if (Advert[RandomAdvertNumber] != '\\')
                                {
                                    Advert2 += Advert[RandomAdvertNumber].ToString();
                                    ii++;
                                }
                            }
                            Advert = Advert2;
                            if (Advert.Length > 140)
                                Advert = Advert.Substring(0, 140);
                            TheLogger.Debug("Advert \"" + Advert + "\"\n");
                            Advert = "@@" + TheMySqlManager.altadvertchannel(Settings.botid).ToString() + " " + Advert;
                            TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#jc " + TheMySqlManager.altadvertchannel(Settings.botid).ToString()));
                            TheTCPWrapper.Send(CommandCreator.RAW_TEXT(Advert));
                            TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#lc " + TheMySqlManager.altadvertchannel(Settings.botid).ToString()));
                            TheMySqlManager.advertlog(Advert);
                        }
                        else
                        {
                            TheLogger.Debug("There are no alt adverts\n");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        TheLogger.ErrorLog("AltAdvertTimer_Tick:", ex);
                    }
                }
                else
                {
                    return;
                }
                str = "UPDATE bots SET lastaltadverttime = now() WHERE id = '" + Settings.botid + "' LIMIT 1;";
                TheMySqlManager.raw_sql(str);
            }
            catch (Exception exception)
            {
                TheErrorHandler.writeErrorMessage("Exception raised in the altadvert handler..." + exception);
            }
            finally
            { 
            }
        }

        private void OnGotConnected(object sender, EventArgs e)
        {
            AltAdvertTimer.Start();
        }

        private void OnGotDisconnected(object sender, EventArgs e)
        {
            AltAdvertTimer.Stop();
        }
    }
}
