// Eternal Lands Bot
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
    public class changeMap
    {
        private TCPWrapper TheTCPWrapper;

        public changeMap(TCPWrapper MyTCPWrapper)
        {
            TheTCPWrapper = MyTCPWrapper;
            TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
        }

        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            if (e.CommandBuffer[0] == 0x07) //CHANGE_MAP
            {
                MainClass.mapName = System.Text.ASCIIEncoding.ASCII.GetString(e.CommandBuffer, 5, e.CommandBuffer.Length - 6);
                if (!AdvancedCommunication.GotoCommandHandler.gettingLocationInfo)
                {
                    AdvancedCommunication.GotoCommandHandler.gettingLocationInfo = true;
                    TheTCPWrapper.Send(CommandCreator.LOCATE_ME());
                }
            }
        }
    }
}
