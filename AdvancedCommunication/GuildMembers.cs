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
    /// description of MessageParser.
    /// </summary>
    public class GuildTextHandler
    {
        private BasicCommunication.RawText TheRawText;
        private Logger TheLogger;
        private MySqlManager TheMySqlManager;
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;

        public GuildTextHandler(BasicCommunication.RawText MyRawText, TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, MySqlManager MyMySqlManager, Logger MyLogger)
        {
            this.TheLogger = MyLogger;
            this.TheMessageParser = MyMessageParser;
            this.TheRawText = MyRawText;
            this.TheMySqlManager = MyMySqlManager;
            this.TheTCPWrapper = MyTCPWrapper;
            TheMessageParser.Get_GlobalMessage += new BasicCommunication.MessageParser.Get_GlobalMessageEventHandler(Got_GlobalMessage);
        }

        private void Got_GlobalMessage(object sender, BasicCommunication.MessageParser.Get_GlobalMessageEventArgs e)
        {
            string Message = e.Message;
            string username = "";

            if (Message.StartsWith("You kicked "))
            {
                string[] CommandArray = Message.Split(' ');
                
                username = CommandArray[2];
                
                TheLogger.Log(username + " has been removed from the guild");

                TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#gm " + System.Convert.ToChar(TheMySqlManager.playerleftguildcolor(Settings.botid)) + "### " + username.ToUpper() + " HAS LEFT THE GUILD ###"));
                return;
            }

            if (Message.StartsWith("Great, you accepted "))
            {
            	int guildrank = 0;
            	
                string[] CommandArray = Message.Split(' ');

                username = CommandArray[3];
                
                guildrank = TheMySqlManager.GetGuildRank(username, Settings.botid);
                
                TheLogger.Log(username + " has joined the guild at rank " + guildrank.ToString());

                TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#change_rank " + username + " " + guildrank.ToString()));
                TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#gm " + System.Convert.ToChar(TheMySqlManager.playerjoinedguildcolor(Settings.botid)) + "### " + username.ToUpper() + " HAS JOINED THE GUILD AT RANK " + guildrank.ToString() + " ###"));

                TheMySqlManager.GetGuildRank(username, Settings.botid);
                return;
            }
        }
    }
}
