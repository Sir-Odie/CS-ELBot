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

namespace cs_elbot
{
    /// <summary>
    /// description of Inventory.
    /// </summary>

    public class Stats
    {
        // OnGotNewStatsList
        public delegate void GotNewStatsListEventHandler(object sender, GotNewStatsListEventArgs e);

        public class GotNewStatsListEventArgs : EventArgs
        {
        }

        private TCPWrapper TheTCPWrapper;
        private Logger TheLogger;
        private MySqlManager TheMySqlManager;
        public int MyCapacity;
        public int MyCurrentCarryingAmt;

        public Stats(TCPWrapper MyTCPWrapper, Logger MyLogger, MySqlManager MyMySqlManager)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
            this.TheLogger = MyLogger;
            this.TheMySqlManager = MyMySqlManager;
        }

        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            if (e.CommandBuffer[0] == 18)
            {
                HERE_YOUR_STATS(e.CommandBuffer);
            }
            if (e.CommandBuffer[0] == 49)
            {
                SEND_PARTIAL_STAT(e.CommandBuffer);
            }
            if (e.CommandBuffer[0] == 42)
            {
                GET_YOUR_SIGILS(e.CommandBuffer);
            }
        }

        private void HERE_YOUR_STATS(byte[] data)
        {
            int i = 3;
            i += 80;

            
            MyCurrentCarryingAmt = System.BitConverter.ToUInt16(data, i); i += 2;
            MyCapacity = System.BitConverter.ToUInt16(data, i); i += 2;
	    TheLogger.Log("Got carry capacity: " + MyCurrentCarryingAmt + "/" + MyCapacity);
            TheMySqlManager.UpdateAllStats(Settings.botid, data);
        }

        private void SEND_PARTIAL_STAT(byte[] data)
        {
            string str = "";
            byte name;
            int value;
            int i = 3, j = data.Length;

            while (i < j)
            {
                name = data[i];
                value = System.BitConverter.ToInt32(data, i + 1);
                i = i + 5;
                switch (name)
                {
                    case 0:
                        str = "physiquecur";
                        break;
                    case 1:
                        str = "physiquebase";
                        break;
                    case 2:
                        str = "coordinationcur";
                        break;
                    case 3:
                        str = "coordinationbase";
                        break;
                    case 4:
                        str = "reasoningcur";
                        break;
                    case 5:
                        str = "reasoningbase";
                        break;
                    case 6:
                        str = "willcur";
                        break;
                    case 7:
                        str = "willbase";
                        break;
                    case 8:
                        str = "instinctcur";
                        break;
                    case 9:
                        str = "instinctbase";
                        break;
                    case 10:
                        str = "vitalitycur";
                        break;
                    case 11:
                        str = "vitalitybase";
                        break;
                    case 12:
                        str = "humannexuscur";
                        break;
                    case 13:
                        str = "humannexusbase";
                        break;
                    case 14:
                        str = "animalnexuscur";
                        break;
                    case 15:
                        str = "animalnexusbase";
                        break;
                    case 16:
                        str = "vegetalnexuscur";
                        break;
                    case 17:
                        str = "vegetalnexusbase";
                        break;
                    case 18:
                        str = "inorganicnexuscur";
                        break;
                    case 19:
                        str = "inorganicnexusbase";
                        break;
                    case 20:
                        str = "artificialnexuscur";
                        break;
                    case 21:
                        str = "artificialnexusbase";
                        break;
                    case 22:
                        str = "magicnexuscur";
                        break;
                    case 23:
                        str = "magicnexusbase";
                        break;
                    case 24:
                        str = "manufacturingskillcur";
                        break;
                    case 25:
                        str = "manufacturingskillbase";
                        break;
                    case 26:
                        str = "harvestingskillcur";
                        break;
                    case 27:
                        str = "harvestingskillbase";
                        break;
                    case 28:
                        str = "alchemyskillcur";
                        break;
                    case 29:
                        str = "alchemyskillbase";
                        break;
                    case 30:
                        str = "overallskillcur";
                        break;
                    case 31:
                        str = "overallskillbase";
                        break;
                    case 32:
                        str = "defenseskillcur";
                        break;
                    case 33:
                        str = "defenseskillbase";
                        break;
                    case 34:
                        str = "attackskillcur";
                        break;
                    case 35:
                        str = "attackskillbase";
                        break;
                    case 36:
                        str = "magicskillcur";
                        break;
                    case 37:
                        str = "magicskillbase";
                        break;
                    case 38:
                        str = "potionskillcur";
                        break;
                    case 39:
                        str = "potionskillbase";
                        break;
                    case 40:
                        str = "carrycapacitycur";
                        MyCurrentCarryingAmt = value;
			TheLogger.Log("Got carry capacity: " + MyCurrentCarryingAmt + "/" + MyCapacity);
                        break;
                    case 41:
                        str = "carrycapacitybase";
                        MyCapacity = value;
                        break;
                    case 42:
                        str = "materialpointscur";
                        break;
                    case 43:
                        str = "materialpointsbase";
                        break;
                    case 44:
                        str = "etherealpointscur";
                        break;
                    case 45:
                        str = "etherealpointsbase";
                        break;
                    case 46:
                        str = "foodlevel";
                        break;
                    case 47:
                        str = "researching";
                        break;
                    case 49:
                        str = "manufacturingexp";
                        break;
                    case 50:
                        str = "manufacturingexpnextlevel";
                        break;
                    case 51:
                        str = "harvestingexp";
                        break;
                    case 52:
                        str = "harvestingexpnextlevel";
                        break;
                    case 53:
                        str = "alchemyexp";
                        break;
                    case 54:
                        str = "alchemyexpnextlevel";
                        break;
                    case 55:
                        str = "overallexp";
                        break;
                    case 56:
                        str = "overallexpnextlevel";
                        break;
                    case 57:
                        str = "defenseexp";
                        break;
                    case 58:
                        str = "defenseexpnextlevel";
                        break;
                    case 59:
                        str = "attackexp";
                        break;
                    case 60:
                        str = "attackexpnextlevel";
                        break;
                    case 61:
                        str = "magicexp";
                        break;
                    case 62:
                        str = "magicexpnextlevel";
                        break;
                    case 63:
                        str = "potionexp";
                        break;
                    case 64:
                        str = "potionexpnextlevel";
                        break;
                    case 65:
                        str = "researchcompleted";
                        break;
                    case 66:
                        str = "researchTotal";
                        break;
                    case 67:
                        str = "summoningexp";
                        break;
                    case 68:
                        str = "summoningexpnextlevel";
                        break;
                    case 69:
                        str = "summoningskillcur";
                        break;
                    case 70:
                        str = "summoningskillbase";
                        break;
                    case 71:
                        str = "craftingexp";
                        break;
                    case 72:
                        str = "craftingexpnextlevel";
                        break;
                    case 73:
                        str = "craftingskillcur";
                        break;
                    case 74:
                        str = "craftingskillbase";
                        break;
                    case 75:
                        str = "engineeringexp";
                        break;
                    case 76:
                        str = "engineeringexpnextlevel";
                        break;
                    case 77:
                        str = "engineeringskillcur";
                        break;
                    case 78:
                        str = "engineeringskillbase";
                        break;
                    case 83:
                        str = "tailoringexp";
                        break;
                    case 84:
                        str = "tailoringexpnextlevel";
                        break;
                    case 85:
                        str = "tailoringskillcur";
                        break;
                    case 86:
                        str = "tailoringskillbase";
                        break;
                    default:
                        break;
                }
                if (str.Length > 0)
                {
                    TheMySqlManager.UpdateSingleStat(str, value);
                }
            }
        }

        private void GET_YOUR_SIGILS(byte[] data)
        {
            int sigils_we_have = System.BitConverter.ToInt32(data, 3);
            int i;
            int po2 = 1;
            string sigilsWeHave = "";

            // the first 32 sigils (mostly ripped off from client code, what we're doing is bitwise comparrison to see if the bit is on or off...)
            for (i = 0; i < 32; i++)
            {
                //Console.WriteLine("i = : " + i + " " + ((sigils_we_have & po2)> 0));
                if ((sigils_we_have & po2) > 0) 
                {
                    sigilsWeHave += "(" + Settings.botid + "," + i + ")";
                }
                po2 *= 2;
            }

            if (data.Length > 7)
            {
                // the next optional sigils
                po2 = 1;
                int sigils2 = System.BitConverter.ToInt32(data, 7);
                for (i = 32; i < 64; i++)
                {
                    //Console.WriteLine("i = : " + i + " " + ((sigils2 & po2) > 0));
                    if ((sigils2 & po2) > 0)
                    {
                        sigilsWeHave += "(" + Settings.botid + "," + i + ")";
                    }
                    po2 *= 2;
                }
            }
            if (sigilsWeHave == "")
            {
                sigilsWeHave = "No Sigils";
            }
            else
            {
                TheMySqlManager.writeSigils(sigilsWeHave);
            }
            //Console.WriteLine("Sigils: " + sigilsWeHave);
        }
    }
}
