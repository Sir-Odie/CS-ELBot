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
    //public class HeartBeat
    //{
    //    private static TCPWrapper TheTCPWrapper;
    //    private static errorHandler TheErrorHandler;
		
    //    // Setup Timer for the Heart_Beat
    //    private System.Timers.Timer HeartBeatTimer = new System.Timers.Timer();
		
    //    public HeartBeat(TCPWrapper MyTCPWrapper, errorHandler MyErrorHandler)
    //    {
    //        TheErrorHandler = MyErrorHandler;
    //        TheTCPWrapper = MyTCPWrapper;
    //        HeartBeatTimer.Interval=25000;
    //        HeartBeatTimer.Elapsed += new ElapsedEventHandler(HeartBeatTimer_Tick);
			
    //        TheTCPWrapper.GotConnected += new TCPWrapper.GotConnectedEventHandler(OnGotConnected);
    //        TheTCPWrapper.GotDisconnected += new TCPWrapper.GotDisconnectedEventHandler(OnGotDisconnected);
    //    }
		
    //    private void HeartBeatTimer_Tick(object sender,ElapsedEventArgs eArgs)
    //    {
    //        try
    //        {
    //            byte[] TxBuffer = new byte[3];	// Send data buffer
    //            TxBuffer[0] = 0x0e;
    //            TxBuffer[1] = 0x01;
    //            TxBuffer[2] = 0x00;
    //            TheTCPWrapper.Send(TxBuffer);
    //        }
    //        catch (Exception theException)
    //        {
    //            Console.WriteLine(theException);
    //        }
    //        finally
    //        {
    //        }
    //    }
    //    private static DateTime lastHeartbeatTime = new DateTime();
    //    private static bool stopHeartbeat = false;
    //    private void OnGotConnected(object sender, EventArgs e)
    //    {
    //        //HeartBeatTimer.Start();
    //        //set a variable to break out of the while loop, loop until it's true (will be set by disconnecte)
    //        stopHeartbeat = false;
    //        if (!heartBeatThread.IsAlive)
    //        {
    //            heartBeatThread.Start();
    //        }
    //    }
    //    System.Threading.Thread heartBeatThread = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadMethod));
    //    static void ThreadMethod()
    //    {
    //        lastHeartbeatTime = DateTime.Now;
    //        //Console.WriteLine("sending heartbeat now...." + DateTime.Now);
    //        SendHeartbeat();
    //        while (!stopHeartbeat)
    //        {
    //            System.Threading.Thread.Sleep(10);
    //            TimeSpan timeElapsed = DateTime.Now - lastHeartbeatTime;
    //            if (timeElapsed.Seconds > 25)
    //            {
    //                SendHeartbeat();
    //                //Console.WriteLine("sending heartbeat now...." + DateTime.Now);
    //                lastHeartbeatTime = DateTime.Now;
    //            }
    //        }
    //        //Console.WriteLine("stopping heartbeat thread");
    //    }
    //    private static void SendHeartbeat()
    //    {
    //        try
    //        {
    //            byte[] TxBuffer = new byte[3];	// Send data buffer
    //            TxBuffer[0] = 0x0e;
    //            TxBuffer[1] = 0x01;
    //            TxBuffer[2] = 0x00;
    //            TheTCPWrapper.Send(TxBuffer);
    //        }
    //        catch (Exception theException)
    //        {
    //            Console.WriteLine(theException);
    //        }
    //        finally
    //        {
    //        }
    //    }
    //    private void OnGotDisconnected(object sender, EventArgs e)
    //    {
    //        //HeartBeatTimer.Stop();
    //        //stopHeartbeat = true;
    //        //if (heartBeatThread.IsAlive)
    //        //{
    //        //    heartBeatThread.Join();
    //        //}
    //    }
    //}
    public class HeartBeat
    {
        private TCPWrapper TheTCPWrapper;

        // Setup Timer for the Heart_Beat
        private System.Timers.Timer HeartBeatTimer = new System.Timers.Timer();

        public HeartBeat(TCPWrapper MyTCPWrapper)
        {
            TheTCPWrapper = MyTCPWrapper;
            HeartBeatTimer.Interval = 25000;
            HeartBeatTimer.Elapsed += new ElapsedEventHandler(HeartBeatTimer_Tick);

            TheTCPWrapper.GotConnected += new TCPWrapper.GotConnectedEventHandler(OnGotConnected);
            TheTCPWrapper.GotDisconnected += new TCPWrapper.GotDisconnectedEventHandler(OnGotDisconnected);
        }

        private void HeartBeatTimer_Tick(object sender, ElapsedEventArgs eArgs)
        {

            byte[] TxBuffer = new byte[3];	// Send data buffer
            TxBuffer[0] = 0x0e;
            TxBuffer[1] = 0x01;
            TxBuffer[2] = 0x00;
            TheTCPWrapper.Send(TxBuffer);
        }

        private void OnGotConnected(object sender, EventArgs e)
        {
            HeartBeatTimer.Start();
        }

        private void OnGotDisconnected(object sender, EventArgs e)
        {
            HeartBeatTimer.Stop();
        }
    }

}
