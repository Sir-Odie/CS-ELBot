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

namespace cs_elbot.Logging
{
	/// <summary>
	/// description of CommandDebugger.
	/// </summary>
	public class CommandLogger
	{
		private TCPWrapper TheTCPWrapper;
		private Logger TheLogger;
        public bool RxDebug = false;
        public bool TxDebug = false;
        bool debug = false;
		
		public CommandLogger(TCPWrapper MyTCPWrapper, Logger MyLogger)
		{
            if (debug)
            {
                RxDebug = true;
                TxDebug = true;
            }
            this.TheLogger = MyLogger;
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotRXData);
			this.TheTCPWrapper.SentCommand += new TCPWrapper.SentCommandEventHandler(OnGotTXData);
			this.TheTCPWrapper.GotConnected += new TCPWrapper.GotConnectedEventHandler(OnGotConnected);
			this.TheTCPWrapper.GotDisconnected += new TCPWrapper.GotDisconnectedEventHandler(OnGotDisconnected);
		}
		
		private void OnGotConnected(object sender, EventArgs e)
		{	
			TheLogger.Log("Connected to server");
		}
		
		private void OnGotDisconnected(object sender, EventArgs e)
		{
			TheLogger.Log("Disconnected from server");
		}
		
		private void OnGotRXData(object sender, TCPWrapper.GotCommandEventArgs e)
		{
			if (RxDebug==false) return;
			
			string CmdString = ConvertByteCommandToString_ToClient(e.CommandBuffer[0]);
//            if (e.CommandBuffer[0] != 47 && e.CommandBuffer[0] != 69)
            {
                TheLogger.Log("Received Command:" + CmdString + " Bytes: " + e.CommandBuffer.Length.ToString());
            }
        }
		
		private void OnGotTXData(object sender, TCPWrapper.SentCommandEventArgs e)
		{
			if (TxDebug==false) return;
			
			string CmdString = ConvertByteCommandToString_ToServer(e.CommandBuffer[0]);
//            if (e.CommandBuffer[0] != 67 && e.CommandBuffer[0] != 68 && e.CommandBuffer[0] != 69 && e.CommandBuffer[0] != 44 && e.CommandBuffer[0] != 47)
            {
                TheLogger.Log("Send Command:" + CmdString + " Bytes: " + e.CommandBuffer.Length.ToString());
            }
		}
		
		private string ConvertByteCommandToString_ToServer (byte cmd)
		{
			int i;
			string[] CmdArray = new string[256];
			
			for (i=0;i<256;i++)
			{
				CmdArray[i]="UNKNOWN "+i.ToString();
			}
			CmdArray[0]="RAW_TEXT 0";
			CmdArray[1]="MOVE_TO 1";
			CmdArray[2]="SEND_PM 2";
			CmdArray[5]="GET_PLAYER_INFO 5";
			CmdArray[6]="RUN_TO 6";
			CmdArray[7]="SIT_DOWN 7";
			CmdArray[8]="SEND_ME_MY_ACTORS 8";
			CmdArray[9]="SEND_OPENING_SCREEN 9";
			CmdArray[10]="SEND_VERSION 10";
			CmdArray[11]="TURN_LEFT 11";
			CmdArray[12]="TURN_RIGHT 12";
			CmdArray[13]="PING 13";
			CmdArray[14]="HEART_BEAT 14";
			CmdArray[15]="LOCATE_ME 15";
			CmdArray[16]="USE_MAP_OBJECT 16";
			CmdArray[17]="SEND_MY_STATS 17";
			CmdArray[18]="SEND_MY_INVENTORY 18";
			CmdArray[19]="LOOK_AT_INVENTORY_ITEM 19";
			CmdArray[20]="MOVE_INVENTORY_ITEM 20";
			CmdArray[21]="harveST 21";						
			CmdArray[22]="DROP_ITEM 22";
			CmdArray[23]="PICK_UP_ITEM 23";
			CmdArray[24]="LOOK_AT_GROUND_ITEM 24";
			CmdArray[23]="HERE_YOUR_GROUND_ITEMS 23";
			CmdArray[25]="INSPECT_BAG 25";
			CmdArray[26]="S_CLOSE_BAG 26";
			CmdArray[27]="LOOK_AT_MAP_OBJECT 27";
			CmdArray[28]="TOUCH_PLAYER 28";
			CmdArray[29]="RESPOND_TO_NPC 29";
			CmdArray[30]="manufACTURE_THIS 30";
            CmdArray[31] = "USE_INVENTORY_ITEM 31";
            CmdArray[32] = "TRADE_WITH 32";
			CmdArray[33]="ACCEPT_TRADE 33";
			CmdArray[34]="REJECT_TRADE 34";
			CmdArray[35]="EXIT_TRADE 35";
			CmdArray[36]="PUT_OBJECT_ON_TRADE 36";
			CmdArray[37]="REMOVE_OBJECT_FROM_TRADE 37";
			CmdArray[38]="LOOK_AT_TRADE_ITEM 38";
			CmdArray[39]="CAST_SPELL 39";
			CmdArray[40]="attacK_SOMEONE 40";
			CmdArray[41]="GET_KNOWLEDGE_INFO 41";
			CmdArray[42]="ITEM_ON_ITEM 42";
			CmdArray[43]="SEND_BOOK 43";
			CmdArray[44]="GET_STORAGE_CATEGORY 44";
			CmdArray[45]="DEPOSITE_ITEM 45";
			CmdArray[46]="WITHDRAW_ITEM 46";
			CmdArray[47]="LOOK_AT_STORAGE_ITEM 47";
			CmdArray[48]="SPELL_name 48";
			CmdArray[60]="PING_RESPONSE 60";
			CmdArray[61]="SET_ACTIVE_CHANNEL 61";
			CmdArray[140]="LOG_IN 140";
			CmdArray[141]="CREATE_CHAR 141";
			CmdArray[230]="GET_DATE 230";
			CmdArray[231]="GET_TIME 231";
			CmdArray[232]="SERVER_STATS 232";
			CmdArray[233]="ORIGINAL_IP 233";
			
			return CmdArray[(int)cmd];
		}


        private string ConvertByteCommandToString_ToClient(byte cmd)
        {
            int i;
            string[] CmdArray = new string[256];

            for (i = 0; i < 256; i++)
            {
                CmdArray[i] = "UNKNOWN " + i.ToString();
            }
            CmdArray[0] = "RAW_TEXT 0";
            CmdArray[1] = "ADD_NEW_ACTOR 1";
            CmdArray[2] = "ADD_ACTOR_COMMAND 2";
            CmdArray[3] = "YOU_ARE 3";
            CmdArray[4] = "SYNC_CLOCK 4";
            CmdArray[5] = "NEW_MINUTE 5";
            CmdArray[6] = "REMOVE_ACTOR 6";
            CmdArray[7] = "CHANGE_MAP 7";
            CmdArray[8] = "COMBAT_MODE 8";
            CmdArray[9] = "KILL_ALL_ACTORS 9";
            CmdArray[10] = "GET_TELEPORTERS_LIST 10";
            CmdArray[11] = "PONG 11";
            CmdArray[12] = "TELEPORT_IN 12";
            CmdArray[13] = "TELEPORT_OUT 13";
            CmdArray[14] = "PLAY_SOUND 14";
            CmdArray[15] = "START_RAIN 15";
            CmdArray[16] = "STOP_RAIN 16";
            CmdArray[17] = "THUNDER 17";
            CmdArray[18] = "HERE_YOUR_STATS 18";
            CmdArray[19] = "HERE_YOUR_INVENTORY 19";
            CmdArray[20] = "INVENTORY_ITEM_TEXT 20";
            CmdArray[21] = "GET_NEW_INVENTORY_ITEM 21";
            CmdArray[22] = "REMOVE_ITEM_FROM_INVENTORY 22";
            CmdArray[23] = "HERE_YOUR_GROUND_ITEMS 23";
            CmdArray[24] = "GET_NEW_GROUND_ITEM 24";
            CmdArray[25] = "REMOVE_ITEM_FROM_GROUND 25";
            CmdArray[26] = "CLOSE_BAG 26";
            CmdArray[27] = "GET_NEW_BAG 27";
            CmdArray[28] = "GET_BAGS_LIST 28";
            CmdArray[29] = "DESTROY_BAG 29";
            CmdArray[30] = "NPC_TEXT 30";
            CmdArray[31] = "NPC_OPTIONS_LIST 31";
            CmdArray[32] = "CLOSE_NPC_MENU 32";
            CmdArray[33] = "SEND_NPC_INFO 33";
            CmdArray[34] = "GET_TRADE_INFO 34";
            CmdArray[35] = "GET_TRADE_OBJECT 35";
            CmdArray[36] = "GET_TRADE_ACCEPT 36";
            CmdArray[37] = "GET_TRADE_REJECT 37";
            CmdArray[38] = "GET_TRADE_EXIT 38";
            CmdArray[39] = "REMOVE_TRADE_OBJECT 39";
            CmdArray[40] = "GET_YOUR_TRADEOBJECTS 40";
            CmdArray[41] = "GET_TRADE_PARTNER_name 41";
            CmdArray[42] = "GET_YOUR_SIGILS 42";
            CmdArray[43] = "SPELL_ITEM_TEXT 43";
            CmdArray[44] = "GET_ACTIVE_SPELL 44";
            CmdArray[45] = "GET_ACTIVE_SPELL_LIST 45";
            CmdArray[46] = "REMOVE_ACTIVE_SPELL 46";
            CmdArray[47] = "GET_ACTOR_DAMAGE 47";
            CmdArray[48] = "GET_ACTOR_HEAL 48";
            CmdArray[49] = "SEND_PARTIAL_STAT 49";
            CmdArray[50] = "SPAWN_BAG_PARTICLES 50";
            CmdArray[51] = "ADD_NEW_ENHANCED_ACTOR 51";
            CmdArray[52] = "ACTOR_WEAR_ITEM 52";
            CmdArray[53] = "ACTOR_UNWEAR_ITEM 53";
            CmdArray[54] = "PLAY_MUSIC 54";
            CmdArray[55] = "GET_KNOWLEDGE_LIST 55";
            CmdArray[56] = "GET_NEW_KNOWLEDGE 56";
            CmdArray[57] = "GET_KNOWLEDGE_TEXT 57";
            CmdArray[59] = "BUDDY_EVENT 59";
            CmdArray[60] = "PING_REQUEST 60";
            CmdArray[61] = "FIRE_PARTICLES 61";
            CmdArray[62] = "REMOVE_FIRE_AT 62";
            CmdArray[63] = "DISPLAY_CLIENT_WINDOW 63";
            CmdArray[64] = "OPEN_BOOK 64";
            CmdArray[65] = "READ_BOOK 65";
            CmdArray[66] = "CLOSE_BOOK 66";
            CmdArray[67] = "STORAGE_LIST 67";
            CmdArray[68] = "STORAGE_ITEMS 68";
            CmdArray[69] = "STORAGE_TEXT 69";
            CmdArray[70] = "SPELL_CAST 70";
            CmdArray[71] = "GET_ACTIVE_CHANNELS 71";
            CmdArray[72] = "MAP_FLAGS 72";
            CmdArray[73] = "GET_ACTOR_HEALTH 73";
            CmdArray[74] = "GET_3D_OBJ_LIST 74";
            CmdArray[75] = "GET_3D_OBJ 75";
            CmdArray[76] = "REMOVE_3D_OBJ 76";
            CmdArray[77] = "GET_ITEMS_COOLDOWN 77";
            CmdArray[78] = "SEND_BUFFS 78";
            CmdArray[79] = "SEND_SPECIAL_EFFECT 79";
            CmdArray[80] = "REMOVE_MINE 80";
            CmdArray[81] = "GET_NEW_MINE 81";
            CmdArray[82] = "GET_MINES_LIST 82";
            CmdArray[220] = "MAP_SET_OBJECTS 220";
            CmdArray[221] = "MAP_STATE_OBJECTS 221";
            CmdArray[240] = "UPGRADE_NEW_VERSION 240";
            CmdArray[241] = "UPGRADE_TOO_OLD 241";
            CmdArray[248] = "REDEFINE_YOUR_COLORS 248";
            CmdArray[249] = "YOU_DONT_EXIST 249";
            CmdArray[250] = "LOG_IN_OK 250";
            CmdArray[251] = "LOG_IN_NOT_OK 251";
            CmdArray[252] = "CREATE_CHAR_OK 252";
            CmdArray[253] = "CREATE_CHAR_NOT_OK 253";
            return CmdArray[(int)cmd];
        }
	}
}
