// Eternal Lands Bot
// Copyright (C) 2006  Artem Makhutov
// artem?makhutov.org
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
using System.IO;
using MySql.Data.MySqlClient;
using System.Timers;

namespace cs_elbot
{
    /// <summary>
    /// description of MySqlManager.
    /// </summary>

    public class MySqlManager
    {
        private Logger TheLogger;
        private TCPWrapper TheTCPWrapper;
        private static errorHandler myErrorHandler;
        private string SqlServer = "localhost";
        private string SqlUsername = "";
        private string SqlPassword = "";
        private string SqlDatabase = "";
        private int SqlPort = 3306;
        public string playerguildstored = "";
        public string usernamestored = "";
        private static object myLock = new object();

        public MySqlManager(string SqlServer, string SqlUsername, string SqlPassword, string SqlDatabase, int SqlPort, Logger MyLogger, TCPWrapper MyTCPWrapper, errorHandler MyErrorHandler)
        {
            this.TheLogger = MyLogger;
            this.TheTCPWrapper = MyTCPWrapper;
            this.SqlServer = SqlServer;
            this.SqlUsername = SqlUsername;
            this.SqlPassword = SqlPassword;
            this.SqlDatabase = SqlDatabase;
            this.SqlPort = SqlPort;
            myErrorHandler = MyErrorHandler;
            //this.TheInventory = MyInventory;
        }
        public void updatePerks(string botPerks)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT id, name FROM perks";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            MySqlDataReader reader = cmd.ExecuteReader();

            string insertCommand = "INSERT INTO botperks (botid, perkid) VALUES ";
            int count = 0;

            try
            {
                while (reader.Read())
                {
                    count++;
                    int perkid = reader.GetInt16(0);
                    string perkname = reader.GetString(1);
                    if (botPerks.Contains(perkname))
                    {
                        insertCommand += "(" + Settings.botid + "," + perkid + ")";
                    }
                }
                TheLogger.Debug("Getting perk list complete\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            reader.Close();
            if (count > 0)
            {
                string deleteCommand = "DELETE FROM botperks WHERE botid = " + Settings.botid;
                raw_sql(deleteCommand);
                insertCommand = insertCommand.Replace(")(", "),(");
                raw_sql(insertCommand);
            }
            MyConnection.Close();
        }

        public string getStorageNPCName()
        {
            string npcName = "";
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT name FROM npc WHERE id= (SELECT npcid FROM botpath WHERE botpath.id = (SELECT botpathid FROM botdestination " +
                "WHERE botid = ?botid AND destid = 1 AND npcid IS NOT NULL))";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    npcName = reader.GetString(0);
                    TheLogger.Debug("Complete\n");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return npcName;

        }
        public void SetupConstants(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }

            string sql = "SELECT botid, locationdescription, welcomedescription, infodescription, name FROM bots WHERE id=?botid;";
            int err = 0;
            try
            {
                TheLogger.Debug(sql + "\n");
                MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
                cmd.Parameters.AddWithValue("?botid", botid.ToString());
                MySqlDataReader reader = cmd.ExecuteReader(); 
                try
                {
                    reader.Read();
                    Settings.botid = reader.GetInt32(0);
                    Settings.locationdescription = reader.GetString(1);
                    Settings.welcomedescription = reader.GetString(2);
                    Settings.infodescription = reader.GetString(3);
                }
                catch (MySqlException oMySQLException)
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
                catch (Exception oException)
                {
                    myErrorHandler.errorWriter(oException);
                }
                finally
                {
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                err = 1;
                Console.WriteLine("Error in SetupConstants: " + ex.Message);
            }
            finally
            {
                if (err == 1)
                {
                    Console.WriteLine("Exiting due to the above");
                    System.Environment.Exit(3);
                }
            }
        }
        public AdvancedCommunication.ActorHandler.position getNPCPosition(int npcID)
        {
            AdvancedCommunication.ActorHandler.position NPCPosition = new cs_elbot.AdvancedCommunication.ActorHandler.position();
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT x,y FROM npc WHERE id = ?npcID;";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);

            cmd.Parameters.AddWithValue("?npcID", npcID);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    NPCPosition.x = reader.GetInt16(0);
                    NPCPosition.y = reader.GetInt16(1);
                    TheLogger.Debug("Complete\n");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return NPCPosition;
        }
        public AdvancedCommunication.ActorHandler.position getObjectPosition(string mapName, int objectID, out int useWithObject)
        {
            AdvancedCommunication.ActorHandler.position objectPosition = new cs_elbot.AdvancedCommunication.ActorHandler.position();
            useWithObject = -1;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT x,y,use_with_item FROM mapobjects, maps_area, maps WHERE object_id = ?objectID " +
                "AND maps_area.id = mapobjects.maps_area_in AND maps.id = maps_area.maps_id AND maps.file_name = ?mapName LIMIT 1";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);

            cmd.Parameters.AddWithValue("?objectID", objectID);
            cmd.Parameters.AddWithValue("?mapName", mapName);

            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    objectPosition.x = reader.GetInt16(0);
                    objectPosition.y = reader.GetInt16(1);
                    if (!reader["use_with_item"].Equals(System.DBNull.Value))
                    {
                        useWithObject = reader.GetInt16(2);
                    }
                    TheLogger.Debug("Complete\n");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return objectPosition;
        }
        public AdvancedCommunication.ActorHandler.position getDestination(string mapName, string targetDest, out int objectID, out int useWithObject, ref int lastMoveNumber)
        {
            useWithObject = -1;
            objectID = 0;
            int npcID = 0;
            AdvancedCommunication.ActorHandler.position destination = new cs_elbot.AdvancedCommunication.ActorHandler.position();
            destination.x = -1;
            destination.y = -1;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT botpath.moveobject,botpath.npcid, botpath.movenumber FROM botpath, maps,botdestination, destination " +
                "WHERE botpath.mapid = maps.id AND maps.file_name = ?mapName AND botdestination.destid = destination.id " +
                "AND LOWER(destination.name) = ?targetDest AND botdestination.botid = ?botid AND botdestination.botpathid = botpath.id " +
                "AND botpath.movenumber > ?lastMoveNumber ORDER BY botpath.movenumber LIMIT 1";
            if (mapName == "maps/mapunderworld1.elm" && lastMoveNumber > 0)
            {
                lastMoveNumber = 0;
            }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);

            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            cmd.Parameters.AddWithValue("?mapName", mapName);
            cmd.Parameters.AddWithValue("?targetDest", targetDest.ToLower());
            cmd.Parameters.AddWithValue("?lastMoveNumber", lastMoveNumber);

            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    if (!reader["moveobject"].Equals(System.DBNull.Value))
                    {
                        objectID = reader.GetInt16(0);
                    }
                    if (!reader["npcid"].Equals(System.DBNull.Value))
                    {
                        npcID = reader.GetInt16(1);
                    }
                    if (!reader["movenumber"].Equals(System.DBNull.Value))
                    {
                        lastMoveNumber = reader.GetInt16(2);
                    }
                    TheLogger.Debug("Complete\n");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            //MyConnection.Close();
            //find out if we're going to an npc, or an object
            //then find the coords for the object/npc
            if (objectID > 0)
            {
                destination = getObjectPosition(mapName, objectID, out useWithObject);
            }
            else if (npcID > 0)
            {
                destination = getNPCPosition(npcID);
            }
            else
            {
                //lol, we have real issues if we get here...
                Console.WriteLine("Cannot find destination for map walking..." + destination.x + "," + destination.y);
            }
            MyConnection.Close();
            return destination;
        }
        public void getHomeInfo()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT bot_x, bot_y, onmap, heading FROM bots WHERE botid = ?botid";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);

            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    if (!reader["bot_x"].Equals(System.DBNull.Value))
                    {
                        MainClass.myHome.x = reader.GetInt16(0);
                    }
                    if (!reader["bot_y"].Equals(System.DBNull.Value))
                    {
                        MainClass.myHome.y = reader.GetInt16(1);
                    }
                    if (!reader["onmap"].Equals(System.DBNull.Value))
                    {
                        MainClass.myHome.mapName = reader.GetString(2);
                    }
                    if (!reader["heading"].Equals(System.DBNull.Value))
                    {
                        MainClass.myHome.heading = reader.GetInt16(3);
                    }
                    TheLogger.Debug("Complete\n");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
        }
        public string botname(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.

            string name = "";
            string sql = "SELECT name FROM bots WHERE id=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid.ToString());
            MySqlDataReader reader = cmd.ExecuteReader(); 

            try
            {
                while (reader.Read())
                {
                    name = reader.GetString(0);
                    TheLogger.Debug("Complete\n");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return name;
        }
        public bool enabledebuging(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool disabled = true;

            string sql = "SELECT enabledebuging FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid.ToString());
            MySqlDataReader reader = cmd.ExecuteReader(); 

            try
            {
                while (reader.Read())
                {
                    byte tmp = reader.GetByte(0);
                    if (tmp == 0)
                    {
                        disabled = false;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return disabled;
        }
        public bool sendreceivedpms(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool sendreceivedpms = true;

            string sql = "SELECT sendreceivedpms FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid.ToString());
            MySqlDataReader reader = cmd.ExecuteReader(); 

            try
            {
                while (reader.Read())
                {
                    byte tmp = reader.GetByte(0);
                    if (tmp == 0)
                    {
                        sendreceivedpms = false;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return sendreceivedpms;
        }
        public bool senderrorpms(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool senderrorpms = true;

            string sql = "SELECT senderrorpms FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid.ToString());
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    byte tmp = reader.GetByte(0);
                    if (tmp == 0)
                    {
                        senderrorpms = false;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return senderrorpms;
        }
        //public bool guildfeedgossip(int botid)
        //{
        //    MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
        //    MyConnection.Open();
        //    bool disabled = true;

        //    string sql = "SELECT guildfeedgossip FROM bots WHERE id=?botid;";
        //    TheLogger.Debug(sql + "\n");

        //    MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
        //    cmd.Parameters.AddWithValue("?botid", botid.ToString());
        //    TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

        //    try
        //    {
        //        while (reader.Read())
        //        {
        //            byte tmp = reader.GetByte(0);
        //            if (tmp == 0)
        //            {
        //                disabled = false;
        //            }
        //        }
        //    }
        //    catch (MySqlException oMySQLException)
        //    {
        //        TheLogger.Log("ExecSql Error in {0} due to : " + oMySQLException.Message + "\n");
        //        TheLogger.Log("ExecSql by SQL : " + sql
        //            + "\n");
        //        Console.Beep(4400, 50);
        //    }
        //    catch (Exception oException)
        //    {
        //        TheLogger.Log("ExecSql Error in {0} due to : " + oException.Message + "\n");
        //        TheLogger.Log("ExecSql by SQL : " + sql + "\n");
        //        Console.Beep(4400, 50);
        //    }
        //    finally
        //    {
        //        reader.Close();
        //    }
        //    MyConnection.Close();
        //    return disabled;
        //}
        //public void guildfeedgossip(int botid, bool feed)
        //{
        //    string sql = "UPDATE bots SET guildfeedgossip = ";
        //    if (feed)
        //    {
        //        sql += "TRUE";
        //    }
        //    else
        //    {
        //        sql += "FALSE";
        //    }
        //    sql += " WHERE botid='" + botid.ToString() + "' LIMIT 1;";
        //    TheLogger.Debug(sql + "\n");
        //    raw_sql(sql);
        //}
        public bool sendCoinPMs()
        {
            bool showownercoins = false;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT showownercoins FROM bots WHERE id = ?botid";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);

            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    showownercoins = reader.GetBoolean(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return showownercoins;
        }
        public void LogTrade(TradeHandler.TradeLogItem MyTradeLogItem, string username, int botid, bool sendCoinPMs)
        {
            if (username == "")
                return;
            bool sendPM = true;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "[" + MyTradeLogItem.action + "] [" + username + "] [" + MyTradeLogItem.quantity.ToString() + "] [" + GetKnownItemsname(MyTradeLogItem.KnownItemsSqlID) + "] [" + MyTradeLogItem.price * MyTradeLogItem.quantity + "gc";
            string[] names = botowner(Settings.botid).Split('|');
            if (MyTradeLogItem.action.ToLower().Contains("transaction coins") && !sendCoinPMs)
            {
                sendPM = false;
            }
            if (this.sendreceivedpms(Settings.botid) && sendPM)// Added this check * Draavell //
            {
                foreach (string name in names)
                {
                    if (MySqlManager.playerIsOnline(name.Trim()))
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(name.Trim(), sql));
                    }
                }
            }
//            sql = "INSERT DELAYED INTO tradelog (knownitemsid, quantity, price, username, tradetime, action, payedout, botid) VALUES(?knownitemsid, ?quantity, ?price, ?username, now(), ?action, ?payedout, ?botid);";
            sql = "INSERT INTO tradelog (knownitemsid, quantity, price, username, tradetime, action, payedout, botid) VALUES(?knownitemsid, ?quantity, ?price, ?username, now(), ?action, ?payedout, ?botid);";
            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?knownitemsid", MyTradeLogItem.KnownItemsSqlID);
            cmd.Parameters.AddWithValue("?quantity", MyTradeLogItem.quantity);
            cmd.Parameters.AddWithValue("?price", MyTradeLogItem.price);
            cmd.Parameters.AddWithValue("?username", username);
            //cmd.Parameters.AddWithValue("?tradetime", "now()");
            cmd.Parameters.AddWithValue("?action", MyTradeLogItem.action);
            cmd.Parameters.AddWithValue("?payedout", 0);
            cmd.Parameters.AddWithValue("?botid", botid);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            return;
        }
        public uint advertchannel(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            uint fl = 128;
            string sql = "SELECT advertchannel FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {

                while (reader.Read() == true)
                {
                    fl = reader.GetUInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public uint altadvertchannel(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            uint fl = 128;
            string sql = "SELECT altadvertchannel FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {

                while (reader.Read() == true)
                {
                    fl = reader.GetUInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public bool loggotpms(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 128;
            string sql = "SELECT loggotpms FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); 
            MySqlDataReader reader = cmd.ExecuteReader(); 
            TheLogger.Debug("MySQLDataReader complete");
            try
            {

                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            if (fl != 0)
                return true;
            else
                return false;
        }
        public int numberoftextcommands(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 0;
            string sql = "SELECT COUNT(*) FROM textcommands WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);

            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {
                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public bool logsendpms(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 128;
            string sql = "SELECT logsendpms FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {

                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            if (fl != 0)
                return true;
            else
                return false;
        }
        public Int32 lastadverttime(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 128;
            string sql = "SELECT unix_timestamp(now()) - unix_timestamp(lastadverttime) FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {

                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public Int32 lastaltadverttime(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 128;
            string sql = "SELECT unix_timestamp(now()) - unix_timestamp(lastaltadverttime) FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {

                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public int newhourcolor(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 128;
            string sql = "SELECT newhourcolour FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {

                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public int SendHNHtoGM(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 0;

            string sql = "SELECT gmhnhmessage FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }

                TheLogger.Debug("Complete\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public int SendHNHtoLocal(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 0;

            string sql = "SELECT localhnhmessage FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }

                TheLogger.Debug("Complete\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public string welcomedescription(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.

            string name = "";
            string sql = "SELECT welcomedescription FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    name = reader.GetString(0);
                    name = name.Replace("\\\\", "\\").Replace("\\\'", "\'").Replace("\\\"", "\"");
                    TheLogger.Debug("Complete\n");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return name;
        }
        public string locationdescription(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.

            string name = "";
            string sql = "SELECT locationdescription FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    name = reader.GetString(0);
                    name = name.Replace("\\\\", "\\").Replace("\\\'", "\'").Replace("\\\"", "\"");
                    TheLogger.Debug("Complete\n");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return name;
        }
        public string hnhmessage(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.

            string name = "";
            string sql = "SELECT hnhmessage FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    name = reader.GetString(0);
                    //name = name.Replace("\\\\", "\\").Replace("\\\'", "\'").Replace("\\\"", "\"");
                    TheLogger.Debug("Complete\n");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return name;
        }
        public string PlayerGuild(string username)
        {

            username = username.Trim().ToLower();            
            if (username == usernamestored)
            {
                return playerguildstored;
            }
            if (username == "" || username == "console:\\>" || username == "gossip")
                return "";
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }

            //TheTCPWrapper.Send(CommandCreator.SEND_PM("gossip", "g? " + username));
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.

            string name = "";//, lastseen = "";
            string sql = "SELECT guild FROM players WHERE LOWER(name) = ?name LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", username.ToLower());

            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    name = reader.GetString(0);
                    //lastseen = reader.GetString(1);
                    TheLogger.Debug("Complete\n");

                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
                //TheLogger.Debug("Player [" + username + "] guild [" + name + "]\n");
            }
            playerguildstored = name;
            usernamestored = username;
            MyConnection.Close();
            return name;
        }
        public void BanGuild(string guildname, int banned, string username)
        {
            // banned: -1 banned, 0 normal, 1 prefer
            if (guildname == "")
                return;

            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int guildval = 0;
            int i;
            if (guildname.Length > 0)
            {
                for (i = guildname.Length - 1; i >= 0; i--)
                {
                    guildval *= 256;
                    guildval += guildname[i];
                }
            }

            int fl = -1;
            string sql = "SELECT id FROM guilds WHERE guildname=?guildname AND botid=?botid LIMIT 1;";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?guildname", guildval.ToString());
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            TheLogger.Debug(sql + "\n");
            try
            {
                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }

                TheLogger.Debug("Complete, fl is " + fl.ToString() + "\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            if (fl != -1)
            {
                sql = "UPDATE guilds SET rank=?rank, notes=?notes WHERE id=?id LIMIT 1;";
            }
            else
            {
                sql = "INSERT INTO guilds (guildname, rank, notes, botid) VALUES (?guildname, ?rank, ?notes, ?botid)";
            }
            cmd.Parameters.Clear();
            cmd = new MySqlCommand(sql, MyConnection);
            if (fl == -1)
            {
                cmd.Parameters.AddWithValue("?guildname", guildval.ToString());
                cmd.Parameters.AddWithValue("?botid", Settings.botid);

            }
            else
            {
                cmd.Parameters.AddWithValue("?id", fl);
            }
            cmd.Parameters.AddWithValue("?rank", banned);
            if (banned == -1)
            {
                cmd.Parameters.AddWithValue("?notes", "Banned by " + username);
            }
            else if (banned == 0)
            {
                cmd.Parameters.AddWithValue("?notes", "Normalised by " + username);
            }
            else
            {
                cmd.Parameters.AddWithValue("?notes", "Prefered by " + username);
            }
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            return;
        }
        public int CheckIfBannedGuild(string username, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
           int fl = 0;
            
            string sql = "SELECT rank FROM guilds WHERE guildname = ?guildname AND botid = ?botid";
            //'" + PlayerGuild(username).ToLower() + "' AND botid = '" + botid.ToString() + "';";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?guildname", PlayerGuild(username).ToLower());
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }

                TheLogger.Debug("Complete, fl is " + fl.ToString() + "\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            //TheLogger.Debug("%%%%% Player " + username.ToLower() + " of guild [" + PlayerGuild(username) + "] is rank " + fl.ToString() + "\n");
            MyConnection.Close();
            return fl;
        }
        public string guildlongname(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.

            string name = "Unknown";
            string sql = "SELECT guildlongname FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    name = reader.GetString(0);
                    name = name.Replace("\\\\", "\\").Replace("\\\'", "\'").Replace("\\\"", "\"");
                    TheLogger.Debug("Complete\n");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return name;
        }
        public void deleteBuddy(string buddyName)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "DELETE FROM botbuddy WHERE lower(buddyname) = lower(?buddyname);";
            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?buddyname", buddyName);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
        }
        public int playerloggedoncolor(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 128;
            string sql = "SELECT playerloggedoncolor FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {
                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public int playerloggedoffcolor(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 128;
            string sql = "SELECT playerloggedoffcolor FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }

                TheLogger.Debug("Complete\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public int playerjoinedguildcolor(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 128;
            string sql = "SELECT playerjoinedguildcolor FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {
                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }

                TheLogger.Debug("Complete\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public int playerleftguildcolor(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 128;
            string sql = "SELECT playerleftguildcolor FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {
                while (reader.Read() == true)
                {
                    fl = reader.GetInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public int foodlevel(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 0;
            string sql = "SELECT foodlevel FROM bots WHERE id = '" + botid.ToString() + "';";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    fl = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public int currenthealth(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 0;
            string sql = "SELECT materialpointscur FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {
                while (reader.Read())
                {
                    fl = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public int maxhealth(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int fl = 0;
            string sql = "SELECT materialpointsbase FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    fl = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return fl;
        }
        public void UpdatePlayer(string name, int rank, string username)
        {
            if (name.Length == 0)
                return;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT id,istrademember FROM users WHERE lower(name)=?name AND botid=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", name.ToLower());
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            int pos = -1;
            int istrademember = 0;
            try
            {
                while (reader.Read())
                {
                    pos = reader.GetInt32(0);
                    istrademember = reader.GetInt32(1);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                if (!reader.IsClosed)
                    reader.Close();
            }


            if (pos == -1)
                sql = "INSERT INTO users (name,istrademember,rank,banreason,botid) VALUES (?name,?trademember,?rank,?banreason,?botid);";
            else
                sql = "UPDATE users SET name=?name, istrademember=?trademember, rank=?rank, banreason=?banreason, botid=?botid WHERE id=?pos LIMIT 1;";

            TheLogger.Debug(sql + "\n");
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", name);
            if (rank == -1)
            {
                cmd.Parameters.AddWithValue("?rank", -1);
                cmd.Parameters.AddWithValue("?trademember", "0");
                cmd.Parameters.AddWithValue("?banreason", "Banned by " + username);
            }
            else if (rank == 0)
            {
                cmd.Parameters.AddWithValue("?rank", 0);
                cmd.Parameters.AddWithValue("?trademember", "0");
                cmd.Parameters.AddWithValue("?banreason", "Normalised by " + username);
            }
            else if(rank==1)
            {
                cmd.Parameters.AddWithValue("?rank", 0);
                cmd.Parameters.AddWithValue("?trademember", "1");
                cmd.Parameters.AddWithValue("?banreason", "Preferred by " + username);
            }
            else
            {
                cmd.Parameters.AddWithValue("?rank", rank);
                cmd.Parameters.AddWithValue("?trademember", istrademember);
                cmd.Parameters.AddWithValue("?banreason", "Rank changed by " + username);
            }
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            if (pos != -1)
                cmd.Parameters.AddWithValue("?pos", pos);

            try
            {
		cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            return;
        }
        public void UpdatePlayer(string name, string guild)
        {
            if (name.Length == 0)
                return;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT id FROM players WHERE lower(name)=?name;";
            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", name.ToLower());
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            int pos = -1;
            try
            {
                while (reader.Read())
                {
                    pos = reader.GetInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                if (!reader.IsClosed)
                    reader.Close();
            }

            int guildval = 0;
            string guild2 = "";
            int i;
            if (guild.Length > 0)
            {
                for (i = guild.Length - 1; i >= 0; i--)
                {
                    guildval *= 256;
                    guildval += guild[i];
                }
            }
            guild2 = guildval.ToString();
            TheLogger.Debug("\nGuild tag [" + guild + "] is [" + guildval + "]");
            if (pos == -1)
                sql = "INSERT INTO players (name,guild,lastseen,lastseenby) VALUES (?name,?guild,?lastseen,?lastseenby);";
            else
                sql = "UPDATE players SET name=?name, guild=?guild, lastseen=?lastseen, lastseenby=?lastseenby WHERE id=?pos LIMIT 1;";

            TheLogger.Debug(sql + "\n");
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", name);
            if (guildval == 0)
                cmd.Parameters.AddWithValue("?guild", 0);
            else
                cmd.Parameters.AddWithValue("?guild", guild2);

            cmd.Parameters.AddWithValue("?lastseen", System.DateTime.Now );
            cmd.Parameters.AddWithValue("?lastseenby", Settings.Loginname);
            if (pos != -1)
                cmd.Parameters.AddWithValue("?pos", pos);

            try
            {
            	cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            return;
        }
        public void advertlog(string adverttext)
        {
            if (adverttext.Length < 5)
                return;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }

            string advert = adverttext.Replace("\\", "\\\\");
            advert = advert.Replace("\'", "\\\'");
            string sql = "INSERT INTO advertlog (adverttext,botid) VALUES (?adverttext,?botid);";
            TheLogger.Debug(sql + "\n");

            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
                cmd.Parameters.AddWithValue("?adverttext", adverttext);
                cmd.Parameters.AddWithValue("?botid", Settings.botid);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            
            return;
        }
        public int GetUserRank(string username, int botid)
        {
            if (username == "Console:\\>")
            {
                return 100;
            }

            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int UserRank = 0;
            string sql = "SELECT rank FROM users WHERE LOWER(name)=?name AND botid=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", username.ToLower());
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    UserRank = reader.GetInt16(0);
                }

                TheLogger.Debug("Complete\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return UserRank;
        }
        public int GetTimeoutInterval(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int timeoutInterval = 0;
            string sql = "SELECT tradetimeoutinterval FROM bots WHERE botid=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    timeoutInterval = reader.GetInt16(0);
                }

                TheLogger.Debug("Complete\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return timeoutInterval;
        }

        public int GetGuildRank(string username, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            // Get guildrank of a GuildMember. If no guildrank was found, set it to the default value (6).
            int UserRank = 0;
            string sql = "SELECT guildrank FROM users WHERE LOWER(name)=?name AND botid=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", username.ToLower());
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    UserRank = reader.GetInt16(0);

                    if (UserRank == 0)
                    {
                        UserRank = 6;
                    }
                }

            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return UserRank;
        }
        public bool CheckIfCommandIsDisabled(string Command, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool disabled = true;

            string sql = "SELECT disabled FROM commands WHERE name=?name AND botid=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", Command.ToLower());
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    byte tmp = reader.GetByte(0);
                    if (tmp == 0)
                    {
                        disabled = false;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return disabled;
        }
        public bool CheckIfTextCommandIsDisabled(string Command, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool disabled = true;
            string command = "";
            if (Command[0] != '#')
                command += "#";
            command += Command;
            string sql = "SELECT disabled FROM textcommands WHERE LOWER(command)=?command AND botid=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?command", Command.ToLower());
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    byte tmp = reader.GetByte(0);
                    if (tmp == 0)
                    {
                        disabled = false;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return disabled;
        }
        public bool sendtextcommandtogm(string Command, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool disabled = true;
            string command = "";
            if (Command[0] != '#')
                command += "#";
            command += Command;
            string sql = "SELECT sendtogm FROM textcommands WHERE LOWER(command)=?command AND botid=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?command", Command.ToLower());
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    byte tmp = reader.GetByte(0);
                    if (tmp == 0)
                    {
                        disabled = false;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return disabled;
        }
        public string TextCommandHelpText(string Command, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT helptext FROM textcommands WHERE LOWER(command)=?command AND botid=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?command", Command.ToLower());
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            string tmp;
            tmp = "";
            try
            {
                while (reader.Read())
                {
                    tmp = reader.GetString(0);
                    if (!tmp.StartsWith(" - "))
                    {
                        tmp = " - " + tmp;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return tmp;
        }
        public string TextCommandText(string Command, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT text FROM textcommands WHERE LOWER(command)=?command AND botid=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?command", Command.ToLower());
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            string tmp;
            tmp = "";
            try
            {
                while (reader.Read())
                {
                    tmp = reader.GetString(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return tmp;
        }
        public string TextCommandlist(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT command FROM textcommands WHERE botid=?botid ORDER BY command ASC;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            string tmp, buffer;
            tmp = "";
            buffer = "";
            try
            {
                while (reader.Read())
                {
                    tmp = reader.GetString(0);
                    if (!tmp.StartsWith("#"))
                    {
                        tmp = "#" + tmp;
                    }
                    if (buffer.Length == 0)
                        buffer = tmp;
                    else
                        buffer += " " + tmp;
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return buffer;
        }
        public bool advertise()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool advertise = true;

            string sql = "SELECT advertise FROM bots WHERE id=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    byte tmp = reader.GetByte(0);
                    if (tmp == 0)
                    {
                        advertise = false;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return advertise;
        }
        public bool altadvertise()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool advertise = true;

            string sql = "SELECT altadvertise FROM bots WHERE id=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    byte tmp = reader.GetByte(0);
                    if (tmp == 0)
                    {
                        advertise = false;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return advertise;
        }
        public string advertendmessage()
        {
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }

            string name = "";
            string sql = "SELECT advertendmessage FROM bots WHERE id=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    name = reader.GetString(0);
                    name = name.Replace("\\\\", "\\").Replace("\\\'", "\'").Replace("\\\"", "\"");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();

            return name;
        }
        public int minadverttime()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int minadverttime = 900;

            string sql = "SELECT minadverttime FROM bots WHERE id=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    minadverttime = reader.GetInt32(0);
                }
                TheLogger.Debug("Complete\n");
                if (minadverttime < 900)
                    minadverttime = 900;
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return minadverttime;
        }
        public int minaltadverttime()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int minadverttime = 900;

            string sql = "SELECT minaltadverttime FROM bots WHERE id=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    minadverttime = reader.GetInt32(0);
                }
                TheLogger.Debug("Complete\n");
                if (minadverttime < 900)
                    minadverttime = 900;
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return minadverttime;
        }
        public Int32 randomadvertdelay()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            Int32 randomadvertdelay = 0;

            string sql = "SELECT randomadvertdelay FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); 
            MySqlDataReader reader = cmd.ExecuteReader(); 
            TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    randomadvertdelay = reader.GetInt32(0);
                }
                TheLogger.Debug("Complete\n");
                if (randomadvertdelay < 0)
                    randomadvertdelay = 0;
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return randomadvertdelay;
        }
        public int randomaltadvertdelay()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int randomadvertdelay = 0;

            string sql = "SELECT randomaltadvertdelay FROM bots WHERE id=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    randomadvertdelay = reader.GetInt32(0);
                }
                TheLogger.Debug("Complete\n");
                if (randomadvertdelay < 0)
                    randomadvertdelay = 0;
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return randomadvertdelay;
        }
        public int GetCommandRank(string Command, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int CommandRank = 100;

            string sql = "SELECT requriedrank FROM commands WHERE LOWER(name)=?name AND botid=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", Command.ToLower());
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    CommandRank = reader.GetInt16(0);
                }
                TheLogger.Debug("Complete\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return CommandRank;
        }
        public int GetTextCommandRank(string Command, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int CommandRank = 100;

            string sql = "SELECT rank FROM textcommands WHERE LOWER(command)=?command AND botid=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?command", Command.ToLower());
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    CommandRank = reader.GetInt16(0);
                }
                TheLogger.Debug("Complete\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return CommandRank;
        }
        public int GetStorageSQLID(TradeHandler.TradeItem MyTradeItem)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            TradeHandler.TradeItem MyTempTradeItem = new TradeHandler.TradeItem();
            MyTempTradeItem = MyTradeItem;
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.
            int id = -1;
            int count = -1;
            //string sql = "SELECT id FROM knownitems WHERE imageid = ?imageid and name = ?name order by imageid, name";
            string sql = "SELECT id FROM knownitems WHERE lower(name) = ?name order by name";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?imageid", MyTempTradeItem.imageid);
            cmd.Parameters.AddWithValue("?name", MyTempTradeItem.name.ToLower());
            TheLogger.Debug(sql + "\n");
            MySqlDataReader reader = cmd.ExecuteReader();
            TheLogger.Debug("### cmd.ExecuteReader() in GetStorageSQLID\n");

            try
            {
                while (reader.Read())
                {
                    id = reader.GetInt16(0);
                    count++;
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
                TheLogger.Debug("### reader.Close() in GetStorageSQLID\n");
            }
            if (id != -1 && count == 0)
            {
                MyConnection.Close();
                return id;//we found a perfect match
            }
            count = -1;
            sql = "SELECT id FROM knownitems WHERE imageid = ?imageid and lower(name) = ?name order by imageid, name";
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?imageid", -1);
            cmd.Parameters.AddWithValue("?name", MyTempTradeItem.name.ToLower());
            TheLogger.Debug(sql + "\n");
            reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    id = reader.GetInt16(0);
                    count++;
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            if (id != -1 && count == 0)
            {
                //update the imageid here from -1 to the right one
                // imageid = -1 update it
                Inventory.inventory_item MyInventoryItem = new Inventory.inventory_item();
                MyInventoryItem.imageid = MyTradeItem.imageid;
                MyInventoryItem.name = MyTradeItem.name;
                MyInventoryItem.description = "";
                MyInventoryItem.weight = MyTradeItem.weight;
                MyInventoryItem.is_resource = false;
                MyInventoryItem.is_reagent = false;
                MyInventoryItem.is_stackable = false;
                MyInventoryItem.use_with_inventory = false;
                updateknownitems(MyInventoryItem, id);
                MyConnection.Close();
                return id;//we found a perfect match
            }
            if (id == -1)
            {
		if(MyTradeItem.imageid == 0) TheLogger.ErrorLog("Possibly invalid Item: " + MyTradeItem.name + " added by " + Settings.botid + " in GetStorageSQLID()");
                sql = "INSERT INTO knownitems (name,description,imageid,weight,is_resource,is_reagent,is_stackable,use_with_inventory) VALUES (?name,?description,?imageid,?weight,?is_resource,?is_reagent,?is_stackable,?use_with_inventory)";
                cmd.Parameters.Clear();
                cmd = new MySqlCommand(sql, MyConnection);
                cmd.Parameters.AddWithValue("?name", MyTradeItem.name);
                cmd.Parameters.AddWithValue("?description", " ");
                cmd.Parameters.AddWithValue("?imageid", MyTradeItem.imageid);
                cmd.Parameters.AddWithValue("?weight", MyTradeItem.weight);
                cmd.Parameters.AddWithValue("?is_resource", -1);
                cmd.Parameters.AddWithValue("?is_reagent", -1);
                cmd.Parameters.AddWithValue("?is_stackable", -1);
                cmd.Parameters.AddWithValue("?use_with_inventory", -1);
                try
                {
                    cmd.ExecuteNonQuery();
                    id = GetKnownItemsSQLID(MyTradeItem);
                }
                catch (MySqlException oMySQLException)
                {
                    TheLogger.Log("ExecSql Error in " + oMySQLException.TargetSite + " due to : " + oMySQLException.Message + "\n");
                    TheLogger.Log("ExecSql by SQL : " + sql
                        + "\n");
                    myErrorHandler.errorWriter(oMySQLException);
                }
                catch (Exception oException)
                {
                    TheLogger.Log("ExecSql Error in " + oException.TargetSite + " due to : " + oException.Message + "\n");
                    TheLogger.Log("ExecSql by SQL : " + sql + "\n");
                    myErrorHandler.errorWriter(oException);
                }
                finally
                {
                    //reader.Close();
                }
                reader.Close();
                MyConnection.Close();
                return id;
            }
            reader.Close();
            MyConnection.Close();
            return id;
        }
        public int GetKnownItemsSQLID(TradeHandler.TradeItem MyTradeItem)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            TradeHandler.TradeItem MyTempTradeItem = new TradeHandler.TradeItem();
            MyTempTradeItem = MyTradeItem;
            TheLogger.Debug("### SqlConnectionOpen() in GetKnownItemsSQLID\n");
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.
            int id = -1;
            int count = -1;
            string sql = "SELECT id FROM knownitems WHERE imageid = ?imageid and lower(name) = ?name order by imageid, name";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?imageid", MyTempTradeItem.imageid);
            cmd.Parameters.AddWithValue("?name", MyTempTradeItem.name.ToLower());
            TheLogger.Debug(sql + "\n");
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    id = reader.GetInt16(0);
                    count++;
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            if (id != -1 && count == 0)
            {
                MyConnection.Close();
                return id;//we found a perfect match
            }
            count = -1;
            sql = "SELECT id FROM knownitems WHERE imageid = ?imageid and lower(name) = ?name order by imageid, name";
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?imageid", -1);
            cmd.Parameters.AddWithValue("?name", MyTempTradeItem.name.ToLower());
            TheLogger.Debug(sql + "\n");
            reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    id = reader.GetInt16(0);
                    count++;
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            if (id != -1 && count == 0)
            {
                //update the imageid here from -1 to the right one
                // imageid = -1 update it
                Inventory.inventory_item MyInventoryItem = new Inventory.inventory_item();
                MyInventoryItem.imageid = MyTradeItem.imageid;
                MyInventoryItem.name = MyTradeItem.name;
                MyInventoryItem.description = "";
                MyInventoryItem.weight = MyTradeItem.weight;
                MyInventoryItem.is_resource = false;
                MyInventoryItem.is_reagent = false;
                MyInventoryItem.is_stackable = false;
                MyInventoryItem.use_with_inventory = false;
                updateknownitems(MyInventoryItem, id);
                MyConnection.Close();
                return id;//we found a perfect match
            }

            if (id == -1)
            {
                if(MyTradeItem.imageid == 0) TheLogger.ErrorLog("Possibly invalid Item: " + MyTradeItem.name + " added by " + Settings.botid + " in GetKnownItemsSQLID()");
                sql = "INSERT INTO knownitems (name,description,imageid,weight,is_resource,is_reagent,is_stackable,use_with_inventory) VALUES (?name,?description,?imageid,?weight,?is_resource,?is_reagent,?is_stackable,?use_with_inventory)";
                cmd.Parameters.Clear();
                cmd = new MySqlCommand(sql, MyConnection);
                cmd.Parameters.AddWithValue("?name", MyTradeItem.name);
                cmd.Parameters.AddWithValue("?description", " ");
                cmd.Parameters.AddWithValue("?imageid", MyTradeItem.imageid);
                cmd.Parameters.AddWithValue("?weight", MyTradeItem.weight);
                cmd.Parameters.AddWithValue("?is_resource", -1);
                cmd.Parameters.AddWithValue("?is_reagent", -1);
                cmd.Parameters.AddWithValue("?is_stackable", -1);
                cmd.Parameters.AddWithValue("?use_with_inventory", -1);
                try
                {
                    cmd.ExecuteNonQuery();
                    id = GetKnownItemsSQLID(MyTradeItem);
                }
                catch (MySqlException oMySQLException)
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
                catch (Exception oException)
                {
                    myErrorHandler.errorWriter(oException);
                }
                finally
                {
                    reader.Close();
                }
                MyConnection.Close();
                return id;
            }
            MyConnection.Close();
            return id;
        }
        public string GetKnownItemsname(int SQLID)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.

            string name = "";
            string sql = "SELECT name FROM knownitems WHERE id=?id;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?id", SQLID);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    name = reader.GetString(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }

            MyConnection.Close();
            return name;
        }
        public string botowner(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.

            string name = "Unknown";
            string sql = "SELECT botowner FROM bots WHERE id=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    name = reader.GetString(0);
                    name = name.Replace("\\\\", "\\").Replace("\\\'", "\'").Replace("\\\"", "\"");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return name;
        }
        public string pmmonitorlist(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.

            string name = "Unknown";
            string sql = "SELECT pmmonitorlist FROM bots WHERE id=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    name = reader.GetString(0);
                    name = name.Replace("\\\\", "\\").Replace("\\\'", "\'").Replace("\\\"", "\"");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return name;
        }
        public int SetSQLID(Inventory.inventory_item item)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            // Add Item it the Global Items List and to the Inventory List
            // First check if the Item exists in the global Database, if not Add it.

            int id = -1;
            int is_resource = -1;
            string sql = "SELECT id,is_resource FROM knownitems WHERE imageid = ?imageid and name=?name";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?imageid", item.imageid);
            cmd.Parameters.AddWithValue("?name", item.name.ToLower());
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    id = reader.GetInt16(0);
                    is_resource = reader.GetInt16(1);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            if (id != -1)
            {
                if (is_resource == -1)
                {
                    //string description;
                    //description = item.description.Replace("'", "`");
                    updateknownitems(item, id);
                }
                MyConnection.Close();
                return id;
            }

            cmd.Parameters.Clear();
            sql = "SELECT id FROM knownitems WHERE imageid = ?imageid and name = ?name order by imageid, name;";
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?imageid", -1);
            cmd.Parameters.AddWithValue("?name", item.name.ToLower() );

            TheLogger.Debug(sql + "\n");
            reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    id = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            if (id != -1)
            {
                // imageid = -1 update it
                updateknownitems(item, id);
                MyConnection.Close();
                return id;
            }

            cmd.Parameters.Clear();
            sql = "SELECT id FROM knownitems WHERE imageid = ?imageid and name LIKE ?name order by imageid, name;";
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?imageid", item.imageid);
            cmd.Parameters.AddWithValue("?name", "%" + item.name.ToLower() + "%");

            TheLogger.Debug(sql + "\n");
            reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    id = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            if (id != -1)
            {
                MyConnection.Close();
                return id;
            }
            cmd.Parameters.Clear();
            sql = "SELECT id FROM knownitems WHERE imageid = ?imageid and name LIKE ?name order by imageid, name;";
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?imageid", -1);
            cmd.Parameters.AddWithValue("?name", "%" + item.name.ToLower() + "%");

            TheLogger.Debug(sql + "\n");
            reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    id = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            if (id != -1)
            {
                // imageid = -1 update it
                updateknownitems(item, id);
                MyConnection.Close();
                return id;
            }
            cmd.Parameters.Clear();
            sql = "SELECT id FROM knownitems WHERE imageid = ?imageid and name sounds LIKE ?name order by imageid, name;";
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?imageid", item.imageid);
            cmd.Parameters.AddWithValue("?name", "%" + item.name.ToLower() + "%");

            TheLogger.Debug(sql + "\n");
            reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    id = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            if (id != -1)
            {
                MyConnection.Close();
                return id;
            }
            cmd.Parameters.Clear();
            sql = "SELECT id FROM knownitems WHERE imageid = ?imageid and name sounds LIKE ?name order by imageid, name;";
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?imageid", -1);
            cmd.Parameters.AddWithValue("?name", "%" + item.name.ToLower() + "%");

            TheLogger.Debug(sql + "\n");
            reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    id = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            if (id != -1)
            {
                // imageid = -1 update it
                updateknownitems(item, id);
                MyConnection.Close();
                return id;
            }
            if (id == -1)
            {
                //item.name = item.name.Replace("'","''");
                //item.description = item.description.Replace("'","''");

                cmd.Parameters.Clear();

                // requires unique index on name
                if(item.imageid == 0) TheLogger.ErrorLog("Possibly invalid Item: " + item.name + " added by " + Settings.botid + " in SetSQLID()");
                sql = "insert into knownitems (name,description,imageid,weight,is_resource,is_reagent,is_stackable,use_with_inventory) VALUES(?name,?description,?imageid,?weight,?is_resource,?is_reagent,?is_stackable,?use_with_inventory);";
                cmd = new MySqlCommand(sql, MyConnection);
                cmd.Parameters.AddWithValue("?name", item.name);
                cmd.Parameters.AddWithValue("?description", item.description);
                cmd.Parameters.AddWithValue("?imageid", item.imageid);
                cmd.Parameters.AddWithValue("?weight", item.weight);
                cmd.Parameters.AddWithValue("?is_resource", item.is_resource);
                cmd.Parameters.AddWithValue("?is_reagent", item.is_reagent);
                cmd.Parameters.AddWithValue("?is_stackable", item.is_stackable);
                cmd.Parameters.AddWithValue("?use_with_inventory", item.use_with_inventory);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException oMySQLException)
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
                catch (Exception oException)
                {
                    myErrorHandler.errorWriter(oException);
                }
                finally
                {
                    reader.Close();
                }

                sql = "SELECT id FROM knownitems WHERE imageid = ?imageid and name=?name order by imageid, name LIMIT 1;";
                cmd.Parameters.Clear();
                cmd = new MySqlCommand(sql, MyConnection);
                cmd.Parameters.AddWithValue("?imageid", item.imageid);
                cmd.Parameters.AddWithValue("?name", item.name.ToLower());

                reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        id = reader.GetInt16(0);
                    }
                }
                catch (MySqlException oMySQLException)
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
                catch (Exception oException)
                {
                    myErrorHandler.errorWriter(oException);
                }
                finally
                {
                    reader.Close();
                }
            }
            try
            {
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return id;
        }
        public void updateknownitems(Inventory.inventory_item item, int id)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "UPDATE knownitems SET imageid = ?imageid, description=?description, weight=?weight, is_resource=?is_resource, is_reagent=?is_reagent, is_stackable=?is_stackable, use_with_inventory=?use_with_inventory WHERE id=?id;";
            if (item.description == "")
            {
                sql = "UPDATE knownitems SET imageid = ?imageid, weight=?weight, is_resource=?is_resource, is_reagent=?is_reagent, is_stackable=?is_stackable, use_with_inventory=?use_with_inventory WHERE id=?id;";
            }

//            string sql = "UPDATE knownitems SET imageid = ?imageid, name=?name, description=?description, weight=?weight, is_resource=?is_resource, is_reagent=?is_reagent, is_stackable=?is_stackable, use_with_inventory=?use_with_inventory WHERE id=?id;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?imageid", item.imageid);
            cmd.Parameters.AddWithValue("?name", item.name);
            if (item.description != "")
            {
                cmd.Parameters.AddWithValue("?description", item.description);
            }
            cmd.Parameters.AddWithValue("?weight", item.weight);
            cmd.Parameters.AddWithValue("?is_resource", item.is_resource);
            cmd.Parameters.AddWithValue("?is_reagent", item.is_reagent);
            cmd.Parameters.AddWithValue("?is_stackable", item.is_stackable);
            cmd.Parameters.AddWithValue("?use_with_inventory", item.use_with_inventory);
            cmd.Parameters.AddWithValue("?id", id);

            try
            {
                TheLogger.Debug("MySQLDataReader start");
                cmd.ExecuteNonQuery(); 
                TheLogger.Debug("MySQLDataReader complete");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            return;
        }
        public void KillSleepingMySQLConnections(int iMinSecondsToExpire)
        {
            //need to fix this section so it only happens if the bot wasn't launched by bot manager...
            //don't really need _every_ bot checking for lost connections, one would suffice...
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SHOW PROCESSLIST";
            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            MySqlDataReader reader;
            System.Collections.ArrayList m_ProcessesToKill = new System.Collections.ArrayList();

            try
            {
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    uint iPID = Convert.ToUInt32(reader["Id"].ToString());
                    string strState = reader["Command"].ToString();
                    string db = reader["db"].ToString();
                    string user = reader["User"].ToString();
                    int iTime = Convert.ToInt32(reader["Time"].ToString());
                    if (user == this.SqlUsername && db == this.SqlDatabase && strState == "Sleep" && iTime >= iMinSecondsToExpire && iPID > 0)
                    {
                        // This connection is sitting around doing nothing. Kill it.
                        Console.WriteLine(iPID + " " + strState + " " + db + " " + user + " " + iTime);
                        m_ProcessesToKill.Add(iPID);
                    }
                }
                reader.Close();

                foreach (uint aPID in m_ProcessesToKill)
                {
                    sql = "kill " + aPID;
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
        }
        public void ClearInventoryList(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "DELETE FROM inventory WHERE botid=?botid;";
            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            return;
        }
        public void raw_sql(string sql)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            TheLogger.Debug("### SqlConnectionOpen() in raw_sql\n");
            TheLogger.Debug("raw_sql is [" + sql + "]\n");
            MySqlCommand cmd;
            try
            {
                cmd = new MySqlCommand(sql, MyConnection);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                TheLogger.Debug("### SqlConnectionClose() in raw_sql\n");
            }
            MyConnection.Close();

        }
        public void AddInventoryItem(Inventory.inventory_item item, int botid, bool delayed)
        {
            return;
        }
        public System.Collections.ArrayList GetSellingList(int advertIndcator)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            System.Collections.ArrayList SellingItemList = new System.Collections.ArrayList();

            MySqlCommand cmd;
            MySqlDataReader reader;

            string sql = "SELECT sellingitems.id, sellingitems.pricelistid, pricelist.knownitemsid, sellingitems.announce, pricelist.pricesale, pricelist.pricesalemembers FROM sellingitems,pricelist WHERE sellingitems.botid=?botid AND sellingitems.pricelistid=pricelist.id AND sellingitems.disabled = 0 AND sellingitems.announce >= " + advertIndcator + " order by rand() ";
            TheLogger.Debug(sql + "\n");
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);

            reader = cmd.ExecuteReader();
            try
            {

                while (reader.Read())
                {
                    TradeHandler.SellingItem MySellingItem = new TradeHandler.SellingItem();
                    MySellingItem.SellingItemsSqlID = reader.GetInt16(0);
                    MySellingItem.PricelistSqlID = reader.GetInt16(1);
                    MySellingItem.KnownItemsSqlID = reader.GetInt16(2);
                    MySellingItem.announce = reader.GetBoolean(3);
                    MySellingItem.pricesale = reader.GetDouble(4);
                    MySellingItem.pricesalemembers = reader.GetDouble(5);

                    SellingItemList.Add(MySellingItem);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return SellingItemList;
        }
        public System.Collections.Hashtable GetSellingItemList(int advertIndcator)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            System.Collections.Hashtable SellingItemList = new System.Collections.Hashtable();

            MySqlCommand cmd;
            MySqlDataReader reader;

            string sql = "SELECT sellingitems.id, sellingitems.pricelistid, pricelist.knownitemsid, sellingitems.announce, pricelist.pricesale, pricelist.pricesalemembers FROM sellingitems,pricelist WHERE sellingitems.botid=?botid AND sellingitems.pricelistid=pricelist.id AND sellingitems.disabled = 0 AND sellingitems.announce >= " + advertIndcator;
            if (advertIndcator > 0)
            {
                sql += " order by rand() ";
            }
            TheLogger.Debug(sql + "\n");
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);

            reader = cmd.ExecuteReader();
            try
            {

                while (reader.Read())
                {
                    TradeHandler.SellingItem MySellingItem = new TradeHandler.SellingItem();
                    MySellingItem.SellingItemsSqlID = reader.GetInt16(0);
                    MySellingItem.PricelistSqlID = reader.GetInt16(1);
                    MySellingItem.KnownItemsSqlID = reader.GetInt16(2);
                    MySellingItem.announce = reader.GetBoolean(3);
                    MySellingItem.pricesale = reader.GetDouble(4);
                    MySellingItem.pricesalemembers = reader.GetDouble(5);

                    SellingItemList.Add(MySellingItem.KnownItemsSqlID, MySellingItem);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return SellingItemList;
        }
        public System.Collections.ArrayList GetWantedList(int advertIndicator)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            TheLogger.Debug("Entered GetWantedItemList\nMaking connection\n");
            System.Collections.ArrayList WantedItemList = new System.Collections.ArrayList();

            TheLogger.Debug("MySqlDataReader reader;\n");
            MySqlDataReader reader;
            TheLogger.Debug("string sql=\n");
            string sql = "SELECT wanteditems.id, wanteditems.pricelistid, pricelist.knownitemsid, wanteditems.announce, pricelist.pricepurchase,wanteditems.maxquantity,pricelist.pricepurchasemembers, knownitems.name, knownitems.weight FROM wanteditems,pricelist,knownitems WHERE wanteditems.botid=?botid AND wanteditems.pricelistid=pricelist.id AND knownitems.id = pricelist.knownitemsid AND wanteditems.disabled = 0 AND wanteditems.announce >= " + advertIndicator + " order by rand() ";
            //string sql = "SELECT wanteditems.id, wanteditems.pricelistid, pricelist.knownitemsid, wanteditems.announce, pricelist.pricepurchase,wanteditems.maxquantity + (IF((wanteditems.maxquantity = 0),0,(SELECT count(inv.quantity) from inventory inv WHERE inv.knownitemsid = knownitems.id and inv.pos > 36 and inv.botid = wanteditems.botid))),pricelist.pricepurchasemembers, knownitems.name, knownitems.weight FROM wanteditems,pricelist,knownitems WHERE wanteditems.botid=?botid AND wanteditems.pricelistid=pricelist.id AND knownitems.id = pricelist.knownitemsid AND wanteditems.disabled = 0 AND wanteditems.announce >= " + advertIndicator + ";";
            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            reader = cmd.ExecuteReader();
            try
            {

                //get wanted item name here? already have the knownitemsid
                while (reader.Read())
                {
                    TradeHandler.WantedItem MyWantedItem = new TradeHandler.WantedItem();
                    MyWantedItem.SellingItemsSqlID = reader.GetInt16(0);
                    MyWantedItem.PricelistSqlID = reader.GetInt16(1);
                    MyWantedItem.KnownItemsSqlID = reader.GetInt16(2);
                    MyWantedItem.announce = reader.GetBoolean(3);
                    if (Settings.IsTradeBot == true)
                        MyWantedItem.pricepurchase = reader.GetDouble(4);
                    else
                        MyWantedItem.pricepurchase = 0;
                    MyWantedItem.maxquantity = reader.GetInt32(5);
                    if (Settings.IsTradeBot == true)
                        MyWantedItem.pricepurchasemembers = reader.GetDouble(6);
                    else
                        MyWantedItem.pricepurchasemembers = 0;
                    MyWantedItem.name = reader.GetString(7);
                    MyWantedItem.weight = reader.GetInt16(8);
                    WantedItemList.Add( MyWantedItem);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return WantedItemList;
        }
        public System.Collections.ArrayList GetWantedItemListIDs()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            TheLogger.Debug("Entered GetWantedItemList\nMaking connection\n");
            System.Collections.ArrayList WantedItemList = new System.Collections.ArrayList();

            TheLogger.Debug("MySqlDataReader reader;\n");
            MySqlDataReader reader;
            TheLogger.Debug("string sql=\n");
            string sql = "SELECT knownitems.id FROM wanteditems,pricelist,knownitems WHERE wanteditems.botid=?botid AND wanteditems.pricelistid=pricelist.id AND knownitems.id = pricelist.knownitemsid AND wanteditems.disabled = 0 ";
            //string sql = "SELECT wanteditems.id, wanteditems.pricelistid, pricelist.knownitemsid, wanteditems.announce, pricelist.pricepurchase,wanteditems.maxquantity + (IF((wanteditems.maxquantity = 0),0,(SELECT count(inv.quantity) from inventory inv WHERE inv.knownitemsid = knownitems.id and inv.pos > 36 and inv.botid = wanteditems.botid))),pricelist.pricepurchasemembers, knownitems.name, knownitems.weight FROM wanteditems,pricelist,knownitems WHERE wanteditems.botid=?botid AND wanteditems.pricelistid=pricelist.id AND knownitems.id = pricelist.knownitemsid AND wanteditems.disabled = 0 AND wanteditems.announce >= " + advertIndicator + ";";
            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            reader = cmd.ExecuteReader();
            try
            {

                //get wanted item name here? already have the knownitemsid
                while (reader.Read())
                {
                    TradeHandler.WantedItem MyWantedItem = new TradeHandler.WantedItem();
                    MyWantedItem.SellingItemsSqlID = reader.GetInt16(0);
                    WantedItemList.Add(MyWantedItem.SellingItemsSqlID);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return WantedItemList;
        }
        public System.Collections.Hashtable GetWantedItemList(int advertIndicator)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            TheLogger.Debug("Entered GetWantedItemList\nMaking connection\n");
            System.Collections.Hashtable WantedItemList = new System.Collections.Hashtable();

            TheLogger.Debug("MySqlDataReader reader;\n");
            MySqlDataReader reader;
            TheLogger.Debug("string sql=\n");
            string sql = "SELECT wanteditems.id, wanteditems.pricelistid, pricelist.knownitemsid, wanteditems.announce, pricelist.pricepurchase,wanteditems.maxquantity,pricelist.pricepurchasemembers, knownitems.name, knownitems.weight FROM wanteditems,pricelist,knownitems WHERE wanteditems.botid=?botid AND wanteditems.pricelistid=pricelist.id AND knownitems.id = pricelist.knownitemsid AND wanteditems.disabled = 0 AND wanteditems.announce >= " + advertIndicator + " ";
            if (advertIndicator > 0)
            {
                sql += " order by rand() ";
            }
            //string sql = "SELECT wanteditems.id, wanteditems.pricelistid, pricelist.knownitemsid, wanteditems.announce, pricelist.pricepurchase,wanteditems.maxquantity + (IF((wanteditems.maxquantity = 0),0,(SELECT count(inv.quantity) from inventory inv WHERE inv.knownitemsid = knownitems.id and inv.pos > 36 and inv.botid = wanteditems.botid))),pricelist.pricepurchasemembers, knownitems.name, knownitems.weight FROM wanteditems,pricelist,knownitems WHERE wanteditems.botid=?botid AND wanteditems.pricelistid=pricelist.id AND knownitems.id = pricelist.knownitemsid AND wanteditems.disabled = 0 AND wanteditems.announce >= " + advertIndicator + ";";
            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            reader = cmd.ExecuteReader();
            try
            {

                //get wanted item name here? already have the knownitemsid
                while (reader.Read())
                {
                    TradeHandler.WantedItem MyWantedItem = new TradeHandler.WantedItem();
                    MyWantedItem.SellingItemsSqlID = reader.GetInt16(0);
                    MyWantedItem.PricelistSqlID = reader.GetInt16(1);
                    MyWantedItem.KnownItemsSqlID = reader.GetInt16(2);
                    MyWantedItem.announce = reader.GetBoolean(3);
                    if (Settings.IsTradeBot == true)
                        MyWantedItem.pricepurchase = reader.GetDouble(4);
                    else
                        MyWantedItem.pricepurchase = 0;
                    MyWantedItem.maxquantity = reader.GetInt32(5);
                    if (Settings.IsTradeBot == true)
                        MyWantedItem.pricepurchasemembers = reader.GetDouble(6);
                    else
                        MyWantedItem.pricepurchasemembers = 0;
                    MyWantedItem.name = reader.GetString(7);
                    MyWantedItem.weight = reader.GetInt16(8);
                    WantedItemList.Add(MyWantedItem.KnownItemsSqlID, MyWantedItem);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return WantedItemList;
        }

        public string Getgreeting(string username, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string greeting = "", str = "";
            int i;
            string sql = "SELECT greeting FROM users WHERE LOWER(name)=?name AND botid=?botid AND date_add(last_greet_time, INTERVAL greet_interval MINUTE) < NOW() LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", username.ToLower());
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {
                while (reader.Read())
                {
                    if (!reader["greeting"].Equals(System.DBNull.Value))
                    {
                        greeting = reader.GetString(0);
                        if (greeting.ToLower().Contains("%username"))
                        {
                            str = greeting;
                            i = str.ToLower().IndexOf("%username");
                            str = str.Substring(0, i) + "%username" + str.Substring(i + 9);
                            greeting = str;
                        }
                        greeting = greeting.Replace("%username", username);
                        greeting = greeting.Replace("\\\\", "\\").Replace("\\\'", "\'").Replace("\\\"", "\"");
                    }
                }
                TheLogger.Debug("Complete\n");
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return greeting;
        }
        public void updateGreeting(string username, int botid)
        {
            string sql = "UPDATE users SET last_greet_time = now()";
            sql += " WHERE botid= " + Settings.botid.ToString() + " AND name = '" + username + "'";
            TheLogger.Debug(sql + "\n");
            raw_sql(sql);
        }
        public System.Collections.ArrayList GetBotAdverts(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            System.Collections.ArrayList AdvertsArrayList = new System.Collections.ArrayList();

            string sql = "SELECT adverttext FROM adverts WHERE botid=?botid AND disabled=0;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    AdvertsArrayList.Add(reader.GetString(0));
                }

            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return AdvertsArrayList;
        }
        public System.Collections.ArrayList GetBotAltAdverts(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            System.Collections.ArrayList AdvertsArrayList = new System.Collections.ArrayList();

            string sql = "SELECT text FROM otheradverts WHERE botid=?botid AND disabled=0;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    AdvertsArrayList.Add(reader.GetString(0));
                }

            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return AdvertsArrayList;
        }
        public bool CheckIfTradeMember(string username, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool istrademember = false;

            string sql = "SELECT istrademember FROM users WHERE LOWER(name)=?name AND botid=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", username.ToLower());
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            try
            {
                while (reader.Read())
                {
                    if (reader.GetInt32(0) == 1)
                    {
                        istrademember = true;
                        TheLogger.Log("Trademember: " + istrademember.ToString() + "\n");
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            if (istrademember)
                return istrademember;
            int r = CheckIfBannedGuild(username, Settings.botid);
            if (r > 0)
                return true;
            else
                return false;
            //return false;

        }
        public bool CheckIfGuildMember(string username, int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool isguildmember = false;

            string sql = "SELECT isguildmember FROM users WHERE LOWER(name)=?name AND botid=?botid LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", username.ToLower());
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    if (reader.GetInt32(0) == 1)
                    {
                        isguildmember = true;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return isguildmember;
        }
        public int GetItemID(string name, bool fromStorage)
        {
            int itemID = GetItemID(name, 0, fromStorage);
            return itemID;
        }
        public int GetItemID(string name, int botid, bool fromStorage)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int id = -1;
            string itemname = "";
            string namePlural = name.ToLower();
            namePlural = namePlural.Replace('s', '%');
            string sql = "SELECT id,name FROM knownitems WHERE ((LOWER(name)=?name) or (LOWER(name) like ?namelike) or (LOWER(name) sounds like ?namelike)) order by name;";
            if (botid > 0)
            {
                if (fromStorage)
                {
                    sql = "SELECT distinct knownitems.id,knownitems.name FROM knownitems, storage WHERE ((LOWER(knownitems.name)=?name) or (LOWER(knownitems.name) like ?namelike) or " +
                          "(LOWER(knownitems.name) sounds like ?namelike)) AND storage.knownitemsid = knownitems.id AND storage.botid = ?botid AND storage.quantity > 0 order by name;";
                }
                else
                {
                    sql = "SELECT distinct knownitems.id,knownitems.name FROM knownitems, inventory WHERE ((LOWER(knownitems.name)=?name) or (LOWER(knownitems.name) like ?namelike) or " +
                          "(LOWER(knownitems.name) sounds like ?namelike)) AND inventory.knownitemsid = knownitems.id AND inventory.botid = ?botid AND inventory.quantity > 0 order by name;";
                }
            }
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", name.Trim().ToLower());
            cmd.Parameters.AddWithValue("?namelike", "%"+namePlural.Trim().ToLower()+"%");
            if (botid > 0)
            {
                cmd.Parameters.AddWithValue("?botid", botid);
            }

            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");
            int tempID = -1;
            string tempName = "";
            bool notFound = true;
            try
            {
                while (reader.Read())
                {
                    id = reader.GetInt16(0);
                    itemname = reader.GetString(1);
                    if (itemname.ToLower() == name.Trim().ToLower())
                    {
                        notFound = false;
                        break;
                    }
                    if (itemname.ToLower().Contains(name.Trim().ToLower()))
                    {
                        tempID = id;
                        tempName = itemname;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                cmd.Parameters.Clear();
                reader.Close();
            }
            MyConnection.Close();
            if (tempID != -1 && notFound)
            {
                id = tempID;
            }
            return id;
        }
        public string GetDescription(int ITEMID)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string str = "";
            string sql = "SELECT description FROM knownitems WHERE id=?id;";
            //'" + ITEMID.ToString() + "'";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?id", ITEMID);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    str = reader.GetString(0);
                    str = str.Replace("\\\\", "\\").Replace("\\\'", "\'").Replace("\\\"", "\"");
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return str;
        }
        public string GetDescriptionExtra(int ITEMID)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string str = "";
            int i = 0;
            string sql = "SELECT is_resource, is_reagent, is_stackable, use_with_inventory FROM knownitems WHERE id=?id";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?id", ITEMID);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    i = reader.GetInt16(0);
                    str += i != 0 ? "Resource" : "";
                    i = reader.GetInt16(1);
                    str += i != 0 ? " reagent" : "";
                    i = reader.GetInt16(2);
                    str += i != 0 ? " stackable" : "";
                    i = reader.GetInt16(3);
                    str += i != 0 ? " usable" : "";
                }
                str = str.Trim();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return str;
        }
        public int GetBotPhysqiue(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int Physique = 4;
            //string sql = "SELECT Physique FROM bots WHERE botid='" + botid.ToString() + "' and id='" + botid.ToString() + "';";
            string sql = "SELECT physiquecur FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    Physique = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return Physique;
        }
        public int GetBotCoordination(int botid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int Coordination = 4;
            //string sql = "SELECT Coordination FROM bots WHERE botid='" + botid.ToString() + "' and id='" + botid.ToString() + "';";
            string sql = "SELECT coordinationcur FROM bots WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", botid);
            TheLogger.Debug("MySQLDataReader start"); MySqlDataReader reader = cmd.ExecuteReader(); TheLogger.Debug("MySQLDataReader complete");

            try
            {
                while (reader.Read())
                {
                    Coordination = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return Coordination;
        }
        public int GetBotUsedSlots(int botid)
        {
            int UsedSlots = 0;
            return UsedSlots;
        }
        public int GetBotUsedSpace(int botid)
        {
            int UsedSpace = 0;
            return UsedSpace;
        }
        public void UpdateAllStats(int botid, byte[] data)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int i = 3;

            int physiquecur = System.BitConverter.ToUInt16(data, i); i += 2;
            int physiquebase = System.BitConverter.ToUInt16(data, i); i += 2;
            int coordinationcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int coordinationbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int reasoningcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int reasoningbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int willcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int willbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int instinctcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int instinctbase = System.BitConverter.ToUInt16(data, i); i += 2;
           
            int vitalitycur = System.BitConverter.ToUInt16(data, i); i += 2;
            int vitalitybase = System.BitConverter.ToUInt16(data, i); i += 2;

            int humannexuscur = System.BitConverter.ToUInt16(data, i); i += 2;
            int humannexusbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int animalnexuscur = System.BitConverter.ToUInt16(data, i); i += 2;
            int animalnexusbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int vegetalnexuscur = System.BitConverter.ToUInt16(data, i); i += 2;
            int vegetalnexusbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int inorganicnexuscur = System.BitConverter.ToUInt16(data, i); i += 2;
            int inorganicnexusbase = System.BitConverter.ToUInt16(data, i); i += 2;
            
            int artificialnexuscur = System.BitConverter.ToUInt16(data, i); i += 2;
            int artificialnexusbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int magicnexuscur = System.BitConverter.ToUInt16(data, i); i += 2;
            int magicnexusbase = System.BitConverter.ToUInt16(data, i); i += 2;

            int manufacturingskillcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int manufacturingskillbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int harvestingskillcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int harvestingskillbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int alchemyskillcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int alchemyskillbase = System.BitConverter.ToUInt16(data, i); i += 2;
            
            int overallskillcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int overallskillbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int attackskillcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int attackskillbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int defenseskillcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int defenseskillbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int magicskillcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int magicskillbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int potionskillcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int potionskillbase = System.BitConverter.ToUInt16(data, i); i += 2;
            
            int carrycapacitycur = System.BitConverter.ToUInt16(data, i); i += 2;
            int carrycapacitybase = System.BitConverter.ToUInt16(data, i); i += 2;
            int materialpointscur = System.BitConverter.ToUInt16(data, i); i += 2;
            int materialpointsbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int etherealpointscur = System.BitConverter.ToUInt16(data, i); i += 2;
            int etherealpointsbase = System.BitConverter.ToUInt16(data, i); i += 2;
            int foodlevel = System.BitConverter.ToUInt16(data, i); i += 6;
            if (foodlevel > 255)
                foodlevel -= 65536;

            UInt32 manufacturingexp = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 manufacturingexpnextlevel = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 harvestingexp = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 harvestingexpnextlevel = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 alchemyexp = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 alchemyexpnextlevel = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 overallexp = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 overallexpnextlevel = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 attackexp = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 attackexpnextlevel = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 defenseexp = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 defenseexpnextlevel = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 magicexp = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 magicexpnextlevel = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 potionexp = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 potionexpnextlevel = System.BitConverter.ToUInt32(data, i); i += 8;

            int summoningskillcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int summoningskillbase = System.BitConverter.ToUInt16(data, i); i += 2;
            UInt32 summoningexp = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 summoningexpnextlevel = System.BitConverter.ToUInt32(data, i); i += 4;
            int craftingskillcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int craftingskillbase = System.BitConverter.ToUInt16(data, i); i += 2;
            UInt32 craftingexp = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 craftingexpnextlevel = System.BitConverter.ToUInt32(data, i); i += 4;

            int engineeringskillcur = System.BitConverter.ToUInt16(data, i); i += 2;
            int engineeringskillbase = System.BitConverter.ToUInt16(data, i); i += 2;
            UInt32 engineeringexp = System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 engineeringexpnextlevel = System.BitConverter.ToUInt32(data, i); i += 4;

            int tailoringskillcur = 0; // System.BitConverter.ToUInt16(data, i); i += 2;
            int tailoringskillbase = 0; // System.BitConverter.ToUInt16(data, i); i += 2;
            UInt32 tailoringexp = 42; // System.BitConverter.ToUInt32(data, i); i += 4;
            UInt32 tailoringexpnextlevel = 1066; // System.BitConverter.ToUInt32(data, i);

            int researchcompleted = System.BitConverter.ToUInt16(data, 99);//
            int researching = System.BitConverter.ToInt16(data, 165);
            int researchTotal = System.BitConverter.ToUInt16(data, 167);

            string sql = "UPDATE bots SET ";
            sql += "physiquecur=?physiquecur,";
            sql += "physiquebase= ?physiquebase,";
            sql += "coordinationbase=?coordinationbase,";
            sql += "coordinationcur=?coordinationcur,";
            sql += "reasoningbase=?reasoningbase,";
            sql += "reasoningcur=?reasoningcur,";
            sql += "willbase=?willbase,";
            sql += "willcur=?willcur,";
            sql += "instinctbase=?instinctbase,";
            sql += "instinctcur=?instinctcur,";
            sql += "vitalitybase=?vitalitybase,";
            sql += "vitalitycur=?vitalitycur,";

            sql += "humannexuscur=?humannexuscur,";
            sql += "humannexusbase=?humannexusbase,";
            sql += "animalnexuscur=?animalnexuscur,";
            sql += "animalnexusbase=?animalnexusbase,";
            sql += "vegetalnexuscur=?vegetalnexuscur,";
            sql += "vegetalnexusbase=?vegetalnexusbase,";
            sql += "inorganicnexuscur=?inorganicnexuscur,";
            sql += "inorganicnexusbase=?inorganicnexusbase,";
            sql += "artificialnexuscur=?artificialnexuscur,";
            sql += "artificialnexusbase=?artificialnexusbase,";
            sql += "magicnexuscur=?magicnexuscur,";
            sql += "magicnexusbase=?magicnexusbase,";

            sql += "manufacturingskillcur=?manufacturingskillcur,";
            sql += "manufacturingskillbase=?manufacturingskillbase,";
            sql += "harvestingskillcur=?harvestingskillcur,";
            sql += "harvestingskillbase=?harvestingskillbase,";
            sql += "alchemyskillcur=?alchemyskillcur,";
            sql += "alchemyskillbase=?alchemyskillbase,";
            sql += "overallskillcur=?overallskillcur,";
            sql += "overallskillbase=?overallskillbase,";
            sql += "attackskillcur=?attackskillcur,";
            sql += "attackskillbase=?attackskillbase,";
            sql += "defenseskillcur=?defenseskillcur,";
            sql += "defenseskillbase=?defenseskillbase,";
            sql += "magicskillcur=?magicskillcur,";
            sql += "magicskillbase=?magicskillbase,";
            sql += "potionskillcur=?potionskillcur,";
            sql += "potionskillbase=?potionskillbase,";
            sql += "carrycapacitycur=?carrycapacitycur,";
            sql += "carrycapacitybase=?carrycapacitybase,";
            sql += "materialpointscur=?materialpointscur,";
            sql += "materialpointsbase=?materialpointsbase,";
            sql += "etherealpointscur=?etherealpointscur,";
            sql += "etherealpointsbase=?etherealpointsbase,";
            sql += "foodlevel=" + foodlevel.ToString() + ",";

            sql += "manufacturingexp=?manufacturingexp,";
            sql += "manufacturingexpnextlevel=?manufacturingexpnextlevel,";
            sql += "harvestingexp=?harvestingexp,";
            sql += "harvestingexpnextlevel=?harvestingexpnextlevel,";
            sql += "alchemyexp=?alchemyexp,";
            sql += "alchemyexpnextlevel=?alchemyexpnextlevel,";
            sql += "overallexp=?overallexp,";
            sql += "overallexpnextlevel=?overallexpnextlevel,";
            sql += "attackexp=?attackexp,";
            sql += "attackexpnextlevel=?attackexpnextlevel,";
            sql += "defenseexp=?defenseexp,";
            sql += "defenseexpnextlevel=?defenseexpnextlevel,";
            sql += "magicexp=?magicexp,";
            sql += "magicexpnextlevel=?magicexpnextlevel,";
            sql += "potionexp=?potionexp,";
            sql += "potionexpnextlevel=?potionexpnextlevel,";

            sql += "summoningskillcur=?summoningskillcur,";
            sql += "summoningskillbase=?summoningskillbase,";
            sql += "summoningexp=?summoningexp,";
            sql += "summoningexpnextlevel=?summoningexpnextlevel,";
            sql += "craftingskillcur=?craftingskillcur,";
            sql += "craftingskillbase=?craftingskillbase,";
            sql += "craftingskillnextlevel=?craftingexpnextlevel,";
            sql += "craftingexp=?craftingexp,";
            sql += "craftingexpnextlevel=?craftingexpnextlevel,";

            sql += "engineeringskillcur=?engineeringskillcur,";
            sql += "engineeringskillbase=?engineeringskillbase,";
            sql += "engineeringskillnextlevel=?engineeringexpnextlevel,";
            sql += "engineeringexp=?engineeringexp,";
            sql += "engineeringexpnextlevel=?engineeringexpnextlevel,";

            sql += "tailoringskillcur=?tailoringskillcur,";
            sql += "tailoringskillbase=?tailoringskillbase,";
            sql += "tailoringskillnextlevel=?tailoringexpnextlevel,";
            sql += "tailoringexp=?tailoringexp,";
            sql += "tailoringexpnextlevel=?tailoringexpnextlevel,";

            sql += "researchcompleted=?researchcompleted,";
            sql += "researching=?researching,";
            sql += "researchTotal=?researchTotal ";
            sql += "WHERE id=?botid;";
            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?physiquecur", physiquecur);
            cmd.Parameters.AddWithValue("?physiquebase", physiquebase);
            cmd.Parameters.AddWithValue("?coordinationbase", coordinationbase);
            cmd.Parameters.AddWithValue("?coordinationcur", coordinationcur);
            cmd.Parameters.AddWithValue("?reasoningbase", reasoningbase);
            cmd.Parameters.AddWithValue("?reasoningcur", reasoningcur);
            cmd.Parameters.AddWithValue("?willbase", willbase);
            cmd.Parameters.AddWithValue("?willcur", willcur);
            cmd.Parameters.AddWithValue("?instinctbase", instinctbase);
            cmd.Parameters.AddWithValue("?instinctcur", instinctcur);
            cmd.Parameters.AddWithValue("?vitalitybase", vitalitybase);
            cmd.Parameters.AddWithValue("?vitalitycur", vitalitycur);
            cmd.Parameters.AddWithValue("?humannexuscur", humannexuscur);
            cmd.Parameters.AddWithValue("?humannexusbase", humannexusbase);
            cmd.Parameters.AddWithValue("?animalnexuscur", animalnexuscur);
            cmd.Parameters.AddWithValue("?animalnexusbase", animalnexusbase);
            cmd.Parameters.AddWithValue("?vegetalnexuscur", vegetalnexuscur);
            cmd.Parameters.AddWithValue("?vegetalnexusbase", vegetalnexusbase);
            cmd.Parameters.AddWithValue("?inorganicnexuscur", inorganicnexuscur);
            cmd.Parameters.AddWithValue("?inorganicnexusbase", inorganicnexusbase);
            cmd.Parameters.AddWithValue("?artificialnexuscur", artificialnexuscur);
            cmd.Parameters.AddWithValue("?artificialnexusbase", artificialnexusbase);
            cmd.Parameters.AddWithValue("?magicnexuscur", magicnexuscur);
            cmd.Parameters.AddWithValue("?magicnexusbase", magicnexusbase);
            cmd.Parameters.AddWithValue("?manufacturingskillcur", manufacturingskillcur);
            cmd.Parameters.AddWithValue("?manufacturingskillbase", manufacturingskillbase);
            cmd.Parameters.AddWithValue("?harvestingskillcur", harvestingskillcur);
            cmd.Parameters.AddWithValue("?harvestingskillbase", harvestingskillbase);
            cmd.Parameters.AddWithValue("?alchemyskillcur", alchemyskillcur);
            cmd.Parameters.AddWithValue("?alchemyskillbase", alchemyskillbase);
            cmd.Parameters.AddWithValue("?overallskillcur", overallskillcur);
            cmd.Parameters.AddWithValue("?overallskillbase", overallskillbase);
            cmd.Parameters.AddWithValue("?attackskillcur", attackskillcur);
            cmd.Parameters.AddWithValue("?attackskillbase", attackskillbase);
            cmd.Parameters.AddWithValue("?defenseskillcur", defenseskillcur);
            cmd.Parameters.AddWithValue("?defenseskillbase", defenseskillbase);
            cmd.Parameters.AddWithValue("?magicskillcur", magicskillcur);
            cmd.Parameters.AddWithValue("?magicskillbase", magicskillbase);
            cmd.Parameters.AddWithValue("?potionskillcur", potionskillcur);
            cmd.Parameters.AddWithValue("?potionskillbase", potionskillbase);
            cmd.Parameters.AddWithValue("?carrycapacitycur", carrycapacitycur);
            cmd.Parameters.AddWithValue("?carrycapacitybase", carrycapacitybase);
            cmd.Parameters.AddWithValue("?materialpointscur", materialpointscur);
            cmd.Parameters.AddWithValue("?materialpointsbase", materialpointsbase);
            cmd.Parameters.AddWithValue("?etherealpointscur", etherealpointscur);
            cmd.Parameters.AddWithValue("?etherealpointsbase", etherealpointsbase);
            cmd.Parameters.AddWithValue("?foodlevel", foodlevel);
            cmd.Parameters.AddWithValue("?manufacturingexp", manufacturingexp);
            cmd.Parameters.AddWithValue("?manufacturingexpnextlevel", manufacturingexpnextlevel);
            cmd.Parameters.AddWithValue("?harvestingexp", harvestingexp);
            cmd.Parameters.AddWithValue("?harvestingexpnextlevel", harvestingexpnextlevel);
            cmd.Parameters.AddWithValue("?alchemyexp", alchemyexp);
            cmd.Parameters.AddWithValue("?alchemyexpnextlevel", alchemyexpnextlevel);
            cmd.Parameters.AddWithValue("?overallexp", overallexp);
            cmd.Parameters.AddWithValue("?overallexpnextlevel", overallexpnextlevel);
            cmd.Parameters.AddWithValue("?attackexp", attackexp);
            cmd.Parameters.AddWithValue("?attackexpnextlevel", attackexpnextlevel);
            cmd.Parameters.AddWithValue("?defenseexp", defenseexp);
            cmd.Parameters.AddWithValue("?defenseexpnextlevel", defenseexpnextlevel);
            cmd.Parameters.AddWithValue("?magicexp", magicexp);
            cmd.Parameters.AddWithValue("?magicexpnextlevel", magicexpnextlevel);
            cmd.Parameters.AddWithValue("?potionexp", potionexp);
            cmd.Parameters.AddWithValue("?potionexpnextlevel", potionexpnextlevel);
            cmd.Parameters.AddWithValue("?summoningskillcur", summoningskillcur);
            cmd.Parameters.AddWithValue("?summoningskillbase", summoningskillbase);
            cmd.Parameters.AddWithValue("?summoningexp", summoningexp);
            cmd.Parameters.AddWithValue("?summoningexpnextlevel", summoningexpnextlevel);
            cmd.Parameters.AddWithValue("?craftingskillcur", craftingskillcur);
            cmd.Parameters.AddWithValue("?craftingskillbase", craftingskillbase);
            cmd.Parameters.AddWithValue("?craftingskillnextlevel", craftingexpnextlevel);
            cmd.Parameters.AddWithValue("?craftingexp", craftingexp);
            cmd.Parameters.AddWithValue("?craftingexpnextlevel", craftingexpnextlevel);
            cmd.Parameters.AddWithValue("?engineeringskillcur", engineeringskillcur);
            cmd.Parameters.AddWithValue("?engineeringskillbase", engineeringskillbase);
            cmd.Parameters.AddWithValue("?engineeringskillnextlevel", engineeringexpnextlevel);
            cmd.Parameters.AddWithValue("?engineeringexp", engineeringexp);
            cmd.Parameters.AddWithValue("?engineeringexpnextlevel", engineeringexpnextlevel);
            cmd.Parameters.AddWithValue("?tailoringskillcur", tailoringskillcur);
            cmd.Parameters.AddWithValue("?tailoringskillbase", tailoringskillbase);
            cmd.Parameters.AddWithValue("?tailoringskillnextlevel", tailoringexpnextlevel);
            cmd.Parameters.AddWithValue("?tailoringexp", tailoringexp);
            cmd.Parameters.AddWithValue("?tailoringexpnextlevel", tailoringexpnextlevel);
            cmd.Parameters.AddWithValue("?researchcompleted", researchcompleted);
            cmd.Parameters.AddWithValue("?researching", researching);
            cmd.Parameters.AddWithValue("?researchTotal", researchTotal);
            cmd.Parameters.AddWithValue("?botid", botid);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            return;
        }
        public void UpdateSingleStat(string str, int value)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "UPDATE `bots` SET `" + str + "` = '" + value.ToString() + "' WHERE id=?botid;";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            return;
        }
        public bool validLaunch(string botName, string userName)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            Int16 botid = 0;
            bool launchValid = false;
            string sql = "SELECT users.botid FROM users, bots WHERE LOWER(users.name)=?userName AND users.botid = bots.id AND LOWER(bots.name) = ?botName AND may_launch = 1 LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?userName", userName.ToLower());
            cmd.Parameters.AddWithValue("?botName", botName.ToLower());
            TheLogger.Debug("MySQLDataReader start");
            MySqlDataReader reader = cmd.ExecuteReader();
            TheLogger.Debug("MySQLDataReader complete");
            try
            {
                while (reader.Read())
                {
                    botid = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                if (oException.Message != "Data is Null. This method or property cannot be called on Null values.")
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
                reader.Close();
                TheLogger.Debug("### SqlConnectionClose() in getLaunchPath\n");
            }
            if (botid > 0)
            {
                launchValid = true;
            }
            MyConnection.Close();
            return launchValid;

        }
        public string getConnectionStatus(string botName)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string connectionStatus = "ALREADY LOGGED IN";
            int connectedToServer = 0;

            string sql = "SELECT connectedToServer FROM bots WHERE name=?name LIMIT 1;";
            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?name", botName.ToLower());
            TheLogger.Debug("MySQLDataReader start"); 
            MySqlDataReader reader = cmd.ExecuteReader(); 
            TheLogger.Debug("MySQLDataReader complete");
            try
            {
                while (reader.Read())
                {
                    connectedToServer = reader.GetUInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            if (connectedToServer == 0)
            { 
                connectionStatus = "LOGGED OUT";
            }
            else if (connectedToServer == 2)
            {
                connectionStatus = "ALREADY PENDING";
            }
            else if (connectedToServer == 3)
            {
                connectionStatus = "STARTED";
            }
            MyConnection.Close();
            return connectionStatus;
        }
        public void ImPending(string botName)
        {
            string sql = "UPDATE bots SET connectedToServer = 2 WHERE name = '" + botName + "'";
            TheLogger.Debug(sql + "\n");
            raw_sql(sql);
            return;
        }
        public void ImStarted(int botID)
        {
            string sql = "UPDATE bots SET connectedToServer = 3 WHERE botid = " + botID.ToString();
            TheLogger.Debug(sql + "\n");
            raw_sql(sql);
            return;
        }
	public void ImLoggedIn(int botID)
        {
            string sql = "UPDATE bots SET connectedToServer = 1 WHERE botid = " + botID.ToString();
            TheLogger.Debug(sql + "\n");
            raw_sql(sql);
            return;
        }
        public void ImLoggedOut(int botID)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "UPDATE bots SET connectedToServer = 9 WHERE botid = " + botID.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            TheLogger.Debug(sql + "\n");
            //System.Console.WriteLine(myException.Message); Environment.Exit(0);
            
            //raw_sql(sql);

            //System.Threading.Thread.Sleep(150);
//            MainClass.MyConnection.Close();

            return;
        }
        public void InsertStorageCategory( Storage.StorageCategory MyStorageCategory )
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "INSERT INTO storagecategory (id, name, botid) VALUES(?id, ?name, ?botid);";

            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?id", MyStorageCategory.num );
            cmd.Parameters.AddWithValue("?name", MyStorageCategory.name );
            cmd.Parameters.AddWithValue("?botid", Settings.botid );

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            return;
        }
        public void InsertStorageItems(System.Collections.ArrayList myStorage)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            //string sql = "INSERT INTO storage (name, quantity, knownitemsid, categorynum, imageid, botid) VALUES(?name, ?quantity, ?knownitemsid, ?categorynum, ?imageid, ?botid);";
            //string sql = "INSERT INTO storage (name, quantity, knownitemsid, categorynum, imageid, botid) VALUES + ;";

            //TheLogger.Debug(sql + "\n");
            //MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            string sql = "";
            try
            {
                string values = "";
                int count = 0;
                foreach (Storage.StorageItem MyStorageItem in myStorage)
                {
                    count++;
                    string tempName = MyStorageItem.name.Replace("'", "\\'");
                    values += "('" + tempName + "'" + ',';
                    values += MyStorageItem.quantity.ToString() + ',';
                    values += MyStorageItem.knownItemsID.ToString() + ',';
                    values += MyStorageItem.category_num.ToString() + ',';
                    values += MyStorageItem.imageid.ToString() + ',';
                    values += Settings.botid.ToString() + ")";
                    if (count < myStorage.Count)
                    {
                        values += ',';
                    }
                }
                sql = "INSERT INTO storage (name, quantity, knownitemsid, categorynum, imageid, botid) VALUES " + values;

                TheLogger.Debug(sql + "\n");
                MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            return;
        }
        public void DeleteStorageItem(Storage.StorageItem MyStorageItem)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "DELETE FROM storage WHERE knownitemsid = ?knownitemsid AND botid = ?botid;";

            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?knownitemsid", MyStorageItem.knownItemsID);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            MyConnection.Close();
            return;
        }
        public void UpdateStorageItem(TradeHandler.TradeLogItem MyTradeLogItem, bool replaceQuantity)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            int tempQuantity;
            tempQuantity = (int)MyTradeLogItem.quantity;
            if (MyTradeLogItem.action.Contains("sold") || MyTradeLogItem.action.Contains("gave") || MyTradeLogItem.action.ToLower().Contains("withdrew"))
            {
                if (!replaceQuantity)
                {
                    tempQuantity = tempQuantity * -1;
                }
            }
            int rowsAffected = 0;
            string itemName = "";
            int imageid = 0;
            string sql = "UPDATE Storage SET quantity = quantity + ?quantity WHERE knownitemsid = ?knownitemsid AND botid = ?botid;";
            if (replaceQuantity)
            {
                sql = "UPDATE Storage SET quantity = ?quantity WHERE knownitemsid = ?knownitemsid AND botid = ?botid;";
            }

            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?quantity", tempQuantity);
            cmd.Parameters.AddWithValue("?knownitemsid", MyTradeLogItem.KnownItemsSqlID);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            try
            {
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            //didn't update any rows, must be a new item, so insert it with what we have...
            //the next time the storage is open, the rest of the info will be known
            if (rowsAffected == 0)
            {
                //ok, we need some info from the known items table about this item
                //since this function may end up being called from several locations
                //the amount of known information isn't standard....
                //easier to get it here than try to change the entire program....
                sql = "SELECT name, imageid FROM knownitems WHERE id = ?id ;";
                TheLogger.Debug(sql + "\n");
                cmd = new MySqlCommand(sql, MyConnection);
                cmd.Parameters.AddWithValue("?id", MyTradeLogItem.KnownItemsSqlID);
                TheLogger.Debug("MySQLDataReader start");
                MySqlDataReader reader = cmd.ExecuteReader();
                TheLogger.Debug("MySQLDataReader complete");
                try
                {
                    while (reader.Read())
                    {
                        itemName = reader.GetString(0);
                        imageid = reader.GetInt16(1);
                    }
                }
                catch (MySqlException oMySQLException)
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
                catch (Exception oException)
                {
                    myErrorHandler.errorWriter(oException);
                }
                finally
                {
                    reader.Close();
                }

//                sql = "INSERT DELAYED INTO Storage (name, knownitemsid, imageid, quantity, botid, categorynum) VALUES (?name, ?knownitemsid, ?imageid, ?quantity, ?botid, ?categorynum);";
                sql = "INSERT INTO storage (name, knownitemsid, imageid, quantity, botid, categorynum) VALUES (?name, ?knownitemsid, ?imageid, ?quantity, ?botid, ?categorynum);";
                TheLogger.Debug(sql + "\n");
                cmd = new MySqlCommand(sql, MyConnection);
                cmd.Parameters.AddWithValue("?quantity", MyTradeLogItem.quantity);
                cmd.Parameters.AddWithValue("?name", itemName);
                cmd.Parameters.AddWithValue("?imageid", imageid);
                cmd.Parameters.AddWithValue("?knownitemsid", MyTradeLogItem.KnownItemsSqlID);
                cmd.Parameters.AddWithValue("?categorynum", MyTradeLogItem.categoryNum );
                cmd.Parameters.AddWithValue("?botid", Settings.botid);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException oMySQLException)
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
                catch (Exception oException)
                {
                    myErrorHandler.errorWriter(oException);
                }
                finally
                {
                    TheLogger.Debug("### SqlConnectionClose() in UpdateStorageItem \n");
                }
            }
            MyConnection.Close();
            return;
        }
        public void ClearStorage()
        {
            string sql = "DELETE QUICK FROM storage WHERE botid = " + Settings.botid.ToString() + ";";
            TheLogger.Debug(sql + "\n");
            raw_sql(sql);
            sql = "DELETE QUICK FROM storagecategory WHERE botid = " + Settings.botid.ToString() + ";";
            TheLogger.Debug(sql + "\n");
            raw_sql(sql);
        }
        public uint ReservedAmountForUser(int SQLID, string userid)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            uint amountReserved = 0;
            string sql = "SELECT SUM(quantity) FROM reservedamount WHERE knownitemsid = ?knownitemsid AND botid = ?botid AND reservedBy = ?reservedBy AND claimed = 0 AND expirationdate >= now();";

            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?knownitemsid", SQLID);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            cmd.Parameters.AddWithValue("?reservedBy", userid);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    amountReserved = reader.GetUInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                if (oException.Message != "Data is Null. This method or property cannot be called on Null values.")
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
                if (!reader.IsClosed)
                    reader.Close();
            }

            MyConnection.Close();
            return amountReserved;
        }
        public uint ReservedAmount(int SQLID)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            uint amountReserved = 0;
            string sql = "SELECT SUM(quantity) FROM reservedamount WHERE knownitemsid = ?knownitemsid AND botid = ?botid AND claimed = 0 AND expirationdate >= now();";

            TheLogger.Debug(sql + "\n");

            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?knownitemsid", SQLID);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    amountReserved = reader.GetUInt32(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                if (oException.Message != "Data is Null. This method or property cannot be called on Null values.")
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
                if (!reader.IsClosed)
                    reader.Close();
            }

            MyConnection.Close();
            return amountReserved;
        }
        public bool reserveItem(int itemId, uint numberToReserve, string username)
        {
            bool prizeIndicator = false;
            return reserveItem(itemId, numberToReserve, username, prizeIndicator);
        }
        public bool reserveItem(int itemId, uint numberToReserve, string username, bool prizeIndicator)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool itemReserved = false;
            int rowsAffected = 0;
            string sql = "";
            if (prizeIndicator)
            {
                sql = "REPLACE INTO reservedamount (knownitemsid, quantity, reservedBy, botid, projectid, prize, expirationdate, claimed) VALUES(?knownitemsid, ?quantity, ?reservedBy, ?botid, ?projectid, ?prizeIndicator, adddate(now(), INTERVAL (SELECT defaultclaimhours FROM botconfig WHERE botid = ?botid) HOUR), 0);";
            }
            else
            {
                sql = "REPLACE INTO reservedamount (knownitemsid, quantity, reservedBy, botid, projectid, prize, claimed) VALUES(?knownitemsid, ?quantity, ?reservedBy, ?botid, ?projectid, ?prizeIndicator, 0);";
            }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            TheLogger.Debug("### SqlConnectionOpen() in LogTrade\n");
            cmd.Parameters.AddWithValue("?knownitemsid", itemId);
            cmd.Parameters.AddWithValue("?quantity", numberToReserve);
            cmd.Parameters.AddWithValue("?reservedBy", username);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            cmd.Parameters.AddWithValue("?projectid", -1);
            cmd.Parameters.AddWithValue("?prizeIndicator", (prizeIndicator ? 1:0));

            try
            {
                // here here
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                if (rowsAffected > 0)
                {
                    itemReserved = true;
                }
                TheLogger.Debug("### SqlConnectionClose() in reserveItem\n");
            }
            MyConnection.Close();
            return itemReserved;
        }
        internal void reservedDetails(int botid, string username)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            //connect to server and open mysql loop
            string sql = "SELECT knownitems.name, quantity, reservedBy, reservedamount.id, prize FROM reservedamount, knownitems WHERE botid = ?botid AND quantity > 0 AND knownitems.id = reservedamount.knownitemsid AND reservedamount.claimed = 0 ORDER BY knownitems.name, reservedBy";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                lock (myLock)
                {
                    //loop through the result set
                    int rowCount = 0;
                    int lineSize = 62;
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "[".PadRight(lineSize, '-')));
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "[Index".PadRight(10, ' ') + "quantity".PadRight(10, ' ') + "itemId".PadRight(20, ' ') + "reservedBy".PadRight(16, ' ') + "prize?"));
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "[".PadRight(lineSize, '-')));
                    while (reader.Read())
                    {
                        rowCount++;
                        //get the data from the row
                        string itemName = reader.GetString(0);
                        Int32 quantity = reader.GetInt32(1);
                        string reservedBy = reader.GetString(2);
                        Int32 rowIndex = reader.GetInt32(3);
                        bool prizeInd = reader.GetBoolean(4);
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "[" + rowIndex.ToString().PadRight(10, ' ') + quantity.ToString().PadRight(10, ' ') + itemName.PadRight(20, ' ') + reservedBy.PadRight(15, ' ') + prizeInd.ToString().PadRight(6, ' ')));
                    }
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "[".PadRight(lineSize, '-')));
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return;
        }
        internal bool reservedDelete(int botid, string username, Int32 rowIndex)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool rowDeleted = false;
            int rowsAffected = 0;
            string sql = "DELETE FROM reservedAmount WHERE id=?rowIndex;";
            TheLogger.Debug(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?rowIndex", rowIndex.ToString());
            try
            {
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
            }
            if (rowsAffected > 0)
            {
                rowDeleted = true;
            }
            MyConnection.Close();
            return rowDeleted;
        }
        public bool isBot(string name)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool nameIsBot = false;
            string botName = "";
            string sql = "SELECT name FROM botsonline";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    botName = reader.GetString(0);
                    if (botName == name)
                    {
                        nameIsBot = true;
                        break;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return nameIsBot;
        }
        public void updateMyLocation()
        {
            string sql = "UPDATE bots SET textlocation = \"" + Settings.LocationInfo + "\"";
            sql += " WHERE botid='" + Settings.botid.ToString() + "' LIMIT 1;";
            TheLogger.Debug(sql + "\n");
            raw_sql(sql);
        }
        public void updateVersion()
        {
            string sql = "UPDATE bots SET codeversion = '" + Settings.Version + "'";
            sql += " WHERE botid='" + Settings.botid.ToString() + "' LIMIT 1;";
            TheLogger.Debug(sql + "\n");
            raw_sql(sql);
        }

        public char getInvFiller()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            char invFiller = '~';
            string sql = "SELECT invfiller FROM botconfig WHERE botid = ?botid;";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    invFiller = reader.GetChar(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                if (oMySQLException.Message == "Index was outside the bounds of the array.")
                {
                    invFiller = ' ';
                }
                else
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                if (oException.Message == "Index was outside the bounds of the array.")
                {
                    invFiller = ' ';
                }
                else
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return invFiller;
        }
        public char getWantedFiller()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            char wantedFiller = '~';
            string sql = "SELECT wantedfiller FROM botconfig WHERE botid = ?botid";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    wantedFiller = reader.GetChar(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                if (oMySQLException.Message == "Index was outside the bounds of the array.")
                {
                    wantedFiller = ' ';
                }
                else
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                if (oException.Message == "Index was outside the bounds of the array.")
                {
                    wantedFiller = ' ';
                }
                else
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return wantedFiller;
        }


        public bool getShowZeroPrice()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            bool showZeroPrice = false;
            string sql = "SELECT showzeroprice FROM botconfig WHERE botid = ?botid";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    showZeroPrice = reader.GetBoolean(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return showZeroPrice;
        }

        public double getAdvertTextRate()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            double myRate = 0;
            string sql = "SELECT textrate FROM advertrate WHERE botid = ?botid;";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    myRate = reader.GetDouble(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                    myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                    myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return myRate;
        }

        public double getSellRate()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            double myRate = 0;
            string sql = "SELECT sellrate FROM advertrate WHERE botid = ?botid;";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    myRate = reader.GetDouble(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return myRate;
        }

        public string getTextAdvert()
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string myAdvert = "";
            string sql = "select adverttext from adverts where botid = ?botid and disabled = 0 order by rand() LIMIT 1;";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    myAdvert = reader.GetString(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                myErrorHandler.errorWriter(oMySQLException);
            }
            catch (Exception oException)
            {
                myErrorHandler.errorWriter(oException);
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return myAdvert;
        }

        public int getItemPos(int itemID)
        {
            int itemPos = -1;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT pos FROM inventory WHERE botid = ?botid AND knownitemsid = ?itemID";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            cmd.Parameters.AddWithValue("?itemID", itemID);

            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    itemPos = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return itemPos;
        }
        public static bool playerIsOnline(string playerName)
        {
            bool playerLoggedIn = false;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT name FROM playersonline WHERE name = ?playerName";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?playerName", playerName);

            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    if (!reader["name"].Equals(System.DBNull.Value))
                    {
                        playerLoggedIn = true;
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return playerLoggedIn;
        }
        public static void setPlayerOnline(string playerName)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "REPLACE INTO playersonline VALUES (?playerName)";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?playerName", playerName);

            //MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            MyConnection.Close();
        }
        public static void setPlayerOffline(string playerName)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "DELETE FROM playersonline WHERE name = ?playerName";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?playerName", playerName);

            //MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            MyConnection.Close();
        }

        internal void setBuddyOnline(string playerName)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "REPLACE INTO botbuddy (botid, buddyname, loggedon, lastloggedon, lastloggedoff) SELECT botid, '" + playerName + "', 1, now(), lastloggedoff FROM botbuddy WHERE botid = ?botid AND buddyname = ?playername AND loggedon = 0";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?playerName", playerName);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);

            //MySqlDataReader reader = cmd.ExecuteReader();
            int rowsAffected = 0;
            try
            {
                rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    setPlayerOnline(playerName);
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            if (rowsAffected == 0)
            {
                sql = "INSERT INTO botbuddy (botid, buddyname, loggedon, lastloggedon, lastloggedoff)" +
                    " VALUES (?botid, ?playerName, 1, now(), now())";
                cmd = new MySqlCommand(sql, MyConnection); 
                cmd.Parameters.AddWithValue("?playerName", playerName);
                cmd.Parameters.AddWithValue("?botid", Settings.botid);
                try
                {
                    rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        setPlayerOnline(playerName);
                    }
                }
                catch (MySqlException oMySQLException)
                {
                    if (!oMySQLException.Message.Contains("Duplicate"))
                    {
                        myErrorHandler.errorWriter(oMySQLException);
                    }
                }
                catch (Exception oException)
                {
                    {
                        myErrorHandler.errorWriter(oException);
                    }
                }
                finally
                {
                }
            }
            MyConnection.Close();
        }
        internal void setBuddyOffline(string playerName)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "REPLACE INTO botbuddy (botid, buddyname, loggedon, lastloggedon, lastloggedoff) SELECT botid, buddyname, 0, lastloggedon, now() FROM botbuddy WHERE botid = ?botid AND buddyname = ?playername AND loggedon = 1";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?playerName", playerName);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);

            //MySqlDataReader reader = cmd.ExecuteReader();
            int rowsAffected = 0;
            try
            {
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            //if (rowsAffected > 0)
            //{
                setPlayerOffline(playerName);
            //}
            MyConnection.Close();
        }

        internal bool IGamble()
        {
            bool gamble = false;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" +MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "SELECT gamble FROM botconfig WHERE botid = ?botid";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    gamble = reader.GetBoolean(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
                reader.Close();
            }
            MyConnection.Close();
            return gamble;
        }

        internal void displayPrizes(string argument, string username)
        {
            string sql = "SELECT price, odds, levelid, description FROM prizelevel, botticket WHERE description LIKE '%" + argument + "%' AND prizelevel.levelid = botticket.prizelevelid AND botticket.botid = " + Settings.botid;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            MySqlDataReader reader = cmd.ExecuteReader();
            int levelid = 0;
            decimal price = (decimal)0.00;
            decimal odds = (decimal)0.00;
            try
            {
                while (reader.Read())
                {
                    price = reader.GetDecimal(0);
                    odds = reader.GetDecimal(1);
                    levelid = reader.GetUInt16(2);
                    string prizeText = reader.GetString(3);
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, prizeText + " : Price per ticket: " + price + "gc :odds of winning: " + odds));
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            reader.Close();
            sql = "select botprize.quantity, knownitems.name, knownitems.id, count(botprize.quantity), sum(inventory.quantity) from knownitems, botprize, inventory where knownitems.id = botprize.knownitemsid and botprize.botid = ?botid and botprize.prizelevel = ?levelid and inventory.knownitemsid = botprize.knownitemsid and inventory.botid = botprize.botid GROUP BY botprize.quantity, knownitems.name, knownitems.id HAVING sum(inventory.quantity) >= botprize.quantity";
            uint quantity = 0;
            string itemName = "";
            uint itemID = 0;
            uint reservedAmt = 0;
            uint invQuantity = 0;
            uint prizeCount = 0;
            cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            cmd.Parameters.AddWithValue("?levelid", levelid);
            reader = cmd.ExecuteReader();
            int counter = 0;
            try
            {
                while (reader.Read())
                {
                    counter++;
                    quantity = reader.GetUInt16(0);
                    itemName = reader.GetString(1);
                    itemID = reader.GetUInt32(2);
                    prizeCount = reader.GetUInt32(3);
                    invQuantity = reader.GetUInt32(4);
                    reservedAmt = ReservedAmount((int)itemID);
                    if ((invQuantity - reservedAmt) >= quantity)
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, quantity + " " + itemName + "(" + prizeCount + "x)"));
                    }
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
           {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
                reader.Close();
            }
            UInt32 jackpotAmount = 0;
            decimal jackpotNumber = getJackpotNumber(argument, out jackpotAmount);
            if (jackpotNumber > 0)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "The jackpot number is: " + jackpotNumber + ": Amount: " + jackpotAmount));
            }
            MyConnection.Close();
            if (counter == 0)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I have no prizes at this level."));
            }
        }

        internal void getGambleInfo(string prizeLevel, out decimal oddsOfWinning, out uint priceOfTicket)
        {
            oddsOfWinning = (decimal)0.00;
            priceOfTicket = 0;
            string sql = "SELECT price, odds FROM prizelevel, botticket WHERE description LIKE '%" + prizeLevel + "%' AND prizelevel.levelid = botticket.prizelevelid AND botticket.botid = " + Settings.botid;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    priceOfTicket = reader.GetUInt16(0);
                    oddsOfWinning = reader.GetDecimal(1);
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            reader.Close();
            MyConnection.Close();
        }

        internal string reservePrize(string prizeLevel, string username)
        {
            string prizeWon = "";
            uint quantityWon = 0;
            uint prizeWonID = 0;
            //figure out the prize and reserve it for the person...
            string sql = "select botprize.quantity, knownitems.name, knownitems.id, inventory.quantity from knownitems, botprize, inventory, botticket, prizelevel where knownitems.id = botprize.knownitemsid and botprize.botid = ?botid and botprize.prizelevel = botticket.prizelevelid and botticket.prizelevelid = prizelevel.levelid and botticket.botid = botprize.botid and lower(prizelevel.description) like '%" + prizeLevel + "%' and inventory.knownitemsid = botprize.knownitemsid and inventory.botid = botprize.botid and inventory.quantity  - (SELECT IFNULL(SUM(quantity),0) FROM reservedamount WHERE botid = inventory.botid AND claimed = 0 AND knownitemsid = inventory.knownitemsid) >= botprize.quantity ORDER BY RAND() LIMIT 1";
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            //cmd.Parameters.AddWithValue("?levelid", prizeLevel);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    quantityWon = reader.GetUInt16(0);
                    prizeWon = reader.GetString(1);
                    prizeWonID = reader.GetUInt16(2);
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            reader.Close();
            MyConnection.Close();
            TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Your prize is: " + quantityWon + " " + prizeWon));
            reserveItem((int)prizeWonID, quantityWon, username, true);
            return prizeWon;
        }

        internal void listBuddies(string username)
        {
            string sql = "SELECT buddyname, date_format(lastloggedon, '%m/%d/%Y %h:%i %p'), date_format(lastloggedoff, '%m/%d/%Y %h:%i %p'), loggedon FROM botbuddy WHERE botid = ?botid";
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username,reader.GetString(0).PadRight(20, ' ') + "|" + reader.GetString(1) + "|" + reader.GetString(2) + "|" + (reader.GetBoolean(3) ? "X": " ")));
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            reader.Close();
            MyConnection.Close();
            TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Bot server time: " + DateTime.Now + ":" + TimeZone.CurrentTimeZone.StandardName));
        }

        internal int putPrizesInWindow(string username)
        {
            int itemCount = 0;
            //put the prizes in the window and click accept...
            string sql = "SELECT inventory.pos, inventory.knownitemsid, sum(reservedamount.quantity), sum(inventory.quantity) FROM reservedamount, inventory WHERE reservedamount.botid = ?botid AND prize = 1 AND reservedBy = ?username AND claimed = 0 AND reservedamount.knownitemsid = inventory.knownitemsid AND reservedamount.botid = inventory.botid GROUP BY inventory.pos";
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            cmd.Parameters.AddWithValue("?username", username);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                int quantityForItem = 0;
                int currentItem = 0;
                while (reader.Read())
                {
                    int pos = reader.GetInt16(0);
                    int itemID = reader.GetInt16(1);
                    uint quantity = reader.GetUInt16(2);
                    uint invQuantity = reader.GetUInt32(3);
                    if (currentItem!=itemID)
                    {
                        currentItem = itemID;
                        quantityForItem = 0;
                        itemCount++;
                    }
                    if (quantity <= invQuantity)
                    {
                        if (quantityForItem < quantity)
                        {
                            TheTCPWrapper.Send(CommandCreator.PUT_OBJECT_ON_TRADE((byte)pos, quantity));
                            quantityForItem += (int)quantity;
                        }
                    }
                    else
                    {
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "For some reason I don't have enough items to cover your reservation, please contact my bot owner!"));
                        itemCount = 0; ;
                    }
                    //TheTCPWrapper.Send(CommandCreator.SEND_PM(username, reader.GetString(0).PadRight(20, ' ') + "|" + reader.GetDateTime(1) + "|" + reader.GetDateTime(2) + "|" + reader.GetBoolean(3)));
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            reader.Close();
            MyConnection.Close();
            if (itemCount == 0)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "You don't have any prizes to claim, sorry!"));
                TheTCPWrapper.Send(CommandCreator.EXIT_TRADE());
            }
            return itemCount;
        }

        internal void updatePrizes(string username)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            string sql = "UPDATE reservedamount SET claimed = 1 WHERE botid = ?botid AND reservedBy = ?username AND prize = 1 AND claimed = 0";
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?username", username);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);

            //MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            MyConnection.Close();
        }

        internal decimal getJackpotNumber(string prizeLevel, out UInt32 jackpotAmount)
        {
            jackpotAmount = 0;
            decimal jackpotNumber = 0;
            string sql = "SELECT jackpotnumber, jackpottotal FROM botticket,prizelevel WHERE botid = ?botid AND prizelevelid = prizelevel.levelid AND prizelevel.description like '%" + prizeLevel + "%'";
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    jackpotNumber = reader.GetDecimal(0);
                    jackpotAmount = reader.GetUInt32(1);
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            reader.Close();
            MyConnection.Close();
            return jackpotNumber;
        }

        internal void updateJackpotAmount(string prizeLevel, bool resetJackpotAmount)
        {
            string sql = "UPDATE botticket SET jackpottotal = jackpottotal + jackpotpart WHERE botid = " + Settings.botid + " AND botticket.prizelevelid = (SELECT levelid FROM prizelevel WHERE description LIKE '%" + prizeLevel + "%')"; ;
            if (resetJackpotAmount)
            {
                sql = "UPDATE botticket SET jackpottotal = jackpotpart WHERE botid = " + Settings.botid + " AND botticket.prizelevelid = (SELECT levelid FROM prizelevel WHERE description LIKE '%" + prizeLevel + "%')"; ;
            }
            raw_sql(sql);
        }

        internal void listPrizesWon(string username)
        {
            string sql = "SELECT quantity, knownitems.name, expirationdate FROM reservedamount, knownitems WHERE botid = ?botid AND claimed = 0 AND reservedBy = ?username AND prize = 1 AND reservedamount.knownitemsid = knownitems.id";
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            cmd.Parameters.AddWithValue("?username", username);
            MySqlDataReader reader = cmd.ExecuteReader();
            int prizeCount = 0;
            try
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Quantity".PadRight(20, ' ') + "|Item".PadRight(16, ' ') + "|Expiration date"));
                while (reader.Read())
                {
                    prizeCount++;
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, reader.GetUInt16(0).ToString().PadRight(20, ' ') + "|" + reader.GetString(1).PadRight(15, ' ') + "|" + reader.GetString(2)));
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            if (prizeCount == 0)
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "No prizes to be claimed..."));
            }
            reader.Close();
            MyConnection.Close();
        }

        internal bool isPlayerOnline(string username, string playerName, string serverName)
        {
            bool playerIsOnline = false;
            string sql = "SELECT name FROM playersonline WHERE name = ?playerName";
            //string sql = "select name from live.playersonline where name = 'lamorian'";
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + serverName + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?playerName", playerName);
            cmd.Parameters.AddWithValue("?serverName", serverName);
            Console.WriteLine(username + " " + serverName + " " + playerName);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    playerIsOnline = true;
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            reader.Close();
            MyConnection.Close();
            return playerIsOnline;
        }

        internal int getBotType(int p)
        {
            int botType = 0;
            string sql = "SELECT type FROM bots WHERE botid = ?botid";
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    botType = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            reader.Close();
            MyConnection.Close();
            return botType;
        }

        internal bool checkForPrizes(string prizeLevel)
        {
            int prizeCount = 0;
            string sql = "select botprize.quantity, knownitems.name, knownitems.id, inventory.quantity from knownitems, botprize, inventory, botticket, prizelevel where knownitems.id = botprize.knownitemsid and botprize.botid = ?botid and botprize.prizelevel = botticket.prizelevelid and botticket.prizelevelid = prizelevel.levelid and botticket.botid = botprize.botid and lower(prizelevel.description) like '%" + prizeLevel + "%' and inventory.knownitemsid = botprize.knownitemsid and inventory.botid = botprize.botid and inventory.quantity >= botprize.quantity ORDER BY RAND() LIMIT 1";
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    prizeCount = reader.GetInt16(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            reader.Close();
            MyConnection.Close();
            return (prizeCount > 0);
        }

        internal void writeSigils(string sigilsWeHave)
        {
            string sql = "REPLACE INTO botsigils (botid, sigilid) VALUES " + sigilsWeHave.Replace(")(", "),(");
            raw_sql(sql);
            //Console.WriteLine(sql);
        }

        public string getURL()
        {
            string URL = "";
            string sql = "SELECT weburl FROM globalsettings WHERE serverport = " + Settings.ServerPort;
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?botid", Settings.botid);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    URL = reader.GetString(0);
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            reader.Close();
            MyConnection.Close();
            return URL;
        }

        internal void updateCoins(uint priceOfTicket)
        {
            string sql = "UPDATE inventory SET quantity = quantity + " + priceOfTicket + " WHERE botid = " + Settings.botid + " AND knownitemsid = 1";
            raw_sql(sql);
        }

        public void getItemInfo(ref TradeHandler.TradeItem MyTradeItem)
        {
            string sql = "SELECT id, name, weight FROM knownitems WHERE elitemid = ?elitemid";
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
	    try
	    {
            	MyConnection.Open();
	    }
	    catch (Exception myException)
	    {
		Console.WriteLine(myException.Message); Environment.Exit(0);
	    }
            MySqlCommand cmd = new MySqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("?elitemid", MyTradeItem.ELServerItemID);
            MySqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    MyTradeItem.KnownItemsSqlID = reader.GetInt16(0);
                    MyTradeItem.name = reader.GetString(1);
                    MyTradeItem.weight = reader.GetUInt16(2);
                }
            }
            catch (MySqlException oMySQLException)
            {
                {
                    myErrorHandler.errorWriter(oMySQLException);
                }
            }
            catch (Exception oException)
            {
                {
                    myErrorHandler.errorWriter(oException);
                }
            }
            finally
            {
            }
            reader.Close();
            MyConnection.Close();
        }
    }
}
