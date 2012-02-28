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
using System.Configuration;
using System.Reflection;

namespace cs_elbot
{
	/// <summary>
	/// description of Settings.
	/// </summary>
	public class Settings
	{
		// Server Settings
		public static string ServerHostname;
		public static int ServerPort;	

		// Login Settings
		public static string Loginname;
		public static string LoginPassword;
		
		// Bot settings
		public static int botid;

        //Settings from the database now...
        public static bool AutoReconnect;

        public static string LocationInfo;
		public static string locationdescription;
		public static string infodescription;
		public static string welcomedescription;

		public static bool IsTradeBot;
		public static int TradeTimeOut = 90;
		
		//public static bool guildfeedgossip=false;
		
		public static bool DebugingToFile = true;
		public static bool EventLogToFile = true;
		public static bool ChatLogToFile = true;
		public static bool PMLogToFile = true;
		public static bool enabledebuging = false;
		
		public static bool LogGotPM = true;
		public static bool LogSendPM = true;
		public static bool LogGlobalMessage = true;
		public static bool LogGossipFeed = false;
		
		private static Assembly assembly = Assembly.GetExecutingAssembly();
		public static string Version = assembly.GetName().Version.ToString();
        public static string URL = "";

		public Settings()
		{
			
		}

        public void LoadSettingsFromDatabase()
        {
        }

        public void LoadSqlSettingsFromConfigfile()
        {
            Console.WriteLine("SQL information not provided in parameters, using config file");
            System.Configuration.AppSettingsReader SettingsReader = new AppSettingsReader();
            MainClass.SqlServer = (string)SettingsReader.GetValue("SqlServer", typeof(string));
            MainClass.SqlUsername = (string)SettingsReader.GetValue("SqlUsername", typeof(string));
            MainClass.SqlPassword = (string)SettingsReader.GetValue("SqlPassword", typeof(string));
            MainClass.SqlDatabase = (string)SettingsReader.GetValue("SqlDatabase", typeof(string));
            MainClass.SqlPort = Int16.Parse((string)SettingsReader.GetValue("SqlPort", typeof(string)));
        }
		public void LoadSettings()
		{
			System.Configuration.AppSettingsReader SettingsReader = new AppSettingsReader();

            //Load settings for configuration file
            botid = int.Parse((string)SettingsReader.GetValue("botid", typeof(string)));


            Loginname = (string)SettingsReader.GetValue("Loginname", typeof(string));
            LoginPassword = (string)SettingsReader.GetValue("LoginPassword", typeof(string));

            AutoReconnect = bool.Parse((string)SettingsReader.GetValue("AutoReconnect", typeof(string)));

            ServerHostname = (string)SettingsReader.GetValue("ServerHostname", typeof(string));
            ServerPort = int.Parse((string)SettingsReader.GetValue("ServerPort", typeof(string)));

            if (ServerPort == 2000) //live
            {
                URL = "http://dogpound.embril.net/" + Loginname + ".php";
            }
            else if (ServerPort == 2002) //pk
            {
                URL = "http://dogpound.embril.net/pk/" + Loginname + ".php";
            }
            else if (ServerPort == 2001) //test
            {
                URL = "http://dogpound.embril.net/bot_test/" + Loginname + ".php";
            }
            else
            {
                URL = "Port not found, bug!";
            }


            IsTradeBot = bool.Parse((string)SettingsReader.GetValue("IsTradeBot", typeof(string)));
            //moved bot type to mysql, calling from maind...
            //if (IsTradeBot)
            //{
            //    MainClass.botType = 1;
            //}
            //else
            //{
            //    MainClass.botType = 2;
            //}
            TradeTimeOut = int.Parse((string)SettingsReader.GetValue("TradeTimeOut", typeof(string)));


            // Load debuging settings
            DebugingToFile = bool.Parse((string)SettingsReader.GetValue("DebugingToFile", typeof(string)));
            EventLogToFile = bool.Parse((string)SettingsReader.GetValue("EventLogToFile", typeof(string)));
            ChatLogToFile = bool.Parse((string)SettingsReader.GetValue("ChatLogToFile", typeof(string)));
            PMLogToFile = bool.Parse((string)SettingsReader.GetValue("PMLogToFile", typeof(string)));
            enabledebuging = bool.Parse((string)SettingsReader.GetValue("enabledebuging", typeof(string)));

            LogGotPM = bool.Parse((string)SettingsReader.GetValue("LogGotPM", typeof(string)));
            LogSendPM = bool.Parse((string)SettingsReader.GetValue("LogSendPM", typeof(string)));
            LogGlobalMessage = bool.Parse((string)SettingsReader.GetValue("LogGlobalMessage", typeof(string)));
            LogGossipFeed = bool.Parse((string)SettingsReader.GetValue("LogGossipFeed", typeof(string)));

            //guildfeedgossip = bool.Parse((string)SettingsReader.GetValue("guildfeedgossip", typeof(string)));

            //SqlServer = "192.168.0.1";
            //SqlUsername = "el-bot";
            //SqlPassword = "";
            //SqlDatabase = "testbot";
            //SqlPort = 3306;
            //botid = 1;
            //Loginname = "squeeky";
            //LoginPassword = "";
            //AutoConnect = true;
            //AutoReconnect = true;
            //ServerHostname = "game.eternal-lands.com";
            //ServerPort = 2001;
            //IsTradeBot = true;
            //TradeTimeOut = 90;
            //DebugingToFile = false;
            //EventLogToFile = true;
            //ChatLogToFile = false;
            //PMLogToFile = false;
            //enabledebuging = false;
            //LogGotPM = true;
            //LogSendPM = false;
            //LogGlobalMessage = false;
            //LogGossipFeed = false;
        }
	}
}
