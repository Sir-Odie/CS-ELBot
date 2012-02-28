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

namespace cs_elbot.AdvancedCommunication
{
	/// <summary>
	/// description of GreetPlayers.
	/// </summary>
	public class AssessThreat
	{
		private TCPWrapper TheTCPWrapper;
        private ActorHandler TheActorHandler;
        private MySqlManager TheMySqlManager;
        
		public AssessThreat(TCPWrapper MyTCPWrapper, MySqlManager MyMySqlManager, ActorHandler MyActorHandler)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMySqlManager = MyMySqlManager;
            this.TheActorHandler = MyActorHandler;
            
            this.TheActorHandler.AddNewActor += new ActorHandler.AddNewActorEventHandler (OnAddNewActor);
		}
        private int userRank = 0;
		private void OnAddNewActor(object sender, ActorHandler.AddNewActorEventArgs e)
		{
            if (MainClass.botType != 3)
            {
                //shouldn't be here anyways, since we're not creating this object if it's not a guard bot, but just in case, lol
                return;
            }
            if (e.NewActor.id == TheActorHandler.MyActorID)
            {
                return;
            }
            userRank = TheMySqlManager.GetUserRank(e.NewActor.name, Settings.botid);
            string tempText = "";
            if (userRank < -2)
            {
                userRank = -2;
            }
            switch (userRank)
            {
                case -2:
                    //send out a message that will be picked up by the guardbot handler
                    //send message to guild/owners if specified.... (have to think this out...)
                    //kill on sight
                    tempText = e.NewActor.name + ", you are kill on sight and must be terminated!";
                    break;
                case -1:
                    //check the timer to see if they should be killed, if so, change rank to -2
                    //send messages...
                    tempText = e.NewActor.name + ", you are a threat and will be terminated if you continue to tresspass!";
                    break;
                case 0:
                    //give a warning and set the timer, change rank to -1
                    tempText = e.NewActor.name + ", you are tresspassing, if you continue to do so you will be designated for termination!";
                    break;
                default:
                    //check the timer, if it's expired, switch to rank 0
                    //otherwise don't do a thing...
                    tempText = e.NewActor.name + ", you are not a threat.  You may pass.";
                    break;
            }
            if (tempText != "")
            {
                TheTCPWrapper.Send(CommandCreator.RAW_TEXT(tempText));
                System.Threading.Thread.Sleep(20);
            }
            string guild = TheMySqlManager.PlayerGuild(Settings.Loginname);
            tempText = "#gm " + e.NewActor.name + " has entered the field of play(rank: " + userRank + ")";
            if (guild == "0")
            {
                tempText = "FAKE " + tempText;

            }
            TheTCPWrapper.Send(CommandCreator.RAW_TEXT(tempText));
            TheActorHandler.setActorThreatLevel(e.NewActor, userRank);
        }
	}
}
