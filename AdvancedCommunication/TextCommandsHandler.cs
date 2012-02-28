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
    /// description of DONATECommandHandler.
    /// </summary>
    public class TextCommandsHandler
    {
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private MySqlManager TheMySqlManager;
        //private bool CommandIsDisabled;
        private HelpCommandHandler TheHelpCommandHandler;
        private Logger TheLogger;

        public TextCommandsHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, HelpCommandHandler MyHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheHelpCommandHandler = MyHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.TheLogger = MyLogger;
            //this.CommandIsDisabled = MyMySqlManager.CheckIfCommandIsDisabled("#donate",Settings.botid);
            string commands = TheMySqlManager.TextCommandlist(Settings.botid);
            if (commands.Length == 0)
            {
                return;
            }
            string[] commandsarray = commands.Split(' ');
            foreach (string command in commandsarray)
            {
                string paddedcommand = "";
                if (command[0] != '#')
                    paddedcommand = "#";
                paddedcommand = paddedcommand + command;
                if (TheMySqlManager.CheckIfTextCommandIsDisabled(paddedcommand, Settings.botid) == false)
                {
                    string text = TheMySqlManager.TextCommandHelpText(paddedcommand, Settings.botid);
                    TheHelpCommandHandler.AddCommand(paddedcommand + text);
                }

            }
            TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
        }

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {
            string Message = e.Message.ToLower().Replace("\'","\\\'").Replace("\"","\\\"");

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');


            //if (CommandArray[0] == "#donate" || (CommandArray[0] == "#d" && CommandArray.Length == 1))
            {
                if (TheMySqlManager.CheckIfTextCommandIsDisabled(CommandArray[0], Settings.botid))
                {
                    //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetTextCommandRank(CommandArray[0], Settings.botid))
                {
                    //TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
            }
            if (TheMySqlManager.sendtextcommandtogm(CommandArray[0], Settings.botid))
            {
                TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#gm " + System.Convert.ToChar(TheMySqlManager.playerloggedoncolor(Settings.botid)) + "### " + e.username.ToUpper() + " JUST SENT " + CommandArray[0] + " ###"));
            }
            string text = TheMySqlManager.TextCommandText(CommandArray[0], Settings.botid);
            int maxwidth = 60;
            if (text.Length < maxwidth)
                maxwidth = text.Length;
            //text = text.Replace(System.Convert.ToString(13) + System.Convert.ToString(10), System.Convert.ToString(13)).Replace(System.Convert.ToString(10), System.Convert.ToString(13)).Replace(System.Convert.ToString(13)+System.Convert.ToString(13),System.Convert.ToString(13));
            if (text.Length < maxwidth)
                maxwidth = text.Length;
            
            string[] lines = text.Split(' ');
            if (lines.Length == 0)
                return;

            string separator="[", spaces="";
            for(int i=0;i<=maxwidth;i++)
            {
                separator += "=";
                spaces += " ";
            }
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, separator));
            string str = "";
            foreach(string tl in lines)
            {
                if (tl.Contains("\n"))
                {
                    if (str.Length > 0)
                        str += " ";
                    //str += tl.Substring(0, tl.IndexOf("\n"));
                    if (str.Length + tl.Substring(0, tl.IndexOf("\n")).Length > maxwidth)
                    {
                        str += spaces;
                        str = "[" + str.Substring(0, maxwidth + 1);
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                        str = tl.Substring(0, tl.IndexOf("\n"));
                    }
                    else
                    {
                        str += tl.Substring(0, tl.IndexOf("\n"));
                        str += spaces;
                        str = "[" + str.Substring(0, maxwidth + 1);
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                        //str = tl.Substring(0, tl.IndexOf("\n"));
                        str = "";
                    }
                    if (str.Length > 0)
                    {
                        str += spaces;
                        str = "[" + str.Substring(0, maxwidth + 1);
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                        str = "";
                    }
                    if (str.Length > 0)
                        str += " ";
                    if (tl.Length > tl.IndexOf("\n"))
                    {
                        str = tl.Substring(tl.IndexOf("\n") + 1);
                    }

                }
                else
                {
                    if (str.Length + tl.Length > maxwidth && str.Length > 0 && !tl.Contains("\n"))
                    {
                        str += spaces;
                        str = "[" + str.Substring(0, maxwidth + 1);
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));
                        str = "";
                    }
                    if (str.Length > 0)
                        str += " ";
                    str += tl;
                }
            }
            str += spaces;
            str = "[" + str.Substring(0, maxwidth + 1);
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, str));           

            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, separator));
            return;
        }
    }
}
