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
	public class OpenStorage
	{
		private TCPWrapper TheTCPWrapper;
        private ActorHandler TheActorHandler;
        private MySqlManager TheMySqlManager;
        private BasicCommunication.MessageParser TheMessageParser;
        private AdminHelpCommandHandler TheAdminHelpCommandHandler;
        private HelpCommandHandler TheHelpCommandHandler;
        
		public OpenStorage(HelpCommandHandler MyHelpCommandHandler, TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, MySqlManager MyMySqlManager, ActorHandler MyActorHandler,AdminHelpCommandHandler MyAdminHelpCommandHandler)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMySqlManager = MyMySqlManager;
            this.TheActorHandler = MyActorHandler;
            this.TheMessageParser = MyMessageParser;
            this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
            this.TheHelpCommandHandler = MyHelpCommandHandler;

            TheAdminHelpCommandHandler.AddCommand("#openstorage / #os - Open storage");
            TheHelpCommandHandler.AddCommand("#os - null");
            TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
        }

        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {
            string Message = e.Message.ToLower();

            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#openstorage" || CommandArray[0] == "#os")
            {
                if (CommandArray.Length > 2)
                    goto WrongArguments;
                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#openstorage", Settings.botid);
                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }
                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#openstorage", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                string npcName = "";
                if (CommandArray.Length > 1)
                {
                    npcName = CommandArray[1];
                }
                if (npcName == "")
                {
                    //get the npc name from their storage selection...
                    npcName = TheMySqlManager.getStorageNPCName();
                }
                int storageActorID = TheActorHandler.GetUserIDFromname(npcName);
                ActorHandler.Actor myActor = (ActorHandler.Actor)TheActorHandler.ActorsHashTable[TheActorHandler.GetUserIDFromname(npcName)];
                try
                {
                    if (myActor.kind_of_actor == 2)
                    {
                        TheTCPWrapper.Send(CommandCreator.TOUCH_PLAYER((uint)storageActorID));
                        TheTCPWrapper.Send(CommandCreator.RESPOND_TO_NPC((ushort)storageActorID));
                        TradeHandler.openingStorage = true;
                        TradeHandler.storageOpen = false;
                        TradeHandler.username = e.username;
                        Settings.IsTradeBot = false;
                    }
                }
                catch
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "It appears that " + npcName + " is not a storage NPC! (or I'm not in range...)"));
                }
            }
            return;
        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Here is the usage of the #move command:   "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[#openstorage <name>                       "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #openstorage                     "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #openstorage raven               "));
            return;
        }
	}
}
