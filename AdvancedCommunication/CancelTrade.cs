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
using System.Diagnostics;

namespace cs_elbot.AdvancedCommunication
{
    /// <summary>
    /// description of DropCommandHandler.
    /// </summary>
    public class CancelTradeCommandHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private MySqlManager TheMySqlManager;
        ////private bool CommandIsDisabled;
        private AdminHelpCommandHandler TheAdminHelpCommandHandler;
        private Logger TheLogger;
        private TradeHandler TheTradeHandler;

        public CancelTradeCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheMySqlManager = MyMySqlManager;
            this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
            this.TheLogger = MyLogger;
            this.TheTradeHandler = MyTradeHandler;
            TheAdminHelpCommandHandler.AddCommand("#canceltrade - cancels trade and resets trade indicator");
            TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
        }

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {

            string Message = e.Message.ToLower().Replace("\'", "\\\'").Replace("\"", "\\\"");

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#canceltrade")
            {
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#canceltrade", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("canceltrade", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "The trade was cancelled!"));
                return;
            }
        }
    }
}
	

