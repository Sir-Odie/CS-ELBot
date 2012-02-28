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
	/// description of Login.
	/// </summary>
	public class Login
	{
		private TCPWrapper TheTCPWrapper;
		
		// On_Got_LOG_IN_OK
		public delegate void Got_LOG_IN_OK_EventHandler(object sender, EventArgs e);
			
		public event Got_LOG_IN_OK_EventHandler Got_LOG_IN_OK;
		
		private void OnGot_LOG_IN_OK(EventArgs e)
		{
			if(Got_LOG_IN_OK!=null)
            	Got_LOG_IN_OK(this,e);
		}
		
		// On_Got_LOG_IN_NOT_OK
		public delegate void Got_LOG_IN_NOT_OK_EventHandler(object sender, EventArgs e);
			
		public event Got_LOG_IN_NOT_OK_EventHandler Got_LOG_IN_NOT_OK;
		
		private void OnGot_LOG_IN_NOT_OK(EventArgs e)
		{
			if(Got_LOG_IN_NOT_OK!=null)
            	Got_LOG_IN_NOT_OK(this,e);
		}
		
		public Login(TCPWrapper MyTCPWrapper)
		{
			TheTCPWrapper = MyTCPWrapper;
			TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
			TheTCPWrapper.GotConnected += new TCPWrapper.GotConnectedEventHandler(OnGotConnected);
		}
		
		private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
		{
			if (e.CommandBuffer[0] == 250)
			{
				OnGot_LOG_IN_OK(new EventArgs());
				return;
			}
			if (e.CommandBuffer[0] == 251)
			{
				OnGot_LOG_IN_NOT_OK(new EventArgs());
				return;
			}
			
		}
		
		private void OnGotConnected(object sender, EventArgs e)
		{
            //SEND_OPENING_SCREEN();
            //SEND_VERSION();
			LOG_IN();
            //SEND_ME_MY_ACTORS();
			//LOCATE_ME();
		}

        private void SEND_ME_MY_ACTORS()
        {
            byte[] TxBuffer = new byte[3];
            TxBuffer[0] = 0x08;
            TxBuffer[1] = 0x01;
            TxBuffer[2] = 0x00;
            TheTCPWrapper.Send(TxBuffer);
        }
		private void LOCATE_ME ()
		{
			byte []	TxBuffer = new byte[3];
			TxBuffer[0]=0x0f;
			TxBuffer[1]=0x01;
			TxBuffer[2]=0x00;
			TheTCPWrapper.Send(TxBuffer);
		}
		
		private void SEND_OPENING_SCREEN()
		{
			byte []	TxBuffer = new byte[3];	// Send data buffer
			TxBuffer[0]=9;
			TxBuffer[1]=1;
			TxBuffer[2]=0;
			TheTCPWrapper.Send(TxBuffer);
		}
		
		private void SEND_VERSION()
		{
			System.Net.IPAddress ServerIPAdress = System.Net.Dns.GetHostEntry(Settings.ServerHostname).AddressList[0];
			int ServerPort = Settings.ServerPort;
			
			byte[] Port = System.BitConverter.GetBytes(ServerPort);
			byte[] Host = ServerIPAdress.GetAddressBytes();
			//#define VER_STRING	"1.1.2.0"
			//#define VER_MAJOR	1
			//#define VER_MINOR	1
			//#define VER_RELEASE	2
			//#define VER_BUILD	0
			//  10  15   0  10   0  16   0   1   3   2   0  62  93 225  26   7 208
			byte []	TxBuffer = new byte[17]; // Send data buffer
			TxBuffer[0]=10;		//0a
			TxBuffer[1]=15;		//0f
			TxBuffer[2]=0;		//00
			//version_first_digit
			TxBuffer[3]=10;		//0a
			TxBuffer[4]=0;		//00
			//version_second_digit
			TxBuffer[5]=16;		//0b
			TxBuffer[6]=0;		//00
			//client_version_major
			TxBuffer[7]=1;		//01
			//client_version_minor
			TxBuffer[8]=3;		//01
			//client_version_release
			TxBuffer[9]=3;		//02
			//client_version_patch
			TxBuffer[10]=0;		//00
			
			//Server IP
			TxBuffer[11]=Host[0];	//3e
			TxBuffer[12]=Host[1];	//5d
			TxBuffer[13]=Host[2];	//e1
			TxBuffer[14]=Host[3];	//1a
			
			//Server Port
			TxBuffer[15]=Port[0];	//07
			TxBuffer[16]=Port[1];	//d0
			
			//TheTCPWrapper.Send(TxBuffer);
		}
		
		private void LOG_IN()
		{
			string username = Settings.Loginname;
			string Password = Settings.LoginPassword;
			
			int i=0;
			int Login_Len = 5;
			Login_Len+=username.Length;
			Login_Len+=Password.Length;
			
			byte []	TxBuffer = new byte[Login_Len];	// Send data buffer
			TxBuffer[0]=0x8c;
			TxBuffer[1]=(byte)(Login_Len-2);
			TxBuffer[2]=0x00;
			for (i=0;i<username.Length;i++)
			{
				TxBuffer[i+3]=(byte)username[i];
			}
			TxBuffer[i+3]=0x20;
			for (i=0;i<Password.Length;i++)
			{
				TxBuffer[i+4+username.Length]=(byte)Password[i];
			}
			TxBuffer[i+4+username.Length]= 0x00;
			
			TheTCPWrapper.Send(TxBuffer);
		}
	}
}
