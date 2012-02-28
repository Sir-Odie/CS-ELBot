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
    /// Description of help_command_handler.
    /// </summary>
    public class SitStandCommandHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private AdminHelpCommandHandler TheAdminHelpCommandHandler;
        private MySqlManager TheMySqlManager;
        private PMHandler ThePMHandler;
        //private bool CommandIsDisabled;
        private System.Collections.ArrayList CommandArrayList = new System.Collections.ArrayList();

        public SitStandCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, PMHandler MyPMHandler)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.ThePMHandler = MyPMHandler;
            //this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#help",Settings.botid);

            //if (CommandIsDisabled == false)
            {
                TheAdminHelpCommandHandler.AddCommand("#sit - Make me sit down");
                TheAdminHelpCommandHandler.AddCommand("#stand - Make me stand up");
                ThePMHandler.AddCommand("#sit");
                ThePMHandler.AddCommand("#stand");

                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
            }
        }

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {
            string Message = e.Message.ToLower();

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#sit")
            {
//                if (CommandArray.Length < 3)
//                    goto WrongArguments;
//
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#sit", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#move", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                TheTCPWrapper.Send(CommandCreator.SIT_DOWN(true));

            }
            if (CommandArray[0] == "#stand")
            {
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#stand", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#tl", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                TheTCPWrapper.Send(CommandCreator.SIT_DOWN(false));
            }
            return;
        //WrongArguments:
        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Here is the usage of the #move command:   "));
        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[#move x y                                 "));
        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[------------------------------------------"));
        //    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #move 192 168                    "));
        //    return;
        }
    }
}
