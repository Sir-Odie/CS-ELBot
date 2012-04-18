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
	/// description of PMHandler.
	/// </summary>
	public class PMHandler
	{
		private TCPWrapper TheTCPWrapper;
        private MySqlManager TheMySqlManager;
		private BasicCommunication.MessageParser TheMessageParser;
		private System.Collections.ArrayList CommandArrayList = new System.Collections.ArrayList();
		
		public PMHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, MySqlManager MyMySqlManager)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheMessageParser = MyMessageParser;
            this.TheMySqlManager = MyMySqlManager;
			CommandArrayList.Clear();
			TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
		}
		
		private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
		{
			if (e.username.ToLower()=="gossip")
                return; // Do not echo gossip feed
            string Message = e.Message;
			
			if (Message[0]!='#')
			{
				Message = "#" + Message;
			}
			
			string[] CommandArray = Message.Split(' ');
            string pmList = TheMySqlManager.pmmonitorlist(Settings.botid);
            string[] names = pmList.Split('|');
            if (pmList.ToLower().Contains(e.username.ToLower()))
            {
                MySqlManager.setPlayerOnline(e.username);
            }
            if (CommandArrayList.Contains(CommandArray[0].ToLower()) == false)
            {
                foreach (string name in names)
                {
                    if (Message.ToLower().Contains("afk") || Message.ToLower().Contains("automessage"))
                    {
                        continue;
                    }
                    else if (name.ToLower() == e.username.ToLower())
                    {
                        if (Message.Length > 79)
                        {
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(name.Trim(), "Unknown command " + Message.Substring(0, 80).Trim()));
                        }
                        else
                        {
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(name.Trim(), "Unknown command " + Message));
                        }
                        return;
                    }
                }

                if (TheMySqlManager.senderrorpms(Settings.botid))
                {
                    foreach (string name in names)
                    {
                   	if (Message.ToLower().Contains("afk") || Message.ToLower().Contains("automessage"))
               	     	{
                        	continue;
                    	}
                        //fix that pm error message here...
                        if (!MySqlManager.playerIsOnline(name))
                        {
                            //Console.WriteLine(PMArray[0] + "is not online!");
                            continue;
                        }
                        if (Message.Length > 79)
                        {
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(name, "Unknown command received from " + e.username + ": " + Message.Substring(0,80).Trim()));

                        }
                        else
                        {
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(name.Trim(), "Unknown command received from " + e.username + ": " + Message.Trim()));
                        }
                    }
                }

                if (!TheMySqlManager.isBot(e.username) && !(Message.ToLower().Contains("afk") || Message.ToLower().Contains("automessage")))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "To see the list of available commands send me an ingame message with #help (Just type in /" + Settings.Loginname + " #help )"));
                }
            }
            else
            {
                if (TheMySqlManager.sendreceivedpms(Settings.botid))
                {

                    if (Message.ToLower().Contains("afk") || Message.ToLower().Contains("automessage"))
                    {
                    }
                    else
                    {
                        if (!TheMySqlManager.isBot(e.username) && !(Message.ToLower().Contains("buy") || Message.ToLower().Contains("give")))
                        {
                            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "Hello " + e.username + ", my name is " + Settings.Loginname + ". " + this.TheMySqlManager.welcomedescription(Settings.botid).Trim()));
                        } 
//                        string[] names = TheMySqlManager.pmmonitorlist(Settings.botid).Split('|');
                        foreach (string name in names)
                        {
                            if (!MySqlManager.playerIsOnline(name))
                            {
                                //Console.WriteLine(PMArray[0] + "is not online!");
                                continue;
                            }

                            if (e.username.Trim().ToLower() != name.Trim().ToLower())
                            {
                                TheTCPWrapper.Send(CommandCreator.SEND_PM(name.Trim(), "PM received from " + e.username + ": " + Message.Trim()));
                            }
                        }
                    }
                }
            }
        }
		
		public void AddCommand(string Command)
		{
            CommandArrayList.Add(Command);
		}
		
		public void RemoveCommand(string Command)
		{
			CommandArrayList.Remove(Command);
		}
	}
}
