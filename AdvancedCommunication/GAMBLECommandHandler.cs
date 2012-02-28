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
	/// description of DONATECommandHandler.
	/// </summary>
	public class GAMBLECommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		//private bool CommandIsDisabled;
		private HelpCommandHandler TheHelpCommandHandler;
		private Logger TheLogger;
		private TradeHandler TheTradeHandler;
        private Inventory TheInventory;

        private string username = "";
        public uint totalCoins = 0;
        private bool acceptedOnce = false;
        private decimal winningPercent = (decimal)0.00;
        private uint priceOfTicket = 0;
        private string prizeLevel = "";

        //random number generator (let's hope this isn't bugged, rofl...)
        static Random _r = new Random();
        public static int getRandom()
        {
            // Use class-level Random so that when this
            // method is called many times, it still has
            // good Randoms.
            int n = _r.Next(1, 1000);
            // If this declared a local Random, it would
            // repeat itself.
            return n;
        }

        public GAMBLECommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, HelpCommandHandler MyHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler, Inventory MyInventory)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheHelpCommandHandler = MyHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheLogger = MyLogger;
			this.TheTradeHandler = MyTradeHandler;
            this.TheInventory = MyInventory;
            this.TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
            TheInventory.GotNewInventoryList += new Inventory.GotNewInventoryListEventHandler(TheInventory_GotNewInventoryList);
            TheMessageParser.Got_AbortTrade += new cs_elbot.BasicCommunication.MessageParser.Got_AbortTrade_EventHandler(TheMessageParser_Got_AbortTrade);
            //this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#donate",Settings.botid);
			
			//if (CommandIsDisabled == false)
			{
                TheHelpCommandHandler.AddCommand("#gamble - gamble with me for a prize.");
                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
			}
		}

        void TheMessageParser_Got_AbortTrade(object sender, EventArgs e)
        {
            if (TheTradeHandler.Gambling && !TheInventory.justGambled)
            {
                TheTradeHandler.Gambling = false;
                TheInventory.Gambling = false;
                username = "";
                acceptedOnce = false;
                totalCoins = 0;
                TheTradeHandler.Trading = false;
                TradeHandler.username = "";
            }
            prizeLevel = "";
        }
        public UInt32 quantityBeforeTrade = 0;
        void TheInventory_GotNewInventoryList(object sender, Inventory.GotNewInventoryListEventArgs e)
        {
            if (TheInventory.justGambled)
            {
                TheInventory.justGambled = false;
                if ((quantityBeforeTrade + priceOfTicket) == TheInventory.Quantity(1))
                {
                    TradeHandler.TradeLogItem MyTradeLogItem = new TradeHandler.TradeLogItem();
                    MyTradeLogItem.action = "Gambling ticket: " + prizeLevel;
                    MyTradeLogItem.KnownItemsSqlID = 1;
                    MyTradeLogItem.price = priceOfTicket;
                    MyTradeLogItem.quantity = 1;
                    this.TheMySqlManager.LogTrade(MyTradeLogItem, username, Settings.botid, true);
                    TheTradeHandler.Trading = false;
                    TheTradeHandler.Gambling = false;
                    TheInventory.Gambling = false;
                    TheTradeHandler.SentThanks = false;
                    TheTradeHandler.stopTimer();
                    acceptedOnce = false;
                    totalCoins = 0;
                    decimal myNumber = getRandom() * (decimal)0.001;
                    bool youWin = myNumber < winningPercent;
                    UInt32 jackpotAmount = 0;
                    bool resetJackpotAmount = false;
                    //check to see if they won the jackpot, if so they don't get another prize or loss message...
                    //get the jackpot number and see if they won, if they do, reserver it for them...
                    decimal jackpotNumber = TheMySqlManager.getJackpotNumber(prizeLevel, out jackpotAmount);
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "The jackpot number is: " + jackpotNumber + ":Amount: " + jackpotAmount));
                    //myNumber = jackpotNumber;
                    if (myNumber == jackpotNumber)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Congratulations! You've won the jackpot!!! " + jackpotAmount + "gc"));
                        TheMySqlManager.reserveItem(1, (uint)jackpotAmount, username, true);
                        resetJackpotAmount = true;
                    }
                    else if (youWin)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Your number is: " + myNumber + " You win!"));
                        string prize = TheMySqlManager.reservePrize(prizeLevel, username);
                    }
                    else
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Your number is: " + myNumber + " You lose, try again."));
                    }
                    TheMySqlManager.updateJackpotAmount(prizeLevel, resetJackpotAmount);
                    prizeLevel = "";
                    //TheInventory.updateCoins(priceOfTicket);
                }
            }
        }
        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            if (TheTradeHandler.claimingPrize)
            {
                return;
            }
            if ( !TheTradeHandler.Gambling)
            {
                return;
            }
            if (e.CommandBuffer[0] == 35) // GET_TRADE_OBJECT
            {
                GET_TRADE_OBJECT(e.CommandBuffer);
                return;
            }

            if (e.CommandBuffer[0] == 37)
            {
                GET_TRADE_REJECT(e.CommandBuffer);
                return;
            }

            if (e.CommandBuffer[0] == 36)
            {
                GET_TRADE_ACCEPT(e.CommandBuffer);
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
        }
        private void GET_TRADE_ABORT(byte[] buffer)
        {
        }
        private void GET_TRADE_OBJECT(byte[] buffer)
        {
            if (!TheTradeHandler.Gambling)
            {
                return;
            }
            // buffer[11]==1 --> New trade object on the trade partner side
            // buffer[11]==0 --> New trade object on my side
            byte pos = buffer[10];
            int imageid = System.BitConverter.ToInt16(buffer, 3);
            uint quantity = System.BitConverter.ToUInt32(buffer, 5);
            totalCoins += quantity;
            if (imageid != 3)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I only gamble for coins..."));
                TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
            }
            //if (quantity != priceOfTicket)
            //{
            //    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Please put exactly " + priceOfTicket + " coins in the trade window!"));
            //    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
            //}
            //if (totalCoins > priceOfTicket)
            //{
            //    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Please put exactly " + priceOfTicket + " coins in the trade window!"));
            //    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
            //}
        }
        private void REMOVE_TRADE_OBJECT(byte[] buffer)
        {
            // buffer[11]==1 --> New trade object on the trade partner side
            // buffer[11]==0 --> New trade object on my side
            if (TheTradeHandler.Gambling)
            {
                byte pos = buffer[7];
                UInt32 quantity = System.BitConverter.ToUInt32(buffer, 3);
                if (pos == 0)
                {
                    totalCoins -= quantity;
                }
                else
                {
                    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
                }
            }
        }
        private void GET_TRADE_ACCEPT(byte[] buffer)
        {
            if (!TheTradeHandler.Gambling)
            {
                return;
            }
            if (buffer[3] == 0)
            {
                return;
            }
            if (totalCoins != priceOfTicket)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Please put exactly " + priceOfTicket + " coins in the trade window!"));
                TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
            }
            if (!acceptedOnce)
            {
                TheTCPWrapper.Send(CommandCreator.ACCEPT_TRADE());
                acceptedOnce = true;
            }
            else
            {
                TheTCPWrapper.Send(CommandCreator.ACCEPT_TRADE_ENTIRE(false));
                quantityBeforeTrade = (uint)TheInventory.Quantity(1);
                TheInventory.justGambled = true;
                TheInventory.Gambling = false;
                TheInventory.inventoryRequested = true;
            }
        }
        private void GET_TRADE_REJECT(byte[] buffer)
        {

            if (buffer[3] == 0)
            {
                TheLogger.Debug("RX : ME: GET_TRADE_REJECT\n");
                acceptedOnce = false;
            }
            else
            {
                TheLogger.Debug("RX : PARTNER: GET_TRADE_REJECT\n");
                TheTCPWrapper.Send(CommandCreator.REJECT_TRADE());
            }
        }
        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
		{
			string Message = e.Message.ToLower();
			
			if (Message[0]!='#')
			{
				Message = "#" + Message;
			}

			string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#gamble")
			{
                if (!TheMySqlManager.IGamble())
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I don't gamble, sorry!"));
                    return;
                }

                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#gamble", Settings.botid);
                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#gamble", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }


                if (Settings.IsTradeBot == false && TheTradeHandler.storageAvailable == false)
				{
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I am not a trade bot!"));
					return;
				}

                if (TheTradeHandler.claimingPrize)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I can't gamble while you're claiming your prize :P!"));
                    return;
                }

                if (TheTradeHandler.Trading == false || TradeHandler.username.ToLower() != e.username.ToLower())
				{
					TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Please trade with me first!"));
					return;
				}

                if (prizeLevel != "")
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You cannot change gamble levels during a trade, cancel if you want to use a different level!"));
                    return;
                }
                try
				{
                    if (CommandArray.Length != 2)
                    {
                        goto WrongArguments;
                    }
                    prizeLevel = CommandArray[1];
                    if (prizeLevel != "high" && prizeLevel != "low" && prizeLevel != "medium")
                    {
                        goto WrongArguments;
                    }
                    if (TheMySqlManager.checkForPrizes(prizeLevel))
                    {
                        TheMySqlManager.getGambleInfo(prizeLevel, out winningPercent, out priceOfTicket);
                        if (TheTradeHandler.PutOnSale == false && TheTradeHandler.Billance == 0 && TheTradeHandler.PartnerTradeItemsList.Count == 0 && TheTradeHandler.MyItemList.Count == 0)
                        {
                            TheTradeHandler.Gambling = true;
                            TheInventory.Gambling = true;
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Please place " + priceOfTicket + " coins in the trade window and click accept."));
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You must get lower than " + winningPercent + " to win, good luck!"));
                            username = e.username;
                        }
                        else
                        {
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Plese restart the trade. And put no items on the trade before sending #gamble."));
                        }
                    }
                    else
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I have no prizes at the " + prizeLevel + " level. (Try a different level or canel trade.)"));
                    }
				}
				catch
				{
					goto WrongArguments;
				}
			}
			
			return;
					
			WrongArguments:
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"Here is the usage of the #gamble command:"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "#gamble <level>"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "------------------------------------------"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Example: #gamble low"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "#gamble medium"));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "#gamble high"));
				return;
		}

	}
}
