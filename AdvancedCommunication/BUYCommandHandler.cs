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
	/// description of BUYCommandHandler.
	/// </summary>
	public class BUYCommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		////private bool CommandIsDisabled;
		private HelpCommandHandler TheHelpCommandHandler;
		private Logger TheLogger;
		private TradeHandler TheTradeHandler;
		
		public BUYCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser,HelpCommandHandler MyHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheHelpCommandHandler = MyHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheLogger = MyLogger;
			this.TheTradeHandler = MyTradeHandler;
			//this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#buy",Settings.botid);
			
			//if (CommandIsDisabled == false)
			{
                if (Settings.IsTradeBot == true)
                {
                    TheHelpCommandHandler.AddCommand("#buy / #b - buy something from me");
                    TheHelpCommandHandler.AddCommand("#b - null");
                    TheHelpCommandHandler.AddCommand("#byt - null");
                }
                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
			}
		}

        //private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        //{
        //    uint quantity = 0;
        //    int SQLID = 0;
        //    int nameToID = -1;
        //    uint ItemsPlacedOnTrade = 0;

        //    string Message = e.Message.ToLower();

        //    if (Message[0] != '#')
        //    {
        //        Message = "#" + Message;
        //    }

        //    string[] CommandArray = Message.Split(' ');

        //    if (CommandArray[0] == "#buy" || CommandArray[0] == "#b" || CommandArray[0] == "#byt")
        //    {
        //        if (Settings.IsTradeBot == false)
        //        {
        //            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I am not a trade bot!"));
        //            return;
        //        }

        //        bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#buy", Settings.botid);

        //        if (disabled == true)
        //        {
        //            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
        //            return;
        //        }

        //        if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#buy", Settings.botid))
        //        {
        //            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
        //            return;
        //        }

        //        if (TheTradeHandler.Trading == false || TradeHandler.username.ToLower() != e.username.ToLower())
        //        {
        //            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Please trade with me first!"));
        //            return;
        //        }

        //        try
        //        {
        //            if (CommandArray.Length < 2)
        //            {
        //                goto WrongArguments;
        //            }
        //            if (CommandArray.Length < 3)
        //            {
        //                quantity = 1;
        //                nameToID = TheMySqlManager.GetItemID(CommandArray[1], Settings.botid, false);
        //                if (nameToID != -1)
        //                {
        //                    SQLID = nameToID;
        //                }
        //                if (nameToID == -1)
        //                {
        //                    SQLID = int.Parse(CommandArray[1]);
        //                }
        //            }
        //            if (CommandArray.Length > 2)
        //            {
        //                string str = "";
        //                int i;
        //                if (!uint.TryParse(CommandArray[1], out quantity))
        //                {
        //                    quantity = 1;
        //                    for (i = 1; i < CommandArray.Length; i++)
        //                    {
        //                        str += CommandArray[i] + " ";
        //                    }
        //                }
        //                else
        //                {
        //                    for (i = 2; i < CommandArray.Length; i++)
        //                    {
        //                        str += CommandArray[i] + " ";
        //                    }
        //                }
        //                str = str.Trim();
        //                nameToID = TheMySqlManager.GetItemID(str, Settings.botid,false);
        //                if (nameToID == -1)
        //                {
        //                    SQLID = int.Parse(CommandArray[2]);
        //                }
        //                if (nameToID != -1)
        //                {
        //                    SQLID = nameToID;
        //                }
        //            }
        //        }
        //        catch
        //        {
        //            goto WrongArguments;
        //        }
        //        if (TheTradeHandler.Donating == true)
        //        {
        //            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I can't sell you items while donating. Please start a new trade session."));
        //            return;
        //        }

        //        System.Collections.Hashtable MySellingItemList = TheMySqlManager.GetSellingItemList(0);

        //        if (MySellingItemList.Contains(SQLID))
        //        {
        //            ItemsPlacedOnTrade = TheTradeHandler.PutItemsOnTrade(SQLID, quantity);
        //            TradeHandler.SellingItem MySellingItem = (TradeHandler.SellingItem)MySellingItemList[SQLID];
        //            if (ItemsPlacedOnTrade > 0)
        //            {
        //                 Check if the user is member if the trade alliance
        //                decimal temp = 0;
        //                if (TheMySqlManager.CheckIfTradeMember(e.username, Settings.botid) == true)
        //                {
        //                    TheTradeHandler.AddTrade(SQLID, MySellingItem.pricesalemembers, ItemsPlacedOnTrade, "sold to");
        //                    temp = (decimal)MySellingItem.pricesalemembers * ItemsPlacedOnTrade;
        //                    TheTradeHandler.ChangeBillance(-(System.Convert.ToInt32(System.Math.Ceiling(temp))));
        //                }
        //                else
        //                {
        //                    TheTradeHandler.AddTrade(SQLID, MySellingItem.pricesale, ItemsPlacedOnTrade, "sold to");
        //                    temp = (decimal)MySellingItem.pricesale * ItemsPlacedOnTrade;
        //                    TheTradeHandler.ChangeBillance(-((System.Convert.ToInt32(System.Math.Ceiling(temp)))));
        //                }
        //            }
        //        }


        //        if (ItemsPlacedOnTrade < quantity)
        //        {
        //            if (ItemsPlacedOnTrade == 0)
        //            {
        //                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I don't have this item!"));
        //            }
        //            else
        //            {
        //                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I only have " + ItemsPlacedOnTrade.ToString() + " of this item!"));
        //            }
        //        }
        //    }

        //    return;

        //WrongArguments:
        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Here is the usage of the #buy command: "));
        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|#buy <quantity> <name or itemid>       "));
        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #buy 12 2                     "));
        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #buy 1 Pickaxe                "));
        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
        //    return;
        //}
        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {
            uint quantity = 0;
            int SQLID = 0;
            int nameToID = -1;
            uint ItemsPlacedOnTrade = 0;

            string Message = e.Message.ToLower();

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#buy" || CommandArray[0] == "#b" || CommandArray[0] == "#byt")
            {
                if (Settings.IsTradeBot == false)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I am not a trade bot!"));
                    return;
                }

                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#buy", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#buy", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }

                if (TheTradeHandler.Trading == false || TradeHandler.username.ToLower() != e.username.ToLower())
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Please trade with me first!"));
                    return;
                }

                if (TheTradeHandler.AcceptStateMe > 0)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You cannont issue more buy commands once I've partially accepted the trade!"));
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
                        nameToID = TheMySqlManager.GetItemID(CommandArray[1], Settings.botid, false);
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
                        string str = "";
                        int i;
                        if (!uint.TryParse(CommandArray[1], out quantity))
                        {
                            quantity = 1;
                            for (i = 1; i < CommandArray.Length; i++)
                            {
                                str += CommandArray[i] + " ";
                            }
                        }
                        else
                        {
                            for (i = 2; i < CommandArray.Length; i++)
                            {
                                str += CommandArray[i] + " ";
                            }
                        }
                        str = str.Trim();
                        nameToID = TheMySqlManager.GetItemID(str, Settings.botid, false);
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
                if (TheTradeHandler.Donating == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I can't sell you items while donating. Please start a new trade session."));
                    return;
                }

                System.Collections.Hashtable MySellingItemList = TheMySqlManager.GetSellingItemList(0);
                bool itemFound = false;
                foreach (int itemId in MySellingItemList.Keys)
                {
                    if (SQLID == itemId)
                    {
                        itemFound = true;
                    }
                }

                if (itemFound)
                {
                    TheTradeHandler.totalCalculated = false;
                    ItemsPlacedOnTrade = TheTradeHandler.PutItemsOnTrade(SQLID, quantity);
                    TradeHandler.SellingItem MySellingItem = (TradeHandler.SellingItem)MySellingItemList[SQLID];
                    if (ItemsPlacedOnTrade > 0)
                    {
                        // Check if the user is member if the trade alliance
                        //decimal temp = 0;
                        if (TheMySqlManager.CheckIfTradeMember(e.username, Settings.botid) == true)
                        {
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I sell " + TheMySqlManager.GetKnownItemsname(MySellingItem.KnownItemsSqlID) + " for " + MySellingItem.pricesalemembers + "gc each."));
                            TheTradeHandler.AddTrade(SQLID, MySellingItem.pricesalemembers, ItemsPlacedOnTrade, "sold to");
                            //temp = (decimal)MySellingItem.pricesalemembers * ItemsPlacedOnTrade;
                            //TheTradeHandler.ChangeBillance(-(System.Convert.ToInt32(System.Math.Ceiling(temp))));
                        }
                        else
                        {
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I sell " + TheMySqlManager.GetKnownItemsname(MySellingItem.KnownItemsSqlID) + " for " + MySellingItem.pricesale + "gc each."));
                            TheTradeHandler.AddTrade(SQLID, MySellingItem.pricesale, ItemsPlacedOnTrade, "sold to");
                            //temp = (decimal)MySellingItem.pricesale * ItemsPlacedOnTrade;
                            //TheTradeHandler.ChangeBillance(-((System.Convert.ToInt32(System.Math.Ceiling(temp)))));
                        }
                    }
                }


                //if (ItemsPlacedOnTrade < quantity)
                {
                    if (ItemsPlacedOnTrade == 0)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I don't have this item!"));
                    }
                    else if (ItemsPlacedOnTrade < quantity)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I only have " + ItemsPlacedOnTrade.ToString() + " of this item!"));
                    }
                }
            }

            return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I don't sell this item or invalid syntax"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Here is the usage of the #buy command: "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|#buy <quantity> <name or itemid>       "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #buy 12 2                     "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #buy 1 Pickaxe                "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|---------------------------------------"));
            return;
        }

	}
}
