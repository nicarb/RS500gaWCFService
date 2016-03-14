using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Configuration;
using log4net;
using RS500gaWCFService.data;
using System.Data;
using System.IO;
using RS500gaWCFService.utils;
using RS500gaWCFService.Logging;
using RS500gaWCFService.rs500ga;

//using MySql.Data.MySqlClient; 

namespace RS500gaWCFService.db
{
    public class DBManager : IDisposable
    {
        private static int connectionCount = 0;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        private bool disposed = false;


        public MySqlConnection Connection
        {
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
                connectionCount++;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="albums"></param>
        /// <returns></returns>
        public static string getAlbumsIdsFromDb(List<LfAlbum> albums)
        {
            //Dictionary<int, double> albumsIDs = new Dictionary<int, double>();
            string retval = string.Empty;
            string funName = "getAlbumsIdsFromDb(List<LfAlbum> albums) - ";

            foreach (LfAlbum album in albums)
            {
                retval = getAlbumsIdFromDb(album);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    retval = funName + retval;
                    DBLogger.logWarn(retval);
                    return retval;
                }

                retval = Constants.RETVAL_MESSAGE_DONE;
                //cmd.Parameters.AddWithValue()
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static string getTagIdsFromDb(List<LfTag> tags)
        {
            //Dictionary<int, double> albumsIDs = new Dictionary<int, double>();
            string retval = string.Empty;
            string funName = "getTagIdsFromDb(List<LfTag> tags) - ";

            int tmpIdTag = -1;
            int result = -1;
            string vresult = string.Empty;

            DBManager dbMng = new DBManager();

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_GET_OR_ADD_TAG;
                string tagname = string.Empty;
                string tagurl = string.Empty;

                foreach (LfTag tag in tags)
                {
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_NAME, Utils.getStringWithinLen(tag.name, Constants.MYSQL_TINYTEXT_LEN));
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_URL, Utils.getStringWithinLen(tag.url, Constants.MYSQL_TINYTEXT_LEN));
                    cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_ID_TAG, tmpIdTag);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                    cmd.Parameters[Constants.SP_GET_ALBUM_LF_PARAM_ID_TAG].Direction = ParameterDirection.Output;
                    cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                    cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                    vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                    if (result == Constants.SP_RESULT_OK)
                    {
                        tag.id = Convert.ToInt32(cmd.Parameters[Constants.SP_GET_ALBUM_LF_PARAM_ID_TAG].Value.ToString());
                    }
                    else
                    {
                        DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, tag.ToString()));
                    }

                    cmd.Dispose();
                }
                dbMng.CloseConnection();
                retval = Constants.RETVAL_MESSAGE_DONE;
                //cmd.Parameters.AddWithValue()
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
            }
            return retval;
        }


        public static Dictionary<int, double> getArtistsIdsFromDb(DataTable dt)
        {
            Dictionary<int, double> artistIDs = new Dictionary<int, double>();

            int tmpIdArtist = -1;
            int result = -1;
            string vresult = string.Empty;

            DBManager dbMng = new DBManager();

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_GET_OR_ADD_ARTIST_LF;
                int artistID = 0;
                foreach (DataRow row in dt.Rows)
                {
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // albumTracks.Columns.Add(Constants.XSD_TAG_LF_NAME);
                    cmd.Parameters.AddWithValue(Constants.SP_GET_ARTIST_PARAM_NAME, row[Constants.XSD_TAG_LF_NAME]);
                    cmd.Parameters.AddWithValue(Constants.SP_GET_ARTIST_PARAM_DESC, null);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MBID, row[Constants.XSD_TAG_LF_MBID]);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_URL, row[Constants.XSD_TAG_LF_URL]);
                    cmd.Parameters.AddWithValue(Constants.SP_GET_ARTIST_PARAM_ID_ARTIST, tmpIdArtist);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                    cmd.Parameters[Constants.SP_GET_ARTIST_PARAM_ID_ARTIST].Direction = ParameterDirection.Output;
                    cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                    cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                    vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                    if (result == Constants.SP_RESULT_OK)
                    {
                        artistID = Convert.ToInt32(cmd.Parameters[Constants.SP_GET_ARTIST_PARAM_ID_ARTIST].Value.ToString());
                        row[Constants.XSD_TAG_LF_ID] = artistID;
                        artistIDs.Add(artistID, Convert.ToDouble(row[Constants.XSD_TAG_LF_MATCH]));
                    }

                    cmd.Dispose();
                }
                dbMng.CloseConnection();

                //cmd.Parameters.AddWithValue()
            }

            return artistIDs;
        }

        public static string getArtistIdFromDb(LfArtist artist)
        {
            string funName = "getArtistIdFromDb(LfArtist artist) - ";
            int tmpIdArtist = -1;
            int result = -1;
            string vresult = string.Empty;

            DBManager dbMng = new DBManager();

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_GET_OR_ADD_ARTIST_LF;

                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                // albumTracks.Columns.Add(Constants.XSD_TAG_LF_NAME);
                cmd.Parameters.AddWithValue(Constants.SP_GET_ARTIST_PARAM_NAME, Utils.getStringWithinLen(artist.name, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_GET_ARTIST_PARAM_DESC, null);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_MBID, artist.mbid);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_URL, Utils.getStringWithinLen(artist.url, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_GET_ARTIST_PARAM_ID_ARTIST, artist.id);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                cmd.Parameters[Constants.SP_GET_ARTIST_PARAM_ID_ARTIST].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                if (result == Constants.SP_RESULT_OK)
                {
                    tmpIdArtist = Convert.ToInt32(cmd.Parameters[Constants.SP_GET_ARTIST_PARAM_ID_ARTIST].Value.ToString());
                    artist.id = tmpIdArtist;
                }
                else
                {
                    DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, artist.ToString()));
                    return funName + vresult;
                }

                cmd.Dispose();
                dbMng.CloseConnection();
            }
            else
            {
                log.Error(Constants.ERROR_ON_CONNECTION);
                DBLogger.logWarn(Constants.ERROR_ON_CONNECTION);
                return funName + Constants.ERROR_ON_CONNECTION;
            }

            return Constants.RETVAL_MESSAGE_DONE;
        }

        public static string getTrackIdFromDb(LfTrack track)
        {
            string retval = string.Empty;
            string funName = "getTrackIdFromDb(LfTrack track)";

            int tmpIdTrack = -1;
            int idArtist = -1;
            int idAlbum = -1;
            int result = -1;
            string vresult = string.Empty;

            DBManager dbMng = new DBManager();

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_GET_OR_ADD_TRACK_LF;

                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                if (track.artist != null)
                {
                    track.artistName = track.artist.name;
                    if (track.artist.id != 0)
                        idArtist = track.artist.id;
                    else if (track.artist.id == 0)
                    {
                        retval = DBManager.getArtistIdFromDb(track.artist);
                        if (retval != Constants.RETVAL_MESSAGE_DONE)
                        {
                            DBLogger.logError(funName + retval);
                            return funName + retval;
                        }
                        else
                            idArtist = track.artist.id;
                    }
                }
                else if (!string.IsNullOrEmpty(track.artistName))
                {
                    track.artist = new LfArtist();
                    track.artist.name = track.artistName;
                    retval = DBManager.getArtistIdFromDb(track.artist);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logError(funName + retval);
                        return funName + retval;
                    }
                    else
                    {
                        idArtist = track.artist.id;
                    }
                }

                if (track.album != null)
                {
                    if (string.IsNullOrEmpty(track.albumName))
                        track.albumName = track.album.name;

                    if (track.album.dbId != 0)
                        idAlbum = track.album.dbId;
                    else if (track.album.dbId == 0)
                    {
                        retval = DBManager.getAlbumsIdFromDb(track.album);
                        if (retval != Constants.RETVAL_MESSAGE_DONE)
                        {
                            DBLogger.logWarn(funName + retval);
                            //return funName + retval;
                        }
                        else
                        {
                            idAlbum = track.album.dbId;
                        }
                    }
                }
                else
                {
                    //int i = 0;
                    //retval = LastfmArtistMethods.
                    //DBLogger.logWarn(Constants.ERROR_NULL_POINTER);
                }


                // albumTracks.Columns.Add(Constants.XSD_TAG_LF_NAME);
                cmd.Parameters.AddWithValue(Constants.SP_GET_TRACK_LF_PARAM_TITLE, Utils.getStringWithinLen(track.name, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_GET_TRACK_LF_PARAM_DESC, Utils.getStringWithinLen(string.Format("{0} - {1}", track.artistName, track.albumName), Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_GET_TRACK_LF_PARAM_NOTE, Utils.getStringWithinLen(track.wiki_content, Constants.MYSQL_TEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_GET_TRACK_LF_PARAM_ID_ARTIST, idArtist);
                cmd.Parameters.AddWithValue(Constants.SP_GET_TRACK_LF_PARAM_ID_ALBUM, idAlbum);
                cmd.Parameters.AddWithValue(Constants.SP_GET_TRACK_LF_PARAM_RANK, track.rank);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_MBID, track.mbid);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_URL, Utils.getStringWithinLen(track.url, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_GET_TRACK_LF_PARAM_PLAYCOUNT, track.playcount);

                cmd.Parameters.AddWithValue(Constants.SP_GET_TRACK_LF_PARAM_ID_TRACK, tmpIdTrack);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                cmd.Parameters[Constants.SP_GET_TRACK_LF_PARAM_ID_TRACK].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                if (result == Constants.SP_RESULT_OK)
                {
                    tmpIdTrack = Convert.ToInt32(cmd.Parameters[Constants.SP_GET_TRACK_LF_PARAM_ID_TRACK].Value.ToString());
                    track.id = tmpIdTrack;
                }
                else
                {
                    DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, track.ToString()));
                }

                cmd.Dispose();
                dbMng.CloseConnection();
                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            else
            {
                log.Error(Constants.ERROR_ON_CONNECTION);
                retval = funName + Constants.ERROR_ON_CONNECTION;
            }

            return retval;
        }

        public static string getAlbumsIdsFromDb(DataTable dt)
        {
            //Dictionary<int, double> albumsIDs = new Dictionary<int, double>();
            string retval = string.Empty;
            string funName = "getAlbumsIdsFromDb(DataTable dt) - ";

            int tmpIdAlbum = -1;
            int result = -1;
            string vresult = string.Empty;

            DBManager dbMng = new DBManager();

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_GET_OR_ADD_ALBUM_LF;
                int artistID = 0;
                foreach (DataRow row in dt.Rows)
                {
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // albumTracks.Columns.Add(Constants.XSD_TAG_LF_NAME);
                    cmd.Parameters.AddWithValue(Constants.SP_GET_ARTIST_PARAM_NAME, row[Constants.XSD_TAG_LF_NAME]);
                    cmd.Parameters.AddWithValue(Constants.SP_GET_ARTIST_PARAM_DESC, null);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MBID, row[Constants.XSD_TAG_LF_MBID]);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_URL, row[Constants.XSD_TAG_LF_URL]);
                    cmd.Parameters.AddWithValue(Constants.SP_GET_ARTIST_PARAM_ID_ARTIST, tmpIdAlbum);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                    cmd.Parameters[Constants.SP_GET_ARTIST_PARAM_ID_ARTIST].Direction = ParameterDirection.Output;
                    cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                    cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                    vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                    if (result == Constants.SP_RESULT_OK)
                    {
                        artistID = Convert.ToInt32(cmd.Parameters[Constants.SP_GET_ARTIST_PARAM_ID_ARTIST].Value.ToString());
                        row[Constants.XSD_TAG_LF_ID] = artistID;

                        //albumsIDs.Add(artistID, Convert.ToDouble(tAlbum[Constants.XSD_TAG_LF_MATCH]));
                    }
                    else
                    {
                        DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, artistID.ToString()));
                    }

                    cmd.Dispose();
                    dbMng.CloseConnection();
                }
                retval = Constants.RETVAL_MESSAGE_DONE;
                //cmd.Parameters.AddWithValue()
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
                DBLogger.logWarn(funName + Constants.ERROR_ON_CONNECTION);
            }

            return retval;
        }

        public static string updateArtistsMatchDB(LfArtist artist)
        {
            string retval = string.Empty;
            string funName = "updateArtistsMatchDB(LfArtist artist) - ";
            DBManager dbMng = new DBManager();
            int result = -1;
            string vresult = string.Empty;

            //retval = getArtistsIdsFromDb()

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_ADD_OR_UPDATE_MATCH_ARTIST_SIMILARS_LF;

                foreach (var elem in artist.similarArtists)
                {
                    if (elem.id == 0)
                    {
                        retval = getArtistIdFromDb(elem);
                        if (retval != Constants.RETVAL_MESSAGE_DONE)
                        {
                            DBLogger.logError(funName + retval);
                            continue;
                        }
                    }
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_IDARTIST1, artist.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_IDARTIST2, elem.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_MATCH, elem.parent_match);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_ASDESC, Utils.getStringWithinLen(string.Concat(artist.name, "-", elem.name), Constants.MYSQL_TINYTEXT_LEN));
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
                        DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, string.Format(artist.ToString(), elem.ToString())));
                    }
                    cmd.Dispose();
                }
                dbMng.CloseConnection();
                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
                DBLogger.logWarn(retval);
            }

            return retval;
        }

        /*
        public static string searchForCandidateArtists(Playlist playlist)
        {
            string retval = string.Empty;
            string funName = "string searchForCandidateArtists(Playlist playlist) - ";
            int result = -1;

            DBManager dbMng = new DBManager();
            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_ADD_NEW_PLAYLIST;
                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.ExecuteNonQuery();

                result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());

                if (result == Constants.SP_RESULT_OK)
                {


                }
                else
                {
                    string retv = funName + retval;
                    DBLogger.logError(retv);
                }
            }

            return retval;
        }
         */

        public static string createPlaylistAndAddSourceTracks(Playlist playlist)
        {
            string retval = string.Empty;
            string funName = "string createPlaylistAndAddSourceTracks(Playlist playlist) - ";
            string plType = string.Empty;
                plType = Constants.PLAYLIST_TYPE_RS500GA;

            string plDesc =  Constants.PLAYLIST_RS500GA_DESC;
            string plTypeDesc = Constants.PLAYLIST_TYPE_RS500GA_DESC;
            int playlist_id = -1;
            int result = -1;
            string vresult = string.Empty;

            //string outputfilePath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTPATH].ToString();
            //string outputfileName = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTFILENAME].ToString();
            //string outputfileExtension = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTEXTENSION].ToString();

            DBManager dbMng = new DBManager();
            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_ADD_NEW_PLAYLIST;
                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TITLE, Utils.getStringWithinLen(playlist.title, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_DESC, Utils.getStringWithinLen(plDesc, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TYPE, Utils.getStringWithinLen(plType, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TYPE_DESC, Utils.getStringWithinLen(plTypeDesc, Constants.MYSQL_TINYTEXT_LEN));

                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_ID, playlist_id);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                cmd.Parameters[Constants.SP_PARAM_PLAYLIST_ID].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                List<string> rejectedTracksList = new List<string>();

                if (result == Constants.SP_RESULT_OK)
                {
                    playlist_id = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_PLAYLIST_ID].Value.ToString());
                    playlist.db_id = playlist_id;
                    retval = addTracksToPlaylistRs(playlist);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        string retv = funName + retval;
                        DBLogger.logError(retv);
                    }
                }
                else
                {
                    string retv = funName + retval;
                    DBLogger.logError(retv);
                }
            }

            return retval;
        }


        public static string searchForCandidateArtists(Playlist playlist)
        {
            string retval = string.Empty;
            string funName = "searchForCandidateArtists(Playlist playlist) - ";

            //if (playlist.lf_id)

            return retval;
        }

        public static string addTracksToPlaylistRs(Playlist playlist)
        {
            string retval = string.Empty;
            string funName = "addTracksToPlaylistRs(Playlist playlist) - ";
            int result = -1;
            string vresult = string.Empty;

            if (string.IsNullOrEmpty(playlist.title))
            {
                string retv = funName + retval;
                DBLogger.logError(retv);
                return retv;
            }

            foreach (LfTrack track in playlist.source_tracks)
            {
                
                if (track.id == 0)
                   retval = DBManager.getTrackIdFromDb(track);
                if (retval == Constants.RETVAL_MESSAGE_DONE) {
                    DBManager dbMng = new DBManager();
                    if (dbMng.OpenConnection())
                    {
                        string rtn = Constants.STORED_PROC_ADD_TRACK_TO_PLAYLIST_RS;
                        MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_ID, playlist.db_id);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TRACK_ID, track.id);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                        cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                        cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();

                        result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                        vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                        if (result != Constants.SP_RESULT_OK)
                        {
                            log.Error(funName + vresult);
                        }
                    }
                }
            }

            return retval;
        }


        public static string sendPlaylstToDb(Dictionary<int, Track> tracksDic, string playlistTitle)
        {
            string retval = string.Empty;
            string funName = "string sendDataToDb(Dictionary<int, Track> tracksDic, string playlistTitle) - ";
            string plType = Constants.PLAYLIST_TYPE_MANUAL;
            string plDesc = Constants.PLAYLIST_MANUAL_DESC;
            string plTypeDesc = Constants.PLAYLIST_TYPE_MANUAL_DESC;
            int playlist_id = -1;
            int result = -1;
            string vresult = string.Empty;

            string outputfilePath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTPATH].ToString();
            string outputfileName = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTFILENAME].ToString();
            string outputfileExtension = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTEXTENSION].ToString();

            DBManager dbMng = new DBManager();
            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_ADD_NEW_PLAYLIST;
                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TITLE, Utils.getStringWithinLen(playlistTitle, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_DESC, Utils.getStringWithinLen(plDesc, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TYPE, Utils.getStringWithinLen(plType, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TYPE_DESC, Utils.getStringWithinLen(plTypeDesc, Constants.MYSQL_TINYTEXT_LEN));

                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_ID, playlist_id);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                cmd.Parameters[Constants.SP_PARAM_PLAYLIST_ID].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                playlist_id = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_PLAYLIST_ID].Value.ToString());
                List<string> rejectedTracksList = new List<string>();
                if (result == Constants.SP_RESULT_OK)
                {
                    foreach (Track track in tracksDic.Values)
                    {
                        rtn = Constants.STORED_PROC_ADD_TRACK_TO_PLAYLIST;
                        cmd = new MySqlCommand(rtn, dbMng.Connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_ID, playlist_id);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TRACK_ID, track.TRACK_ID);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_NAME, Utils.getStringWithinLen(track.NAME, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ARTIST, Utils.getStringWithinLen(track.ARTIST, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM_GA_RANK, track.ALBUM_GA_RANK);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM_ARTIST, Utils.getStringWithinLen(track.ALBUM_ARTIST, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM, Utils.getStringWithinLen(track.ALBUM, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_GENRE, Utils.getStringWithinLen(track.GENRE, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_KIND, Utils.getStringWithinLen(track.KIND, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_SIZE, track.SIZE);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TOTAL_TIME, track.TOTAL_TIME);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TRACK_NUMBER, track.TRACK_NUMBER);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_YEAR, track.YEAR);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DATE_MODIFIED, track.DATE_MODIFIED);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DATE_ADDED, track.DATE_ADDED);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_BIT_RATE, track.BIT_RATE);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DISC_NUMBER, track.DISC_NUMBER);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DISC_COUNT, track.DISC_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TRACK_COUNT, track.TRACK_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_LOCATION, Utils.getStringWithinLen(track.LOCATION, Constants.MYSQL_SHORTTEXT_2048));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_CLEAN_LOC, Utils.getStringWithinLen(track.CLEAN_LOC, Constants.MYSQL_SHORTTEXT_2048));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_FILE_FOLDER_COUNT, track.FILE_FOLDER_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_LIBRARY_FOLDER_COUNT, track.LIBRARY_FOLDER_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                        cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                        cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();

                        result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                        vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                        if (result != Constants.SP_RESULT_OK)
                        {
                            rejectedTracksList.Add(track.ToString());
                            DBLogger.logWarn(funName + track.ToString());
                        }

                        cmd.Dispose();
                    }

                    dbMng.CloseConnection();

                    if (rejectedTracksList.Count > 0)
                    {
                        string timeStamp = "_" + DateTime.Now.ToString(Constants.FULLTIME_FORMAT_STRING);
                        string filename = outputfileName + timeStamp + outputfileExtension;

                        string outputfile = Path.Combine(outputfilePath, filename);
                        StreamWriter file = new System.IO.StreamWriter(outputfile);

                        foreach (string trackFullStr in rejectedTracksList)
                        {
                            file.WriteLine(trackFullStr);
                            DBLogger.logWarn(trackFullStr);
                        }

                        file.Close();
                    }
                }
                else
                {
                    log.Error(vresult);
                    DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, playlistTitle));
                }
                dbMng.CloseConnection();
            }
            else
            {
                retval = Constants.ERROR_ON_CONNECTION;
                log.Error(Constants.ERROR_ON_CONNECTION);
                DBLogger.logWarn(retval);
            }
            return retval;
        }


        public static string sendLibraryToDb(Dictionary<int, Track> tracksDic, string libraryTitle)
        {
            string retval = string.Empty;
            string funName = "string sendDataToDb(Dictionary<int, Track> tracksDic, string playlistTitle) - ";
            string plType = Constants.PLAYLIST_TYPE_MANUAL;
            string plDesc = Constants.PLAYLIST_MANUAL_DESC;
            string plTypeDesc = Constants.PLAYLIST_TYPE_MANUAL_DESC;
            int playlist_id = -1;
            int result = -1;
            string vresult = string.Empty;

            string outputfilePath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTPATH].ToString();
            string outputfileName = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTFILENAME].ToString();
            string outputfileExtension = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTEXTENSION].ToString();

            DBManager dbMng = new DBManager();
            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_ADD_NEW_PLAYLIST;
                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TITLE, Utils.getStringWithinLen(libraryTitle, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_DESC, Utils.getStringWithinLen(plDesc, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TYPE, Utils.getStringWithinLen(plType, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TYPE_DESC, Utils.getStringWithinLen(plTypeDesc, Constants.MYSQL_TINYTEXT_LEN));

                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_ID, playlist_id);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                cmd.Parameters[Constants.SP_PARAM_PLAYLIST_ID].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                playlist_id = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_PLAYLIST_ID].Value.ToString());
                List<string> rejectedTracksList = new List<string>();
                if (result == Constants.SP_RESULT_OK)
                {
                    foreach (Track track in tracksDic.Values)
                    {
                        rtn = Constants.STORED_PROC_ADD_TRACK_TO_PLAYLIST;
                        cmd = new MySqlCommand(rtn, dbMng.Connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_ID, playlist_id);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TRACK_ID, track.TRACK_ID);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_NAME, Utils.getStringWithinLen(track.NAME, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ARTIST, Utils.getStringWithinLen(track.ARTIST, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM_GA_RANK, track.ALBUM_GA_RANK);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM_ARTIST, Utils.getStringWithinLen(track.ALBUM_ARTIST, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM, Utils.getStringWithinLen(track.ALBUM, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_GENRE, Utils.getStringWithinLen(track.GENRE, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_KIND, Utils.getStringWithinLen(track.KIND, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_SIZE, track.SIZE);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TOTAL_TIME, track.TOTAL_TIME);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TRACK_NUMBER, track.TRACK_NUMBER);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_YEAR, track.YEAR);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DATE_MODIFIED, track.DATE_MODIFIED);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DATE_ADDED, track.DATE_ADDED);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_BIT_RATE, track.BIT_RATE);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DISC_NUMBER, track.DISC_NUMBER);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DISC_COUNT, track.DISC_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TRACK_COUNT, track.TRACK_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_LOCATION, Utils.getStringWithinLen(track.LOCATION, Constants.MYSQL_SHORTTEXT_2048));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_CLEAN_LOC, Utils.getStringWithinLen(track.CLEAN_LOC, Constants.MYSQL_SHORTTEXT_2048));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_FILE_FOLDER_COUNT, track.FILE_FOLDER_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_LIBRARY_FOLDER_COUNT, track.LIBRARY_FOLDER_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                        cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                        cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();

                        result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                        vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                        if (result != Constants.SP_RESULT_OK)
                        {
                            rejectedTracksList.Add(track.ToString());
                            DBLogger.logWarn(funName + track.ToString());
                        }

                        cmd.Dispose();
                    }

                    dbMng.CloseConnection();

                    if (rejectedTracksList.Count > 0)
                    {
                        string timeStamp = "_" + DateTime.Now.ToString(Constants.FULLTIME_FORMAT_STRING);
                        string filename = outputfileName + timeStamp + outputfileExtension;

                        string outputfile = Path.Combine(outputfilePath, filename);
                        StreamWriter file = new System.IO.StreamWriter(outputfile);

                        foreach (string trackFullStr in rejectedTracksList)
                        {
                            file.WriteLine(trackFullStr);
                            DBLogger.logWarn(trackFullStr);
                        }

                        file.Close();
                    }
                }
                else
                {
                    log.Error(vresult);
                    DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, libraryTitle));
                }
                dbMng.CloseConnection();
            }
            else
            {
                retval = Constants.ERROR_ON_CONNECTION;
                log.Error(Constants.ERROR_ON_CONNECTION);
                DBLogger.logWarn(retval);
            }
            return retval;
        }


        public static string sendDataToDb(Dictionary<int, Track> tracksDic, string playlistTitle)
        {
            string retval = string.Empty;
            string funName = "string sendDataToDb(Dictionary<int, Track> tracksDic, string playlistTitle) - ";
            string plType = Constants.PLAYLIST_TYPE_MANUAL;
            string plDesc = Constants.PLAYLIST_MANUAL_DESC;
            string plTypeDesc = Constants.PLAYLIST_TYPE_MANUAL_DESC;
            int playlist_id = -1;
            int result = -1;
            string vresult = string.Empty;

            string outputfilePath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTPATH].ToString();
            string outputfileName = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTFILENAME].ToString();
            string outputfileExtension = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTEXTENSION].ToString();

            DBManager dbMng = new DBManager();
            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_ADD_NEW_PLAYLIST;
                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TITLE, Utils.getStringWithinLen(playlistTitle, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_DESC, Utils.getStringWithinLen(plDesc, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TYPE, Utils.getStringWithinLen(plType, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TYPE_DESC, Utils.getStringWithinLen(plTypeDesc, Constants.MYSQL_TINYTEXT_LEN));

                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_ID, playlist_id);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                cmd.Parameters[Constants.SP_PARAM_PLAYLIST_ID].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                playlist_id = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_PLAYLIST_ID].Value.ToString());
                List<string> rejectedTracksList = new List<string>();
                if (result == Constants.SP_RESULT_OK)
                {
                    foreach (Track track in tracksDic.Values)
                    {
                        rtn = Constants.STORED_PROC_ADD_TRACK_TO_PLAYLIST;
                        cmd = new MySqlCommand(rtn, dbMng.Connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_ID, playlist_id);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TRACK_ID, track.TRACK_ID);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_NAME, Utils.getStringWithinLen(track.NAME, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ARTIST, Utils.getStringWithinLen(track.ARTIST, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM_GA_RANK, track.ALBUM_GA_RANK);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM_ARTIST, Utils.getStringWithinLen(track.ALBUM_ARTIST, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM, Utils.getStringWithinLen(track.ALBUM, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_GENRE, Utils.getStringWithinLen(track.GENRE, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_KIND, Utils.getStringWithinLen(track.KIND, Constants.MYSQL_TINYTEXT_LEN));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_SIZE, track.SIZE);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TOTAL_TIME, track.TOTAL_TIME);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TRACK_NUMBER, track.TRACK_NUMBER);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_YEAR, track.YEAR);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DATE_MODIFIED, track.DATE_MODIFIED);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DATE_ADDED, track.DATE_ADDED);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_BIT_RATE, track.BIT_RATE);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DISC_NUMBER, track.DISC_NUMBER);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_DISC_COUNT, track.DISC_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_TRACK_COUNT, track.TRACK_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_LOCATION, Utils.getStringWithinLen(track.LOCATION, Constants.MYSQL_SHORTTEXT_2048));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_CLEAN_LOC, Utils.getStringWithinLen(track.CLEAN_LOC, Constants.MYSQL_SHORTTEXT_2048));
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_FILE_FOLDER_COUNT, track.FILE_FOLDER_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_LIBRARY_FOLDER_COUNT, track.LIBRARY_FOLDER_COUNT);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                        cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                        cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();

                        result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                        vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                        if (result != Constants.SP_RESULT_OK)
                        {
                            rejectedTracksList.Add(track.ToString());
                            DBLogger.logWarn(funName + track.ToString());
                        }

                        cmd.Dispose();
                    }

                    dbMng.CloseConnection();

                    if (rejectedTracksList.Count > 0)
                    {
                        string timeStamp = "_" + DateTime.Now.ToString(Constants.FULLTIME_FORMAT_STRING);
                        string filename = outputfileName + timeStamp + outputfileExtension;

                        string outputfile = Path.Combine(outputfilePath, filename);
                        StreamWriter file = new System.IO.StreamWriter(outputfile);

                        foreach (string trackFullStr in rejectedTracksList)
                        {
                            file.WriteLine(trackFullStr);
                            DBLogger.logWarn(trackFullStr);
                        }

                        file.Close();
                    }
                }
                else
                {
                    log.Error(vresult);
                    DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, playlistTitle));
                }
                dbMng.CloseConnection();
            }
            else
            {
                retval = Constants.ERROR_ON_CONNECTION;
                log.Error(Constants.ERROR_ON_CONNECTION);
                DBLogger.logWarn(retval);
            }
            return retval;
        }


        public static string update_track_tagsDB(LfTrack track)
        {
            string retval = string.Empty;
            string funName = "update_track_tagsDB(LfTrack track) - ";
            DBManager dbMng = new DBManager();
            int result = -1;
            string vresult = string.Empty;

            if (track.id == 0)
            {
                retval = DBManager.getTrackIdFromDb(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    return funName + retval;
                }
            }

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_ADD_OR_UPDATE_TRACK_TAGS_LF;

                retval = DBManager.getTagIdsFromDb(track.tags);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                    return funName + retval;

                foreach (var elem in track.tags)
                {
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_UPDATE_TAG_ID_TRACK, track.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_UPDATE_TAG_ID_TAG, elem.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_UPDATE_TAG_ID_DESC, Utils.getStringWithinLen(string.Format("{0}-{1}", track.name, elem.name), Constants.MYSQL_TINYTEXT_LEN));
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                    cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                    cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                    vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                    if (result != Constants.SP_RESULT_OK)
                    {
                        retval = funName + vresult;
                        DBLogger.logWarn(retval);
                        //return retval;
                    }

                    cmd.Dispose();
                }
                dbMng.CloseConnection();
                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
                DBLogger.logError(retval);
            }

            return retval;
        }

        public static string update_artist_tagsDB(LfArtist artist)
        {
            string retval = string.Empty;
            string funName = "update_artist_tagsDB(LfArtist artist) - ";
            DBManager dbMng = new DBManager();
            int result = -1;
            string vresult = string.Empty;

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_ADD_OR_UPDATE_ARTIST_TAGS_LF;

                foreach (var elem in artist.tags)
                {
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_UPDATE_TAG_ID_ARTIST, artist.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_UPDATE_TAG_ID_TAG, elem.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_UPDATE_TAG_ID_DESC, Utils.getStringWithinLen(string.Format("{0}-{1}", artist.name, elem.name), Constants.MYSQL_TINYTEXT_LEN));
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
                        DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, string.Format(Constants.FORMAT_STRING_2_PARAMS, artist.ToString(), elem.ToString())));
                    }
                    cmd.Dispose();
                }
                dbMng.CloseConnection();
                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
                DBLogger.logError(retval);
            }

            return retval;
        }

        public static string update_album_tagsDB(LfAlbum album)
        {
            string retval = string.Empty;
            string funName = "update_album_tags(LfAlbum album) - ";
            DBManager dbMng = new DBManager();
            int result = -1;
            string vresult = string.Empty;

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_ADD_OR_UPDATE_ALBUM_TAGS_LF;

                foreach (var elem in album.tags)
                {
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_UPDATE_TAG_ID_ALBUM, album.dbId);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_UPDATE_TAG_ID_TAG, elem.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_UPDATE_TAG_ID_DESC, Utils.getStringWithinLen(string.Format("{0}-{1}", album.name, elem.name), Constants.MYSQL_TINYTEXT_LEN));
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
                        DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, string.Format(album.ToString(), elem.ToString())));
                    }

                    cmd.Dispose();
                }
                dbMng.CloseConnection();
                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
            }

            return retval;
        }

        public static List<LfAlbum> getAllAlbumsFromDB()
        {
            List<LfAlbum> albums = new List<LfAlbum>();
            DBManager dbMng = new DBManager();

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_GET_ALL_RS500GA_ALBUMS;
                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                MySqlDataReader reader = cmd.ExecuteReader();
                //DataTable schemaTable = reader.GetSchemaTable();
                int idAlbum;
                int rank;
                int idArtist;

                while (reader.Read())
                {
                    LfAlbum album = new LfAlbum();
                    LfArtist artist = new LfArtist();

                    idAlbum = -1;
                    rank = -1;
                    idArtist = -1;
                    if (Int32.TryParse(reader[Constants.SP_TABLE_COLUMN_IDALBUM].ToString(), out idAlbum))
                        album.dbId = idAlbum;

                    album.name = reader[Constants.SP_TABLE_COLUMN_TITLE].ToString();

                    if (Int32.TryParse(reader[Constants.SP_TABLE_COLUMN_RANK_RS_GA].ToString(), out rank))
                        album.rank = rank;

                    if (Int32.TryParse(reader[Constants.SP_TABLE_COLUMN_ARTIST].ToString(), out idArtist))
                        artist.id = idArtist;

                    artist.name = reader[Constants.SP_TABLE_COLUMN_ANAME].ToString();

                    album.artist = artist;
                    album.artistName = artist.name;
                    album.idArtist = artist.id;
                    albums.Add(album);
                }
                reader.Close();
                cmd.Dispose();

                dbMng.CloseConnection();
            }
            return albums;
        }

        public static List<LfTrack> getAllTracsFromDB()
        {
            List<LfTrack> tracks = new List<LfTrack>();
            DBManager dbMng = new DBManager();

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_GET_ALL_RS500GA_TRACKS;
                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                MySqlDataReader reader = cmd.ExecuteReader();
                //DataTable schemaTable = reader.GetSchemaTable();
                int idTrack;
                int idAlbum;
                int rank;
                int idArtist;

                while (reader.Read())
                {
                    LfTrack track = new LfTrack();
                    LfAlbum album = new LfAlbum();
                    LfArtist artist = new LfArtist();

                    idAlbum = -1;
                    rank = -1;
                    idArtist = -1;
                    if (Int32.TryParse(reader[Constants.SP_TABLE_COLUMN_IDTRACK].ToString(), out idTrack))
                        track.dbId = idTrack;

                    track.name = reader[Constants.SP_TABLE_COLUMN_TITLE].ToString();

                    if (Int32.TryParse(reader[Constants.SP_TABLE_COLUMN_IDALBUM].ToString(), out idAlbum))
                        album.dbId = idAlbum;

                    album.name = reader[Constants.SP_TABLE_COLUMN_ATITLE].ToString();

                    if (Int32.TryParse(reader[Constants.SP_TABLE_COLUMN_RANK_RS_GA].ToString(), out rank))
                        album.rank = rank;

                    if (Int32.TryParse(reader[Constants.SP_TABLE_COLUMN_IDARTIST].ToString(), out idArtist))
                        artist.id = idArtist;

                    artist.name = reader[Constants.SP_TABLE_COLUMN_ANAME].ToString();

                    album.artist = artist;
                    album.artistName = artist.name;
                    album.idArtist = artist.id;

                    track.album = album;
                    track.artist = artist;
                    track.artistName = artist.name;
                    track.albumName = album.name;

                    tracks.Add(track);
                    //artists.Add(album);
                }
                reader.Close();
                cmd.Dispose();

                dbMng.CloseConnection();
            }
            return tracks;
        }

        public static List<LfArtist> getAllArtistsFromDB()
        {
            List<LfArtist> artists = new List<LfArtist>();
            DBManager dbMng = new DBManager();

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_GET_ALL_RS500GA_ARTISTS;
                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                MySqlDataReader reader = cmd.ExecuteReader();
                //DataTable schemaTable = reader.GetSchemaTable();
                int idArtist;

                while (reader.Read())
                {
                    LfArtist artist = new LfArtist();

                    idArtist = -1;

                    if (Int32.TryParse(reader[Constants.SP_TABLE_COLUMN_IDARTIST].ToString(), out idArtist))
                        artist.id = idArtist;

                    artist.name = reader[Constants.SP_TABLE_COLUMN_ANAME].ToString();

                    artists.Add(artist);
                    //artists.Add(album);
                }
                reader.Close();
                cmd.Dispose();

                dbMng.CloseConnection();
            }
            return artists;
        }

        public static string updateAllArtistsEdgesDB(LfArtist artist, List<LfArtist> artists)
        {
            string retval = string.Empty;
            string funName = "updateAllArtistsEdgesDB(LfArtist track, List<LfArtist> tracks) - ";
            DBManager dbMng = new DBManager();
            int result = -1;
            string vresult = string.Empty;

            if (artist.id == 0)
            {
                retval = DBManager.getArtistIdFromDb(artist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    string retv = funName + retval;
                    DBLogger.logError(retv);
                    return retv;
                }
            }

            if (dbMng.OpenConnection())
            {
                //bool skipCall = false;
                string rtn = Constants.STORED_PROC_UPDATE_TWO_NOT_DIRECT_CONNECTED_ARTISTS;
                string asdesc = string.Empty;
                int elemid = 0;

                foreach (var elem in artists)
                {
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (elem.id == artist.id) // no need to search self edges
                        continue;

                    if (elem.id == 0)
                    {
                        retval = DBManager.getArtistIdFromDb(elem);
                        if (retval != Constants.RETVAL_MESSAGE_DONE)
                        {
                            DBLogger.logError(funName + retval + "Skipp Execute query call");
                            continue;
                            //return funName + retval;
                        }
                        else
                        {
                            elemid = elem.id;
                        }
                    }

                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_IDARTIST1, artist.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_IDARTIST2, elem.id);
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
                        DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, string.Format(artist.ToString(), elem.ToString())));
                    }

                    cmd.Dispose();
                }
                dbMng.CloseConnection();
                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
                DBLogger.logWarn(retval);
            }

            return retval;
        }

        public static string updateArtistMatchDB(LfArtist artist)
        {
            string retval = string.Empty;
            string funName = "updateTracksMatchDB(LfTrack track) - ";
            DBManager dbMng = new DBManager();
            int result = -1;
            string vresult = string.Empty;

            if (artist.id == 0)
            {
                retval = DBManager.getArtistIdFromDb(artist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    string retv = funName + retval;
                    DBLogger.logError(retv);
                    return retv;
                }
            }

            if (dbMng.OpenConnection())
            {
                bool skipCall = false;
                string rtn = Constants.STORED_PROC_ADD_OR_UPDATE_MATCH_ARTIST_SIMILARS_LF;
                string asdesc = string.Empty;
                int elemid = 0;

                foreach (var elem in artist.similarArtists)
                {
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (elem.id == 0)
                    {
                        retval = DBManager.getArtistIdFromDb(elem);
                        if (retval != Constants.RETVAL_MESSAGE_DONE)
                        {
                            DBLogger.logError(funName + retval + "Skipp Execute query call");
                            skipCall = true;
                            //return funName + retval;
                        }
                        else
                        {
                            elemid = elem.id;
                        }
                    }
                    else
                    {
                        elemid = elem.id;
                    }

                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_IDARTIST1, artist.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_IDARTIST2, elem.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_TRACK_MATCH, elem.parent_match);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_TRACK_ASDESC, Utils.getStringWithinLen(string.Concat(artist.name, "-", elem.name), Constants.MYSQL_TINYTEXT_LEN));
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                    cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                    cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                    if (!skipCall)
                        cmd.ExecuteNonQuery();

                    result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                    vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                    if (result != Constants.SP_RESULT_OK)
                    {
                        log.Warn(vresult);
                        DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, string.Format(artist.ToString(), elem.ToString())));
                    }

                    cmd.Dispose();
                }
                dbMng.CloseConnection();
                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
                DBLogger.logWarn(retval);
            }

            return retval;
        }


        public static string updateAllTracksEdgesDB(LfTrack track, List<LfTrack> tracks) {
            string retval = string.Empty;
            string funName = "updateAllTracksEdgesDB(LfTrack track, List<LfTrack> tracks) - ";
            DBManager dbMng = new DBManager();
            int result = -1;
            string vresult = string.Empty;

            if (track.id == 0)
            {
                retval = DBManager.getTrackIdFromDb(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    string retv = funName + retval;
                    DBLogger.logError(retv);
                    return retv;
                }
            }

            if (dbMng.OpenConnection())
            {
                //bool skipCall = false;
                string rtn = Constants.STORED_PROC_UPDATE_TWO_NOT_DIRECT_CONNECTED_TRACKS;
                string asdesc = string.Empty;
                int elemid = 0;

                foreach (var elem in tracks)
                {
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (elem.id == track.id) // no need to search self edges
                        continue;

                    if (elem.id == 0)
                    {
                        retval = DBManager.getTrackIdFromDb(elem);
                        if (retval != Constants.RETVAL_MESSAGE_DONE)
                        {
                            DBLogger.logError(funName + retval + "Skipp Execute query call");
                            continue;
                            //return funName + retval;
                        }
                        else
                        {
                            elemid = elem.id;
                        }
                    }

                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_TRACK_IDTRACK1, track.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_TRACK_IDTRACK2, elem.id);
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
                        DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, string.Format(track.ToString(), elem.ToString())));
                    }

                    cmd.Dispose();
                }
                dbMng.CloseConnection();
                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
                DBLogger.logWarn(retval);
            }

            return retval;
        }

        public static string updateTracksMatchDB(LfTrack track)
        {
            string retval = string.Empty;
            string funName = "updateTracksMatchDB(LfTrack track) - ";
            DBManager dbMng = new DBManager();
            int result = -1;
            string vresult = string.Empty;

            if (track.id == 0)
            {
                retval = DBManager.getTrackIdFromDb(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    string retv = funName + retval;
                    DBLogger.logError(retv);
                    return retv;
                }
            }

            if (dbMng.OpenConnection())
            {
                bool skipCall = false;
                string rtn = Constants.STORED_PROC_ADD_OR_UPDATE_MATCH_TRACK_SIMILARS_LF;
                string asdesc = string.Empty;
                int elemid = 0;

                foreach (var elem in track.similarTrack)
                {
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (elem.id == 0)
                    {
                        retval = DBManager.getTrackIdFromDb(elem);
                        if (retval != Constants.RETVAL_MESSAGE_DONE)
                        {
                            DBLogger.logError(funName + retval + "Skipp Execute query call");
                            skipCall = true;
                            //return funName + retval;
                        }
                        else
                        {
                            elemid = elem.id;
                        }
                    }
                    else
                    {
                        elemid = elem.id;
                    }

                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_TRACK_IDTRACK1, track.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_TRACK_IDTRACK2, elem.id);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_TRACK_MATCH, elem.parent_match);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_TRACK_ASDESC, Utils.getStringWithinLen(string.Concat(track.name, "-", elem.name), Constants.MYSQL_TINYTEXT_LEN));
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                    cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                    cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                    if (!skipCall)
                        cmd.ExecuteNonQuery();

                    result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                    vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                    if (result != Constants.SP_RESULT_OK)
                    {
                        log.Warn(vresult);
                        DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, string.Format(track.ToString(), elem.ToString())));
                    }

                    cmd.Dispose();
                }
                dbMng.CloseConnection();
                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
                DBLogger.logWarn(retval);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artistsIDs"></param>
        /// <returns></returns>
        public static string updateArtistsMatchDB(Dictionary<int, double> artistsIDs)
        {
            string retval = string.Empty;
            string funName = "updateArtistsMatchDB(Dictionary<int, double> artistsIDs) - ";
            int targetArtist = artistsIDs.First().Key;
            artistsIDs.Remove(targetArtist);

            DBManager dbMng = new DBManager();
            int result = -1;
            string vresult = string.Empty;

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_ADD_OR_UPDATE_MATCH_ARTIST_SIMILARS_LF;

                foreach (var elem in artistsIDs)
                {
                    MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_IDARTIST1, targetArtist);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_IDARTIST2, elem.Key);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_MATCH, elem.Value);
                    cmd.Parameters.AddWithValue(Constants.SP_PARAM_MATCH_ARTIST_ASDESC, string.Empty);
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
                        DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, string.Format(targetArtist.ToString(), elem.ToString())));
                    }
                    cmd.Dispose();
                }
                dbMng.CloseConnection();

                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
                DBLogger.logError(retval);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="album"></param>
        /// <returns></returns>
        public static string getAlbumsIdFromDb(LfAlbum album)
        {
            //Dictionary<int, double> albumsIDs = new Dictionary<int, double>();
            string retval = string.Empty;
            string funName = "getAlbumsIdsFromDb(List<LfAlbum> albums) - ";
            int tmpIdAlbum = -1;
            int result = -1;
            string vresult = string.Empty;

            DBManager dbMng = new DBManager();

            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_GET_OR_ADD_ALBUM_LF;

                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;
                string artistName = string.Empty;
                int idArtist = -1;


                if (album.artist != null)
                {
                    artistName = album.artist.name;
                    idArtist = album.artist.id;
                }
                else
                {
                    DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, "Error on album artist", album.ToString()));
                }

                // albumTracks.Columns.Add(Constants.XSD_TAG_LF_NAME);
                cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_NAME, album.name);
                cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_SUBTITLE, Utils.getStringWithinLen(album.name + "-" + artistName, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_DESC, Utils.getStringWithinLen(album.wiki_summary, Constants.MYSQL_MEDIUMTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_NOTE, Utils.getStringWithinLen(album.wiki_content, Constants.MYSQL_MEDIUMTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_ID_ARTIST, idArtist);
                cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_RANK, album.rank);
                cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_YEAR, album.releasedate.Year);
                cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_NR_OF_CDS, -1);
                cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_NR_OF_TRACKS, album.tracks_count);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_MBID, album.mbid);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_URL, Utils.getStringWithinLen(album.url, Constants.MYSQL_TINYTEXT_LEN));
                cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_PLAYCOUNT, album.playcount);
                cmd.Parameters.AddWithValue(Constants.SP_GET_ALBUM_LF_PARAM_ID_ALBUM, tmpIdAlbum);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                cmd.Parameters[Constants.SP_GET_ALBUM_LF_PARAM_ID_ALBUM].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                vresult = cmd.Parameters[Constants.SP_PARAM_VRESULT].Value.ToString();

                if (result == Constants.SP_RESULT_OK)
                {
                    album.dbId = Convert.ToInt32(cmd.Parameters[Constants.SP_GET_ALBUM_LF_PARAM_ID_ALBUM].Value.ToString());
                    //albumsIDs.Add(artistID, Convert.ToDouble(tAlbum[Constants.XSD_TAG_LF_MATCH]));
                }
                else
                {
                    // return funName + vresult;
                    DBLogger.logWarn(string.Format(Constants.FORMAT_STRING_2_PARAMS, vresult, album.ToString()));
                }

                cmd.Dispose();
                dbMng.CloseConnection();

                retval = Constants.RETVAL_MESSAGE_DONE;
                //cmd.Parameters.AddWithValue()
            }
            else
            {
                retval = funName + Constants.ERROR_ON_CONNECTION;
                DBLogger.logError(retval);
            }

            return retval;
        }

    }
}