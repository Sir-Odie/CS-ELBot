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
	/// Handles writing to log files using locked statements
    /// also writes to console in some cases.
    /// Each function opens the file, writes the line then closes the file (then
    /// writes to the console if needed.)
	/// </summary>
	public class Logger
	{
		private object ChatLogLock = "";
		private object PMLogLock = "";
		private object EventLogLock = "";
		private object DebugLogLock = "";
		private object ErrorLogLock = "";
		
		public Logger()
		{
			
		}
		
		public void RawMessageLog(string message)
		{
			lock (ChatLogLock)
			{
				System.IO.TextWriter tw = new System.IO.StreamWriter("chat.log",true);
				tw.WriteLine(System.DateTime.Now.ToString()+": " + message);
				tw.Close();
			}
		}
		
		public void PMLog(string message)
		{
			lock (PMLogLock)
			{
				if (Settings.PMLogToFile)
				{
					System.IO.TextWriter tw = new System.IO.StreamWriter("pm.log",true);
					tw.WriteLine(System.DateTime.Now.ToString()+": " + message);
					tw.Close();
				}
				
				System.Console.ForegroundColor = System.ConsoleColor.DarkMagenta;
                System.Console.WriteLine(message);
                System.Console.ResetColor();
			}
		}
		
		public void ChatLog(string message)
		{
			lock (ChatLogLock)
			{
				if (Settings.ChatLogToFile)
				{
					System.IO.TextWriter tw = new System.IO.StreamWriter("chat.log",true);
					tw.WriteLine(System.DateTime.Now.ToString()+": " + message);
					tw.Close();
				}
				
				System.Console.ForegroundColor = System.ConsoleColor.DarkGreen;
				System.Console.Write(System.DateTime.Now.ToString()+": " + message + "\n");
				System.Console.ResetColor();
			}
		}
		
		public void Log(string message)
		{
			lock (EventLogLock)
			{
				if (Settings.EventLogToFile)
				{
					System.IO.TextWriter tw = new System.IO.StreamWriter("event.log",true);
					tw.WriteLine(System.DateTime.Now.ToString()+": " + message);
					tw.Close();
				}
				
				System.Console.ForegroundColor = System.ConsoleColor.DarkYellow;
                System.Console.WriteLine(message);
                System.Console.ResetColor();
			}
		}
		
		public void Debug(string message)
		{
			lock (DebugLogLock)
			{
				if (Settings.DebugingToFile)
				{
					System.IO.TextWriter tw = new System.IO.StreamWriter("debug.log",true);
					tw.Write(System.DateTime.Now.ToString()+": " + message);
                    //if (message[message.Length - 1] != '\n')
                        tw.Write("\n");
					tw.Close();
				}
				
				if (Settings.enabledebuging)
				{
					System.Console.ForegroundColor = System.ConsoleColor.Yellow;
					System.Console.Write(System.DateTime.Now.ToString()+": " + message);
                    if (message[message.Length - 1] != '\n')
                        System.Console.Write("\n");
                    System.Console.ResetColor();
				}
			}
		}
		
		public void ErrorLog(string message)
		{
			lock (ErrorLogLock)
			{
				System.IO.TextWriter tw = new System.IO.StreamWriter("error.log",true);
				tw.WriteLine(System.DateTime.Now.ToString()+": " + message);
				tw.Close();
				
				System.Console.ForegroundColor = System.ConsoleColor.Red;
				System.Console.Write(System.DateTime.Now.ToString()+": " + message + "\n");
				System.Console.ResetColor();
			}
		}
		
		public void ErrorLog(string message,Exception ex)
		{
			lock (ErrorLogLock)
			{
				System.IO.TextWriter tw = new System.IO.StreamWriter("error.log",true);
				tw.WriteLine("------------------");
				tw.WriteLine(System.DateTime.Now.ToString()+": " + message + "\n");
				tw.WriteLine(ex.Message+"\n");
				tw.WriteLine(ex.Source+"\n");
				tw.WriteLine(ex.StackTrace+"\n");
				tw.WriteLine("------------------");
				tw.Close();
				
				System.Console.ForegroundColor = System.ConsoleColor.Red;
				System.Console.Write("------------------");
				System.Console.Write(System.DateTime.Now.ToString()+": " + message + "\n");
				System.Console.Write(ex.Message+"\n");
				System.Console.Write(ex.Source+"\n");
				System.Console.Write(ex.StackTrace+"\n");
				System.Console.Write("------------------");
				System.Console.ResetColor();
			}
		}
	}
}
