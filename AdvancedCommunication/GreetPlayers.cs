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
	public class GreetPlayers
	{
		private TCPWrapper TheTCPWrapper;
        private ActorHandler TheActorHandler;
        private MySqlManager TheMySqlManager;
        
		public GreetPlayers(TCPWrapper MyTCPWrapper, MySqlManager MyMySqlManager, ActorHandler MyActorHandler)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMySqlManager = MyMySqlManager;
            this.TheActorHandler = MyActorHandler;
            
            this.TheActorHandler.AddNewActor += new ActorHandler.AddNewActorEventHandler (OnAddNewActor);
		}
		
		private void OnAddNewActor(object sender, ActorHandler.AddNewActorEventArgs e)
		{
			string greeting = TheMySqlManager.Getgreeting(e.NewActor.name,Settings.botid);
			
			if (greeting!="")
			{
                //don't greet invisible players
                if (e.NewActor.buffs != 0)
                    return;
				TheTCPWrapper.Send(CommandCreator.RAW_TEXT(greeting));
                TheMySqlManager.updateGreeting(e.NewActor.name, Settings.botid);
			}
		}
	}
}
