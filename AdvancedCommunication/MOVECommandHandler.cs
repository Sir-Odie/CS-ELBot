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
    public class MOVECommandHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private AdminHelpCommandHandler TheAdminHelpCommandHandler;
        private MySqlManager TheMySqlManager;
        private PMHandler ThePMHandler;
        //private bool CommandIsDisabled;
        private System.Collections.ArrayList CommandArrayList = new System.Collections.ArrayList();

        public MOVECommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, PMHandler MyPMHandler)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.ThePMHandler = MyPMHandler;
            //this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#help",Settings.botid);

            //if (CommandIsDisabled == false)
            {
                TheAdminHelpCommandHandler.AddCommand("#move / #m - Move the bot (very local range)");
                TheAdminHelpCommandHandler.AddCommand("#turnleft / #tl - Rotate the bot left");
                TheAdminHelpCommandHandler.AddCommand("#turnright / #tr - Rotate the bot left");
                ThePMHandler.AddCommand("#tl");
                ThePMHandler.AddCommand("#m");
                ThePMHandler.AddCommand("#tr");

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

            if (CommandArray[0] == "#move" || CommandArray[0] == "#m")
            {
                if (CommandArray.Length < 3)
                    goto WrongArguments;

                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#move", Settings.botid);

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
                if (CommandArray[1].Contains(","))
                {
                    goto WrongArguments;
                }
                int x = int.Parse(CommandArray[1]);
                int y = int.Parse(CommandArray[2]);
                TheTCPWrapper.Send(CommandCreator.MOVE_TO(x, y));
                System.Threading.Thread.Sleep(1000);
                TheTCPWrapper.Send(CommandCreator.LOCATE_ME());

            }
            if (CommandArray[0] == "#turnleft" || CommandArray[0] == "#tl")
            {
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#move", Settings.botid);

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
                if (CommandArray.Length > 2)
                    goto WrongArgumentsTL;
                int i = 0;
                if (CommandArray.Length > 1)
                {
                    i = int.Parse(CommandArray[1]);
                    i = i % 5;
                }
                i++;
                while (i > 0)
                {
//                    TheTCPWrapper.Send(CommandCreator.TURN_LEFT());
                    TheTCPWrapper.Send(CommandCreator.TURN_RIGHT());
                    i--;
                    System.Threading.Thread.Sleep(1000);
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I have " + (i * 45).ToString() + " degrees remaining"));
                }
            }
            if (CommandArray[0] == "#turnright" || CommandArray[0] == "#tr")
            {
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#move", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#tr", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                if (CommandArray.Length > 2)
                    goto WrongArgumentsTR;
                int i = 0;
                if (CommandArray.Length > 1)
                {
                    i = int.Parse(CommandArray[1]);
                    i = i % 5;
                }
                i++;
                while (i > 0)
                {
//                    TheTCPWrapper.Send(CommandCreator.TURN_RIGHT());
                    TheTCPWrapper.Send(CommandCreator.TURN_LEFT());
                    i--;
                    System.Threading.Thread.Sleep(1000);
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "I have " + (i * 45).ToString() + " degrees remaining"));
                }
            }
            return;
        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Here is the usage of the #move command:   "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[#move x y                                 "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #move 192 168                    "));
            return;
        WrongArgumentsTL:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Here is the usage of the #turnleft command:"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[#turnleft (optional turns)                 "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[-------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #turnleft                         "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Turn left 45 degrees                       "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #turnleft 3                       "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Turn left 135 degrees                      "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[nothing or 1 rotates 45 degrees            "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[2 rotates 90 degrees                       "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[3 rotates 135 degrees                      "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[4 rotates 180 degrees                      "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Any more than 4 will stop at 180 degrees   "));
            return;
        WrongArgumentsTR:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Here is the usage of the #turnright command:"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[#turnright (optional turns)                 "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[--------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #turnright                         "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Turn right 45 degrees                       "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #turnright 3                       "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Turn right 135 degrees                      "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[nothing or 1 rotates 45 degrees             "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[2 rotates 90 degrees                        "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[3 rotates 135 degrees                       "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[4 rotates 180 degrees                       "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Any more than 4 will stop at 180 degrees    "));
            return;

        }
    }
}
