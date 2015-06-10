using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Configuration;
//using MySql.Data.MySqlClient; 

namespace RS500gaWCFService.db
{
    public class DBManager : IDisposable
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        private bool disposed = false;


        public MySqlConnection Connection {
            get
            {
                return this.connection;
            }
        }
        public DBManager()
        {
            initializeDefault();
        }

        private void initializeDefault()
        {
            server = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_CS_SERVER]; // "localhost";
            database = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_CS_DATABASE];  //"0connectcsharptomysql";
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
    }
}