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
    /// description of GiveCommandHandler.
    /// </summary>
    public class DESCRIBECommandHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private MySqlManager TheMySqlManager;
        ////private bool CommandIsDisabled;
//        private HelpCommandHandler TheHelpCommandHandler;
        private AdvHelpCommandHandler TheAdvHelpCommandHandler; 
        private Logger TheLogger;
        private TradeHandler TheTradeHandler;
        private PMHandler ThePMHandler;

        public DESCRIBECommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdvHelpCommandHandler MyHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler, PMHandler MyPMHandler)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheAdvHelpCommandHandler = MyHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.TheLogger = MyLogger;
            this.TheTradeHandler = MyTradeHandler;
            this.ThePMHandler = MyPMHandler;

            //if (CommandIsDisabled == false)
            {
                TheAdvHelpCommandHandler.AddCommand("#describe / #d - describe an item");
                ThePMHandler.AddCommand("#d");
//                TheAdvHelpCommandHandler.AddCommand("#d - null");
                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
            }
        }

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {
            int SQLID = 0;
            int nameToID = -1;

            string Message = e.Message.ToLower().Replace("\'","\\\'").Replace("\"","\\\"");

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#describe" || (CommandArray[0] == "#d" && CommandArray.Length>1))
            {
                if (Settings.IsTradeBot == false)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, I am not a trade bot!"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#d", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }

                try
                {
                    if (CommandArray.Length < 1)
                    {
                        goto WrongArguments;
                    }
                    string str = "",str2="";
                    int i;
                    for (i = 1; i < CommandArray.Length; i++)
                    {
                        str += CommandArray[i] + " ";
                    }
                    str = str.Trim();
                    nameToID = TheMySqlManager.GetItemID(str,false);
                    if (nameToID == -1)
                    {
                        if (str[0] < '0' || str[0] > '9')
                        {
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "description of " + str + " not found"));
                            return;
                        }
                        SQLID = int.Parse(CommandArray[1]);
                    }
                    if (nameToID != -1)
                    {
                        SQLID = nameToID;
                    }

                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Description of " + TheMySqlManager.GetKnownItemsname(SQLID) + " (ID " + SQLID.ToString() + ")"));
                    str = TheMySqlManager.GetDescription(SQLID);
                    i=str.Length;
                    int v = 158 - e.username.Length;
                    if (e.username.Length + i > v)
                    {
                        while (v>0 && str[v] != ' ')
                            v--;

                        str2 = str.Substring(0, v);
                        v++;
                        if (str2.Length > 0)
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str2.Trim()));
                        str = str.Substring(v);
                    }
                    str = str.Trim();
                    if (str.Length > 0)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                    }
                    str = TheMySqlManager.GetDescriptionExtra(SQLID).Trim();
                    if (str.Length > 0)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                    }
                }
                catch
                {
                    goto WrongArguments;
                }
            }

            return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|-------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Here is the usage of the #describe command:"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|#describe <name or itemid>                 "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|-------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #describe 2                       "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #describe Pickaxe                 "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|-------------------------------------------"));
            return;
        }
    }
}
