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

namespace cs_elbot.BasicCommunication
{
	/// <summary>
	/// description of MessageParser.
	/// </summary>
	public class MessageParser
	{
		// Got_AbortTrade
		//Got_AbortTrade_EventArgs
		
		public delegate void Got_AbortTrade_EventHandler(object sender, System.EventArgs e);
			
		public event Got_AbortTrade_EventHandler Got_AbortTrade;
		
		private void OnGot_AbortTrade(System.EventArgs e)
		{
			if(Got_AbortTrade!=null)
            	Got_AbortTrade(this,e);
		}
		
		// OnGot_TradeRequest
		public delegate void Got_TradeRequest_EventHandler(object sender, Got_TradeRequest_EventArgs e);
		
		public class Got_TradeRequest_EventArgs : EventArgs
		{
			public readonly string username;
			public Got_TradeRequest_EventArgs(string username)
		    {
				this.username = username;
		    }
		}
			
		public event Got_TradeRequest_EventHandler Got_TradeRequest;

        private void OnGot_TradeRequest(Got_TradeRequest_EventArgs e)
        {
            //if(Got_TradeRequest!=null)
            Got_TradeRequest(this, e);
        }
        private void OnGot_TradeRequest(Got_TradeRequest_EventArgs e, bool junk)
        {
            //if(Got_TradeRequest!=null)
            Got_TradeRequest(this, e);
        }
		
		// OnGot_LocationInfo
		public delegate void Got_LocationInfo_EventHandler(object sender, Got_LocationInfo_EventArgs e);
		
		public class Got_LocationInfo_EventArgs : EventArgs
		{
			public readonly string LocationInfo;
			public Got_LocationInfo_EventArgs(string LocationInfo)
		    {
				this.LocationInfo = LocationInfo;
		    }
		}
			
		public event Got_LocationInfo_EventHandler Got_LocationInfo;
		
		private void OnGot_LocationInfo(Got_LocationInfo_EventArgs e)
		{
			if(Got_LocationInfo!=null)
            	Got_LocationInfo(this,e);
		}
		
		// OnGot_PM
		public delegate void Got_PM_EventHandler(object sender, Got_PM_EventArgs e);
		
		public class Got_PM_EventArgs : EventArgs
		{
			public readonly string username;
			public readonly string Message;
			public Got_PM_EventArgs(string username, string Message)
		    {
				this.username = username;
		        this.Message = Message;
                //MySqlManager.setPlayerOnline(username);
		    }
		}
			
		public event Got_PM_EventHandler Got_PM;
		
		private void OnGot_PM(Got_PM_EventArgs e)
		{
			if(Got_PM!=null)
            	Got_PM(this,e);
		}
		
		// OnSend_PM
		public delegate void Send_PM_EventHandler(object sender, Send_PM_EventArgs e);
		
		public class Send_PM_EventArgs : EventArgs
		{
			public readonly string username;
			public readonly string Message;
			public Send_PM_EventArgs(string username, string Message)
		    {
				this.username = username;
		        this.Message = Message;
		    }
		}
			
		public event Send_PM_EventHandler Send_PM;
		
		private void OnSend_PM(Send_PM_EventArgs e)
		{
			if(Send_PM!=null)
            	Send_PM(this,e);
		}
		
		// OnGet_GlobalMessage
		public delegate void Get_GlobalMessageEventHandler(object sender, Get_GlobalMessageEventArgs e);
		
		public class Get_GlobalMessageEventArgs : EventArgs
		{
			public readonly string Message;
			public Get_GlobalMessageEventArgs(string Message)
		    {
		        this.Message = Message;
		    }
		}
			
		public event Get_GlobalMessageEventHandler Get_GlobalMessage;
		
		private void OnGet_GlobalMessage(Get_GlobalMessageEventArgs e)
		{
			if(Get_GlobalMessage!=null)
            	Get_GlobalMessage(this,e);
		}
		
		// OnSend_GlobalMessage
		public delegate void Send_GlobalMessageEventHandler(object sender, Send_GlobalMessageEventArgs e);
		
		public class Send_GlobalMessageEventArgs : EventArgs
		{
			public readonly string Message;
			public Send_GlobalMessageEventArgs(string Message)
		    {
		        this.Message = Message;
		    }
		}
			
		public event Send_GlobalMessageEventHandler Send_GlobalMessage;
		
		private void OnSend_GlobalMessage(Send_GlobalMessageEventArgs e)
		{
			if(Send_GlobalMessage!=null)
            	Send_GlobalMessage(this,e);
		}

		
		private BasicCommunication.RawText TheRawText;
		private Logger TheLogger;
        private TCPWrapper TheTCPWrapper;
        private MySqlManager TheMySqlManager;
		
		public MessageParser(BasicCommunication.RawText MyRawText, Logger MyLogger, TCPWrapper MyTCPWrapper, MySqlManager MyMySqlManager)
		{
			this.TheLogger = MyLogger;
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMySqlManager = MyMySqlManager;
			TheRawText = MyRawText;
			TheRawText.Got_RAW_TEXT += new BasicCommunication.RawText.Got_RAW_TEXT_EventHandler(OnGot_RAW_TEXT);
		}
		
		
		private void OnGot_RAW_TEXT(object sender, BasicCommunication.RawText.Got_RAW_TEXT_EventArgs e)
		{
			string Message = e.Message;
			string username = "";

            //Console.WriteLine("Raw text received: " + Message);
            if (Message.StartsWith("[PM from "))
			{
				Message = Message.Remove(0,9);
				username = Message.Substring(0,Message.IndexOf(':'));
				Message = Message.Remove(0,username.Length+2);
				Message = Message.Remove(Message.Length-1,1);
				OnGot_PM(new Got_PM_EventArgs(username,Message));
				return;
			}
			
			if (Message.StartsWith("[PM to "))
			{
				Message = Message.Remove(0,7);
				username = Message.Substring(0,Message.IndexOf(':'));
				Message = Message.Remove(0,username.Length+2);
				Message = Message.Remove(Message.Length-1,1);
				OnSend_PM(new Send_PM_EventArgs(username,Message));
				return;
			}

			if (Message.StartsWith("You are in"))
			{
				Message = Message.Remove(0,11);
				Settings.LocationInfo = Message;
				TheLogger.Log("Got my location info: " + Message);
                TheMySqlManager.updateMyLocation();
                TheMySqlManager.updateVersion();
                Settings.LocationInfo = Message;
                OnGot_LocationInfo(new Got_LocationInfo_EventArgs(Message));
				return;
			}
			
			if (Message.EndsWith("her in order to accept the trade."))
			{
				username = Message.Substring(0,Message.IndexOf(' '));
				TheLogger.Log(username + " wants to trade!");
                OnGot_TradeRequest(new Got_TradeRequest_EventArgs(username),true);
				return;
			}


            if (Message.StartsWith("You are too far away! Get closer!"))
			{
                TheLogger.Log("The trade was aborted: " + Message);
                if (TradeHandler.openingStorage)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(TradeHandler.username, "I'm too far away from storage to open it, move me closer please!"));
                    TradeHandler.openingStorage = false;
                }
                else
                {
                    OnGot_AbortTrade(new System.EventArgs());
                }
                //TheTCPWrapper.Send(CommandCreator.SIT_DOWN(false));
                return;
			}
			

            if (Message.EndsWith("can't carry all the items."))
			{
				TheLogger.Log("The trade was aborted: " + Message);
				OnGot_AbortTrade(new System.EventArgs());
				return;
			}
			
			if (Message.EndsWith("aborted the trade."))
			{
				TheLogger.Log("The trade was aborted: " + Message);
				OnGot_AbortTrade(new System.EventArgs());
				return;
			}
            if (!Message.StartsWith("#Hint") && !Message.Contains("Game Time"))
            {
                OnGet_GlobalMessage(new Get_GlobalMessageEventArgs(Message));
            }
            else
            {
                //Console.WriteLine("Yep, we can ignore hints...");
            }
		}
		
		// Fake a PM message from the console
		public void FakePM(string username, string Message)
		{
			OnGot_PM(new Got_PM_EventArgs(username,Message));
		}

        public void FakeTrade(string username)
        {
            OnGot_TradeRequest(new Got_TradeRequest_EventArgs(username));
        }

	}
}
