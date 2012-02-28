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

namespace cs_elbot
{
	/// <summary>
	/// description of CLI.
	/// </summary>
	public class CLI
	{
		private TCPWrapper TheTCPWrapper;
		private BasicCommunication.MessageParser TheMessageParser;
		
		public CLI(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
			
			string VersionLine = "| Version: ";
			VersionLine = VersionLine.Insert(VersionLine.Length,Settings.Version);
			VersionLine = VersionLine.PadRight(49);
			VersionLine = VersionLine + "|";
		}
		
		public bool Interprete(string Command)
		{
			Command = Command.Trim();
	
			switch (Command.ToLower())
			{
				case "login":
					TheTCPWrapper.ReconnectToServer();
					break;
				case "logout":
					TheTCPWrapper.DisconnectFromServer();
					break;
				case "quit":
					TheMessageParser.FakePM("Console:\\>","#say  ### GOING DOWN FOR MAINTENANCE ###");
					TheMessageParser.FakePM("Console:\\>","#say #gm ### GOING DOWN FOR MAINTENANCE ###");
                    System.Environment.Exit(0);
					break;
				default:
					if (Command.Length>0)
					{
						if (Command[0] == '#')
						{
							TheMessageParser.FakePM("Console:\\>",Command);
						}
					}
					break;
			}
			
			return true;
		}
		
		public void DoLoop()
		{
			string ConsoleInput;
			bool CLIresult=true;
			
//			if (Settings.AutoConnect==true)
//			{
//				TheTCPWrapper.ReconnectToServer();
//			}
            TheTCPWrapper.ReconnectToServer();

            string input = "";
            while ((input = Console.ReadLine()) != "quit")
//                while (true)
			{
				System.Console.ForegroundColor = System.ConsoleColor.Gray;
				System.Console.Write("CLI " + Settings.Loginname +" :>");
				ConsoleInput = System.Console.ReadLine();
				System.Console.ResetColor();
				CLIresult = Interprete(ConsoleInput);
			}
		}
	}
}
