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
	/// description of CommandCreator.
	/// </summary>
	public class CommandCreator
	{
		public CommandCreator()
		{
		}
        //public static bool gettingInventoryItems;
        public static byte[] SYNC_CLOCK()
        {
            byte[] outputMessage = new byte[3];
            outputMessage[0] = 0xE7;
            outputMessage[1] = 0x01;
            outputMessage[2] = 0x00;
            return outputMessage;
        }
        public static byte[] USE_MAP_OBJECT(uint object_id, int useWithPosition)
        {
            byte[] objectID = System.BitConverter.GetBytes(object_id);
            byte[] useWithPos = System.BitConverter.GetBytes(useWithPosition);

            byte[] TxBuffer = new byte[11];
            TxBuffer[0] = 16;
            TxBuffer[1] = 0x09;
            TxBuffer[2] = 0x00;
            TxBuffer[3] = objectID[0];
            TxBuffer[4] = objectID[1];
            TxBuffer[5] = objectID[2];
            TxBuffer[6] = objectID[3];
            TxBuffer[7] = useWithPos[0];
            TxBuffer[8] = useWithPos[1];
            TxBuffer[9] = useWithPos[2];
            TxBuffer[10] = useWithPos[3];

            return TxBuffer;
        }
        public static byte[] DEPOSIT_ITEM( byte pos, UInt32 quantity)
        {
            byte[] depositQuantity = System.BitConverter.GetBytes(quantity);
            byte[] TxBuffer = new byte[8];
            TxBuffer[0] = 45;
            TxBuffer[1] = 6;
            TxBuffer[2] = 0x00;
            TxBuffer[3] = pos;
            TxBuffer[4] = depositQuantity[0];
            TxBuffer[5] = depositQuantity[1];
            TxBuffer[6] = depositQuantity[2];
            TxBuffer[7] = depositQuantity[3];
            return TxBuffer;
        }
        public static byte[] WITHDRAW_ITEM(UInt16 pos, UInt32 quantity)
        {
            byte[] withdrawQuantity = System.BitConverter.GetBytes(quantity);
            byte[] sendPos = System.BitConverter.GetBytes(pos);
            byte[] TxBuffer = new byte[9];
            TxBuffer[0] = 46;
            TxBuffer[1] = 0x07;
            TxBuffer[2] = 0x00;
            TxBuffer[3] = sendPos[0];
            TxBuffer[4] = sendPos[1];
            TxBuffer[5] = withdrawQuantity[0];
            TxBuffer[6] = withdrawQuantity[1];
            TxBuffer[7] = withdrawQuantity[2];
            TxBuffer[8] = withdrawQuantity[3];
            return TxBuffer;
        }
        public static byte[] RESPOND_TO_NPC(UInt16 NPCID)
        {
            byte[] TxBuffer = new byte[7];
            byte[] npcID = System.BitConverter.GetBytes(NPCID);
            TxBuffer[0] = 29;
            TxBuffer[1] = 0x05;
            TxBuffer[2] = 0x00;
            TxBuffer[3] = npcID[0];
            TxBuffer[4] = npcID[1];
            TxBuffer[5] = 1;
            TxBuffer[6] = 0x00;
            return TxBuffer;
            //(29)
        }
        public static byte[] TOUCH_PLAYER( uint NPCID)
        {
            byte[] TxBuffer = new byte[7];
            byte[] npcID = System.BitConverter.GetBytes(NPCID);
            TxBuffer[0] = 0x1c;
            TxBuffer[1] = 0x05;
            TxBuffer[2] = 0x00;
            TxBuffer[3] = npcID[0];
            TxBuffer[4] = npcID[1];
            TxBuffer[5] = npcID[2];
            TxBuffer[6] = npcID[3];
            return TxBuffer;
        }
        public static byte[] LOCATE_ME()
        {
            byte[] TxBuffer = new byte[3];
            TxBuffer[0] = 0x0f;
            TxBuffer[1] = 0x01;
            TxBuffer[2] = 0x00;

            return TxBuffer;
        }

        public static byte[] LOOK_AT_STORAGE_ITEM(UInt16 pos)
        {
            byte[] sendPos = System.BitConverter.GetBytes(pos);
            byte[] TxBuffer = new byte[5];
            TxBuffer[0] = 47;
            TxBuffer[1] = 3;
            TxBuffer[2] = 0;
            TxBuffer[3] = sendPos[0];
            TxBuffer[4] = sendPos[1];

            return TxBuffer;
            ////byte[] TxBuffer = new byte[4];
            ////TxBuffer[0] = 47;
            ////TxBuffer[1] = 2;
            ////TxBuffer[2] = 0;
            ////TxBuffer[3] = pos;

            ////return TxBuffer;
        }

        public static byte[] GET_STORAGE_CATEGORY(byte pos)
        {
            byte[] TxBuffer = new byte[4];
            TxBuffer[0] = 44;
            TxBuffer[1] = 2;
            TxBuffer[2] = 0;
            TxBuffer[3] = pos;

            return TxBuffer;
        }
		
		public static byte[] RAW_TEXT (string Message)
		{
			int i=0;
			int Len = 3;
			Len+=Message.Length;
			
			byte [] Len_bytes = System.BitConverter.GetBytes(Len-2);
			byte []	TxBuffer = new byte[Len];	// Send data buffer
			
			TxBuffer[0]=0x00;
			TxBuffer[1]=Len_bytes[0];
			TxBuffer[2]=Len_bytes[1];
			for (i=0;i<Message.Length;i++)
			{
				TxBuffer[i+3]=(byte)Message[i];
			}
			
			return TxBuffer;
		}
		
		public static byte[] SEND_PM(string username, string M)
		{
            int i = 0;
            int Len = 4;
            Len += username.Length;
            //string Message = M.Trim();
            string Message = M;

            if (Message.Length + username.Length > 150)
            {
                Message = Message.Substring(0, 150 - Len);
            }
            Len += Message.Length;
            byte[] Len_bytes = System.BitConverter.GetBytes(Len - 2);
            byte[] TxBuffer = new byte[Len];	// Send data buffer

            TxBuffer[0] = 0x02;
            TxBuffer[1] = Len_bytes[0];
            TxBuffer[2] = Len_bytes[1];
            for (i = 0; i < username.Length; i++)
            {
                TxBuffer[i + 3] = (byte)username[i];
            }
            TxBuffer[i + 3] = 0x20;
            for (i = 0; i < Message.Length; i++)
            {
                TxBuffer[i + 4 + username.Length] = (byte)Message[i];
            }
			
			return TxBuffer;
		}

        public static byte[] MOVE_TO(int pos_x, int pos_y)
        {
            int Len = 7;

            byte[] Len_bytes = System.BitConverter.GetBytes(Len - 2);
            byte[] TxBuffer = new byte[7];	// Send data buffer

            TxBuffer[0] = 0x01;
            TxBuffer[1] = Len_bytes[0];
            TxBuffer[2] = Len_bytes[1];
            byte[] x = System.BitConverter.GetBytes(pos_x);
            TxBuffer[3] = x[0];
            TxBuffer[4] = x[1];
            byte[] y = System.BitConverter.GetBytes(pos_y);
            TxBuffer[5] = y[0];
            TxBuffer[6] = y[1];

            return TxBuffer;
        }

        public static byte[] TURN_LEFT()
        {
            int Len = 3;

            byte[] Len_bytes = System.BitConverter.GetBytes(Len - 2);
            byte[] TxBuffer = new byte[3];	// Send data buffer

            TxBuffer[0] = 11;
            TxBuffer[1] = Len_bytes[0];
            TxBuffer[2] = Len_bytes[1];
            return TxBuffer;
        }

        public static byte[] TURN_RIGHT()
        {
            int Len = 3;

            byte[] Len_bytes = System.BitConverter.GetBytes(Len - 2);
            byte[] TxBuffer = new byte[3];	// Send data buffer

            TxBuffer[0] = 12;
            TxBuffer[1] = Len_bytes[0];
            TxBuffer[2] = Len_bytes[1];
            return TxBuffer;
        }

        public static byte[] LOOK_AT_INVENTORY_ITEM(byte id)
		{
			byte []	TxBuffer = new byte[4];
			TxBuffer[0]=19;
			TxBuffer[1]=2;
			TxBuffer[2]=0;
			TxBuffer[3]=id;
			
			return TxBuffer;
		}
		
		public static byte[] LOOK_AT_TRADE_ITEM (byte id, bool theirtrade)
		{
			byte []	TxBuffer = new byte[5];
			TxBuffer[0]=38;
			TxBuffer[1]=3;
			TxBuffer[2]=0;
			TxBuffer[3]=id;
			
			if (theirtrade==true)
			{
				TxBuffer[4]=1;
			}
			else
			{
				TxBuffer[4]=0;
			}
			
			return TxBuffer;
		}
		
		public static byte[] TRADE_WITH (int PlayerID)
		{
			byte[] PlayerIDBytes = System.BitConverter.GetBytes(PlayerID);
			
			byte []	TxBuffer = new byte[7];
			TxBuffer[0]=32;
			TxBuffer[1]=0x05;
			TxBuffer[2]=0x00;
			TxBuffer[3]=PlayerIDBytes[0];
			TxBuffer[4]=PlayerIDBytes[1];
			TxBuffer[5]=PlayerIDBytes[2];
			TxBuffer[6]=PlayerIDBytes[3];
			
			return TxBuffer;
		}

        public static byte[] SIT_DOWN(bool sit)
        {
            byte[] TxBuffer = new byte[4];
            TxBuffer[0] = 7;
            TxBuffer[1] = 0x02;
            TxBuffer[2] = 0x0;
            if (sit)
                TxBuffer[3] = 0x01;
            else
                TxBuffer[3] = 0x00;
            return TxBuffer;
        }

        public static byte[] USE_INVENTORY_ITEM(byte pos)
        {
            byte[] TxBuffer = new byte[4];
            TxBuffer[0] = 31;
            TxBuffer[1] = 0x02;
            TxBuffer[2] = 0x0;
            TxBuffer[3] = pos;
            return TxBuffer;
        }

        public static byte[] MOVE_INVENTORY_ITEM(byte frompos, byte topos)
        {
            byte[] TxBuffer = new byte[5];
            TxBuffer[0] = 20;
            TxBuffer[1] = 0x03;
            TxBuffer[2] = 0x0;
            TxBuffer[3] = frompos;
            TxBuffer[4] = topos;
            return TxBuffer;
        }

        public static byte[] EXIT_TRADE()
		{
			byte []	TxBuffer = new byte[3];
			TxBuffer[0]=35;
			TxBuffer[1]=0x01;
			TxBuffer[2]=0x00;
			
			return TxBuffer;
		}
		
		public static byte[] ACCEPT_TRADE ()
		{
			byte []	TxBuffer = new byte[3];
			TxBuffer[0]=33;
			TxBuffer[1]=0x01;
			TxBuffer[2]=0x00;
			
			return TxBuffer;
		}

        public static byte[] ACCEPT_TRADE_ENTIRE(bool storageAvailable)
		{
			byte []	TxBuffer = new byte[19];
			TxBuffer[0]=33;
			TxBuffer[1]=17;
			TxBuffer[2]=0x00;
			
			for (int i = 0;i<16;i++)
			{
                if (storageAvailable == true)    
                    TxBuffer[3 + i] = 2;
                else
                    TxBuffer[3+i] = 1;
			}
			
			return TxBuffer;
		}
		
		public static byte[] REJECT_TRADE ()
		{
			byte []	TxBuffer = new byte[3];
			TxBuffer[0]=34;
			TxBuffer[1]=0x01;
			TxBuffer[2]=0x00;
			
			return TxBuffer;
		}
		
		public static byte[] SEND_MY_INVENTORY()
		{
			byte []	TxBuffer = new byte[3];
			TxBuffer[0]=0x12;
			TxBuffer[1]=0x01;
			TxBuffer[2]=0x00;

            return TxBuffer;
		}
		
		public static byte[] REMOVE_OBJECT_FROM_TRADE (byte pos, uint quantity)
		{
			byte[] quantityBytes = System.BitConverter.GetBytes(quantity);
			
			byte []	TxBuffer = new byte[8];
			TxBuffer[0]=37;
			TxBuffer[1]=0x06;
			TxBuffer[2]=0x00;
			TxBuffer[3]=pos;
			TxBuffer[4]=quantityBytes[0];
			TxBuffer[5]=quantityBytes[1];
			TxBuffer[6]=quantityBytes[2];
			TxBuffer[7]=quantityBytes[3];
				
			return TxBuffer;
		}
		public static byte[] PUT_OBJECT_ON_TRADE (UInt16 pos, UInt32 quantity)
        {
            return PUT_OBJECT_ON_TRADE(pos, quantity, false);
        }
        public static byte[] PUT_OBJECT_ON_TRADE(byte pos, UInt32 quantity)
        {
            return PUT_OBJECT_ON_TRADE(pos, quantity, false);
        }
        public static byte[] PUT_OBJECT_ON_TRADE(byte pos, UInt32 quantity, bool fromInventory)
        {
            byte[] TxBuffer = new byte[9];
            byte[] quantityBytes = System.BitConverter.GetBytes(quantity);
            TxBuffer[0] = 36;
            TxBuffer[1] = 0x07;
            TxBuffer[2] = 0x00;
            if (fromInventory == true || Settings.IsTradeBot)
                TxBuffer[3] = 1;
            else
                TxBuffer[3] = 2;
            TxBuffer[4] = pos;
            TxBuffer[5] = quantityBytes[0];
            TxBuffer[6] = quantityBytes[1];
            TxBuffer[7] = quantityBytes[2];
            TxBuffer[8] = quantityBytes[3];
            return TxBuffer;
        }
		
		public static byte[] PUT_OBJECT_ON_TRADE (UInt16 pos, UInt32 quantity, bool fromInventory)
		{
            byte[] quantityBytes = System.BitConverter.GetBytes(quantity);
            byte[] sendPos = System.BitConverter.GetBytes(pos);
			
			byte []	TxBuffer = new byte[10];
			TxBuffer[0]=36;
			TxBuffer[1]=0x08;
			TxBuffer[2]=0x00;
            if (fromInventory == true)
			    TxBuffer[3]=1;
            else
			    TxBuffer[3]=2;
            TxBuffer[4] = sendPos[0];
            TxBuffer[5] = sendPos[1];
			TxBuffer[6]=quantityBytes[0];
			TxBuffer[7]=quantityBytes[1];
			TxBuffer[8]=quantityBytes[2];
			TxBuffer[9]=quantityBytes[3];
				
			return TxBuffer;
		} // 22 6 0 pos quantity0 1 2 3
		
		public static byte[] DROP_ITEM(byte pos, UInt32 quantity)
		{
			byte[] quantityBytes = System.BitConverter.GetBytes(quantity);
			
			byte []	TxBuffer = new byte[8];
			TxBuffer[0]=22;
			TxBuffer[1]=0x06;
			TxBuffer[2]=0x00;
			TxBuffer[3]=pos;
            TxBuffer[4] = quantityBytes[0];
            TxBuffer[5] = quantityBytes[1];
            TxBuffer[6] = quantityBytes[2];
            TxBuffer[7] = quantityBytes[3];
			
			return TxBuffer;
		}

        public static byte[] USE_ITEM(int pos)
        {
            byte[] quantityBytes = System.BitConverter.GetBytes(pos);
            byte[] TxBuffer = new byte[5];
            TxBuffer[0] = 31;
            TxBuffer[1] = 0x03;
            TxBuffer[2] = 0x00;
            TxBuffer[3] = quantityBytes[0];
            TxBuffer[4] = 0x00;
            return TxBuffer;
        }
    }
}
