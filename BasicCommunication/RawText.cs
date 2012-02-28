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
	/// description of RawText.
	/// </summary>
	public class RawText
	{
		// OnGot_RAW_TEXT
		public delegate void Got_RAW_TEXT_EventHandler(object sender, Got_RAW_TEXT_EventArgs e);
		
		public class Got_RAW_TEXT_EventArgs : EventArgs
		{
			public readonly string Message;
			public Got_RAW_TEXT_EventArgs(string Message)
		    {
		        this.Message = Message;
		    }
		}
			
		public event Got_RAW_TEXT_EventHandler Got_RAW_TEXT;
		
		private void OnGot_RAW_TEXT(Got_RAW_TEXT_EventArgs e)
		{
			if(Got_RAW_TEXT!=null)
            	Got_RAW_TEXT(this,e);
		}

		
		private TCPWrapper TheTCPWrapper;
		
		public RawText(TCPWrapper MyTCPWrapper)
		{
			TheTCPWrapper = MyTCPWrapper;
			TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
		}
		
		private void RxRAW_TEXT(byte[] data)
		{
			string Message = "";
            string MessageBuffer = System.Text.ASCIIEncoding.ASCII.GetString(data, 5, data.Length - 5);
			
			// remove bad some chars (eg color tags)
            MessageBuffer = MessageBuffer.Replace((char)10, ' ');
			
			for (int i=0;i<MessageBuffer.Length;i++)
			{
                if (!(MessageBuffer[i] < 32 || MessageBuffer[i] > 126))
                {
                    Message = Message + MessageBuffer[i];
                }
			}
			
			OnGot_RAW_TEXT(new Got_RAW_TEXT_EventArgs(Message));
		}
		
		private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
		{
			if (e.CommandBuffer[0]==0x00)
			{
				RxRAW_TEXT(e.CommandBuffer);
			}
		}
	}
}
