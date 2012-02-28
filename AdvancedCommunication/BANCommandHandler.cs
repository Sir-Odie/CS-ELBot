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
    /// description of BANCommandHandler.
    /// </summary>
    public class BANCommandHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private MySqlManager TheMySqlManager;
        private AdminHelpCommandHandler TheAdminHelpCommandHandler;
        private Logger TheLogger;
        private HelpCommandHandler TheHelpCommandHandler;

        public BANCommandHandler(HelpCommandHandler MyHelpCommandHandler, TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdminHelpCommandHandler MyAminHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheAdminHelpCommandHandler = MyAminHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.TheLogger = MyLogger;
            this.TheHelpCommandHandler = MyHelpCommandHandler;
            //this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#BAN",Settings.botid);

            //if (CommandIsDisabled == false)
            {
                TheAdminHelpCommandHandler.AddCommand("#ban - ban a player");
                TheAdminHelpCommandHandler.AddCommand("#unban - unban a banned player");
                TheAdminHelpCommandHandler.AddCommand("#banguild / #bg - ban a guild");
                TheAdminHelpCommandHandler.AddCommand("#unbanguild / #ug - unban a banned guild");
                TheAdminHelpCommandHandler.AddCommand("#prefer - give a player preferential rates");
                TheAdminHelpCommandHandler.AddCommand("#preferguild / #pg - give a guild preferential rates");
                TheAdminHelpCommandHandler.AddCommand("#rank - change player's admin level rank");
                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
                TheHelpCommandHandler.AddCommand("#bg - null");
                TheHelpCommandHandler.AddCommand("#ug - null");
                TheHelpCommandHandler.AddCommand("#pg - null");
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

            if (CommandArray[0].ToLower() == "#ban" 
                || CommandArray[0].ToLower() == "#unban"
                || CommandArray[0].ToLower() == "#banguild"
                || CommandArray[0].ToLower() == "#bg"
                || CommandArray[0].ToLower() == "#unbanguild"
                || CommandArray[0].ToLower() == "#ug"
                || CommandArray[0].ToLower() == "#prefer"
                || CommandArray[0].ToLower() == "#preferguild"
                || CommandArray[0].ToLower() == "#pg"
                || CommandArray[0].ToLower() == "#rank"
                )
            {
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#ban", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }
                
                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank(CommandArray[0], Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                if (CommandArray[0].ToLower() == "#rank")
                {
                    if (CommandArray.Length != 3)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "The " + CommandArray[0].ToLower() + " command requires two parameters, the name and the rank."));
                        return;
                    }
                    TheMySqlManager.UpdatePlayer(CommandArray[1].ToLower(),int.Parse(CommandArray[2]), e.username);
                    return;

                }
                else if (CommandArray.Length != 2)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "The " + CommandArray[0].ToLower() + " command requires one parameter, the name."));
                    return;
                }
                string name=CommandArray[1];
                if (CommandArray[0] == "#ban")
                {
                    TheMySqlManager.UpdatePlayer(name.ToLower(), -1,e.username);
                }
                else if (CommandArray[0] == "#unban")
                {
                    TheMySqlManager.UpdatePlayer(name.ToLower(), 0,e.username);
                }
                else if (CommandArray[0] == "#prefer")
                {
                    TheMySqlManager.UpdatePlayer(name.ToLower(), 1,e.username);
                }
                else if (CommandArray[0] == "#banguild" || CommandArray[0] == "#bg")
                {
                    TheMySqlManager.BanGuild(CommandArray[1], -1,e.username);
                }
                else if (CommandArray[0] == "#unbanguild" || CommandArray[0] == "#ug")
                {
                    TheMySqlManager.BanGuild(CommandArray[1], 0,e.username);
                }
                else if (CommandArray[0] == "#preferguild" || CommandArray[0] == "#pg")
                {
                    TheMySqlManager.BanGuild(CommandArray[1], 1,e.username);
                }
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Command executed."));
            }

            return;
        }
    }
}
