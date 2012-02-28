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
    public class KICKMECommandHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private MySqlManager TheMySqlManager;
        //private bool CommandIsDisabled;
        private AdvHelpCommandHandler TheAdvHelpCommandHandler;
        private Logger TheLogger;

        public KICKMECommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdvHelpCommandHandler MyAdvHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheAdvHelpCommandHandler = MyAdvHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.TheLogger = MyLogger;
            //this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#kickme", Settings.botid);

            //if (this.CommandIsDisabled == false)
            {
                this.TheAdvHelpCommandHandler.AddCommand("#kickme - kicks you out of the guild");
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

            if (CommandArray[0].ToLower() == "#kickme")
            {
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#kickme", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#kickme", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                
                if (TheMySqlManager.CheckIfGuildMember(e.username, Settings.botid) == false)
                {
                	TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Sorry, but you are not a guild member."));
                    return;
                }

                Message = "#remove " + e.username;

                TheTCPWrapper.Send(CommandCreator.RAW_TEXT(Message));
            }

            return;
        }
    }
}
