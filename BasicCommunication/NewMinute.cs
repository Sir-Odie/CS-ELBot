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
    public class new_minute
    {
        private TCPWrapper TheTCPWrapper;
        private MySqlManager TheMySqlManager;
        private Inventory TheInventory;

        public new_minute(TCPWrapper MyTCPWrapper, MySqlManager MyMySqlManager, Inventory MyInventory)
        {
            TheTCPWrapper = MyTCPWrapper;
            TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
            this.TheMySqlManager = MyMySqlManager;
            this.TheInventory = MyInventory;
            TheTCPWrapper.GotConnected += new TCPWrapper.GotConnectedEventHandler(OnGotConnected);
            TheTCPWrapper.GotDisconnected += new TCPWrapper.GotDisconnectedEventHandler(OnGotDisconnected);
        }

        private void do_new_minute(byte[] data)
        {
            int m,g,l;
            string str;
            
            //str[4] = c;
            if (data[0] != 0x05)
                return;

            m = System.BitConverter.ToInt16(data, 3) % 360;
            if ((m%60) == 0)
            {
                g = TheMySqlManager.SendHNHtoGM(Settings.botid);
                l = TheMySqlManager.SendHNHtoLocal(Settings.botid);
                if (g == 0 && l == 0)
                    return;
                if (g != 0)//send message via #gm
                {
                    str = "#gm " + System.Convert.ToChar(TheMySqlManager.newhourcolor(Settings.botid));
                    str += TheMySqlManager.hnhmessage(Settings.botid);
                    //"* * * Happy New Hour - Get mining * * *";
                    TheTCPWrapper.Send(CommandCreator.RAW_TEXT(str));
                }
                if (l != 0)
                {
                    str = TheMySqlManager.hnhmessage(Settings.botid);
                    if ( l == 1)
                    {
                        TheTCPWrapper.Send(CommandCreator.RAW_TEXT(str));
                    }
                    else // if l other than 0 or 1 send message to channel 'l'
                    {
                        TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#jc " + l));
                        TheTCPWrapper.Send(CommandCreator.RAW_TEXT("@@" + l + " " + str));
                        TheTCPWrapper.Send(CommandCreator.RAW_TEXT("#lc " + l));
                    }
                }
            }
            //return;
            
            {
                //might want to make this optional some how, a setting in the database perhaps
                int foodlevel, currenthealth, maxhealth;
                foodlevel = TheMySqlManager.foodlevel(Settings.botid);
                currenthealth = TheMySqlManager.currenthealth(Settings.botid);
                maxhealth = TheMySqlManager.maxhealth(Settings.botid);
                if (currenthealth < maxhealth && foodlevel < 10)
                {
                    int foodloc = (int)TheInventory.HaveItem("Fruits");
                    if (foodloc == -1)
                        foodloc = (int)TheInventory.HaveItem("Vegetables");
                    if (foodloc == -1)
                        foodloc = (int)TheInventory.HaveItem("potion of Feasting");
                    if (foodloc == -1)
                        return;
                    TheTCPWrapper.Send(CommandCreator.USE_ITEM(foodloc));
                    TheInventory.requestInventory();
                }
            }
        }
        public static DateTime serverTime  = new DateTime();
        public static DateTime getServerTime()
        {
            return serverTime;
        }
        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            if (MainClass.tryingToConnect)
            {
                return;
            }
            if (e.CommandBuffer[0] == 0x05) //new minute
            {
                if (serverTime == new DateTime())
                {
                    TheTCPWrapper.Send(CommandCreator.SYNC_CLOCK());
                }
                else
                {
                    serverTime = serverTime.AddMinutes(1);
                }
                do_new_minute(e.CommandBuffer);
            }
            if (e.CommandBuffer[0] == 0x00 && e.CommandBuffer[4] == 130)  //this means raw text
            {
                string MessageBuffer = System.Text.ASCIIEncoding.ASCII.GetString(e.CommandBuffer, 5, e.CommandBuffer.Length - 5);
                if (MessageBuffer.Contains("Game Time"))
                {
                    string [] messageArray = MessageBuffer.Split(':');
                    string dateTimeString = "";
                    for (int i = 1;i<messageArray.Length;i++)
                    {
                        if (i < messageArray.Length - 1)
                        {
                            dateTimeString += messageArray[i];
                            dateTimeString += ":";
                        }
                        else
                        {
                            dateTimeString += "00";
                            break;
                        }
                    }
                    serverTime = Convert.ToDateTime(dateTimeString);
                }
            }
        }
        private void OnGotConnected(object sender, EventArgs e)
        {
            serverTime = new DateTime();
        }
        private void OnGotDisconnected(object sender, EventArgs e)
        {
            serverTime = new DateTime();
            //uhhh, not sure what to do if not connected, lol...
        }
    }
}
