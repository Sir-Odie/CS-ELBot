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
    /// description of ABOUTCommandHandler.
    /// </summary>
    public class AFKCommandHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private MySqlManager TheMySqlManager;
        private HelpCommandHandler TheHelpCommandHandler;
        private Logger TheLogger;

        public AFKCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, HelpCommandHandler MyHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheHelpCommandHandler = MyHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.TheLogger = MyLogger;

            TheHelpCommandHandler.AddCommand("#automessage: - null");
            TheHelpCommandHandler.AddCommand("#automessage - null");
            TheHelpCommandHandler.AddCommand("#afk - null");
            TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
        }

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {

        }
    }
}
