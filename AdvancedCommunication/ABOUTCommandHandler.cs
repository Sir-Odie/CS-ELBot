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
	public class ABOUTCommandHandler
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		private MySqlManager TheMySqlManager;
		private HelpCommandHandler TheHelpCommandHandler;
		private Logger TheLogger;
		
		public ABOUTCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser,HelpCommandHandler MyHelpCommandHandler, MySqlManager MyMySqlManager, Logger MyLogger)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			this.TheHelpCommandHandler = MyHelpCommandHandler;
			this.TheMySqlManager = MyMySqlManager;
			this.TheLogger = MyLogger;

            TheHelpCommandHandler.AddCommand("#about / #a - displays some information about me");
            TheHelpCommandHandler.AddCommand("#a - null");
            TheHelpCommandHandler.AddCommand("#hi - null");
            TheHelpCommandHandler.AddCommand("#hello - null");
            TheHelpCommandHandler.AddCommand("#g'day - null");
            TheHelpCommandHandler.AddCommand("#ver / #version - display my software version");
            TheHelpCommandHandler.AddCommand("#v - null");
            TheHelpCommandHandler.AddCommand("#version - null");
            TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
		}
		
		private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
		{			
			string Message = e.Message.ToLower().Replace("\'","\\\'").Replace("\"","\\\"");
			
			if (Message[0]!='#')
			{
				Message = "#" + Message;
			}
			
			string[] CommandArray = Message.Split(' ');
            int linsize = 55;

            if (CommandArray[0] == "#about" || CommandArray[0] == "#a" || CommandArray[0] == "#hi" || CommandArray[0] == "#gday" || CommandArray[0] == "#hello")
			{
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[".PadRight(linsize, '-')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Hello, I am a C# bot by DogBreath.".PadRight(linsize, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[PM me in game if you need me... or on the test server.".PadRight(linsize, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, ("[Version: " + Settings.Version.ToString()).PadRight(linsize, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[".PadRight(linsize, '-')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[This code relies heavily on code written by:".PadRight(linsize, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Artem Makhutov (Artem) and Gregory Fenton (LabRat)".PadRight(linsize, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Artem's homepage: http://www.makhutov.org/el".PadRight(linsize, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[labRat's homepage: http://www.labby.co.uk".PadRight(linsize, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[DogBreath's homepage: http://forum.el-services.net".PadRight(linsize, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[".PadRight(linsize, '-')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Special thanks to draavell my faithful assistant :)".PadRight(linsize, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Special thanks to VinoVeritas for the web hosting.".PadRight(linsize, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[".PadRight(linsize, '-')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, ("[My bot owner(s) is(are) " + TheMySqlManager.botowner(Settings.botid).ToString()).PadRight(linsize, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[" + Settings.URL.PadRight(linsize-1, ' ')));
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[".PadRight(linsize, '-')));
            }

            if (CommandArray[0] == "#version" || CommandArray[0] == "#v" || CommandArray[0] == "#ver")
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Version: " + Settings.Version));
            }
            return;
		}
	}
}
