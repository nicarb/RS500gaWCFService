using log4net;
using MySql.Data.MySqlClient;
using RS500gaWCFService.db;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace RS500gaWCFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "LoadPlaylistDataService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select LoadPlaylistDataService.svc or LoadPlaylistDataService.svc.cs at the Solution Explorer and start debugging.
    public class LoadPlaylistDataService : ILoadPlaylistDataService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string xmlfilefolderPath = string.Empty;
        private string xsdfilename = string.Empty;
        private string outputfilePath = string.Empty;
        private string outputfileName = string.Empty;
        private string outputfileExtension = string.Empty;

        public string sendLibrary(string xmlcontent, string libraryTitle)
        {
            string retval = string.Empty;
            if (string.IsNullOrEmpty(xmlcontent))
            {
                retval = "# " + string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, "xmlcontent");
                log.Error(retval);
            }
            if (string.IsNullOrEmpty(libraryTitle))
            {
                retval += "# " + string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, "libraryTitle");
                log.Error(retval);
            }
            if (!string.IsNullOrEmpty(retval))
                return retval;

            log.Info("sendPlaylist(xmlcontentlen=" + xmlcontent.Length + "; libraryTitle=" + libraryTitle + ")");
            xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XSDFILE].ToString();
            xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            outputfilePath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTPATH].ToString();
            outputfileName = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTFILENAME].ToString();
            outputfileExtension = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTEXTENSION].ToString();
            //
            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;
            Dictionary<int, Track> tracksDic = new Dictionary<int, Track>();

            string xmlCleanCOntent = removeIllegalChars(xmlcontent);

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);
                //var resource = Application.GetResourceStream(new Uri(xmlfilename, UriKind.Relative));
                StringReader sr = new StringReader(xmlCleanCOntent);

                //XmlDocument doc = new XmlDocument();
                //doc.Load(xmlcontent);

                xReader = XmlReader.Create(sr, vr);

                xpDoc = new XPathDocument(xReader);
                xpNav = xpDoc.CreateNavigator();
                string tracksNodePos = Constants.XSD_TAG_PLIST +
                                       Constants.XSD_TAG_SEPARATOR +
                                       Constants.XSD_TAG_DICT +
                                       Constants.XSD_TAG_SEPARATOR +
                                       Constants.XSD_TAG_DICT;

                xpRootNodeIter = xpNav.Select(tracksNodePos);

                xpRootNodeIter.MoveNext();

                XPathNavigator node = xpRootNodeIter.Current;
                locNodeIter = node.Select(Constants.XSD_TAG_DICT);

                while (locNodeIter.MoveNext())
                {
                    //XPathNodeIterator keynode = locNodeIter.Current.Select(Constants.XSD_TAG_DICT);
                    //keynode.MoveNext();


                    //XPathNodeIterator dictnode = locNodeIter.Current.Select(Constants.XSD_TAG_DICT);
                    //dictnode.MoveNext();

                    int index = -1;
                    Track currTrack = new Track();
                    string albumFullName = string.Empty;
                    newphewNodeIter = locNodeIter.Current.Select(Constants.XSD_TAG_KEY);

                    while (newphewNodeIter.MoveNext())
                    {
                        XPathNavigator keyNode = newphewNodeIter.Current;

                        switch (keyNode.Value)
                        {
                            case Constants.XSD_KEY_TRACK_ID:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.TRACK_ID = keyNode.ValueAsInt;
                                    index = currTrack.TRACK_ID;
                                } break;
                            case Constants.XSD_KEY_NAME:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.NAME = replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ARTIST:
                                if (keyNode.MoveToNext())
                                {

                                    currTrack.ARTIST = replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ALBUM_ARTIST:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.ALBUM_ARTIST = replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ALBUM:
                                if (keyNode.MoveToNext())
                                {
                                    //currTrack.ALBUM = keyNode.Value;
                                    albumFullName = replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_GENRE:
                                if (keyNode.MoveToNext())
                                {
                                    string genre = keyNode.Value;
                                    if (genre.Contains(Constants.GENRES_SEPARATOR_1) ||
                                        genre.Contains(Constants.GENRES_SEPARATOR_2))
                                    {
                                        genre = parseAndTranslateGenres(genre);
                                    }
                                    currTrack.GENRE = genre;
                                } break;
                            case Constants.XSD_KEY_KIND:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.KIND = keyNode.Value;
                                } break;
                            case Constants.XSD_KEY_SIZE:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.SIZE = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_TOTAL_TIME:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.TOTAL_TIME = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_TRACK_NUMBER:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.TRACK_NUMBER = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_YEAR:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.YEAR = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_DATE_MODIFIED:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.DATE_MODIFIED = keyNode.ValueAsDateTime;
                                } break;
                            case Constants.XSD_KEY_DATE_ADDED:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.DATE_ADDED = keyNode.ValueAsDateTime;
                                } break;
                            case Constants.XSD_KEY_BIT_RATE:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.BIT_RATE = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_DISC_NUMBER:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.DISC_NUMBER = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_DISC_COUNT:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.DISC_COUNT = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_TRACK_COUNT:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.TRACK_COUNT = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_LOCATION:
                                if (keyNode.MoveToNext())
                                {
                                    string location = replace_blackListChars(keyNode.Value);
                                    currTrack.LOCATION = location;
                                } break;
                            case Constants.XSD_KEY_FILE_FOLDER_COUNT:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.FILE_FOLDER_COUNT = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_LIBRARY_FOLDER_COUNT:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.LIBRARY_FOLDER_COUNT = keyNode.ValueAsInt;
                                } break;
                        }

                    }

                    if (albumFullName.Contains(Constants.GA_RANK_DEF_SPLIT_CHAR))
                    {
                        getAlbumGARank(albumFullName, currTrack);
                    }
                    else
                    {
                        currTrack.ALBUM = albumFullName;
                    }
                    tracksDic.Add(index, currTrack);
                }
            }
            catch (Exception ex)
            {
                // Constants.ERROR_LOADING + ":" + ex.Message;
                //return retval;
                log.Error(Constants.ERROR_LOADING, ex);
                retval = "ERROR:" + ex.Message;
            }

            retval = sendDataToDb(tracksDic, libraryTitle);

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlcontent"></param>
        /// <param name="playlistTitle"></param>
        /// <returns></returns>
        public string sendPlaylist(string xmlcontent, string playlistTitle)
        {
            string retval = "Done";
            log.Info("sendPlaylist(xmlcontentlen=" + xmlcontent.Length + "; playlistTitle=" + playlistTitle + ")");
            xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XSDFILE].ToString();
            xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            outputfilePath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTPATH].ToString();
            outputfileName = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTFILENAME].ToString();
            outputfileExtension = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_OUTPUTEXTENSION].ToString();
            //
            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;
            Dictionary<int, Track> tracksDic = new Dictionary<int, Track>();

            string xmlCleanCOntent = removeIllegalChars(xmlcontent);

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);
                //var resource = Application.GetResourceStream(new Uri(xmlfilename, UriKind.Relative));
                StringReader sr = new StringReader(xmlCleanCOntent);

                //XmlDocument doc = new XmlDocument();
                //doc.Load(xmlcontent);

                xReader = XmlReader.Create(sr, vr);

                xpDoc = new XPathDocument(xReader);
                xpNav = xpDoc.CreateNavigator();
                string tracksNodePos = Constants.XSD_TAG_PLIST +
                                       Constants.XSD_TAG_SEPARATOR +
                                       Constants.XSD_TAG_DICT +
                                       Constants.XSD_TAG_SEPARATOR +
                                       Constants.XSD_TAG_DICT;

                xpRootNodeIter = xpNav.Select(tracksNodePos);

                xpRootNodeIter.MoveNext();

                XPathNavigator node = xpRootNodeIter.Current;
                locNodeIter = node.Select(Constants.XSD_TAG_DICT);

                while (locNodeIter.MoveNext())
                {
                    //XPathNodeIterator keynode = locNodeIter.Current.Select(Constants.XSD_TAG_DICT);
                    //keynode.MoveNext();


                    //XPathNodeIterator dictnode = locNodeIter.Current.Select(Constants.XSD_TAG_DICT);
                    //dictnode.MoveNext();

                    int index = -1;
                    Track currTrack = new Track();
                    string albumFullName = string.Empty;
                    newphewNodeIter = locNodeIter.Current.Select(Constants.XSD_TAG_KEY);

                    while (newphewNodeIter.MoveNext())
                    {
                        XPathNavigator keyNode = newphewNodeIter.Current;

                        switch (keyNode.Value)
                        {
                            case Constants.XSD_KEY_TRACK_ID:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.TRACK_ID = keyNode.ValueAsInt;
                                    index = currTrack.TRACK_ID;
                                } break;
                            case Constants.XSD_KEY_NAME:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.NAME = replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ARTIST:
                                if (keyNode.MoveToNext())
                                {
                                    
                                    currTrack.ARTIST = replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ALBUM_ARTIST:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.ALBUM_ARTIST = replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ALBUM:
                                if (keyNode.MoveToNext())
                                {
                                    //currTrack.ALBUM = keyNode.Value;
                                    albumFullName = replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_GENRE:
                                if (keyNode.MoveToNext())
                                {
                                    string genre = keyNode.Value;
                                    if (genre.Contains(Constants.GENRES_SEPARATOR_1) ||
                                        genre.Contains(Constants.GENRES_SEPARATOR_2))
                                    {
                                        genre = parseAndTranslateGenres(genre);
                                    }
                                    currTrack.GENRE = genre;
                                } break;
                            case Constants.XSD_KEY_KIND:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.KIND = keyNode.Value;
                                } break;
                            case Constants.XSD_KEY_SIZE:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.SIZE = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_TOTAL_TIME:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.TOTAL_TIME = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_TRACK_NUMBER:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.TRACK_NUMBER = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_YEAR:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.YEAR = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_DATE_MODIFIED:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.DATE_MODIFIED = keyNode.ValueAsDateTime;
                                } break;
                            case Constants.XSD_KEY_DATE_ADDED:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.DATE_ADDED = keyNode.ValueAsDateTime;
                                } break;
                            case Constants.XSD_KEY_BIT_RATE:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.BIT_RATE = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_DISC_NUMBER:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.DISC_NUMBER = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_DISC_COUNT:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.DISC_COUNT = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_TRACK_COUNT:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.TRACK_COUNT = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_LOCATION:
                                if (keyNode.MoveToNext())
                                {
                                    string location = replace_blackListChars(keyNode.Value);
                                    currTrack.LOCATION = location;
                                } break;
                            case Constants.XSD_KEY_FILE_FOLDER_COUNT:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.FILE_FOLDER_COUNT = keyNode.ValueAsInt;
                                } break;
                            case Constants.XSD_KEY_LIBRARY_FOLDER_COUNT:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.LIBRARY_FOLDER_COUNT = keyNode.ValueAsInt;
                                } break;
                        }

                    }

                    if (albumFullName.Contains(Constants.GA_RANK_DEF_SPLIT_CHAR))
                    {
                        getAlbumGARank(albumFullName, currTrack);
                    }
                    else
                    {
                        currTrack.ALBUM = albumFullName;
                    }
                    tracksDic.Add(index, currTrack);
                }
            }
            catch (Exception ex)
            {
                // Constants.ERROR_LOADING + ":" + ex.Message;
                //return retval;
                log.Error(Constants.ERROR_LOADING, ex);
            }

            retval = sendDataToDb(tracksDic, playlistTitle);

            return retval;
        }

        public string getSongLyrics(string song, string artist)
        {
            string retval = string.Empty;
            string uri = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LYRICSURI].ToString();
            string urlFAddress = uri + "{0}/{1}.html";

            string urlAddress = string.Format(urlFAddress, normaliseTitle(artist), normaliseTitle(song));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                retval = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }

            return retval;
        }

        public string generatePlaylist(string xmlcontent)
        {
            return "not yet implemented.";
        }

        private string parseAndTranslateGenres(string genres)
        {
            string retval = string.Empty;
            string[] tokens = genres.Split(Constants.GENRES_SEPARATOR_1);
            foreach (string genre in tokens)
            {
                if (Constants.TRANSLATE_RU_EN.ContainsKey(genre))
                {
                    retval = retval + Constants.TRANSLATE_RU_EN[genre] + Constants.GENRES_SEPARATOR_1;
                }
                else
                {
                    string gnr = string.Empty;
                    if (genre.Contains(Constants.GENRES_SEPARATOR_2))
                    {
                        string[] subgen = genre.Split(Constants.GENRES_SEPARATOR_2);
                        foreach (string sg in subgen)
                        {
                            if (Constants.TRANSLATE_RU_EN.ContainsKey(sg))
                            {
                                gnr = gnr + Constants.TRANSLATE_RU_EN[sg] + Constants.GENRES_SEPARATOR_2;
                            }
                            else
                            {
                                gnr = gnr + sg + Constants.GENRES_SEPARATOR_2;
                            }
                        }
                        gnr = gnr.Substring(0, gnr.Length - 1);
                    }
                    else
                    {
                        gnr = genre;
                    }
                    retval = retval + gnr + Constants.GENRES_SEPARATOR_1;
                }
            }
            retval = retval.Substring(0, retval.Length - 1);
            return retval;
        }

        private string normaliseTitle(string title)
        {
            return title.ToLower().Replace(Constants.WHITESPACE_STRING, string.Empty);
        }

        private string replace_blackListChars(string source)
        {
            return source.Replace(Constants.GA_RANK_BLK_LIST_1, Constants.GA_RANK_WHT_LIST_1).
                          Replace(Constants.GA_RANK_BLK_LIST_2, Constants.GA_RANK_WHT_LIST_2).
            Replace(Constants.GA_RANK_BLK_LIST_3, Constants.GA_RANK_WHT_LIST_3).
            Replace(Constants.GA_RANK_BLK_LIST_4, Constants.GA_RANK_WHT_LIST_4).
            Replace(Constants.GA_RANK_BLK_LIST_5, Constants.GA_RANK_WHT_LIST_5);
        }

        private string removeIllegalChars(string xmlStr)
        {
            string retval = xmlStr;
            List<char> charsToSubstitute = new List<char>();
            charsToSubstitute.Add((char)0x19);
            charsToSubstitute.Add((char)0x1C);
            charsToSubstitute.Add((char)0x1D);

            foreach (char c in charsToSubstitute)
                retval = retval.Replace(Convert.ToString(c), string.Empty);

            return retval;
        }

        private void getAlbumGARank(string albumFullName, Track track)
        {
            int trackRank;
            if (Int32.TryParse(albumFullName.Substring(Constants.GA_RANK_DEF_STR_INIT, Constants.GA_RANK_DEF_STR_LEN), out trackRank))
            {
                track.ALBUM_GA_RANK = trackRank;
            }
            else
            {
                track.ALBUM_GA_RANK = Constants.GA_RANK_UDEFINED;
            }

            int pos = albumFullName.IndexOf(Constants.GA_RANK_DEF_SPLIT_CHAR) + 2;
            track.ALBUM = albumFullName.Substring(pos).Trim();
        }

        private string sendDataToDb(Dictionary<int, Track> tracksDic, string playlistTitle)
        {
            string retval = "OK";
            string plType = Constants.PLAYLIST_TYPE_MANUAL;
            string plDesc = Constants.PLAYLIST_MANUAL_DESC;
            string plTypeDesc = Constants.PLAYLIST_TYPE_MANUAL_DESC;
            int playlist_id = -1;
            int result = -1;
            string vresult = string.Empty;

            DBManager dbMng = new DBManager();
            if (dbMng.OpenConnection())
            {
                string rtn = Constants.STORED_PROC_ADD_NEW_PLAYLIST;
                MySqlCommand cmd = new MySqlCommand(rtn, dbMng.Connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TITLE, playlistTitle);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_DESC, plDesc);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TYPE, plType);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_TYPE_DESC, plTypeDesc);

                cmd.Parameters.AddWithValue(Constants.SP_PARAM_PLAYLIST_ID, playlist_id);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_RESULT, result);
                cmd.Parameters.AddWithValue(Constants.SP_PARAM_VRESULT, vresult);

                cmd.Parameters[Constants.SP_PARAM_PLAYLIST_ID].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_RESULT].Direction = ParameterDirection.Output;
                cmd.Parameters[Constants.SP_PARAM_VRESULT].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                result = Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_RESULT].Value.ToString());
                playlist_id =  Convert.ToInt32(cmd.Parameters[Constants.SP_PARAM_PLAYLIST_ID].Value.ToString());
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
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_NAME, track.NAME);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ARTIST, track.ARTIST);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM_GA_RANK, track.ALBUM_GA_RANK); 
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM_ARTIST, track.ALBUM_ARTIST);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_ALBUM, track.ALBUM);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_GENRE, track.GENRE);
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_KIND, track.KIND);
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
                        cmd.Parameters.AddWithValue(Constants.SP_PARAM_LOCATION, track.LOCATION);
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
                        }
                    }

                    if (rejectedTracksList.Count > 0)
                    {
                        string timeStamp = "_" + DateTime.Now.ToString(Constants.FULLTIME_FORMAT_STRING);
                        string filename = outputfileName + timeStamp + outputfileExtension;

                        string outputfile = Path.Combine(outputfilePath, filename);
                        StreamWriter file = new System.IO.StreamWriter(outputfile);

                        foreach (string trackFullStr in rejectedTracksList)
                        {
                            file.WriteLine(trackFullStr);
                        }

                        file.Close();
                    }
                }
                else
                {
                    log.Error(vresult);
                }
                dbMng.CloseConnection();
            }
            else
            {
                retval = Constants.ERROR_ON_CONNECTION;
                log.Error(Constants.ERROR_ON_CONNECTION);
            }
            return retval;
        }
    }
}
