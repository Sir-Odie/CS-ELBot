// Eternal Lands Bot
// Copyright (C) 2006  Artem Makhutov, 2008 John Godsey (dogbreath.el@gmail.com)
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
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace cs_elbot
{
	class MainClass
	{
        public static int ONE_MINUTE = 60000;
        public static string serverName = "";
        public static string SqlServer = "";
        public static string SqlUsername = "";
        public static string SqlPassword = "";
        public static string SqlDatabase = "";
        public static Int16 SqlPort;
        public static int botType = 0;
        public static bool atHome = false;
        public static bool gettingPerks = false;
        public static string mapName = "";
        public static bool tryingToConnect = false;
        public static bool launchedByBotManager = true;
	public static string launchPath = "";
        public struct homeInfo
        {
            public int x;
            public int y;
            public int heading;
            public string mapName;
        }
        public static homeInfo myHome = new homeInfo();
        //public static myCoords myCurrentCoords = new myCoords();

        //public struct myCoords
        //{
        //    public int x;
        //    public int y;
        //}
        public static void Main(string[] args)
		{
            myHome.x = 0;
            myHome.y = 0;
            myHome.heading = 0;
            myHome.mapName = "";
            // Create instance of the log file handler
			Logger MyLogger = new Logger();

            // Create instance of the TCP client
            TCPClient.TCPClient MyTCPClient = new TCPClient.TCPClient();

            // Initialize the TCPWrapper
			TCPWrapper MyTCPWrapper = new TCPWrapper(MyTCPClient, MyLogger);

            // set up working directory
            if (args.Length > 0)
            {
                string botName = args[0];
                launchPath = args[1];
                SqlServer = args[2];
                SqlPort = Convert.ToInt16(args[3]);
                SqlUsername = args[4];
                SqlPassword = args[5];
                SqlDatabase = args[6];
                Environment.CurrentDirectory = launchPath + botName;
            }
            if (Environment.CurrentDirectory.ToLower().Contains("Live".ToLower()))
            {
                serverName = "live";
            }
            else if (Environment.CurrentDirectory.ToLower().Contains("Test".ToLower()))
            {
                serverName = "test";
            }
            else if (Environment.CurrentDirectory.ToLower().Contains("PK".ToLower()))
            {
                serverName = "pk";
            }
            else
            {
                launchedByBotManager = false;
		launchPath = "C:\\bot\\test\\";
                Console.WriteLine("serverName not found in file path, defaulting to test");
                Console.WriteLine("path containing cs-elbot.exe should be something like c:/bot/test/botname/");
                Console.WriteLine("If you're not using bot manager, don't worry about this message");
            }

            Settings MySettings = new Settings();

            //Load Sql Configuration if not supplied in parameters
            if (SqlServer == "")
            {
                MySettings.LoadSqlSettingsFromConfigfile();
            }

            MySettings.LoadSettings();
            errorHandler MyErrorHandler = new errorHandler();

            //Setup the MySQL Database
            MySqlManager MyMySqlManager = new MySqlManager(SqlServer, SqlUsername, SqlPassword, SqlDatabase, SqlPort, MyLogger, MyTCPWrapper, MyErrorHandler);
            Settings.TradeTimeOut = MyMySqlManager.GetTimeoutInterval(Settings.botid);
            botType = MyMySqlManager.getBotType(Settings.botid);
            //Console.WriteLine("Bot type: " + botType);

            MyMySqlManager.ImStarted(Settings.botid);
            Settings.URL = MyMySqlManager.getURL() + Settings.Loginname + ".php";


            try
            {
                System.Net.IPHostEntry ServerIPHostEntry = System.Net.Dns.GetHostEntry(Settings.ServerHostname);
                MyTCPWrapper.HostIp = ServerIPHostEntry.AddressList[0];
                MyTCPWrapper.Port = Settings.ServerPort;

                Logging.CommandLogger MyCommandDebugger = new Logging.CommandLogger(MyTCPWrapper, MyLogger);

                //BasicCommunication.HeartBeat MyHeartBeat = new BasicCommunication.HeartBeat(MyTCPWrapper, MyErrorHandler);
                BasicCommunication.HeartBeat MyHeartBeat = new BasicCommunication.HeartBeat(MyTCPWrapper);
                BasicCommunication.Ping MyPing = new BasicCommunication.Ping(MyTCPWrapper);
                Inventory MyInventory = new Inventory(MyTCPWrapper, MyLogger, MyMySqlManager);
                Storage MyStorage = new Storage(MyTCPWrapper, MyLogger, MyMySqlManager);
                BasicCommunication.new_minute MyMewMinute = new BasicCommunication.new_minute(MyTCPWrapper, MyMySqlManager, MyInventory);

                // Login here
                BasicCommunication.Login MyLogin = new BasicCommunication.Login(MyTCPWrapper);
                BasicCommunication.RawText MyRawText = new BasicCommunication.RawText(MyTCPWrapper);
                BasicCommunication.MessageParser MyMessageParser = new BasicCommunication.MessageParser(MyRawText, MyLogger, MyTCPWrapper, MyMySqlManager);
                BasicCommunication.changeMap MyChangeMap = new BasicCommunication.changeMap(MyTCPWrapper);

                //if this is a storage bot (not a tradebot) start a shutdown timer
                if (Settings.IsTradeBot == false && botType == 2)
                {
                    BasicCommunication.Shutdown MyShutdown = new BasicCommunication.Shutdown(MyTCPWrapper, MyMessageParser, MyMySqlManager, MyErrorHandler);
                }

                Logging.LoginLogger MyLoginLogger = new Logging.LoginLogger(MyLogin, MyLogger, MyTCPWrapper, MyMySqlManager);
                Logging.MessageLogger MyMessageLogger = new Logging.MessageLogger(MyMessageParser, MyLogger, MyMySqlManager);

                AdvancedCommunication.ActorHandler MyActorHandler = new AdvancedCommunication.ActorHandler(MyTCPWrapper, MyLogger, MyMySqlManager);

                AdvancedCommunication.PMHandler MyPMHandler = new AdvancedCommunication.PMHandler(MyTCPWrapper, MyMessageParser, MyMySqlManager);
                AdvancedCommunication.HelpCommandHandler MyHelpCommandHandler = new AdvancedCommunication.HelpCommandHandler(MyTCPWrapper, MyMessageParser, MyPMHandler, MyMySqlManager);
                AdvancedCommunication.AdvHelpCommandHandler MyAdvHelpCommandHandler = new AdvancedCommunication.AdvHelpCommandHandler(MyTCPWrapper, MyMessageParser, MyPMHandler, MyMySqlManager, MyHelpCommandHandler);
                AdvancedCommunication.AdminHelpCommandHandler MyAdminHelpCommandHandler = new AdvancedCommunication.AdminHelpCommandHandler(MyTCPWrapper, MyMessageParser, MyPMHandler, MyMySqlManager, MyAdvHelpCommandHandler);
                AdvancedCommunication.SAYCommandHandler MySAYCommandHandler = new AdvancedCommunication.SAYCommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyLogger, MyPMHandler);
                AdvancedCommunication.PMCommandHandler MyPMCommandHandler = new AdvancedCommunication.PMCommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager);

                AdvancedCommunication.BuddyHandler MyBuddyHandler = new AdvancedCommunication.BuddyHandler(MyTCPWrapper, MyMessageParser, MyAdvHelpCommandHandler, MyMySqlManager, MyLogin);

                //get my stats
                Stats MyStats = new Stats(MyTCPWrapper, MyLogger, MyMySqlManager);


                TradeHandler MyTradeHandler = new TradeHandler(MyTCPWrapper, MyMessageParser, MyMySqlManager, MyInventory, MyActorHandler, MyLogger, MyStorage, MyErrorHandler, MyHelpCommandHandler);

                AdvancedCommunication.INVCommandHandler MyINVCommandHandler = new AdvancedCommunication.INVCommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyInventory, MyTradeHandler, MyStats);
                AdvancedCommunication.INVRCommandHandler MyINVRCommandHandler = new AdvancedCommunication.INVRCommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyInventory, MyTradeHandler, MyStats);
                AdvancedCommunication.STOCommandHandler MySTOCommandHandler = new AdvancedCommunication.STOCommandHandler(MyTCPWrapper, MyMessageParser, MyAdvHelpCommandHandler, MyMySqlManager, MyStorage, MyTradeHandler, MyStats);
                AdvancedCommunication.INVLISTCommandHandler MyINVLISTCommandHandler = new AdvancedCommunication.INVLISTCommandHandler(MyHelpCommandHandler, MyTCPWrapper, MyMessageParser, MyAdvHelpCommandHandler, MyMySqlManager, MyInventory, MyTradeHandler, MyStats);
                AdvancedCommunication.ReserveCommandHandler MyReserveCommandHandler = new AdvancedCommunication.ReserveCommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyInventory, MyTradeHandler, MyStats, MyStorage);

                AdvancedCommunication.GiveCommandHandler MyGiveCommandHandler = new AdvancedCommunication.GiveCommandHandler(MyTCPWrapper, MyMessageParser, MyAdvHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler, MyPMHandler);
                AdvancedCommunication.GiveToCommandHandler MyGiveToCommandHandler = new AdvancedCommunication.GiveToCommandHandler(MyTCPWrapper, MyMessageParser, MyAdvHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler, MyPMHandler);
                AdvancedCommunication.BUYCommandHandler MyBUYCommandHandler = new AdvancedCommunication.BUYCommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler);
                AdvancedCommunication.DESCRIBECommandHandler MyDESCRIBECommandHandler = new AdvancedCommunication.DESCRIBECommandHandler(MyTCPWrapper, MyMessageParser, MyAdvHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler, MyPMHandler);

                AdvancedCommunication.DONATECommandHandler MyDONATECommandHandler = new AdvancedCommunication.DONATECommandHandler(MyTCPWrapper, MyMessageParser, MyAdvHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler);
                AdvancedCommunication.PUTONSALECommandHandler MyPUTONSALECommandHandler = new AdvancedCommunication.PUTONSALECommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler);
                AdvancedCommunication.WANTEDCommandHandler MyWANTEDCommandHandler = new AdvancedCommunication.WANTEDCommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler, MyInventory, MyStats);
                AdvancedCommunication.LOCCommandHandler MyLOCCommandHandler = new AdvancedCommunication.LOCCommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyLogger);
                AdvancedCommunication.BANCommandHandler MyBANCommandHandler = new AdvancedCommunication.BANCommandHandler(MyHelpCommandHandler, MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyLogger);
                AdvancedCommunication.DropCommandHandler MyDropCommandHandler = new AdvancedCommunication.DropCommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler, MyInventory);
                AdvancedCommunication.LaunchCommandHandler MyLaunchCommandHandler = new AdvancedCommunication.LaunchCommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler, MyInventory);
                AdvancedCommunication.QuitCommandHandler MyQuitCommandHandler = new AdvancedCommunication.QuitCommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler, MyInventory);
                AdvancedCommunication.WearCommandHandler MyWearCommandHandler = new AdvancedCommunication.WearCommandHandler(MyActorHandler, MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler, MyInventory);
                AdvancedCommunication.UseItemCommandHandler MyUseItemCommandHandler = new AdvancedCommunication.UseItemCommandHandler(MyActorHandler, MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler, MyInventory);

                AdvancedCommunication.ABOUTCommandHandler MyABOUTCommandHandler = new AdvancedCommunication.ABOUTCommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyLogger);
                AdvancedCommunication.AFKCommandHandler MyAFKCommandHandler = new AdvancedCommunication.AFKCommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyLogger);
                AdvancedCommunication.ELTCCommandHandler MyELTCCommandHandler = new AdvancedCommunication.ELTCCommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyLogger);

                AdvancedCommunication.ACCEPTMECommandHandler MyACCEPTMECommandHandler = new AdvancedCommunication.ACCEPTMECommandHandler(MyTCPWrapper, MyMessageParser, MyAdvHelpCommandHandler, MyMySqlManager, MyLogger);
                AdvancedCommunication.KICKMECommandHandler MyKICKMECommandHandler = new AdvancedCommunication.KICKMECommandHandler(MyTCPWrapper, MyMessageParser, MyAdvHelpCommandHandler, MyMySqlManager, MyLogger);
                AdvancedCommunication.GTCommandHandler MyGTCommandHandler = new AdvancedCommunication.GTCommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyLogger);
                AdvancedCommunication.GuildTextHandler MyGuildTextHandler = new AdvancedCommunication.GuildTextHandler(MyRawText, MyTCPWrapper, MyMessageParser, MyMySqlManager, MyLogger);

                AdvancedCommunication.GreetPlayers MyGreetPlayers = new AdvancedCommunication.GreetPlayers(MyTCPWrapper, MyMySqlManager, MyActorHandler);
                AdvancedCommunication.AdvertHandler MyAdvertHandler = new AdvancedCommunication.AdvertHandler(MyTCPWrapper, MyMySqlManager, MyLogger, MyTradeHandler, MyInventory, MyErrorHandler);
                AdvancedCommunication.HouseKeepingHandler MyHouseKeepingHandler = new AdvancedCommunication.HouseKeepingHandler(MyTCPWrapper, MyMySqlManager, MyLogger, MyErrorHandler);
                AdvancedCommunication.AltAdvertHandler MyAltAdvertHandler = new AdvancedCommunication.AltAdvertHandler(MyTCPWrapper, MyMySqlManager, MyLogger, MyTradeHandler, MyInventory, MyErrorHandler);
                //AdvancedCommunication.RulesHandler MyRulesHandler = new AdvancedCommunication.RulesHandler(MyTCPWrapper, MyMySqlManager, MyLogger, MyTradeHandler);
                AdvancedCommunication.MOVECommandHandler MyMOVECommandHandler = new AdvancedCommunication.MOVECommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyPMHandler);
                AdvancedCommunication.SitStandCommandHandler MySitStandCommandHandler = new AdvancedCommunication.SitStandCommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyPMHandler);
                AdvancedCommunication.RetradeCommandHandler MyRetradeCommandHandler = new AdvancedCommunication.RetradeCommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyActorHandler, MyTradeHandler);
                AdvancedCommunication.TextCommandsHandler MyTextCommandsHandler = new AdvancedCommunication.TextCommandsHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyLogger);
                AdvancedCommunication.WhoIsTradingCommandHandler MyWhoIsTradingHandler = new cs_elbot.AdvancedCommunication.WhoIsTradingCommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler);
                AdvancedCommunication.CancelTradeCommandHandler MyCancelTradeHandler = new cs_elbot.AdvancedCommunication.CancelTradeCommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler);

                //goto command handler
                AdvancedCommunication.GotoCommandHandler MyGotoCommandHandler = new cs_elbot.AdvancedCommunication.GotoCommandHandler(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyPMHandler, MyActorHandler);

                //use object command handler
                AdvancedCommunication.UseObject MyUseObjectCommandHandler = new cs_elbot.AdvancedCommunication.UseObject(MyTCPWrapper, MyMessageParser, MyAdminHelpCommandHandler, MyMySqlManager, MyPMHandler);

                //openstorage
                AdvancedCommunication.OpenStorage MyOpenStorageCommandHandler = new cs_elbot.AdvancedCommunication.OpenStorage(MyHelpCommandHandler, MyTCPWrapper, MyMessageParser, MyMySqlManager, MyActorHandler, MyAdminHelpCommandHandler);
                //withdraw
                AdvancedCommunication.WithdrawCommandHandler MyWithdrawCommandHandler = new cs_elbot.AdvancedCommunication.WithdrawCommandHandler(MyTCPWrapper, MyMessageParser, MyAdvHelpCommandHandler, MyMySqlManager, MyStorage, MyTradeHandler, MyStats, MyInventory);
                AdvancedCommunication.DepositCommandHandler MyDepositCommandHandler = new cs_elbot.AdvancedCommunication.DepositCommandHandler(MyTCPWrapper, MyMessageParser, MyAdvHelpCommandHandler, MyMySqlManager, MyInventory, MyTradeHandler, MyStats, MyStorage);

                //gamble
                AdvancedCommunication.GAMBLECommandHandler MyGAMBLECommandHandler = new AdvancedCommunication.GAMBLECommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyLogger, MyTradeHandler, MyInventory);
                AdvancedCommunication.PRIZECommandHandler MyPRIZECommandHandler = new AdvancedCommunication.PRIZECommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyInventory, MyTradeHandler, MyStats, MyLogger, MyActorHandler);

                //isonline command
                AdvancedCommunication.ISOnlineCommandHandler MyISOnlineCommandHandler = new AdvancedCommunication.ISOnlineCommandHandler(MyTCPWrapper, MyMessageParser, MyHelpCommandHandler, MyMySqlManager, MyLogger);


                //Guard bot code below
                if (botType == 3)
                {
                    AdvancedCommunication.AssessThreat MyAssessThreat = new AdvancedCommunication.AssessThreat(MyTCPWrapper, MyMySqlManager, MyActorHandler);
                }

                Perks MyPerks = new Perks(MyTCPWrapper, MyLogger, MyMySqlManager);
                CLI MyCLI = new CLI(MyTCPWrapper, MyMessageParser);

                MyCLI.DoLoop();
            }
            catch (System.Net.Sockets.SocketException se)
            {
                Console.WriteLine("Error in main loop: " + se.Message);
                if (se.ErrorCode == 11001) // Host name not found
                {
                    Console.WriteLine("When this error occurs, check that you are connected to the internet and a firewall is not blocking this program.");
                }
            }
            catch (Exception except)
            {
                MyErrorHandler.writeErrorMessage("Exception in main loop: " + except);
            }
            finally
            {
                Console.ReadLine();
            }
		}
	}
}
