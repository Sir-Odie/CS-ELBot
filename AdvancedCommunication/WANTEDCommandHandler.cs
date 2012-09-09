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
    /// description of WANTEDCommandHandler.
    /// </summary>
    public class WANTEDCommandHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private MySqlManager TheMySqlManager;
        ////private bool CommandIsDisabled;
        private HelpCommandHandler TheHelpCommandHandler;
        private Logger TheLogger;
        private TradeHandler TheTradeHandler;
        private Inventory TheInventory;
        private Stats TheStats;
        public WANTEDCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, HelpCommandHandler MyHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler, Inventory MyInventory, Stats MyStats)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheHelpCommandHandler = MyHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.TheLogger = MyLogger;
            this.TheTradeHandler = MyTradeHandler;
            this.TheInventory = MyInventory;
            this.TheStats = MyStats;
            //this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#wanted",Settings.botid);

            //if (CommandIsDisabled == false)
            {
                if (Settings.IsTradeBot == true)
                {
                    TheHelpCommandHandler.AddCommand("#wanted / #w - see what I am buying");
                    TheHelpCommandHandler.AddCommand("#w - null");
                    TheHelpCommandHandler.AddCommand("#want - null");
                    TheHelpCommandHandler.AddCommand("#buying - null");
                }

                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
            }
        }

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {
            int UsedSlots = 0;
            string Message = e.Message.ToLower().Replace("\'", "\\\'").Replace("\"", "\\\"");
            string[] Inv = new string[512];

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#wanted" || CommandArray[0] == "#w" || CommandArray[0] == "#want" || CommandArray[0] == "#buying")
            {
                if (Settings.IsTradeBot == false)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I am not a trade bot!"));
                    return;
                }
                int rank = TheMySqlManager.GetUserRank(e.username, Settings.botid);
                if (rank < TheMySqlManager.GetCommandRank("#wanted", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }

                if (this.TheTradeHandler.AmITrading() && e.username != TradeHandler.username)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I am currently trading, please retry shortly."));
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

                if (CommandArray.Length < 1)
                    goto WrongArguments;

                string filter = "";
                if (CommandArray.Length > 1)
                {
                    //filter = Message.Substring(CommandArray[0].Length);
                    bool firstTime = true;
                    foreach (string filterPart in CommandArray)
                    {
                        if (firstTime)
                        {
                            firstTime = false;
                            continue;
                        }
                        filter += (" " + filterPart);
                    }
                }
                else
                    filter = "";


                int i = 0;
                for (i = 0; i < 512; i++)
                    Inv[i] = "ZZZZZZZZZZZZZZZZ";
                int c = 0;

                char wantedFiller = TheMySqlManager.getWantedFiller();
                bool showZeroPrice = TheMySqlManager.getShowZeroPrice();
		bool advertfilter = TheMySqlManager.advertfilter();

                System.Collections.Hashtable MyWantedItemsList = TheMySqlManager.GetWantedItemList(0);

                System.Collections.ArrayList MyInventoryList = TheInventory.GetInventoryList();
                foreach (Inventory.inventory_item MyInventoryItem in MyInventoryList)
                {
                    if (MyInventoryItem.pos < 36)
                    {
                        if (MyInventoryItem.is_stackable)
                            UsedSlots++;
                        else
                            UsedSlots = UsedSlots + (int)(MyInventoryItem.quantity);
                    }
                }

                // pm all summed up inventory items on sale
                int maxlen = 20 + 29 - Settings.Loginname.Length;
                bool Member = (TheMySqlManager.CheckIfTradeMember(e.username, Settings.botid) == true);
                TheLogger.Debug("Trade Member " + (Member ? "Yes\n" : "No\n"));
                //maximum characters is 64 on 8x6, server is taking up 12 chars, then length of bot name (eg., josi would result in 16 giving max of the rest of the line being 48
                str = "[";
                str = str.PadRight(maxlen + 23, '-');
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                str = "[name";
                str = str.PadRight(maxlen, ' ');
                str += "|        price|     qty";
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                //Draw separator line ----------------
                str = "[";
                str = str.PadRight(maxlen + 23, '-');
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));

                i = 1;
                foreach (TradeHandler.WantedItem MyWantedItem in MyWantedItemsList.Values)
                {
                    if ((MyWantedItem.pricepurchase == 0 && showZeroPrice == false && Member == false) || (advertfilter == true && MyWantedItem.pricepurchase > TheInventory.GetMoneyAmount() && Member == false))
                    {
                        continue;
                    }
                    //i = 1 - i;
                    string msg = "[";
                    int q = 99999, q2;
                    string item = TheMySqlManager.GetKnownItemsname(MyWantedItem.KnownItemsSqlID);
                    msg += item;
                    msg = msg.PadRight(maxlen, (i == 0) ? ' ' : wantedFiller);
                    msg += "|";
                    if (Member)
                        str = MyWantedItem.pricepurchasemembers.ToString();
                    else
                        str = MyWantedItem.pricepurchase.ToString();
                    if (!str.Contains("."))
                        str += ".00gc";
                    else if (str[str.Length - 2] == '.')
                        str += "0gc";
                    else
                        str += "gc";
                    str = str.PadLeft(13, (i == 0) ? ' ' : wantedFiller);
                    msg += str;

                    if (TheInventory.HaveItem(MyWantedItem.KnownItemsSqlID) == -1)
                    {
                        q2 = MyWantedItem.maxquantity;
                        if (MyWantedItem.maxquantity == 0)
                        {
                            str = "No limit".PadLeft(8, (i == 0) ? ' ' : wantedFiller);
                        }
                        else if (q2 > 0)
                        {
                            str = q2.ToString().PadLeft(8, (i == 0) ? ' ' : wantedFiller);
                        }

                    }
                    else
                    {
                        q2 = MyWantedItem.maxquantity - TheInventory.Quantity(MyWantedItem.KnownItemsSqlID);
                        q = q2;
                        if (MyWantedItem.maxquantity == 0)
                        {
                            str = "No limit".PadLeft(8, (i == 0) ? ' ' : wantedFiller);
                        }
                        else if (q2 > 0)
                        {
                            str = q2.ToString().PadLeft(8, (i == 0) ? ' ' : wantedFiller);
                        }
                    }
                    str = "|" + str;
                    msg += str;
                    //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, msg));
                    if (q > 0 || MyWantedItem.maxquantity == 0)
                    {
                        Inv[c++] = msg;
                    }
                }

                int d;

                Array.Sort(Inv);
                i = maxlen + 13 + 12;

                for (d = 0; d < c; d++)
                {
                    if (filter == "" || Inv[d].ToLower().Contains(filter.ToLower().Trim()))
                    {
                        string[] outFields = Inv[d].Split('|');
                        string outString = Inv[d];
                        if (outFields[0].Length > maxlen)
                        {
                            outString = outFields[0].Substring(0, maxlen - 3) + "...";
                            outString =  outString.PadRight(maxlen, ' ') + "|".PadRight(14,' ') + "|".PadRight(9,' ');
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, outString));
                            outString = ("[..." + outFields[0].Substring(maxlen-3).Trim()).PadRight(maxlen, ' ');
                            int count = 0;
                            foreach (string tempString in outFields)
                            {
                                if (count == 0)
                                {
                                    count++;
                                    continue;
                                }
                                outString += "|" + tempString;
                            }
                        }
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, outString));
                    }
                    else 
                    {
                        //for (i = 1; i < CommandArray.Length; i++)
                        {
                            if (Inv[d].ToLower().Contains(filter.ToLower().Trim()))
                            {
                                //I don't think this is ever getting done, from here down...
                                string[] outFields = Inv[d].Split('|');
                                string outString = Inv[d];
                                if (outFields[0].Length > maxlen)
                                {
                                    outString = outFields[0].Substring(0, maxlen - 3) + "...";
                                    outString = outString.PadRight(maxlen, ' ') + "|".PadRight(14, ' ') + "|".PadRight(9, ' ');
                                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, outString));
                                    outString = "[...".PadRight(maxlen, ' ');
                                    foreach (string tempString in outFields)
                                    {
                                        outString += "|" + tempString;
                                    }
                                }

                                //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, Inv[d]));
                                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, outString));
                                i = CommandArray.Length;
                            }
                        }
                    }
                }

                str = "[";
                str = str.PadRight(maxlen + 23, '-');
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));

                int Phys = TheMySqlManager.GetBotPhysqiue(Settings.botid);
                int Coord = TheMySqlManager.GetBotCoordination(Settings.botid);
                int carry = (Phys + Coord) * 10;
                int UsedSpace = TheStats.MyCurrentCarryingAmt;
                int FreeSpace = carry - UsedSpace;
                str = "[";
                str += FreeSpace.ToString() + " EMU Avail|";
                str += (36 - UsedSlots).ToString() + " Open slot(s)";

                if (rank > 89)
                {
                    str += "|" + TheInventory.GetMoneyAmount().ToString() + " gc";
                }

                str = str.PadRight(maxlen + 11 + 12, ' ');
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));


                if (Member)
                {
                    str = "[";
                    str = str.PadRight(maxlen + 11 + 12, '-');
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                    str = "[You have Member rates on purchases";
                    str = str.PadRight(maxlen + 11 + 12, ' ');
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                }
                str = "[";
                str = str.PadRight(maxlen + 11 + 12, '-');
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                //str = "[To sell something to me just trade with me";
                //str = str.PadRight(maxlen + 11 + 12, ' ');
                //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                //str = "[and place the item(s) you want to sell on";
                //str = str.PadRight(maxlen + 11 + 12, ' ');
                //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                //str = "[the trade";
                //str = str.PadRight(maxlen + 11 + 12, ' ');
                //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                //str = "[";
                //str = str.PadRight(maxlen + 11 + 12, '=');
                //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
            }

            return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Here is the usage of the #wanted command:"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[#wanted [name|id]                        "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[-----------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #wanted essence                 "));
            return;
        }
    }
}
