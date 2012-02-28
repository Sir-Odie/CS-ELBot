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
	/// description of TCPWrapper.
	/// </summary>
	public class TCPWrapper
	{
		private cs_elbot.Logger TheLogger;
		private TCPClient.TCPClient TheTCPClient;
		
		public System.Net.IPAddress HostIp;
		public int Port;
		private int PMCounter = 0;
		
		public bool AutoReconnect = true;
		
		//private byte [] PartitialReceivedData = null;
		//private System.Collections.ArrayList PartitialReceivedData = null;
		
		//private System.Collections.Queue TxQueue = new System.Collections.Queue();
		
		// OnGotDisconnected
		public delegate void GotDisconnectedEventHandler(object sender, EventArgs e);
		public event GotDisconnectedEventHandler GotDisconnected;
				
		protected void OnGotDisconnected(EventArgs e)
		{
			if(GotDisconnected!=null)
            	GotDisconnected(this,e);
		}
		
		// OnGotConnected
		public delegate void GotConnectedEventHandler(object sender, EventArgs e);
		public event GotConnectedEventHandler GotConnected;
		
		protected void OnGotConnected(EventArgs e)
		{
            if (GotConnected != null)
            {
                GotConnected(this, e);
            }
		}
		
		// OnSentCommand
		public delegate void SentCommandEventHandler(object sender, SentCommandEventArgs e);
		
		public class SentCommandEventArgs : EventArgs
		{
			public readonly byte[] CommandBuffer;
			public SentCommandEventArgs(byte[] Buffer)
		    {
		        CommandBuffer = Buffer;
		    }
		}
			
		public event SentCommandEventHandler SentCommand;
		
		private void OnSentCommand(SentCommandEventArgs e)
		{
			if(SentCommand!=null)
            	SentCommand(this,e);
		}
		
		// OnGotCommand
		public delegate void GotCommandEventHandler(object sender, GotCommandEventArgs e);
		
		public class GotCommandEventArgs : EventArgs
		{
			public readonly byte[] CommandBuffer;
			public GotCommandEventArgs(byte[] Buffer)
		    {
		        CommandBuffer = Buffer;
		    }
		}
			
		public event GotCommandEventHandler GotCommand;
		
		private void OnGotCommand(GotCommandEventArgs e)
		{
			if(GotCommand!=null)
            	GotCommand(this,e);
		}
		
		public TCPWrapper(TCPClient.TCPClient TheTCPClient,Logger MyLogger)
		{
			TheLogger = MyLogger;
			this.TheTCPClient = TheTCPClient;
			this.TheTCPClient.GotData += new TCPClient.TCPClient.GotDataEventHandler(GotData);
			this.TheTCPClient.GotConnected += new TCPClient.TCPClient.GotConnectedEventHandler(GotServerConnected);
			this.TheTCPClient.GotDisconnected += new TCPClient.TCPClient.GotDisconnectedEventHandler(GotServerDisconnected);
		}
		
		private bool ConnectToServer()
		{
            return TheTCPClient.Connect(HostIp, Port);
		}
		
		private void GotServerConnected(object Sender, System.EventArgs e)
		{
			OnGotConnected(new System.EventArgs());
		}
		
		public void ReconnectToServer()
		{
			bool GotConnected = false;
				
			while (!GotConnected && !MainClass.tryingToConnect)
			{
                MainClass.tryingToConnect = true;
				TheLogger.Log("Trying to connect to server...");
                GotConnected = ConnectToServer();
                if (GotConnected == false)
                {
                    TheLogger.Log("... failed");
                    MainClass.tryingToConnect = false;
                    System.Threading.Thread.Sleep(10000);
                }
                else
                {
                    TheLogger.Log("... successful");
                    MainClass.tryingToConnect = false;
                }
			}
		}
		
		public void DisconnectFromServer()
		{
			TheTCPClient.Disconnect();
		}
		
		public int Send(byte[] data)
		{
            //need to put a check in here for pms with no username...

            int result;
            if (data[0]==0x02)
			{
				// Check if we send a PM from console
				string Message = System.Text.ASCIIEncoding.ASCII.GetString(data, 3, data.Length-3);
                string[] PMArray = Message.Split(' ');
				if (Message.StartsWith("Console:\\>"))
				{
					TheLogger.Log(Message);
					return data.Length;
				}
				else
				{
					this.PMCounter++;
					if (this.PMCounter>5)
					{
                        // Do a sleep after sending 7 Sends
                        // Do a sleep after sending 5 Sends
                        System.Threading.Thread.Sleep(500);
						this.PMCounter = 0;
					}
				}
                if (data.Length > 159)
                {
                    byte[] check = new byte[159];
                    Array.Copy(data, check, 159);
                    result = TheTCPClient.Send(check);
                    if (result > 0)
                    {
                        OnSentCommand(new SentCommandEventArgs(data));
                    }
                    return result;
                }
                else
                {
                    byte[] check = new byte[data.Length];
                    Array.Copy(data, check, data.Length);
                    result = TheTCPClient.Send(check);
                    if (result > 0)
                    {
                        OnSentCommand(new SentCommandEventArgs(data));
                    }
                    return result;
                }
			}
			
			result = TheTCPClient.Send(data);
			if (result>0)
			{
				OnSentCommand(new SentCommandEventArgs(data));
			}
			return result;
		}
		
		private void GotServerDisconnected(object Sender, EventArgs e)
		{
			TheLogger.Log("TCPWrapper: GotServerDisconnected");
			OnGotDisconnected(new EventArgs());
			if (AutoReconnect==true && !MainClass.tryingToConnect)
			{
				ReconnectToServer();
			}
		}
		
		private void GotData(object Sender, TCPClient.TCPClient.GotDataEventArgs e)
		{
            byte[] b = e.DataBuffer;

            OnGotCommand(new GotCommandEventArgs(e.DataBuffer));
		}
	}
}
