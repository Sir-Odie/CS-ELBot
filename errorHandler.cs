using System;
using MySql.Data.MySqlClient;

namespace cs_elbot
{
    public class errorHandler
    {
        private Logger TheLogger = new Logger();
        public errorHandler()
        {
        }

        public void errorWriter(Exception oException)
        {
            TheLogger.Log("ExecSql Error in {0} due to : " + oException.Message + "\n");
            string errorText = oException.ToString();
            errorWriter(errorText);
        }

        public void errorWriter(MySqlException oMySQLException)
        {
            TheLogger.Log("ExecSql Error in {0} due to : " + oMySQLException.Message + "\n");
            string errorText = oMySQLException.ToString();
            errorWriter(errorText);
        }
        
        private void errorWriter(string errorText)
        {
            writeErrorMessage(errorText);
            Console.Beep(4400, 50);
        }
        public void writeErrorMessage(string errorText)
        {
            MySqlConnection MyConnection = new MySqlConnection("Server=" + MainClass.SqlServer + ";Port=" + MainClass.SqlPort.ToString() + ";Database=" + MainClass.SqlDatabase + ";Uid=" + MainClass.SqlUsername + ";Pwd=" + MainClass.SqlPassword + ";");
            MyConnection.Open();
            string insertCommand = "INSERT INTO botError (botid, errortext, errordate) VALUES (" + Settings.botid + ",\"" + errorText + "\",SYSDATE())";
            MySqlCommand cmd = new MySqlCommand(insertCommand, MyConnection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException oMySQLException)
            {
                TheLogger.Log("ExecSql Error in {0} due to : " + oMySQLException.Message + "\n");
                TheLogger.Log("ExecSql by SQL : " + insertCommand + "\n");
            }
            catch (Exception oException)
            {
                TheLogger.Log("ExecSql Error in {0} due to : " + oException.Message + "\n");
                TheLogger.Log("ExecSql by SQL : " + insertCommand + "\n");
            }
            finally
            {
                Console.Beep(4400, 50);
            }
            MyConnection.Close();
        }

    }
}