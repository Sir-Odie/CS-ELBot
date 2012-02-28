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
using System.Timers;

namespace cs_elbot.BasicCommunication
{
	/// <summary>
	/// description of HeartBeat.
	/// </summary>
	public class Shutdown
	{
        public int numberOfMinutes = 5;
        private TCPWrapper TheTCPWrapper;

        private BasicCommunication.MessageParser TheMessageParser;
        private MySqlManager TheMySqlManager;
        private errorHandler TheErrorHandler;

        private int numberOfTicks;
		
		// Setup Timer for the Heart_Beat
		private System.Timers.Timer ShutdownTimer = new System.Timers.Timer();
		
		public Shutdown(TCPWrapper MyTCPWrapper, MessageParser MyMessageParser, MySqlManager MyMySqlManager, errorHandler MyErrorHandler)
		{
			TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheMySqlManager = MyMySqlManager;
            this.TheErrorHandler = MyErrorHandler;

			ShutdownTimer.Interval=MainClass.ONE_MINUTE;
            ShutdownTimer.Elapsed += new ElapsedEventHandler(ShutdownTimer_Tick);
			
			TheTCPWrapper.GotConnected += new TCPWrapper.GotConnectedEventHandler(OnGotConnected);
			TheTCPWrapper.GotDisconnected += new TCPWrapper.GotDisconnectedEventHandler(OnGotDisconnected);
        }
		
		private void ShutdownTimer_Tick(object sender,ElapsedEventArgs eArgs)
		{
            try
            {
                numberOfTicks++;
                if (numberOfTicks < numberOfMinutes)
                {
                    if (numberOfTicks == 4)
                    {
                        TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#GM *** Warning! You have " + (numberOfMinutes - numberOfTicks) + " minute of trade time remaining."));
                    }
                    else
                    {
                        TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#GM *** You have " + (numberOfMinutes - numberOfTicks) + " minutes of trade time remaining."));
                    }
                }
                else
                {
                    //                TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#GM *** I should be logged off now, I've been on for " + numberOfTicks + " minutes!"));
                    //                TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#gm ### SHUTTING DOWN UNTIL NEEDED AGAIN ###"));
                    TheMessageParser.FakePM("Console:\\>", "#say #gm ### SHUTTING DOWN UNTIL NEEDED AGAIN ###");
                    TheMySqlManager.ImLoggedOut(Settings.botid);
                    //                System.Environment.Exit(0);
                    return;
                }
            }
            catch (Exception exception)
            {
                TheErrorHandler.writeErrorMessage("Exception raised in the shutdown handler timer..." + exception);
            }
            finally
            { }
		}
		
		private void OnGotConnected(object sender, EventArgs e)
		{
			ShutdownTimer.Start();
            TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#GM *** Greetings! You have " + (numberOfMinutes - numberOfTicks) + " minutes of trade time remaining."));
        }
		
		private void OnGotDisconnected(object sender, EventArgs e)
		{
            ShutdownTimer.Stop();
		}
	}
}
