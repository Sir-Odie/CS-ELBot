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
	/// description of HeartBeat.
	/// </summary>
	public class Ping
	{
		private TCPWrapper TheTCPWrapper;
		
		public Ping(TCPWrapper MyTCPWrapper)
		{
			TheTCPWrapper = MyTCPWrapper;
			TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
		}
		
		private void SendPing(byte[] PingRequest)
		{
			int i=0;
			byte [] TxBuffer = new byte[PingRequest.Length+2];
			TxBuffer[0] = 0x3c;
			TxBuffer[1] = (byte)PingRequest.Length;
			TxBuffer[2] = 0x00;
			for (i=1;i<PingRequest.Length;i++)
			{
				TxBuffer[i+2] = PingRequest[i];
			}
			TheTCPWrapper.Send(TxBuffer);
		}
		
		private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
		{
			if (e.CommandBuffer[0]==0x3c)
			{
                SendPing(e.CommandBuffer);
			}
		}
	}
}
