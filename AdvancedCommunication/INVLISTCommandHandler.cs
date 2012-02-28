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
	/// description of INVLISTHandler.
	/// </summary>
	public class INVLISTCommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		////private bool CommandIsDisabled;
		private AdvHelpCommandHandler TheAdvHelpCommandHandler;
		private Inventory TheInventory;
        private TradeHandler TheTradeHandler;
        private Stats TheStats;
        private HelpCommandHandler TheHelpCommandHandler;

        public INVLISTCommandHandler(HelpCommandHandler MyHelpCommandHandler, TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdvHelpCommandHandler MyAdvHelpCommandHandler, MySqlManager MyMySqlManager, Inventory MyInventory, TradeHandler MyTradeHandler, Stats MyStats)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheAdvHelpCommandHandler = MyAdvHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheInventory = MyInventory;
            this.TheTradeHandler = MyTradeHandler;
            this.TheStats = MyStats;
            this.TheHelpCommandHandler = MyHelpCommandHandler;
			//this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#invlist",Settings.botid);
			
			//if (CommandIsDisabled == false)
			{
//                if (Settings.IsTradeBot == true)
                {
                    TheAdvHelpCommandHandler.AddCommand("#invlist / #il - lists my entire inventory");
                    TheHelpCommandHandler.AddCommand("#il - null");
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

            if (CommandArray[0] == "#invlist" || CommandArray[0] == "#il")
            {
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#invlist", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                int rank = TheMySqlManager.GetUserRank(e.username, Settings.botid);
                if (rank < TheMySqlManager.GetCommandRank("#invlist", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }

                if (this.TheTradeHandler.AmITrading() && e.username != TradeHandler.username)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I am currently trading, please retry shortly."));
                    return;
                }

                char invFiller = TheMySqlManager.getInvFiller();

                string str1 = "", str2 = "";
                if (TheInventory.GettingInventoryItems == true)
                {
                    str2 = "I am building my inventory list, please try again in a few seconds";
                    str1 = str1.PadRight(str2.Length, '=');
                    str1 = "[" + str1;
                    str2 = "[" + str2;
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str1));
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str2));
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str1));

                    return;
                }
                if (CommandArray.Length < 1)
                    goto WrongArguments;

                System.Collections.ArrayList MyInventoryList = TheInventory.GetInventoryList();
                System.Collections.Hashtable MySellingItemsList = TheMySqlManager.GetSellingItemList(0);

                // sum up all inventory items if the items are on sale
                int i = 0;
                for (i = 0; i < 512; i++)
                    Inv[i] = "ZZZZZZZZZZZZZZZZ";
                int c = 0;

                System.Collections.SortedList TheInventoryList = new System.Collections.SortedList();
                System.Collections.SortedList TheWornList = new System.Collections.SortedList();
                int ii = 0;
                foreach (Inventory.inventory_item MyInventoryItem in MyInventoryList)
                {
                    ii++;
                    if (TheInventoryList.Contains(MyInventoryItem.SqlID) && MyInventoryItem.pos < 36)
                    {
//                        if (MyInventoryItem.pos < 36)
                        {
                            Inventory.inventory_item TempInventoryItem = (Inventory.inventory_item)TheInventoryList[MyInventoryItem.SqlID];
                            TempInventoryItem.quantity += MyInventoryItem.quantity;
                            TheInventoryList[MyInventoryItem.SqlID] = TempInventoryItem;
                        }
                    }
                    else
                    {
                        if (MyInventoryItem.pos < 36)
                        {
                            TheInventoryList.Add(MyInventoryItem.SqlID, MyInventoryItem);
                        }
                    }
                    if (MyInventoryItem.pos > 35 && MyInventoryItem.pos < 44)
                    {
                        TheWornList.Add(MyInventoryItem.SqlID, MyInventoryItem);
                    }
                }
                int maxlen = 4;
                foreach (Inventory.inventory_item MyInventoryItem in TheInventoryList.Values)
                {
//                    if (maxlen < MyInventoryItem.name.Length && MyInventoryItem.pos < 44)
                    if (maxlen < MyInventoryItem.name.Length && MyInventoryItem.pos < 36)
                    {
                        maxlen = MyInventoryItem.name.Length;
                    }
                }
                foreach (Inventory.inventory_item MyInventoryItem in TheWornList.Values)
                {
                    if (maxlen <= MyInventoryItem.name.Length + 3)
                        maxlen = MyInventoryItem.name.Length + 3;
                }
                str2 = "";
                str1= "[";
                str1= str1.PadRight(maxlen + 16, '-');
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str1));
                str1= "[ id |";
                str2 = "quantity";
                str2 = str2.PadRight(9, ' ')+"|";
                str1+= str2;
                str2 = "name";
                str2 = str2.PadRight(maxlen, ' ');
                str1= str1+ str2;
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str1));
                str1= "[";
                str1= str1.PadRight(maxlen + 16, '-');
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str1));
                // pm all summed up inventory items on sale
                c=0;
                i = 1;
                foreach (Inventory.inventory_item MyInventoryItem in TheWornList.Values)
                {
                    if (MyInventoryItem.pos < 44)
                    {
                        if (MyInventoryItem.pos < 36)
                        {
                            if (MyInventoryItem.is_stackable)
                                UsedSlots++;
                            else
                                UsedSlots = UsedSlots + (int)(MyInventoryItem.quantity);
                        }
                        str1 = "";
                        str2 = "";
                        str2 += MyInventoryItem.SqlID.ToString();
                        str2 = str2.PadLeft(4, i == 0 ? ' ' : invFiller);
                        str2 = "[" + str2 + "|";
                        str1 = MyInventoryItem.quantity.ToString();
                        str1 = str1.PadLeft(9, i == 0 ? ' ' : invFiller);
                        str2 += str1 + "|";
                        if (MyInventoryItem.pos > 35)
                            str1 = "{E}" + MyInventoryItem.name;
                        else
                            str1 = MyInventoryItem.name;
                        str1 = str1.PadRight(maxlen, i == 0 ? ' ' : invFiller);
                        str1 = str2 + str1;
                        //TradeHandler.SellingItem MySellingItem = (TradeHandler.SellingItem)(MySellingItemsList[MyInventoryItem.SqlID]);
                        //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str1));
                        Inv[c++] = str1;
                        //i = 1 - i;
                    }
                }
                foreach (Inventory.inventory_item MyInventoryItem in TheInventoryList.Values)
                {
                    if (MyInventoryItem.pos < 44)
                    {
                        if (MyInventoryItem.pos < 36)
                        {
                            if (MyInventoryItem.is_stackable)
                                UsedSlots++;
                            else
                                UsedSlots = UsedSlots + (int)(MyInventoryItem.quantity);
                        }
                        str1 = "";
                        str2 = "";
                        str2 += MyInventoryItem.SqlID.ToString();
                        str2 = str2.PadLeft(4, i == 0 ? ' ' : invFiller);
                        str2 = "[" + str2 + "|";
                        str1 = MyInventoryItem.quantity.ToString();
                        str1 = str1.PadLeft(9, i == 0 ? ' ' : invFiller);
                        str2 += str1+"|";
                        if (MyInventoryItem.pos > 35)
                            str1 = "{E}" + MyInventoryItem.name;
                        else
                            str1 = MyInventoryItem.name;
                        str1 = str1.PadRight(maxlen, i == 0 ? ' ' : invFiller);
                        str1 = str2 + str1;
                        //TradeHandler.SellingItem MySellingItem = (TradeHandler.SellingItem)(MySellingItemsList[MyInventoryItem.SqlID]);
                        //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str1));
                        Inv[c++] = str1;
                        //i = 1 - i;
                    }
                }
                int d;
                string str;
                for (d = 0; d < c; d++)
                {
                    str = Inv[d].Substring(16) + Inv[d].Substring(0, 16);
                    Inv[d] = str;
                }
                Array.Sort(Inv);
                i = maxlen + 13;

                string filter = "";
                if (CommandArray.Length > 1)
                {
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
                    //filter = Message.Substring(CommandArray[0].Length);
                }
                else
                    filter = "";

                //for (d = c - 1; d >= 0; d--)
                for (d = 0; d <c; d++)
                {
                    i = Inv[d].Length - 16;
                    str = Inv[d].Substring(i);
                    str += Inv[d].Substring(0, i);
                    Inv[d] = str.Substring(str.Length - 16);
                    if (filter == "" || str.ToLower().Contains(filter.ToLower().Trim()))
                    {
                        //string[] outFields = Inv[d].Split('|');
                        //string outString = Inv[d];
                        //if (outFields[0].Length > maxlen)
                        //{
                        //    outString = outFields[0].Substring(0, maxlen - 3) + "...";
                        //    outString = outString.PadRight(maxlen, ' ') + "|".PadRight(14, ' ') + "|".PadRight(9, ' ');
                        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, outString));
                        //    outString = "[...".PadRight(maxlen, ' ');
                        //    foreach (string tempString in outFields)
                        //    {
                        //        outString += "|" + tempString;
                        //    }
                        //}
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                    }
                    else
                    {
                        //for (i = 1; i < CommandArray.Length; i++)
                        {
                            if (str.ToLower().Contains(filter.ToLower().Trim()))
                            {
                                //string[] outFields = Inv[d].Split('|');
                                //string outString = Inv[d];
                                //if (outFields[0].Length > maxlen)
                                //{
                                //    outString = outFields[0].Substring(0, maxlen - 3) + "...";
                                //    outString = outString.PadRight(maxlen, ' ') + "|".PadRight(14, ' ') + "|".PadRight(9, ' ');
                                //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, outString));
                                //    outString = "[...".PadRight(maxlen, ' ');
                                //    foreach (string tempString in outFields)
                                //    {
                                //        outString += "|" + tempString;
                                //    }
                                //}
                                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                                i = CommandArray.Length;
                            }
                        }
                    }
                }

                int Phys = TheMySqlManager.GetBotPhysqiue(Settings.botid);
                int Coord = TheMySqlManager.GetBotCoordination(Settings.botid);
                int carry = (Phys + Coord) * 10;
                int UsedSpace = TheStats.MyCurrentCarryingAmt;
                int FreeSpace = carry - UsedSpace;
                str1= "[";
                str1= str1.PadRight(maxlen + 16, '-');
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str1));
                str1 = "[";
                str1 += FreeSpace.ToString() + " EMU Avail|";
                str1 += (36 - UsedSlots).ToString() + " Open slot(s)";
                if (rank > 89)
                {
                    str1 += "|"+TheInventory.GetMoneyAmount().ToString() + " gc";
                }

                str1 = str1.PadRight(maxlen + 16, ' ');
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str1));

                str1= "[";
                str1= str1.PadRight(maxlen + 16, '-');
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str1));

                return;
            }
            return;
					
			WrongArguments:
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"[Here is the usage of the #invlist command:"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"[#invlist                                  "));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"[------------------------------------------"));
				TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username,"[Example: #invlist                         "));
				return;
        }
	}
}
