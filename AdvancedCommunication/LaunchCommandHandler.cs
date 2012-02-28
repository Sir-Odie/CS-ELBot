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
    public class LaunchCommandHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private MySqlManager TheMySqlManager;
        ////private bool CommandIsDisabled;
        private AdminHelpCommandHandler TheAdminHelpCommandHandler;
        private Logger TheLogger;
        private TradeHandler TheTradeHandler;
        private Inventory TheInventory;

        public LaunchCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger, TradeHandler MyTradeHandler, Inventory MyInventory)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.TheLogger = MyLogger;
            this.TheTradeHandler = MyTradeHandler;
            this.TheInventory = MyInventory;
            TheAdminHelpCommandHandler.AddCommand("#launch - make me launch another bot I know about");
            TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
        }

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {

            string botName = "";
            string Message = e.Message.ToLower().Replace("\'", "\\\'").Replace("\"", "\\\"");

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#launch")
            {
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#launch", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#launch", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                //if (TheTradeHandler.AmITrading())
                //{
                //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I am currently trading, please retry shortly."));
                //    return;
                //}

                if (CommandArray.Length < 2)
                {
                    goto WrongArguments;
                }
                //lanuch the bot here
                botName = CommandArray[1];
                if (!TheMySqlManager.validLaunch(botName, e.username))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to launch this bot!"));
                    return;
                }
                string connectionStatus = TheMySqlManager.getConnectionStatus(botName);

                //botToLaunch.StartInfo.FileName = "C:/Users/Jomama/Documents/testbots/" + botName + "/cs-elbot.exe";
                if (connectionStatus == "ALREADY CONNECTED")
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[It appears that " + botName + " is already logged on!"));
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[If " + botName + " isn't really logged on, please contact your bot administrator!"));
                    return;
                }

                if (connectionStatus == "ALREADY PENDING")
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[" + botName + " is already pending launch!"));
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[If " + botName + " isn't logged on soon, please contact your bot administrator!"));
                    return;
                }


                try
                {
                    TheMySqlManager.ImPending(botName);
//                    botToLaunch.Start();
                }
                catch
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[File not found, try again!"));
                    return;
                }
                finally
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[" + botName + " will launch shortly!"));
                }

            }

            return;

        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|-----------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Here is the usage of the #launch command:"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|#launch <botname>                        "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|-----------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|Example: #launch Josi                    "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "|-----------------------------------------"));
            return;
        }
    }
}
	

