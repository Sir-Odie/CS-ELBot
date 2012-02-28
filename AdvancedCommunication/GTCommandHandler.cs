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
    /// description of ACCEPTMECommandHandler.
    /// </summary>
    public class GTCommandHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private MySqlManager TheMySqlManager;
        //private bool CommandIsDisabled;
        private AdminHelpCommandHandler TheAdminHelpCommandHandler;
        private Logger TheLogger;

        public GTCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.TheLogger = MyLogger;
            //this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#kickme", Settings.botid);

            //if (this.CommandIsDisabled == false)
            {
                this.TheAdminHelpCommandHandler.AddCommand("#gt - Provide guild feed to the bot");
                this.TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
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

            if (CommandArray[0].ToLower() == "#gt")
            {
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#gt", Settings.botid);

                if (disabled == true)
                {
                    //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }
                
                if (e.username.ToLower()!="gossip")
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                string name = "",temp = "", guild = "";
                temp = CommandArray[1].Trim();
                if (guild == "")
                    guild = "";
                if (guild.ToLower() == "noexist")
                    return;
                TheMySqlManager.UpdatePlayer(name, guild);
            }
            return;
        }
    }
}
