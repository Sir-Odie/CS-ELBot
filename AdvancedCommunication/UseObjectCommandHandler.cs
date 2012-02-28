// Eternal Lands Bot
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
    public class UseObject
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private AdminHelpCommandHandler TheAdminHelpCommandHandler;
        private MySqlManager TheMySqlManager;
        private PMHandler ThePMHandler;
        //private bool CommandIsDisabled;
        private System.Collections.ArrayList CommandArrayList = new System.Collections.ArrayList();

        public UseObject(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, PMHandler MyPMHandler)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.ThePMHandler = MyPMHandler;
            //this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#help",Settings.botid);

            //if (CommandIsDisabled == false)
            {
                TheAdminHelpCommandHandler.AddCommand("#useobject - Use a map object(like a banner)");

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

            if (CommandArray[0] == "#useobject")
            {
                if (CommandArray.Length < 2)
                    goto WrongArguments;

                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#useobject", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#useobject", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                if (CommandArray[1].Contains(","))
                {
                    goto WrongArguments;
                }
                int objectid = int.Parse(CommandArray[1]);
                TheTCPWrapper.Send(CommandCreator.USE_MAP_OBJECT((uint)objectid, -1));

            }
            return;
        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Here is the usage of the #move command:   "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[#useobject x                              "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #useobject 520                   "));
            return;
        }
    }
}