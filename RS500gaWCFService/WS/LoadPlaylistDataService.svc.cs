using log4net;
using RS500gaWCFService.data;
using RS500gaWCFService.db;
using RS500gaWCFService.lastfm;
using RS500gaWCFService.Logging;
using RS500gaWCFService.rs500ga;
using RS500gaWCFService.utils;
using RS500gaWCFService.RSPlaylistTypeNS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;


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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlcontent"></param>
        /// <param name="libraryTitle"></param>
        /// <returns></returns>
        public string sendLibrary(string xmlcontent, string libraryTitle)
        {
            string retval = string.Empty;
            string funName = "sendLibrary(string xmlcontent, string libraryTitle) - ";
            if (string.IsNullOrEmpty(xmlcontent))
            {
                retval = string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, funName, "xmlcontent");
                log.Error(retval);
                //return retval;
            }
            if (string.IsNullOrEmpty(libraryTitle))
            {
                retval += "# " + string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, funName, "libraryTitle");
                log.Error(retval);
            }
            if (!string.IsNullOrEmpty(retval))
                return retval;

            log.Info("sendLibrary(xmlcontentlen=" + xmlcontent.Length + "; libraryTitle=" + libraryTitle + ")");
            xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_PLAYLIST_XSDFILE].ToString();
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

            string xmlCleanCOntent = Utils.removeIllegalChars(xmlcontent);

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

                int index = 0;
                while (locNodeIter.MoveNext())
                {
                    //XPathNodeIterator keynode = locNodeIter.Current.Select(Constants.XSD_TAG_DICT);
                    //keynode.MoveNext();


                    //XPathNodeIterator dictnode = locNodeIter.Current.Select(Constants.XSD_TAG_DICT);
                    //dictnode.MoveNext();

                    Track currTrack = new Track();
                    string albumFullName = string.Empty;
                    string location = string.Empty;
                    string cleanLoc = string.Empty;

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
                                    //index = currTrack.TRACK_ID;
                                } break;
                            case Constants.XSD_KEY_NAME:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.NAME = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ARTIST:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.ARTIST = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ALBUM_ARTIST:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.ALBUM_ARTIST = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ALBUM:
                                if (keyNode.MoveToNext())
                                {
                                    //currTrack.ALBUM = keyNode.Value;
                                    albumFullName = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_GENRE:
                                if (keyNode.MoveToNext())
                                {
                                    string genre = keyNode.Value.ToLower();
                                    if (genre.Contains(Constants.GENRES_SEPARATOR_1) ||
                                        genre.Contains(Constants.GENRES_SEPARATOR_2))
                                    {
                                        if (Constants.WHITE_LIST_RU_GENRES.Contains(genre))
                                        {
                                            genre = Constants.TRANSLATE_RU_EN[genre];
                                        }
                                        else if (Constants.WHITE_LIST_EN_GENRES.Contains(genre))
                                        {
                                            // genre is already the one we want, except for the fact that we want to avid it to be split or whatever
                                        }
                                        else
                                        {
                                            genre = Rs500gaUtils.parseAndTranslateGenres(genre);
                                        }
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
                                    //string location = replace_blackListChars(keyNode.Value);
                                    //currTrack.LOCATION = location;
                                    location = keyNode.Value;
                                    currTrack.LOCATION = location;
                                    cleanLoc = Rs500gaUtils.replace_blackListChars(location);
                                    currTrack.CLEAN_LOC = cleanLoc;
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
                        Rs500gaUtils.getAlbumGARank(albumFullName, currTrack);
                    }
                    else
                    {
                        currTrack.ALBUM = albumFullName;
                    }
                    tracksDic.Add(index++, currTrack);

                }
            }
            catch (Exception ex)
            {
                // Constants.ERROR_LOADING + ":" + ex.Message;
                //return retval;
                log.Error(Constants.ERROR_LOADING, ex);
                retval = "ERROR:" + ex.Message;
            }
            if (string.IsNullOrEmpty(retval) && tracksDic.Count > 0)
            {
                retval = DBManager.sendDataToDb(tracksDic, libraryTitle);
            }

            return retval;
        }

        // 
        /// </summary>
        /// <param name="xmlcontent"></param>
        /// <param name="libraryTitle"></param>
        /// <returns></returns>
        public string createLibrary(string xmlcontent, string libraryTitle)
        {
            string retval = string.Empty;
            string funName = "sendLibrary(string xmlcontent, string libraryTitle) - ";
            if (string.IsNullOrEmpty(xmlcontent))
            {
                retval = string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, funName, "xmlcontent");
                log.Error(retval);
                //return retval;
            }
            if (string.IsNullOrEmpty(libraryTitle))
            {
                retval += "# " + string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, funName, "libraryTitle");
                log.Error(retval);
            }
            if (!string.IsNullOrEmpty(retval))
                return retval;

            log.Info("sendLibrary(xmlcontentlen=" + xmlcontent.Length + "; libraryTitle=" + libraryTitle + ")");
            xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_PLAYLIST_XSDFILE].ToString();
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

            string xmlCleanCOntent = Utils.removeIllegalChars(xmlcontent);

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

                int index = 0;
                while (locNodeIter.MoveNext())
                {
                    //XPathNodeIterator keynode = locNodeIter.Current.Select(Constants.XSD_TAG_DICT);
                    //keynode.MoveNext();


                    //XPathNodeIterator dictnode = locNodeIter.Current.Select(Constants.XSD_TAG_DICT);
                    //dictnode.MoveNext();

                    Track currTrack = new Track();
                    string albumFullName = string.Empty;
                    string location = string.Empty;
                    string cleanLoc = string.Empty;

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
                                    //index = currTrack.TRACK_ID;
                                } break;
                            case Constants.XSD_KEY_NAME:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.NAME = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ARTIST:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.ARTIST = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ALBUM_ARTIST:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.ALBUM_ARTIST = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ALBUM:
                                if (keyNode.MoveToNext())
                                {
                                    //currTrack.ALBUM = keyNode.Value;
                                    albumFullName = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_GENRE:
                                if (keyNode.MoveToNext())
                                {
                                    string genre = keyNode.Value.ToLower();
                                    if (genre.Contains(Constants.GENRES_SEPARATOR_1) ||
                                        genre.Contains(Constants.GENRES_SEPARATOR_2))
                                    {
                                        if (Constants.WHITE_LIST_RU_GENRES.Contains(genre))
                                        {
                                            genre = Constants.TRANSLATE_RU_EN[genre];
                                        }
                                        else if (Constants.WHITE_LIST_EN_GENRES.Contains(genre))
                                        {
                                            // genre is already the one we want, except for the fact that we want to avid it to be split or whatever
                                        }
                                        else
                                        {
                                            genre = Rs500gaUtils.parseAndTranslateGenres(genre);
                                        }
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
                                    //string location = replace_blackListChars(keyNode.Value);
                                    //currTrack.LOCATION = location;
                                    location = keyNode.Value;
                                    currTrack.LOCATION = location;
                                    cleanLoc = Rs500gaUtils.replace_blackListChars(location);
                                    currTrack.CLEAN_LOC = cleanLoc;
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
                        Rs500gaUtils.getAlbumGARank(albumFullName, currTrack);
                    }
                    else
                    {
                        currTrack.ALBUM = albumFullName;
                    }
                    tracksDic.Add(index++, currTrack);

                }
            }
            catch (Exception ex)
            {
                // Constants.ERROR_LOADING + ":" + ex.Message;
                //return retval;
                log.Error(Constants.ERROR_LOADING, ex);
                retval = "ERROR:" + ex.Message;
            }
            if (string.IsNullOrEmpty(retval) && tracksDic.Count > 0)
            {
                retval = DBManager.sendDataToDb(tracksDic, libraryTitle);
            }

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
            string retval = string.Empty;
            string funName = "sendPlaylist(string xmlcontent, string playlistTitle) - ";
            log.Info("sendPlaylist(xmlcontentlen=" + xmlcontent.Length + "; playlistTitle=" + playlistTitle + ")");
            xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_PLAYLIST_XSDFILE].ToString();
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

            string xmlCleanCOntent = Utils.removeIllegalChars(xmlcontent);

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

                int index = 0;
                while (locNodeIter.MoveNext())
                {
                    //XPathNodeIterator keynode = locNodeIter.Current.Select(Constants.XSD_TAG_DICT);
                    //keynode.MoveNext();


                    //XPathNodeIterator dictnode = locNodeIter.Current.Select(Constants.XSD_TAG_DICT);
                    //dictnode.MoveNext();


                    Track currTrack = new Track();
                    string albumFullName = string.Empty;
                    string location = string.Empty;
                    string cleanLoc = string.Empty;
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
                                    //index = currTrack.TRACK_ID;
                                } break;
                            case Constants.XSD_KEY_NAME:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.NAME = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ARTIST:
                                if (keyNode.MoveToNext())
                                {

                                    currTrack.ARTIST = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ALBUM_ARTIST:
                                if (keyNode.MoveToNext())
                                {
                                    currTrack.ALBUM_ARTIST = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_ALBUM:
                                if (keyNode.MoveToNext())
                                {
                                    //currTrack.ALBUM = keyNode.Value;
                                    albumFullName = Rs500gaUtils.replace_blackListChars(keyNode.Value);
                                } break;
                            case Constants.XSD_KEY_GENRE:
                                if (keyNode.MoveToNext())
                                {
                                    string genre = keyNode.Value.ToLower();
                                    if (genre.Contains(Constants.GENRES_SEPARATOR_1) ||
                                        genre.Contains(Constants.GENRES_SEPARATOR_2)) // if there are multiple genres
                                    {
                                        if (Constants.WHITE_LIST_RU_GENRES.Contains(genre))
                                        {
                                            genre = Constants.TRANSLATE_RU_EN[genre];
                                        }
                                        else if (Constants.WHITE_LIST_EN_GENRES.Contains(genre))
                                        {
                                            // genre is already the one we want, except for the fact that we want to avid it to be split or whatever
                                        }
                                        else
                                        {
                                            genre = Rs500gaUtils.parseAndTranslateGenres(genre);
                                        }
                                        genre = Rs500gaUtils.parseAndTranslateGenres(genre);
                                    }
                                    else if (Constants.TRANSLATE_RU_EN.ContainsKey(genre))
                                    {
                                        genre = Constants.TRANSLATE_RU_EN[genre];
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
                                    //string location = replace_blackListChars(keyNode.Value);
                                    //currTrack.LOCATION = location;
                                    location = keyNode.Value;
                                    currTrack.LOCATION = location;
                                    cleanLoc = Rs500gaUtils.replace_blackListChars(location);
                                    currTrack.CLEAN_LOC = cleanLoc;
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
                        Rs500gaUtils.getAlbumGARank(albumFullName, currTrack);
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
                retval = funName + Constants.ERROR_LOADING + ex.Message;
            }

            retval = DBManager.sendDataToDb(tracksDic, playlistTitle);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
                retval = funName + retval;

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="song"></param>
        /// <param name="album"></param>
        /// <returns></returns>
        public string getSongLyrics(string song, string artist)
        {
            string retval = string.Empty;
            string funName = "getSongLyrics(string song, string artist) - ";
            string uri = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LYRICSURI].ToString();
            string urlFAddress = uri + "{0}/{1}.html";

            string urlAddress = string.Format(urlFAddress, Rs500gaUtils.normaliseTitle(artist), Rs500gaUtils.normaliseTitle(song));

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
                if (string.IsNullOrEmpty(retval))
                    retval = funName + "Error on calling web request: " + urlAddress;
                response.Close();
                readStream.Close();
            }

            return retval;
        }

        public string getAllRs500GaAlbums1()
        {
            string retval = string.Empty;
            string funName = "getSongLyrics(string song, string artist) - ";
            List<LfAlbum> albums = DBManager.getAllAlbumsFromDB();

            foreach (LfAlbum album in albums)
            {
                retval += string.Format("{0},{1},{2},{3},{4};", album.dbId, album.name, album.rank, album.idArtist, album.artistName);
            }
            if (albums.Count == 0)
            {
                return funName + Constants.ERROR_NO_ARTISTS_OR_TRACKS;
            }

            return retval;
        }

        public string updateAllRs500GaArtistSimilars()
        {
            string retval = string.Empty;
            string funName = "updateAllRs500GaArtistSimilars() - ";
            string username = ConfigurationManager.AppSettings[LastfmMethods.lf_Username_tag].ToString();
            List<LfArtist> artists = DBManager.getAllArtistsFromDB();

            foreach (LfArtist artist in artists)
            {
                DBLogger.logInfo("SIMILARS - " + artist.ToString());

                //List<LfArtist> simArtist = new List<LfArtist>();
                retval = LastfmMethods.getSimilarArtists(artist, Constants.LF_ALL_SIMILAR_ARTISTS_LIMIT.ToString(), true, artist.mbid, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = DBManager.updateArtistsMatchDB(artist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                foreach (LfArtist simArt in artist.similarArtists)
                {
                    retval = LastfmMethods.getSimilarArtists(simArt, Constants.LF_ALL_SIMILAR_ARTISTS_LIMIT.ToString(), true, simArt.mbid, string.Empty);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        continue;
                    }

                    retval = DBManager.updateArtistsMatchDB(simArt);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        continue;
                    }
                }
            }

            return retval;
        }

        public string updateAllRs500GaArtistsGraphConnections()
        {
            //update_two_not_direct_connected_tracks
            string retval = string.Empty;
            string funName = "updateAllRs500GaArtistsGraphConnections() - ";
            string username = ConfigurationManager.AppSettings[LastfmMethods.lf_Username_tag].ToString();

            List<LfArtist> artists = DBManager.getAllArtistsFromDB();

            foreach (LfArtist artist in artists)
            {
                retval = DBManager.updateAllArtistsEdgesDB(artist, artists);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }
            }

            return retval;
        }


        public string updateAllRs500GaTracksGraphConnections()
        {
            //update_two_not_direct_connected_tracks
            string retval = string.Empty;
            string funName = "updateAllRs500GaTracksInfo() - ";
            string username = ConfigurationManager.AppSettings[LastfmMethods.lf_Username_tag].ToString();

            List<LfTrack> tracks = DBManager.getAllTracsFromDB();

            foreach (LfTrack track in tracks)
            {
                retval = DBManager.updateAllTracksEdgesDB(track, tracks);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }
            }

            return retval;
        }

        public string updateAllRs500GaTrackSimilars()
        {
            string retval = string.Empty;
            string funName = "updateAllRs500GaTracksInfo() - ";
            string username = ConfigurationManager.AppSettings[LastfmMethods.lf_Username_tag].ToString();

            List<LfTrack> tracks = DBManager.getAllTracsFromDB();

            foreach (LfTrack track in tracks)
            {
                DBLogger.logInfo("SIMILARS - " + track.ToString());

                List<LfTrack> simTracks = new List<LfTrack>();
                retval = LastfmMethods.getSimilarTrack(track, Constants.LF_SIMILAR_TRACKS_LIMIT.ToString(), track.name, track.artistName, false, track.mbid, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = LastfmMethods.getAndUpdateOnlySimilarTracksList(track.similarTrack, true);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    //continue;
                }

                retval = DBManager.updateTracksMatchDB(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }
            }

            return retval;
        }

        public string updateAllRs500GaTracksInfo()
        {
            string retval = string.Empty;
            string funName = "updateAllRs500GaTrackSimilars - ";
            string username = ConfigurationManager.AppSettings[LastfmMethods.lf_Username_tag].ToString();

            List<LfTrack> tracks = DBManager.getAllTracsFromDB();
            List<LfArtist> parsedArtists = new List<LfArtist>();
            List<LfAlbum> parsedAlbums = new List<LfAlbum>();

            foreach (LfTrack track in tracks)
            {
                DBLogger.logInfo(track.ToString());
                retval = LastfmMethods.getTrackInfo(track, false, username, string.Empty, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                bool newArtist = true;
                bool newAlbum = true;

                if (track.artist != null)
                {
                    foreach (LfArtist artist in parsedArtists)
                    {
                        if (newArtist && track.artist.name == artist.name)
                        {
                            track.artist.id = artist.id;
                            newArtist = false;
                        }
                    }
                    if (track.artist.id == 0)
                    {
                        retval = DBManager.getArtistIdFromDb(track.artist);
                        if (retval != Constants.RETVAL_MESSAGE_DONE)
                        {
                            DBLogger.logWarn(funName + retval);
                            continue;
                        }
                    }
                }
                else
                {
                    DBLogger.logError(funName + Constants.ERROR_NULL_POINTER);
                    continue;
                }

                if (track.album != null)
                {
                    foreach (LfAlbum album in parsedAlbums)
                    {
                        if (newArtist && track.album.name == album.name)
                        {
                            track.album.dbId = album.dbId;
                            newArtist = false;
                        }
                    }
                    if (track.album.dbId == 0)
                    {
                        retval = DBManager.getAlbumsIdFromDb(track.album);
                        if (retval != Constants.RETVAL_MESSAGE_DONE)
                        {
                            DBLogger.logWarn(funName + retval);
                            continue;
                        }
                    }
                }
                else
                {
                    DBLogger.logWarn(funName + Constants.ERROR_NULL_POINTER);
                }

                if (track.id == 0)
                {
                    retval = DBManager.getTrackIdFromDb(track);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        continue;
                    }
                }

                retval = DBManager.update_track_tagsDB(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                List<LfTrack> simTracks = new List<LfTrack>();
                retval = LastfmMethods.getSimilarTracks(Constants.LF_SIMILAR_TRACKS_LIMIT.ToString(), track, false);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = LastfmMethods.getAndUpdateSimilarTracksListInfo(track.similarTrack);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = DBManager.updateTracksMatchDB(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                if (newArtist)
                {
                    updateArtistInfoFromWS(track.artist);
                    parsedArtists.Add(track.artist);
                }

                if (newAlbum)
                {
                    updateAlbumInfoFromWS(track.album);
                    parsedAlbums.Add(track.album);
                }
            }

            return retval;
        }


        private void updateArtistInfoFromWS(LfArtist artist)
        {
            string retval = string.Empty;
            string funName = "updateArtistInfoFromWS(LfArtist artist) - ";

            retval = updateArtistInfo(artist);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
            }

            //return retval;
        }

        private void updateAlbumInfoFromWS(LfAlbum album)
        {
            string retval = string.Empty;
            string funName = "updateAlbumInfoFromWS(LfAlbum album) - ";

            retval = updateAlbumInfo(album);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
            }
            //return retval;
        }


        public string updateAllRs500GaAlbumsInfo()
        {
            string retval = string.Empty;
            string funName = "updateAllRs500GaAlbumsInfo() - ";
            List<LfAlbum> albums = DBManager.getAllAlbumsFromDB();

            foreach (LfAlbum album in albums)
            {
                retval = updateAlbumInfo(album);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                }
            }

            return retval;
        }


        public string updateAlbumInfo(LfAlbum album)
        {
            string retval = string.Empty;
            string funName = "updateAlbumInfo(LfAlbum album) - ";

            retval = LastfmMethods.getAlbumsInfo(album, false, ConfigurationManager.AppSettings[LastfmMethods.lf_Username_tag].ToString(), string.Empty, string.Empty);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
                return funName + retval;
            }

            retval = DBManager.getTagIdsFromDb(album.tags);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
                return funName + retval;
            }

            retval = DBManager.update_album_tagsDB(album);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
                return funName + retval;
            }

            retval = DBManager.getAlbumsIdFromDb(album);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
                return funName + retval;
            }

            if (album.artist != null)
            {
                retval = updateArtistInfo(album.artist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                }
            }

            return retval;
        }

        public string updateArtistInfo(LfArtist artist)
        {
            string retval = string.Empty;
            string funName = "updateAlbumInfo(LfAlbum album) - ";

            retval = LastfmMethods.getArtistInfo(artist, false, ConfigurationManager.AppSettings[LastfmMethods.lf_Username_tag].ToString(), string.Empty, string.Empty, string.Empty);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
                return funName + retval;
            }

            retval = DBManager.getTagIdsFromDb(artist.tags);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
                return funName + retval;
            }

            retval = DBManager.update_artist_tagsDB(artist);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
                return funName + retval;
            }

            retval = DBManager.getArtistIdFromDb(artist);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
                return funName + retval;
            }


            retval = LastfmMethods.getSimilarArtists(artist, Constants.LF_SIMILAR_ARTISTS_LIMIT.ToString(), false, artist.mbid, string.Empty);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
                return funName + retval;
            }

            foreach (LfArtist simArt in artist.similarArtists)
            {
                retval = DBManager.getArtistIdFromDb(simArt);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = DBManager.updateArtistsMatchDB(artist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                }
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="album"></param>
        /// <param name="doAutocorrect"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public string getSimilarArtistsLF(string limit, string artist, bool doAutocorrect, string mbid)
        {
            string retval = string.Empty;
            string funName = "getSimilarArtists(string limit, string artist, bool doAutocorrect, string mbid) - ";
            LfArtist srcArtist = new LfArtist();

            if (!string.IsNullOrEmpty(artist))
                srcArtist.name = artist;

            if (!string.IsNullOrEmpty(mbid))
                srcArtist.mbid = mbid;

            bool skip = false;

            //DataTable dt = meths.getSimilarArtistsLF(limit, tAlbum, doAutocorrect, id, null);
            retval = LastfmMethods.getSimilarArtists(srcArtist, Constants.LF_SIMILAR_ARTISTS_LIMIT.ToString(), false, mbid, string.Empty);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
                skip = true;
            }

            if (!skip)
            {
                retval = DBManager.getArtistIdFromDb(srcArtist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    skip = true;
                }
            }

            if (!skip)
            {
                retval = DBManager.updateArtistsMatchDB(srcArtist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    skip = true;
                }
            }


            //retval = getAristsTopAlbums(srcArtist);
            //if (retval != Constants.RETVAL_MESSAGE_DONE)
            //    return funName + retval;

            if (!skip)
            {
                retval = LastfmMethods.getArtistTopTracks(srcArtist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    skip = true;
                }
            }

            if (srcArtist.similarArtists.Count > 0)
            {
                foreach (LfArtist simArt in srcArtist.similarArtists)
                {
                    retval = DBManager.getArtistIdFromDb(simArt);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        continue;
                    }


                    retval = LastfmMethods.getSimilarArtists(simArt, Constants.LF_SIMILAR_ARTISTS_LIMIT.ToString(), false, mbid, string.Empty);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        continue;
                    }

                    retval = DBManager.updateArtistsMatchDB(simArt);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        continue;
                    }

                    //retval = getAristsTopAlbums(simArt);
                    //if (retval != Constants.RETVAL_MESSAGE_DONE)
                    //    return funName + retval;

                    retval = LastfmMethods.getArtistTopTracks(simArt);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        continue;
                    }

                    //###retval = getSimilarTracks()
                }
                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            //retval = dt.Rows.Count.ToString();

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlcontent"></param>
        /// <returns></returns>
        public string generatePlaylist(string xmlcontent)
        {
            string retval = string.Empty;
            string funName = "generatePlaylist(string xmlcontent) - ";
            if (string.IsNullOrEmpty(xmlcontent))
                return funName + string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, "xmlcontent", "Empty content is passed as input");

            if (string.IsNullOrEmpty(xmlcontent))
                return funName + string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, "xmlcontent", "Empty content is passed as input");

            Playlist playlist;// = new Playlist();
            retval = parseXMLContent(xmlcontent, out playlist);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                string retv = funName + retval;
                DBLogger.logError(retv);
            }

            retval = updatePlaylistTracksAndArtistInfo(playlist);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                string retv = funName + retval;
                DBLogger.logError(retv);
            }

            retval = DBManager.createPlaylistAndAddSourceTracks(playlist);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                string retv = funName + retval;
                DBLogger.logError(retv);
            }

            // search for candidate artists
            retval = DBManager.searchForCandidateArtists(playlist);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                string retv = funName + retval;
                DBLogger.logError(retv);
            }

            // 1
            
            // print output
            switch (playlist.type.type)
            {
                case RS500gaWCFService.RSPlaylistTypeNS.RSPlaylistType.PlType.SPOTIFY:
                    retval = generateLastfmPl(playlist); break;
                case RS500gaWCFService.RSPlaylistTypeNS.RSPlaylistType.PlType.LASTFM:
                    retval = generateSpotifyPl(playlist); break;
                case RS500gaWCFService.RSPlaylistTypeNS.RSPlaylistType.PlType.XSPF:
                    retval = generateXspfPl(playlist); break;
                case RS500gaWCFService.RSPlaylistTypeNS.RSPlaylistType.PlType.M3U:
                    retval = generateM3UPl(playlist); break;
                default:
                    {
                        // do nothing or generate 

                    }
                    break;
            }
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                retval = funName + retval;
                DBLogger.logError(retval);
            }

            return retval;
        }

        public static string parseXMLContent(string xmlcontent, out Playlist playlist)
        {
            playlist = new Playlist();

            string retval = string.Empty;
            string funName = "parseXMLContent(string xmlcontent, out RSPlaylist playlist) - ";

            //if (playlist == null)
            //    playlist = new Playlist();

            if (string.IsNullOrEmpty(xmlcontent))
            {
                retval = funName + string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, "xmlcontent", "Empty content is passed as input");
                DBLogger.logWarn(retval);
                return retval;
            }


            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_PLAYLIST_GEN_INPUT_XSDFILE].ToString();

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                if (!string.IsNullOrEmpty(xmlcontent))
                {
                    StringReader sr = new StringReader(xmlcontent);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();

                    string nodePos = Constants.XSD_TAG_PLI_RS500GA;// +
                    //Constants.XSD_TAG_SEPARATOR +
                    //Constants.XSD_TAG_PLI_PLAYLIST_INPUT;

                    //Playlist playlist = new Playlist();
                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.MoveNext())
                    {
                        XPathNavigator locNode = xpRootNodeIter.Current;

                        locNodeIter = locNode.Select(Constants.XSD_TAG_PLI_PLAYLIST_INPUT);
                        if (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;
                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_PLI_TITLE);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_PLI_TITLE)
                                playlist.title = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_PLI_DESCRIPTION);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_PLI_DESCRIPTION)
                                playlist.description = newphewNodeIter.Current.Value;


                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_PLI_OUTPUT + Constants.XSD_TAG_PLI_TYPE);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_PLI_TYPE)
                                playlist.type = new RSPlaylistType(newphewNodeIter.Current.Value);

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_PLI_OUTPUT + Constants.XSD_TAG_PLI_TRACKS_NR);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_PLI_TRACKS_NR)
                                playlist.lim_track_nr = Int32.Parse(newphewNodeIter.Current.Value);

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_PLI_OUTPUT + Constants.XSD_TAG_PLI_DURATION);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_PLI_DURATION)
                                playlist.lim_duration = Int32.Parse(newphewNodeIter.Current.Value);
                        }
                        XPathNavigator tmpkeyNode = locNodeIter.Current;
                        XPathNodeIterator tmpNodeIter = tmpkeyNode.Select(Constants.XSD_TAG_PLI_ARTISTS);
                        if (tmpNodeIter.MoveNext())
                        {

                            locNodeIter = locNode.SelectDescendants(Constants.XSD_TAG_PLI_ARTIST, "", true);
                            while (locNodeIter.MoveNext())
                            {
                                XPathNavigator keyNode = locNodeIter.Current;
                                LfArtist artist = new LfArtist();

                                newphewNodeIter = keyNode.Select(Constants.XSD_TAG_PLI_NAME);
                                if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                    artist.name = newphewNodeIter.Current.Value;

                                playlist.addArtist(artist);
                            }
                        }

                        tmpkeyNode = locNodeIter.Current;
                        tmpNodeIter = tmpkeyNode.Select(Constants.XSD_TAG_PLI_TRACKS);
                        if (tmpNodeIter.MoveNext())
                        {
                            locNodeIter = locNode.SelectDescendants(Constants.XSD_TAG_PLI_TRACK, "", true);
                            while (locNodeIter.MoveNext())
                            {
                                XPathNavigator keyNode = locNodeIter.Current;
                                LfTrack track = new LfTrack();

                                newphewNodeIter = keyNode.Select(Constants.XSD_TAG_PLI_TITLE);
                                if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_TITLE)
                                    track.name = newphewNodeIter.Current.Value;

                                newphewNodeIter = keyNode.Select(Constants.XSD_TAG_PLI_ARTIST);
                                if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_ARTIST)
                                    track.artistName = newphewNodeIter.Current.Value;

                                playlist.addTrack(track);
                            }

                            retval = Constants.RETVAL_MESSAGE_DONE;
                        }
                        else
                        {
                            //retval = funName + string.Format(Constants.ERROR_PARSING_XML_FSTR, nodePos, "");
                            DBLogger.logWarn(retval);
                        }
                    }
                    else
                    {
                        retval = funName + string.Format(Constants.ERROR_PARSING_XML_FSTR, nodePos, "");
                        DBLogger.logWarn(retval);
                    }


                }
                else
                {
                    retval = funName + Constants.ERROR_NULL_POINTER;
                    DBLogger.logWarn(retval);

                }
            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
                retval = funName + ex.Message;
                DBLogger.logWarn(retval);
            }

            return retval;
        }


        public string updatePlaylistTracksAndArtistInfo(Playlist playlist)
        {

            string retval = string.Empty;
            string funName = "updateTracksAndArtistInfo(Playlist playlist) - ";
            string username = ConfigurationManager.AppSettings[LastfmMethods.lf_Username_tag].ToString();

            foreach (LfTrack track in playlist.tracks)
            {
                retval = LastfmMethods.getTrackInfo(track, false, username, string.Empty, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }

                if (track.id == 0)
                {
                    retval = DBManager.getTrackIdFromDb(track);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logError(funName + retval);
                        continue;
                    }
                }

                retval = DBManager.update_track_tagsDB(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }

                retval = LastfmMethods.getSimilarTracks(Constants.LF_SIMILAR_TRACKS_LIMIT.ToString(), track, false);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }
            }

            foreach (LfArtist artist in playlist.artists)
            {
                retval = LastfmMethods.getArtistInfo(artist, true, username, string.Empty, artist.mbid, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }

                if (artist.id == 0)
                {
                    retval = DBManager.getArtistIdFromDb(artist);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logError(funName + retval);
                        continue;
                    }
                }


                retval = DBManager.update_artist_tagsDB(artist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }

                retval = LastfmMethods.getArtistTopTracks(artist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }

                foreach (LfTrack artTrack in artist.topTracks)
                {
                    retval = DBManager.getTrackIdFromDb(artTrack);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logError(funName + retval);
                        continue;
                    }
                }
            }

            return retval;
        }


        public string generateLastfmPl(Playlist playlist)
        {
            string retval = string.Empty;
            string funName = "generateLastfmPl(Playlist playlist) - ";

            //retval = LastfmMethods.playlistCreate(playlist);
            //if (retval != Constants.RETVAL_MESSAGE_DONE)
            //    return funName + retval;

            //retval = LastfmMethods.searchCandidateTracks(playlist);
            //if (retval != Constants.RETVAL_MESSAGE_DONE)
            //    return funName + retval;

            return retval;
        }

        public string generateSpotifyPl(Playlist playlist)
        {
            string retval = string.Empty;
            string funName = "generateSpotifyPl(Playlist playlist) - ";

            //retval = LastfmMethods.playlistCreate(playlist);
            //if (retval != Constants.RETVAL_MESSAGE_DONE)
            //    return funName + retval;

            //retval = LastfmMethods.searchCandidateTracks(playlist);
            //if (retval != Constants.RETVAL_MESSAGE_DONE)
            //    return funName + retval;

            return retval;
        }

        public string generateXspfPl(Playlist playlist)
        {
            string retval = string.Empty;
            string funName = "generateXspfPl(Playlist playlist) - ";

            //retval = LastfmMethods.playlistCreate(playlist);
            //if (retval != Constants.RETVAL_MESSAGE_DONE)
            //    return funName + retval;

            //retval = LastfmMethods.searchCandidateTracks(playlist);
            //if (retval != Constants.RETVAL_MESSAGE_DONE)
            //    return funName + retval;

            return retval;
        }

        public string generateM3UPl(Playlist playlist)
        {
            string retval = string.Empty;
            string funName = "generateM3UPl(Playlist playlist) - ";

            //retval = LastfmMethods.playlistCreate(playlist);
            //if (retval != Constants.RETVAL_MESSAGE_DONE)
            //    return funName + retval;

            //retval = LastfmMethods.searchCandidateTracks(playlist);
            //if (retval != Constants.RETVAL_MESSAGE_DONE)
            //    return funName + retval;

            return retval;
        }

        public string generatDefaultPl(Playlist playlist)
        {
            string retval = string.Empty;
            string funName = "generatDefaultPl(Playlist playlist) - ";

            //retval = LastfmMethods.playlistCreate(playlist);
            //if (retval != Constants.RETVAL_MESSAGE_DONE)
            //    return funName + retval;

            //retval = LastfmMethods.searchCandidateTracks(playlist);
            //if (retval != Constants.RETVAL_MESSAGE_DONE)
            //    return funName + retval;

            return retval;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="artistName"></param>
        /// <param name="mbid"></param>
        /// <param name="doAutocorrect"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public string getSimilarTracksLF(string track, string artistName, string mbid, bool doAutocorrect, string limit)
        {
            string retval = null;
            string funName = "getSimilarTracksLF(string limit, string track, bool doAutocorrect, string mbid) - ";
            LfTrack srcTrack = new LfTrack();

            if (!string.IsNullOrEmpty(track))
                srcTrack.name = track;

            if (!string.IsNullOrEmpty(artistName))
                srcTrack.artistName = artistName;

            if (!string.IsNullOrEmpty(mbid))
                srcTrack.mbid = mbid;

            //LastfmMethods meths = new LastfmMethods();
            if (string.IsNullOrEmpty(srcTrack.artistName))
                srcTrack.artistName = artistName;
            retval = LastfmMethods.getTrackInfo(srcTrack, false, string.Empty, string.Empty, string.Empty);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
            }

            if (srcTrack.artist != null)
            {
                retval = DBManager.getArtistIdFromDb(srcTrack.artist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                }
            }
            else
            {
                DBLogger.logWarn(funName + Constants.RETVAL_MESSAGE_ERROR);
            }
            //DataTable dt = meths.getSimilarArtistsLF(limit, tAlbum, doAutocorrect, id, null);
            bool skip = false;
            retval = LastfmMethods.getSimilarTrack(srcTrack, Constants.LF_SIMILAR_TRACKS_LIMIT.ToString(), track, artistName, false, mbid, string.Empty);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                DBLogger.logWarn(funName + retval);
                skip = true;
            }

            if (!skip && srcTrack.artist != null)
            {
                if (srcTrack.album != null)
                {
                    if (srcTrack.album.artist != null)
                    {
                        srcTrack.album.artist.id = srcTrack.artist.id;
                    }
                    else
                    {

                    }
                    retval = DBManager.getAlbumsIdFromDb(srcTrack.album);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                    }
                }
                else
                {
                    DBLogger.logWarn(funName + Constants.ERROR_NULL_POINTER);
                }

                retval = DBManager.getTrackIdFromDb(srcTrack);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    skip = true;
                }

                if (!skip)
                {
                    retval = DBManager.getTagIdsFromDb(srcTrack.tags);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        skip = true;
                    }
                }

                if (!skip)
                {
                    retval = DBManager.update_track_tagsDB(srcTrack);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        skip = true;
                    }
                }

                if (!skip)
                {
                    foreach (LfTrack trck in srcTrack.similarTrack)
                    {
                        if (trck.artist != null)
                        {
                            retval = LastfmMethods.getTrackInfo(trck, false, string.Empty, string.Empty, string.Empty);
                            if (retval != Constants.RETVAL_MESSAGE_DONE)
                            {
                                DBLogger.logWarn(funName + retval);
                                continue;
                            }

                            if (trck.artist.id <= 0)
                            {
                                retval = DBManager.getArtistIdFromDb(trck.artist);
                                if (retval != Constants.RETVAL_MESSAGE_DONE)
                                {
                                    DBLogger.logWarn(funName + retval);
                                    continue;
                                }
                            }
                            else
                            {
                                return funName + Constants.ERROR_NULL_POINTER;
                            }

                            if (trck.album != null && trck.album.dbId <= 0)
                            {
                                retval = DBManager.getAlbumsIdFromDb(trck.album);
                                if (retval != Constants.RETVAL_MESSAGE_DONE)
                                    return funName + retval;
                            }
                            else
                            {
                                return funName + Constants.ERROR_NULL_POINTER;
                            }
                            retval = LastfmMethods.getSimilarTrack(trck, Constants.LF_SIMILAR_TRACKS_LIMIT.ToString(), track, artistName, false, mbid, string.Empty);
                            if (retval != Constants.RETVAL_MESSAGE_DONE)
                            {
                                DBLogger.logWarn(funName + retval);
                                continue;
                            }

                            retval = DBManager.getTrackIdFromDb(trck);
                            if (retval != Constants.RETVAL_MESSAGE_DONE)
                            {
                                DBLogger.logWarn(funName + retval);
                                continue;
                            }

                            retval = DBManager.getTagIdsFromDb(trck.tags);
                            if (retval != Constants.RETVAL_MESSAGE_DONE)
                            {
                                DBLogger.logWarn(funName + retval);
                                continue;
                            }

                            retval = DBManager.update_track_tagsDB(trck);
                            if (retval != Constants.RETVAL_MESSAGE_DONE)
                            {
                                DBLogger.logWarn(funName + retval);
                                continue;
                            }

                        }
                    }
                }

                retval = Constants.RETVAL_MESSAGE_DONE;
            }
            else
            {
                retval = funName + Constants.RETVAL_MESSAGE_ERROR;
                DBLogger.logWarn(retval);
            }


            return retval;
        }


        public string createPlaylistLF(string title, string description)
        {
            string retval = null;
            string funName = "createPlaylistLF(string title, string description) - ";

            retval = LastfmMethods.createPlaylist(title, description);
            if (retval == Constants.RETVAL_MESSAGE_ERROR)
            {
                DBLogger.logWarn(funName + retval);
            }
            return retval;
        }

    }
}
