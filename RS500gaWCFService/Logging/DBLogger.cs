using log4net;
using MySql.Data.MySqlClient;
using RS500gaWCFService.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace RS500gaWCFService.Logging
{
    public class DBLogger : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static int logsCount = 0;
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        private const int LOG_DEBUG = 1;
        private const int LOG_INFO = 2;
        private const int LOG_WARN = 3;
        private const int LOG_ERROR = 4;
        private const int LOG_FATAL = 5;

        private bool disposed = false;

        public MySqlConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        public DBLogger()
        {
            initializeDefault();
        }

        private void initializeDefault()
        {
            server = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_CS_SERVER]; // "localhost";
            database = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LOG_DATABASE];  //"0connectcsharptomysql";
            uid = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_CS_UID]; //"username";
            password = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_CS_PASSWORD]; //"password";

            string connectionString;
            connectionString = string.Format(Constants.CONNECTION_STRING_FORMATTED, server, database, uid, password);

            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        public bool OpenConnection()
        {
            try
            {
                logsCount++;
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        //MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        //MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }


        }

        //Close connection
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                //MessageBox.Show(ex.Message);
                log.Error(Constants.ERROR_LOADING, ex);
                return false;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (!disposed)
            {
                if (dispose)
                {
                    if (connection != null)
                    {
                        CloseConnection();
                        connection.Dispose();
                        connection = null;
                        disposed = true;
                    }
                }
            }
        }

        public static void logError(string text,
                        [CallerFilePath] string file = "",
                        [CallerMemberName] string funname = "",
                        [CallerMemberName] string member = "",
                        [CallerLineNumber] int line = 0)
        {
            logToDb(LOG_ERROR, text, file, funname, member, line);
        }

        public static void logDebug(string text,
                        [CallerFilePath] string file = "",
                        [CallerMemberName] string funname = "",
                        [CallerMemberName] string member = "",
                        [CallerLineNumber] int line = 0)
        {
            logToDb(LOG_DEBUG, text, file, funname, member, line);
        }

        public static void logWarn(string text,
                        [CallerFilePath] string file = "",
                        [CallerMemberName] string funname = "",
                        [CallerMemberName] string member = "",
                        [CallerLineNumber] int line = 0)
        {
            logToDb(LOG_WARN, text, file, funname, member, line);
        }

        public static void logFatal(string text,
                        [CallerFilePath] string file = "",
                        [CallerMemberName] string funname = "",
                        [CallerMemberName] string member = "",
                        [CallerLineNumber] int line = 0)
        {
            logToDb(LOG_FATAL, text, file, funname, member, line);
        }

        public static void logInfo(string text,
                        [CallerFilePath] string file = "",
                        [CallerMemberName] string funname = "",
                        [CallerMemberName] string member = "",
                        [CallerLineNumber] int line = 0)
        {
            logToDb(LOG_INFO, text, file, funname, member, line);
        }

        private static void logToDb(int level, string text,  string file, string funname, string member, int line)
        {
            DBLogger dbLog = new DBLogger();
            int result = -1;
            string vresult = string.Empty;

            if (dbLog.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_LOG_APP_ERROR_PROC;

                MySqlCommand cmd = new MySqlCommand(rtn, dbLog.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(Constants.SP_PARAM_LOG_ERR_LOG_LEVEL, level);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_LOG_ERR_FILENAME, Utils.getStringWithinLen(Path.GetFileName(file), Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_LOG_ERR_ERR_DESC, Utils.getStringWithinLen(logsCount.ToString() + ":" + text, Constants.MYSQL_TEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_LOG_ERR_ERR_FUN_NAME, Utils.getStringWithinLen(funname, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_LOG_ERR_ERR_ROW_NR, line);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                if (result != Constants.SP_RESULT_OK)
                {
                    log.Warn(vresult);
                    //DBLogger.logWarn(vresult);
                }
                cmd.Dispose();
            }
            dbLog.CloseConnection();
        }
    }
}