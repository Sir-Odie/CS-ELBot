
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
    /// Description of PMCommandHandler.
    /// </summary>
    public class RetradeCommandHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private HelpCommandHandler TheHelpCommandHandler;
        private MySqlManager TheMySqlManager;
        private ActorHandler TheActorHandler;
        private TradeHandler TheTradeHandler;
        //private bool CommandIsDisabled;

        public RetradeCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, HelpCommandHandler MyHelpCommandHandler, MySqlManager MyMySqlManager, ActorHandler MyActorHandler, TradeHandler MyTradeHandler)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheHelpCommandHandler = MyHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.TheActorHandler = MyActorHandler;
            this.TheTradeHandler = MyTradeHandler;
            //this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#pm",Settings.botid);

            //if (CommandIsDisabled == false)
            {
                TheHelpCommandHandler.AddCommand("#retrade / #r - Reinitialise trade");
                TheHelpCommandHandler.AddCommand("#r - null");
                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
            }
        }

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {
            string Message = e.Message;

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#retrade" || CommandArray[0] == "#r")
            {
                /*
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#pm", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }
                */

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#inv", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                //TheMessageParser.FakeMessage(e.username + " her in order to accept the trade.");
                if (!TheTradeHandler.AmITrading())
                {
                    TheMessageParser.FakeTrade(e.username);
                    //TheTCPWrapper.Send(CommandCreator.TRADE_WITH(TheActorHandler.GetUserIDFromname(e.username)));
                }
                else if (TradeHandler.username == e.username && TheTradeHandler.AmITrading())
                {
                    TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
                    TheMessageParser.FakeTrade(e.username);
                    //TheTCPWrapper.Send(CommandCreator.TRADE_WITH(TheActorHandler.GetUserIDFromname(e.username)));
                }
                else
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "#Retrade will only work when I am not trading with someone else."));
                    //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "#Retrade will only work when I am trading with you."));
                    return;
                }
            }
            return;
        }
    }
}
