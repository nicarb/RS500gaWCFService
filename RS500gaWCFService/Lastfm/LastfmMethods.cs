using log4net;
using RS500gaWCFService.data;
using RS500gaWCFService.db;
using RS500gaWCFService.Logging;
using RS500gaWCFService.rs500ga;
using RS500gaWCFService.RSPlaylistTypeNS;
using RS500gaWCFService.utils;
//using Lastfm.Services;
//using Lpfm.LastFmScrobbler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;


namespace RS500gaWCFService.lastfm
{
    public class LastfmMethods
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string LASTFM_BASE_API_URL = @"http://ws.audioscrobbler.com/2.0/?api_key=";
        private static string LASTFM_API_URL = @"http://ws.audioscrobbler.com/2.0/";
        private static string LASTFM_BASE_AUTH_API_URL = @"http://www.last.fm/api/auth/?api_key=";

        private const string LASTFM_METHOD_TAG = "method";
        private const string LASTFM_METHOD_ARTIST_GET_SIMILAR = "artist.getSimilar";
        private const string LASTFM_METHOD_ARTIST_GET_TOP_ALBUMS = "artist.gettopalbums";
        private const string LASTFM_METHOD_ARTIST_GET_INFO = "artist.getinfo";
        private const string LASTFM_METHOD_ARTIST_GET_CORRECTION = "artist.getcorrection";
        private const string LASTFM_METHOD_ARTIST_GET_TOP_TRACKS = "artist.gettoptracks";
        private const string LASTFM_METHOD_ARTIST_SEARCH = "artist.search";
        private const string LASTFM_METHOD_ARTIST_GET_TOPTAGS = "artist.gettoptags";
        private const string LASTFM_METHOD_ARTIST_GET_TAGS = "artist.gettags";

        private const string LASTFM_METHOD_ALBUM_GET_INFO = "album.getinfo";

        private const string LASTFM_METHOD_TRACK_GET_INFO = "track.getInfo";
        private const string LASTFM_METHOD_TRACK_GET_SIMILAR = "track.getSimilar";

        private const string LASTFM_METHOD_AUTH_GET_TOKEN = "auth.getToken";
        private const string LASTFM_METHOD_AUTH_GET_SESSION = "auth.getSession";

        private const string LASTFM_METHOD_TESTOMETER_COMPARE = "tasteometer.compare";

        private const string LASTFM_METHOD_PLAYLIST_CREATE = "playlist.create";

        private const string LASTFM_METHOD_TAG_GET_SIMILAR = "tag.getSimilar";
        private const string LASTFM_METHOD_TAG_GET_TOP_ARTISTS = "tag.getTopArtists";
        private const string LASTFM_METHOD_TAG_GET_TOP_TRACKS = "tag.getTopTracks";


        private const string LASTFM_PARAM_LIMIT_TAG = "limit";
        private const string LASTFM_PARAM_PAGE_TAG = "page";
        private const string LASTFM_PARAM_ARTIST_TAG = "artist";
        private const string LASTFM_PARAM_ALBUM_TAG = "album";
        private const string LASTFM_PARAM_AUTOCORRECT_TAG = "autocorrect";
        private const string LASTFM_PARAM_MBID_TAG = "mbid";
        private const string LASTFM_PARAM_API_KEY_TAG = "api_key";
        private const string LASTFM_PARAM_USERNAME_TAG = "username";
        private const string LASTFM_PARAM_LANG_TAG = "lang";
        private const string LASTFM_PARAM_TRACK_TAG = "track";
        private const string LASTFM_PARAM_API_SIG_TAG = "api_sig";
        private const string LASTFM_PARAM_TOKEN_TAG = "token";
        private const string LASTFM_PARAM_DESCRIPTION_TAG = "description";
        private const string LASTFM_PARAM_SK_TAG = "sk";
        private const string LASTFM_PARAM_TITLE_TAG = "title";

        private const int LASTFM_ARTIST_SIMILAR_LIM = 5;
        public const string LF_ApiKey_tag = "LF_ApiKey";
        public const string LF_ApiSecret_tag = "LF_ApiSecret";
        private const string LF_SessionKey_tag = "LF_SessionKey";
        public const string lf_Username_tag = "lf_Username";
        public const string WARN_MISMATCH_PARAM_XML_FSTR = "WARNING: {0} - mismatch between artist research: parameter= {1}; xml_response={2}";
        public const string WARN_NODE_IS_MISSING_FSTR = "WARNING: {0} - Specified {1} node is missing";

        private string LF_ApiKey_val = string.Empty;
        private string LF_ApiSecret_val = string.Empty;
        private string LF_Token_val = string.Empty;
        private string LF_SessionKey_val = string.Empty;
        private string LF_SessionName_val = string.Empty;

        public LastfmMethods()
        {
            this.LF_ApiKey_val = ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString();
            this.LF_ApiSecret_val = ConfigurationManager.AppSettings[LF_ApiSecret_tag].ToString();
            generateAuthenticationToken();
            //generateAuthorisation();
            //generateAuthorisation();
            //generateSessionKey();
        }

        private void generateAuthenticationToken()
        {
            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_AUTH_GET_TOKEN_XSDFILE].ToString();

            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_AUTH_GET_TOKEN;

            string signatureStr = LASTFM_PARAM_API_KEY_TAG + ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                 LASTFM_METHOD_TAG + LASTFM_METHOD_AUTH_GET_TOKEN +
                                 this.LF_ApiSecret_val;

            string signature_hash = Utils.encodeMD5(signatureStr);

            requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_API_SIG_TAG) + signature_hash;

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);
                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {
                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                        log.Error(code);
                        log.Error(message);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_TOKEN;

                        xpRootNodeIter = xpNav.Select(nodePos);

                        if (xpRootNodeIter.MoveNext())
                        {
                            this.LF_Token_val = xpRootNodeIter.Current.Value;
                        }
                    }
                }
                else
                {
                    string err = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                    log.Error(err);
                }
            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
            }


            //APP_SETTINGS_LF_AUTH_GET_TOKEN_XSDFILE
        }

        private void generateAuthorisation()
        {
            string username = ConfigurationManager.AppSettings[lf_Username_tag].ToString();
            string requestUrl = LASTFM_BASE_AUTH_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                //string.Format(WEB_URL_PARAM_FSTR, "user") + username +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_TOKEN_TAG) +
                                this.LF_Token_val;

            string serviceResponse = NetUtils.GetServiceResponse(requestUrl);
        }

        private void generateSessionKey()
        {
            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_AUTH_GET_SESSION_XSDFILE].ToString();

            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_AUTH_GET_SESSION +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_TOKEN_TAG) +
                                this.LF_Token_val;

            string signatureStr = LASTFM_PARAM_API_KEY_TAG + ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                  LASTFM_METHOD_TAG + LASTFM_METHOD_AUTH_GET_SESSION +
                                  LASTFM_PARAM_TOKEN_TAG + this.LF_Token_val +
                                  this.LF_ApiSecret_val;

            string signature_hash = Utils.encodeMD5(signatureStr);

            requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_API_SIG_TAG) + signature_hash;

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);
                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {
                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                        log.Error(code);
                        log.Error(message);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_SESSION;

                        xpRootNodeIter = xpNav.Select(nodePos + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_NAME);
                        if (xpRootNodeIter.MoveNext() && xpRootNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                        {
                            this.LF_SessionName_val = xpRootNodeIter.Current.Value;
                        }

                        xpRootNodeIter = xpNav.Select(nodePos + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_KEY);
                        if (xpRootNodeIter.MoveNext() && xpRootNodeIter.Current.Name == Constants.XSD_TAG_LF_KEY)
                        {
                            this.LF_SessionKey_val = xpRootNodeIter.Current.Value;
                        }
                    }
                }
                else
                {
                    string err = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                    log.Error(err);
                }
            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
            }


            //APP_SETTINGS_LF_AUTH_GET_TOKEN_XSDFILE
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="limit">(Optional) : Limit the number of similar artists returned</param>
        /// <param name="tAlbum">(Required (unless id)] : The tAlbum name</param>
        /// <param name="doAutocorrect">[0|1] (Optional) : Transform misspelled tAlbum names into correct tAlbum names, returning the correct version instead. The corrected tAlbum name will be returned in the response.</param>
        /// <param name="id">(Optional) : The musicbrainz id for the tAlbum</param>
        /// <returns></returns>
        public static DataTable getSimilarArtists(string limit, string artistName, bool doAutocorrect, string mbid, string key)
        {
            if (string.IsNullOrEmpty(artistName) && string.IsNullOrEmpty(mbid))
            {
                throw new Exception("Artist name must be specified.");
            }

            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;
            DataTable similarArtists = new DataTable();

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_SIMILAR_ARTIST_XSDFILE].ToString();


            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_ARTIST_GET_SIMILAR;

            if (!string.IsNullOrEmpty(mbid))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_MBID_TAG) +
                                System.Web.HttpUtility.UrlEncode(mbid.Trim());
            }
            else if (!string.IsNullOrEmpty(artistName))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ARTIST_TAG) +
                                System.Web.HttpUtility.UrlEncode(artistName.Trim());
            }



            if (!string.IsNullOrEmpty(limit))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_LIMIT_TAG) +
                                System.Web.HttpUtility.UrlEncode(limit.Trim());
            }

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);
                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {
                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                        log.Error(code);
                        log.Error(message);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_SIMILARARTISTS;

                        xpRootNodeIter = xpNav.Select(nodePos);

                        xpRootNodeIter.MoveNext();
                        XPathNavigator node = xpRootNodeIter.Current;
                        locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_ARTIST, "", true);

                        similarArtists.Columns.Add(Constants.XSD_TAG_LF_NAME);
                        similarArtists.Columns.Add(Constants.XSD_TAG_LF_MBID);
                        similarArtists.Columns.Add(Constants.XSD_TAG_LF_URL);
                        similarArtists.Columns.Add(Constants.XSD_TAG_LF_MATCH, System.Type.GetType("System.Double"));
                        similarArtists.Columns.Add(Constants.XSD_TAG_LF_ID);

                        DataRow albumsRow = similarArtists.NewRow();

                        albumsRow[Constants.XSD_TAG_LF_NAME] = artistName;
                        albumsRow[Constants.XSD_TAG_LF_MBID] = "";
                        albumsRow[Constants.XSD_TAG_LF_URL] = "";
                        albumsRow[Constants.XSD_TAG_LF_MATCH] = 1;

                        similarArtists.Rows.Add(albumsRow);

                        while (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;

                            albumsRow = similarArtists.NewRow();

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                albumsRow[Constants.XSD_TAG_LF_NAME] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_MBID);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                                albumsRow[Constants.XSD_TAG_LF_MBID] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                albumsRow[Constants.XSD_TAG_LF_URL] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_MATCH);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MATCH)
                                albumsRow[Constants.XSD_TAG_LF_MATCH] = newphewNodeIter.Current.ValueAsDouble;

                            similarArtists.Rows.Add(albumsRow);
                        }
                    }
                }
                else
                {
                    log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                    DBLogger.logWarn(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                }
            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
                DBLogger.logError(ex.Message);
            }


            return similarArtists;
        }

        public static DataTable getSimilarArtistsDt(string limit, string artist, bool doAutocorrect, string mbid)
        {
            string retval = null;
            string funName = "getSimilarArtistsDt(string limit, string artist, bool doAutocorrect, string mbid) - ";
            DataTable dt = getSimilarArtists(limit, artist, doAutocorrect, mbid, null);
            Dictionary<int, double> artistIDs;
            if (dt.Rows.Count > 0)
            {
                artistIDs = DBManager.getArtistsIdsFromDb(dt);

                retval = DBManager.updateArtistsMatchDB(artistIDs);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                }
                //throw new Exception(funName + retval);
            }
            //retval = dt.Rows.Count.ToString();

            return dt;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tAlbum"></param>
        /// <param name="limit"></param>
        /// <param name="trackName"></param>
        /// <param name="doAutocorrect"></param>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string getSimilarArtists(LfArtist artist, string limit, bool doAutocorrect, string mbid, string key)
        {
            string retval = string.Empty;
            string funName = "getSimilarArtists(LfArtist artist, string limit, string artistName, bool doAutocorrect, string mbid, string key) - ";
            string artistName = artist.name;
            if (string.IsNullOrEmpty(artistName) && string.IsNullOrEmpty(mbid))
            {
                retval = funName + "Artist name must be specified.";
                DBLogger.logWarn(retval);
                return retval;
            }

            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;
            //DataTable similarArtists = new DataTable();

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_SIMILAR_ARTIST_XSDFILE].ToString();


            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_ARTIST_GET_SIMILAR;

            if (!string.IsNullOrEmpty(mbid))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_MBID_TAG) +
                                System.Web.HttpUtility.UrlEncode(mbid.Trim());

                if (string.IsNullOrEmpty(artist.mbid))
                {
                    artist.mbid = mbid;
                }
            }
            else if (!string.IsNullOrEmpty(artistName))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ARTIST_TAG) +
                                System.Web.HttpUtility.UrlEncode(artistName.Trim());

                if (string.IsNullOrEmpty(artist.name))
                {
                    artist.name = artistName;
                }
            }

            if (!string.IsNullOrEmpty(limit))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_LIMIT_TAG) +
                                System.Web.HttpUtility.UrlEncode(limit.Trim());
            }

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);
                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {
                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                        log.Error(code);
                        log.Error(message);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_SIMILARARTISTS;

                        xpRootNodeIter = xpNav.Select(nodePos);

                        xpRootNodeIter.MoveNext();
                        XPathNavigator node = xpRootNodeIter.Current;
                        locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_ARTIST, "", true);

                        //similarArtists.Columns.Add(Constants.XSD_TAG_LF_NAME);
                        //similarArtists.Columns.Add(Constants.XSD_TAG_LF_MBID);
                        //similarArtists.Columns.Add(Constants.XSD_TAG_LF_URL);
                        //similarArtists.Columns.Add(Constants.XSD_TAG_LF_MATCH, System.Type.GetType("System.Double"));
                        //similarArtists.Columns.Add(Constants.XSD_TAG_LF_ID);

                        //DataRow albumsRow = similarArtists.NewRow();

                        //albumsRow[Constants.XSD_TAG_LF_NAME] = trackName;
                        //albumsRow[Constants.XSD_TAG_LF_MBID] = "";
                        //albumsRow[Constants.XSD_TAG_LF_URL] = "";
                        //albumsRow[Constants.XSD_TAG_LF_MATCH] = 1;

                        //similarArtists.Rows.Add(albumsRow);


                        while (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;

                            //albumsRow = similarArtists.NewRow();
                            LfArtist simArtist = new LfArtist();
                            simArtist.parent_name = artistName;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                simArtist.name = newphewNodeIter.Current.Value;
                            //albumsRow[Constants.XSD_TAG_LF_NAME] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_MBID);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                                simArtist.mbid = newphewNodeIter.Current.Value;
                            //albumsRow[Constants.XSD_TAG_LF_MBID] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                simArtist.url = newphewNodeIter.Current.Value;
                            //albumsRow[Constants.XSD_TAG_LF_URL] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_MATCH);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MATCH)
                                simArtist.parent_match = newphewNodeIter.Current.ValueAsDouble;
                            //albumsRow[Constants.XSD_TAG_LF_MATCH] = newphewNodeIter.Current.ValueAsDouble;

                            //similarArtists.Rows.Add(albumsRow);
                            artist.addSimilarArtists(simArtist);

                        }

                        retval = Constants.RETVAL_MESSAGE_DONE;
                    }
                }
                else
                {
                    string err = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                    log.Error(err);
                    retval = funName + err;
                    DBLogger.logError(retval);
                }
            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
                retval = funName + Constants.ERROR_LOADING + ex.Message;
                DBLogger.logError(retval);
            }


            return retval;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="tAlbum"></param>
        /// <param name="limit"></param>
        /// <param name="trackName"></param>
        /// <param name="artistName"></param>
        /// <param name="doAutocorrect"></param>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string getSimilarTrack(LfTrack track, string limit, string trackName, string artistName, bool doAutocorrect, string mbid, string key)
        {
            string retval = string.Empty;
            string funName = "getSimilarTrack(LfTrack track, string limit, string trackName, bool doAutocorrect, string mbid, string key) - ";
            if (string.IsNullOrEmpty(trackName) && string.IsNullOrEmpty(mbid))
            {
                retval = funName + "Track name must be specified.";
                DBLogger.logError(retval);
                return retval;
            }

            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;
            //DataTable similarArtists = new DataTable();

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_SIMILAR_TRACK_XSDFILE].ToString();


            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_TRACK_GET_SIMILAR;

            if (!string.IsNullOrEmpty(mbid))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_MBID_TAG) +
                                System.Web.HttpUtility.UrlEncode(mbid.Trim());

                if (string.IsNullOrEmpty(track.mbid))
                {
                    track.mbid = mbid;
                }
            }
            else if (!string.IsNullOrEmpty(artistName) && !string.IsNullOrEmpty(trackName))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ARTIST_TAG) +
                                System.Web.HttpUtility.UrlEncode(artistName.Trim()) +
                              string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_TRACK_TAG) +
                                System.Web.HttpUtility.UrlEncode(trackName.Trim());

                if (string.IsNullOrEmpty(track.name))
                {
                    track.name = trackName;
                }
                if (string.IsNullOrEmpty(track.artistName))
                {
                    track.artistName = artistName;
                }
            }

            if (!string.IsNullOrEmpty(limit))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_LIMIT_TAG) +
                                System.Web.HttpUtility.UrlEncode(limit.Trim());
            }

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);
                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {
                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                        log.Error(code);
                        log.Error(message);
                        DBLogger.logError(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl) + " # " + code + " # " + message);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_SIMILARTRACKS;

                        xpRootNodeIter = xpNav.Select(nodePos);

                        xpRootNodeIter.MoveNext();
                        XPathNavigator node = xpRootNodeIter.Current;
                        locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_TRACK, "", true);

                        //similarArtists.Columns.Add(Constants.XSD_TAG_LF_NAME);
                        //similarArtists.Columns.Add(Constants.XSD_TAG_LF_MBID);
                        //similarArtists.Columns.Add(Constants.XSD_TAG_LF_URL);
                        //similarArtists.Columns.Add(Constants.XSD_TAG_LF_MATCH, System.Type.GetType("System.Double"));
                        //similarArtists.Columns.Add(Constants.XSD_TAG_LF_ID);

                        //DataRow albumsRow = similarArtists.NewRow();

                        //albumsRow[Constants.XSD_TAG_LF_NAME] = trackName;
                        //albumsRow[Constants.XSD_TAG_LF_MBID] = "";
                        //albumsRow[Constants.XSD_TAG_LF_URL] = "";
                        //albumsRow[Constants.XSD_TAG_LF_MATCH] = 1;

                        //similarArtists.Rows.Add(albumsRow);


                        while (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;

                            //albumsRow = similarArtists.NewRow();
                            LfTrack simTrack = new LfTrack();
                            simTrack.parent_name = trackName;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                simTrack.name = newphewNodeIter.Current.Value;
                            //albumsRow[Constants.XSD_TAG_LF_NAME] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_PLAYCOUNT);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_PLAYCOUNT)
                                simTrack.playcount = newphewNodeIter.Current.ValueAsInt;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_MBID);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                                simTrack.mbid = newphewNodeIter.Current.Value;
                            //albumsRow[Constants.XSD_TAG_LF_MBID] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_MATCH);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MATCH)
                                simTrack.parent_match = newphewNodeIter.Current.ValueAsDouble;
                            //albumsRow[Constants.XSD_TAG_LF_MATCH] = newphewNodeIter.Current.ValueAsDouble;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                simTrack.url = newphewNodeIter.Current.Value;
                            //albumsRow[Constants.XSD_TAG_LF_URL] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_DURATION);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_DURATION)
                                simTrack.duration = newphewNodeIter.Current.ValueAsInt;
                            //similarArtists.Rows.Add(albumsRow);

                            LfArtist artist = new LfArtist();
                            // TODO: implement album 
                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_ARTIST + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                artist.name = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_ARTIST + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_MBID);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                                artist.mbid = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_ARTIST + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                artist.url = newphewNodeIter.Current.Value;

                            simTrack.artist = artist;

                            track.addSimilarTrack(simTrack);

                        }

                        retval = Constants.RETVAL_MESSAGE_DONE;
                    }
                }
                else
                {
                    string err = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                    log.Error(err);
                    retval = funName + err;
                    DBLogger.logError(retval);
                }
            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
                retval = funName + Constants.ERROR_LOADING + ex.Message;
                DBLogger.logError(ex.Message);
            }


            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public static string playlistCreate(Playlist playlist)
        {
            string retval = string.Empty;
            string funName = "playlistCreate(RSPlaylist playlist) - ";
            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_PLAYLIST_CREATE_XSDFILE].ToString();

            string title = string.Empty;
            string description = string.Empty;

            if (playlist != null)
            {
                if (!string.IsNullOrEmpty(playlist.title))
                {
                    title = playlist.title;
                    description = playlist.description;

                    string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                    XmlReaderSettings vr = new XmlReaderSettings();
                    vr.Schemas.Add(string.Empty, xsdfilePath);

                    string serviceResponse = createPlaylist(title, description);

                    if (!string.IsNullOrEmpty(serviceResponse) || serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                    {
                        StringReader sr = new StringReader(serviceResponse);
                        //var xmlResponse = XElement.Parse(serviceResponse);
                        xReader = XmlReader.Create(sr, vr);

                        xpDoc = new XPathDocument(xReader);
                        xpNav = xpDoc.CreateNavigator();
                        string nodePos = Constants.XSD_TAG_LF_LFM;

                        string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                               Constants.XSD_TAG_SEPARATOR +
                                               Constants.XSD_TAG_LF_ERROR;

                        xpRootNodeIter = xpNav.Select(nodePos);

                        if (xpRootNodeIter.Current.HasAttributes &&
                            xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                        {
                            xpRootNodeIter = xpNav.Select(errorNodePos);
                            string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                            string message = xpRootNodeIter.Current.Value.Trim();
                            log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, LASTFM_API_URL + "-playlist.create(" + title + ")"));
                            log.Error(code);
                            log.Error(message);
                            DBLogger.logError(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, LASTFM_API_URL + "-playlist.create(" + title + ")") + " # " + code + " # " + message);
                        }
                        else
                        {
                            nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_PLAYLISTS;

                            xpRootNodeIter = xpNav.Select(nodePos);

                            if (xpRootNodeIter.MoveNext())
                            {
                                if (xpRootNodeIter.Current.HasAttributes)
                                {
                                    playlist.username = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_USER, "");
                                }

                                XPathNavigator node = xpRootNodeIter.Current;

                                locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_PLAYLIST, "", true);

                                if (locNodeIter.MoveNext())
                                {
                                    newphewNodeIter = node.Select(Constants.XSD_TAG_LF_ID);
                                    if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_ID)
                                        playlist.lf_id = newphewNodeIter.Current.Value;

                                    newphewNodeIter = node.Select(Constants.XSD_TAG_LF_TITLE);
                                    if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_TITLE)
                                        playlist.title = newphewNodeIter.Current.Value;

                                    newphewNodeIter = node.Select(Constants.XSD_TAG_LF_DESCRIPTION);
                                    if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_DESCRIPTION)
                                        playlist.title = newphewNodeIter.Current.Value;


                                    newphewNodeIter = node.Select(Constants.XSD_TAG_LF_DATE);
                                    if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_DATE)
                                    {
                                        playlist.date = DateTime.ParseExact(locNodeIter.Current.Value.Trim(),
                                                                                    Constants.LF_TADETIME_FORMAT1,
                                                                                    CultureInfo.InvariantCulture);
                                    }

                                    newphewNodeIter = node.Select(Constants.XSD_TAG_LF_SIZE);
                                    if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_SIZE)
                                        playlist.lim_track_nr = newphewNodeIter.Current.ValueAsInt;

                                    newphewNodeIter = node.Select(Constants.XSD_TAG_LF_DURATION);
                                    if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_DURATION)
                                        playlist.lim_duration = newphewNodeIter.Current.ValueAsInt;

                                    newphewNodeIter = node.Select(Constants.XSD_TAG_LF_CREATOR);
                                    if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_CREATOR)
                                        playlist.creator_url = newphewNodeIter.Current.Value;

                                    newphewNodeIter = node.Select(Constants.XSD_TAG_LF_URL);
                                    if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                        playlist.url = newphewNodeIter.Current.Value;

                                    return Constants.RETVAL_MESSAGE_DONE;
                                }
                                else
                                {
                                    retval = funName + string.Format(Constants.ERROR_PARSING_XML_FSTR, Constants.XSD_TAG_LF_PLAYLIST, locNodeIter.Current.Value);
                                    DBLogger.logError(retval);
                                    //return retval;
                                }
                            }
                            else
                            {
                                retval = funName + string.Format(Constants.ERROR_PARSING_XML_FSTR, nodePos, xpRootNodeIter.Current.Value);
                                DBLogger.logError(retval);
                                //return retval;
                            }

                        }
                    }
                }
                else
                {
                    retval = funName + string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, "playlist.title", "Empty string");
                    DBLogger.logError(retval);
                    //return retval;
                }
            }
            else
            {
                retval = funName + Constants.ERROR_NULL_POINTER;
                DBLogger.logError(retval);
                //return retval;
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public static string searchCandidateTracks(Playlist playlist)
        {
            string retval = string.Empty;
            string funName = "searchCandidateTracks(LfPlaylist playlist) - ";

            if (playlist != null)
            {
                if (playlist.artists.Count > 0 || playlist.tracks.Count > 0)
                {
                    foreach (LfArtist artist in playlist.artists)
                    {
                        retval = getArtistInfo(artist, false, string.Empty, string.Empty, string.Empty, string.Empty);
                        if (retval != Constants.RETVAL_MESSAGE_DONE)
                        {
                            DBLogger.logError(funName + retval);
                        }

                        retval = DBManager.getArtistIdFromDb(artist);
                        if (retval != Constants.RETVAL_MESSAGE_DONE) DBLogger.logError(funName + retval);

                        retval = getSimilarArtists(artist, Constants.LF_SIMILAR_ARTISTS_LIMIT.ToString(), false, string.Empty, string.Empty);
                        if (retval != Constants.RETVAL_MESSAGE_DONE) DBLogger.logError(funName + retval);

                        retval = DBManager.updateArtistsMatchDB(artist);
                        if (retval != Constants.RETVAL_MESSAGE_DONE) DBLogger.logError(funName + retval);

                        retval = DBManager.update_artist_tagsDB(artist);
                        if (retval != Constants.RETVAL_MESSAGE_DONE) DBLogger.logError(funName + retval);

                        retval = getArtistTopTracks(artist);
                        if (retval != Constants.RETVAL_MESSAGE_DONE) DBLogger.logError(funName + retval);

                        retval = getAndUpdateArtistTracksListInfo(artist.topTracks, artist.name);
                        if (retval != Constants.RETVAL_MESSAGE_DONE) DBLogger.logError(funName + retval);

                        playlist.addCandidateTracks(artist.topTracks);
                    }
                    foreach (LfTrack track in playlist.tracks)
                    {
                        retval = getTrackInfo(track, false, string.Empty, string.Empty, string.Empty);
                        // todo
                    }
                }
                else
                {
                    retval = funName + Constants.ERROR_NO_ARTISTS_OR_TRACKS;
                    DBLogger.logError(retval);
                }
            }
            else
            {
                retval = funName + Constants.ERROR_NULL_POINTER;
                DBLogger.logError(retval);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title">(Optional) : Title for the playlist</param>
        /// <param name="description">(Optional) : Description for the playlist</param>
        /// <returns></returns>
        public static string createPlaylist(string title, string description)
        {
            string funName = "createPlaylist(string title, string description) - ";
            string retval = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
            {
                retval = funName + "Playlist 'title' and 'description' should be specified.";
                log.Warn(retval);
                DBLogger.logWarn(retval);
            }



            string LF_ApiKey_v = ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString();
            string LF_SessionKey_v = ConfigurationManager.AppSettings[LF_SessionKey_tag].ToString();


            string signatureStr = LASTFM_PARAM_API_KEY_TAG + LF_ApiKey_v +
                                 LASTFM_PARAM_DESCRIPTION_TAG + description +
                                 LASTFM_METHOD_TAG + LASTFM_METHOD_PLAYLIST_CREATE +
                                 LASTFM_PARAM_SK_TAG + LF_SessionKey_v +
                                 LASTFM_PARAM_TITLE_TAG + title +
                                 ConfigurationManager.AppSettings[LF_ApiSecret_tag].ToString();

            string signature_hash = Utils.encodeMD5(signatureStr);

            string requestUrl = LASTFM_API_URL;
            /* LASTFM_BASE_API_URL + LF_ApiKey_v +
            string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_DESCRIPTION_TAG) + description +
            string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) + LASTFM_METHOD_PLAYLIST_CREATE +
            string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_SK_TAG) + LF_SessionKey_v +
            string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_TITLE_TAG) + title +
            string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_API_SIG_TAG) + signature_hash */
            ;

            parameters.Add(LASTFM_PARAM_API_KEY_TAG, LF_ApiKey_v);
            parameters.Add(LASTFM_PARAM_DESCRIPTION_TAG, description);
            parameters.Add(LASTFM_METHOD_TAG, LASTFM_METHOD_PLAYLIST_CREATE);
            parameters.Add(LASTFM_PARAM_SK_TAG, LF_SessionKey_v);
            parameters.Add(LASTFM_PARAM_TITLE_TAG, title);
            parameters.Add(LASTFM_PARAM_API_SIG_TAG, signature_hash);

            retval = NetUtils.PostServiceResponse(requestUrl, parameters);
            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tAlbum"></param>
        /// <param name="trackName">Required (unless id)] : The album name</param>
        /// <param name="id">(Optional) : The musicbrainz id for the album</param>
        /// <param name="doAutocorrect">[0|1] (Optional) : Transform misspelled album names into correct album names, returning the correct version instead. The corrected album name will be returned in the response.</param>
        /// <param name="page">(Optional) : The page number to fetch. Defaults to first page.</param>
        /// <param name="limit">Optional) : The number of results to fetch per page. Defaults to 50.</param>
        /// <param name="key">(Required) : A Last.fm API key.</param>
        /// <returns></returns>
        public static string getArtistTopTracks(LfArtist artist, bool doAutocorrect, string page, string limit, string key)
        {
            string retval = string.Empty;
            string funName = "getArtistTopTracks(LfArtist artist, bool doAutocorrect, string page, string limit, string key) - ";
            string artistName = artist.name;
            string mbid = artist.mbid;

            if (string.IsNullOrEmpty(artistName) && string.IsNullOrEmpty(mbid))
            {
                retval = funName + "Artist name must be specified.";
                DBLogger.logError(retval);
                return retval;
            }
            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;
            //DataTable artistTopAlbums = new DataTable();

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_ARTIST_TOP_TRACKS_XSDFILE].ToString();

            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_ARTIST_GET_TOP_TRACKS;

            if (!string.IsNullOrEmpty(mbid))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_MBID_TAG) +
                                System.Web.HttpUtility.UrlEncode(mbid.Trim());
            }
            else if (!string.IsNullOrEmpty(artistName))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ARTIST_TAG) +
                                System.Web.HttpUtility.UrlEncode(artistName.Trim());
            }

            if (!string.IsNullOrEmpty(page))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_PAGE_TAG) +
                                System.Web.HttpUtility.UrlEncode(page.Trim());
            }

            if (!string.IsNullOrEmpty(limit))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_LIMIT_TAG) +
                                System.Web.HttpUtility.UrlEncode(limit.Trim());
            }


            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);
                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {
                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        string err = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                        log.Error(err);
                        log.Error(code);
                        log.Error(message);
                        retval = err;
                        DBLogger.logWarn(err + " # " + code + " # " + message);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_SIMILARTRACKS;

                        xpRootNodeIter = xpNav.Select(nodePos);
                        string artistStr = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_ARTIST, "");
                        if (artistStr != artist.name)
                            log.Warn(string.Format(WARN_MISMATCH_PARAM_XML_FSTR, funName, artist.name, artistStr));

                        xpRootNodeIter.MoveNext();
                        XPathNavigator node = xpRootNodeIter.Current;
                        locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_TRACK, "", true);

                        while (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;
                            LfTrack tTrack = new LfTrack();
                            tTrack.artist = artist;
                            tTrack.artistName = artist.name;

                            tTrack.rank = Int32.Parse(xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_RANK, ""));

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                tTrack.name = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_PLAYCOUNT);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_PLAYCOUNT)
                                tTrack.playcount = newphewNodeIter.Current.ValueAsInt;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_MBID);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                                tTrack.mbid = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                tTrack.url = newphewNodeIter.Current.Value;

                            artist.addTopTrack(tTrack);
                        }
                        retval = Constants.RETVAL_MESSAGE_DONE;
                    }
                }
                else
                {
                    string err = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                    log.Error(err);
                    retval = funName + err;
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

        public static DataTable getArtistTopAlbums(string artistName, string mbid, bool doAutocorrect, string page, string limit, string key)
        {
            string retval = string.Empty;
            if (string.IsNullOrEmpty(artistName) && string.IsNullOrEmpty(mbid))
            {
                retval = "Artist name must be specified.";
                DBLogger.logWarn(retval);
                throw new Exception(retval);
            }

            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;
            DataTable artistTopAlbums = new DataTable();

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_ARTIST_TOP_ALBUMS_XSDFILE].ToString();

            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_ARTIST_GET_TOP_ALBUMS;

            if (!string.IsNullOrEmpty(mbid))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_MBID_TAG) +
                                System.Web.HttpUtility.UrlEncode(mbid.Trim());
            }
            else if (!string.IsNullOrEmpty(artistName))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ARTIST_TAG) +
                                System.Web.HttpUtility.UrlEncode(artistName.Trim());
            }

            if (!string.IsNullOrEmpty(page))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_PAGE_TAG) +
                                System.Web.HttpUtility.UrlEncode(page.Trim());
            }

            if (!string.IsNullOrEmpty(limit))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_LIMIT_TAG) +
                                System.Web.HttpUtility.UrlEncode(limit.Trim());
            }

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);
                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {
                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                        log.Error(code);
                        log.Error(message);
                        retval = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl) + " # " + code + " # " + message;
                        DBLogger.logWarn(retval);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_SIMILARARTISTS;

                        xpRootNodeIter = xpNav.Select(nodePos);

                        xpRootNodeIter.MoveNext();
                        XPathNavigator node = xpRootNodeIter.Current;
                        locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_ALBUM, "", true);

                        artistTopAlbums.Columns.Add(Constants.XSD_TAG_LF_RANK);
                        artistTopAlbums.Columns.Add(Constants.XSD_TAG_LF_NAME);
                        artistTopAlbums.Columns.Add(Constants.XSD_TAG_LF_PLAYCOUNT);
                        artistTopAlbums.Columns.Add(Constants.XSD_TAG_LF_MBID);
                        artistTopAlbums.Columns.Add(Constants.XSD_TAG_LF_URL);
                        artistTopAlbums.Columns.Add(Constants.XSD_TAG_LF_ID);

                        DataRow artistsRow = artistTopAlbums.NewRow();

                        while (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;
                            artistsRow = artistTopAlbums.NewRow();

                            if (locNodeIter.Current.HasAttributes)
                                artistsRow[Constants.XSD_TAG_LF_RANK] = locNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_RANK, "");

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                artistsRow[Constants.XSD_TAG_LF_NAME] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_PLAYCOUNT);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_PLAYCOUNT)
                                artistsRow[Constants.XSD_TAG_LF_PLAYCOUNT] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_MBID);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                                artistsRow[Constants.XSD_TAG_LF_MBID] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                artistsRow[Constants.XSD_TAG_LF_URL] = newphewNodeIter.Current.Value;

                            artistTopAlbums.Rows.Add(artistsRow);
                        }
                    }
                }
                else
                {
                    retval = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                    log.Error(retval);
                    DBLogger.logWarn(retval);
                }
            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
                DBLogger.logWarn(ex.Message);
            }

            return artistTopAlbums;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="album"></param>
        /// <param name="doAutocorrect"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string getArtistTopAlbums(LfArtist artist, bool doAutocorrect, string page, string limit, string key)
        {
            string retval = string.Empty;
            string funName = "getArtistTopAlbums(LfArtist artist, string artistName, string mbid, bool doAutocorrect, string page, string limit, string key)";
            string artistName = artist.name;
            string mbid = artist.mbid;
            if (string.IsNullOrEmpty(artistName) && string.IsNullOrEmpty(mbid))
            {

                retval = funName + "Artist name must be specified.";
                DBLogger.logWarn(retval);
                return retval;
            }
            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;
            //DataTable artistTopAlbums = new DataTable();

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_ARTIST_TOP_ALBUMS_XSDFILE].ToString();

            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_ARTIST_GET_TOP_ALBUMS;

            if (!string.IsNullOrEmpty(mbid))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_MBID_TAG) +
                                System.Web.HttpUtility.UrlEncode(mbid.Trim());

                if (string.IsNullOrEmpty(artist.mbid))
                {
                    artist.mbid = mbid;
                }
            }
            else if (!string.IsNullOrEmpty(artistName))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ARTIST_TAG) +
                                System.Web.HttpUtility.UrlEncode(artistName.Trim());

                if (string.IsNullOrEmpty(artist.name))
                {
                    artist.name = artistName;
                }
            }

            if (!string.IsNullOrEmpty(page))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_PAGE_TAG) +
                                System.Web.HttpUtility.UrlEncode(page.Trim());
            }

            if (!string.IsNullOrEmpty(limit))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_LIMIT_TAG) +
                                System.Web.HttpUtility.UrlEncode(limit.Trim());
            }


            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);
                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {
                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        string err = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                        log.Error(err);
                        log.Error(code);
                        log.Error(message);
                        retval = err;
                        DBLogger.logWarn(err + "#" + code + "#" + message);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_SIMILARARTISTS;

                        xpRootNodeIter = xpNav.Select(nodePos);

                        xpRootNodeIter.MoveNext();
                        XPathNavigator node = xpRootNodeIter.Current;
                        locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_ALBUM, "", true);

                        while (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;
                            LfAlbum tAlbum = new LfAlbum();
                            //tAlbum.artistName = album.name;
                            tAlbum.artist = artist;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                tAlbum.name = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_PLAYCOUNT);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_PLAYCOUNT)
                                tAlbum.playcount = newphewNodeIter.Current.ValueAsInt;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_MBID);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                                tAlbum.mbid = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                tAlbum.url = newphewNodeIter.Current.Value;

                            artist.addAlbum(tAlbum);
                        }
                        retval = Constants.RETVAL_MESSAGE_DONE;
                    }
                }
                else
                {
                    string err = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                    log.Error(err);
                    retval = funName + err;
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

        public LfAlbum getAlbumsInfo(string artistName, string album, string mbid, bool doAutocorrect, string username, string lang, string key)
        {
            if ((string.IsNullOrEmpty(artistName) || string.IsNullOrEmpty(album)) && string.IsNullOrEmpty(mbid))
            {
                DBLogger.logWarn("Artist and album name must be specified.");
                throw new Exception("Artist and album name must be specified.");
            }
            LfAlbum lfAlbum = new LfAlbum();
            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;
            DataTable albumTracks = new DataTable();

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_ALBUM_GET_INFO_XSDFILE].ToString();

            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_ALBUM_GET_INFO;

            if (!string.IsNullOrEmpty(mbid))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_MBID_TAG) +
                                System.Web.HttpUtility.UrlEncode(mbid.Trim());
            }

            if (!string.IsNullOrEmpty(artistName) && !string.IsNullOrEmpty(album))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ARTIST_TAG) +
                                System.Web.HttpUtility.UrlEncode(artistName.Trim()) +
                               string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ALBUM_TAG) +
                                System.Web.HttpUtility.UrlEncode(album.Trim());
            }

            if (!string.IsNullOrEmpty(username))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_USERNAME_TAG) +
                                System.Web.HttpUtility.UrlEncode(username.Trim());
            }

            if (!string.IsNullOrEmpty(lang))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_LANG_TAG) +
                                System.Web.HttpUtility.UrlEncode(lang.Trim());
            }

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);

                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {

                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                        log.Error(code);
                        log.Error(message);
                        DBLogger.logWarn(message);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_ALBUM;

                        xpRootNodeIter = xpNav.Select(nodePos);

                        xpRootNodeIter.MoveNext();
                        XPathNavigator node = xpRootNodeIter.Current;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_NAME);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                            lfAlbum.name = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_ARTIST);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_ARTIST)
                            lfAlbum.artistName = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_ID);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_ID)
                            lfAlbum.lf_id = locNodeIter.Current.ValueAsInt;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_MBID);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                            lfAlbum.mbid = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_URL);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                            lfAlbum.url = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_RELEASEDATE);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_RELEASEDATE)
                        {
                            CultureInfo enUS = new CultureInfo("en-US");

                            lfAlbum.releasedate = DateTime.ParseExact(locNodeIter.Current.Value.Trim(),
                                                                        Constants.LF_TADETIME_FORMAT,
                                                                        enUS);
                        }

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_PLAYCOUNT);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_PLAYCOUNT)
                            lfAlbum.playcount = locNodeIter.Current.ValueAsInt;

                        locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_TRACK, "", true);

                        //albumTracks.Columns.Add(Constants.XSD_TAG_LF_RANK);
                        //albumTracks.Columns.Add(Constants.XSD_TAG_LF_NAME);
                        //albumTracks.Columns.Add(Constants.XSD_TAG_LF_DURATION);
                        //albumTracks.Columns.Add(Constants.XSD_TAG_LF_MBID);
                        //albumTracks.Columns.Add(Constants.XSD_TAG_LF_URL);
                        //albumTracks.Columns.Add(Constants.XSD_TAG_LF_ID);

                        //DataRow artistsRow = albumTracks.NewRow();

                        //lfTrack.tracks_count = 0;
                        LfTrack track;
                        while (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;
                            //artistsRow = albumTracks.NewRow();
                            track = new LfTrack();
                            track.album = lfAlbum;
                            if (lfAlbum.artist != null)
                            {
                                track.artist = lfAlbum.artist;
                                track.artistName = lfAlbum.artistName;
                            }
                            else
                                track.artistName = lfAlbum.artistName;

                            if (locNodeIter.Current.HasAttributes)
                                track.rank = Int32.Parse(locNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_RANK, ""));
                            //artistsRow[Constants.XSD_TAG_LF_RANK] = locNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_RANK, "");

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                track.name = newphewNodeIter.Current.Value;
                            //artistsRow[Constants.XSD_TAG_LF_NAME] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_DURATION);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_DURATION)
                                track.duration = Int32.Parse(newphewNodeIter.Current.Value);
                            //artistsRow[Constants.XSD_TAG_LF_DURATION] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_MBID);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                                track.mbid = newphewNodeIter.Current.Value;
                            //artistsRow[Constants.XSD_TAG_LF_MBID] = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                track.url = newphewNodeIter.Current.Value;
                            //artistsRow[Constants.XSD_TAG_LF_URL] = newphewNodeIter.Current.Value;

                            lfAlbum.addTrack(track);
                            //albumTracks.Rows.Add(artistsRow);
                            //lfTrack.tracks_count += 1;
                        }

                        //lfTrack.artists = albumTracks;
                        LfTag tag;
                        locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_TAG, "", true);
                        while (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;
                            tag = new LfTag();
                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                tag.name = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                tag.url = newphewNodeIter.Current.Value;

                            lfAlbum.addTag(tag);
                        }

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_WIKI + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_SUMMARY);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_SUMMARY)
                            lfAlbum.wiki_summary = NetUtils.RemoveHtmlAndRegexTags(locNodeIter.Current.Value, Constants.REGEX_CDATA_STR);

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_WIKI + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_CONTENT);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_CONTENT)
                            lfAlbum.wiki_content = NetUtils.RemoveHtmlAndRegexTags(locNodeIter.Current.Value, Constants.REGEX_CDATA_STR);


                    }
                }
                else
                {
                    log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                    DBLogger.logWarn(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                }
            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
                DBLogger.logWarn(ex.Message);
            }

            return lfAlbum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lfTrack"></param>
        /// <param name="artistName"></param>
        /// <param name="doAutocorrect"></param>
        /// <param name="username"></param>
        /// <param name="lang"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string getAlbumsInfo(LfAlbum lfAlbum, bool doAutocorrect, string username, string lang, string key)
        {
            //LfAlbum lfTrack = new LfAlbum();
            string retval = string.Empty;
            string funName = "getAlbumsInfo(LfAlbum lfAlbum, string artistName, bool doAutocorrect, string username, string lang, string key) - ";
            string artistName = string.Empty;
            if (lfAlbum == null)
            {
                retval = funName + Constants.ERROR_NULL_POINTER;
                DBLogger.logError(retval);
                return retval;
            }
            else
            {
                if (!string.IsNullOrEmpty(lfAlbum.artistName))
                    artistName = lfAlbum.artistName;
            }

            if ((string.IsNullOrEmpty(artistName) || string.IsNullOrEmpty(lfAlbum.name)) && string.IsNullOrEmpty(lfAlbum.mbid))
            {
                retval = funName + string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, artistName, "Artist and album name must be specified.");
                DBLogger.logError(retval);
                return retval;
            }

            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;
            DataTable albumTracks = new DataTable();

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_ALBUM_GET_INFO_XSDFILE].ToString();

            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_ALBUM_GET_INFO;

            if (!string.IsNullOrEmpty(lfAlbum.mbid))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_MBID_TAG) +
                                System.Web.HttpUtility.UrlEncode(lfAlbum.mbid.Trim());
            }

            if (!string.IsNullOrEmpty(artistName) && !string.IsNullOrEmpty(lfAlbum.name))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ARTIST_TAG) +
                                System.Web.HttpUtility.UrlEncode(artistName.Trim()) +
                               string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ALBUM_TAG) +
                                System.Web.HttpUtility.UrlEncode(lfAlbum.name.Trim());
            }

            if (!string.IsNullOrEmpty(username))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_USERNAME_TAG) +
                                System.Web.HttpUtility.UrlEncode(username.Trim());
            }

            if (!string.IsNullOrEmpty(lang))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_LANG_TAG) +
                                System.Web.HttpUtility.UrlEncode(lang.Trim());
            }

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);

                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {

                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                        log.Error(code);
                        log.Error(message);
                        DBLogger.logError(message);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_ALBUM;

                        xpRootNodeIter = xpNav.Select(nodePos);

                        xpRootNodeIter.MoveNext();
                        XPathNavigator node = xpRootNodeIter.Current;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_NAME);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                            lfAlbum.name = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_ARTIST);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_ARTIST)
                            lfAlbum.artistName = locNodeIter.Current.Value;
                        if (lfAlbum.artistName != artistName)
                        {
                            log.Warn(string.Format(WARN_MISMATCH_PARAM_XML_FSTR, funName, lfAlbum.artistName, artistName));
                        }


                        locNodeIter = node.Select(Constants.XSD_TAG_LF_ID);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_ID)
                            lfAlbum.lf_id = locNodeIter.Current.ValueAsInt;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_MBID);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                            lfAlbum.mbid = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_URL);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                            lfAlbum.url = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_RELEASEDATE);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_RELEASEDATE)
                        {
                            CultureInfo enUS = new CultureInfo("en-US");

                            lfAlbum.releasedate = DateTime.ParseExact(locNodeIter.Current.Value.Trim(),
                                                                        Constants.LF_TADETIME_FORMAT,
                                                                        enUS);
                        }

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_PLAYCOUNT);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_PLAYCOUNT)
                            lfAlbum.playcount = locNodeIter.Current.ValueAsInt;

                        locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_TRACK, "", true);
                        LfTrack track;
                        while (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;
                            track = new LfTrack();

                            if (locNodeIter.Current.HasAttributes)
                                track.rank = Int32.Parse(locNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_RANK, ""));

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                track.name = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_DURATION);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_DURATION)
                                track.duration = newphewNodeIter.Current.ValueAsInt;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_MBID);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                                track.mbid = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                track.url = newphewNodeIter.Current.Value;

                            lfAlbum.addTrack(track);
                        }


                        LfTag tag;
                        locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_TAG, "", true);
                        while (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;
                            tag = new LfTag();
                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                tag.name = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                tag.url = newphewNodeIter.Current.Value;

                            lfAlbum.addTag(tag);
                        }

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_WIKI + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_SUMMARY);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_SUMMARY)
                            lfAlbum.wiki_summary = NetUtils.RemoveHtmlAndRegexTags(locNodeIter.Current.Value, Constants.REGEX_CDATA_STR);// RemoveHtmlAndRegexTags(locNodeIter.Current.Value);

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_WIKI + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_CONTENT);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_CONTENT)
                            lfAlbum.wiki_content = NetUtils.RemoveHtmlAndRegexTags(locNodeIter.Current.Value, Constants.REGEX_CDATA_STR);// RemoveHtmlAndRegexTags(locNodeIter.Current.Value);

                        retval = Constants.RETVAL_MESSAGE_DONE;
                    }
                }
                else
                {
                    string err = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                    log.Error(err);
                    retval = funName + err;
                    DBLogger.logError(retval);
                }
            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
                retval = funName + Constants.ERROR_LOADING;
                DBLogger.logError(retval);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lfArtist"></param>
        /// <param name="doAutocorrect"></param>
        /// <param name="username"></param>
        /// <param name="lang"></param>
        /// <param name="mbid"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string getArtistInfo(LfArtist lfArtist, bool doAutocorrect, string username, string lang, string mbid, string key)
        {
            //LfAlbum lfTrack = new LfAlbum();
            string retval = string.Empty;
            string funName = "getArtistInfo(LfArtist lfArtist, bool doAutocorrect, string username, string lang, string key) - ";
            string artistName = string.Empty;
            if (lfArtist != null)
            {
                artistName = lfArtist.name;
            }
            else
            {
                retval = funName + Constants.ERROR_NULL_POINTER + ": Artist - " + "Artist must be specified.";
                DBLogger.logError(retval);
            }

            if (string.IsNullOrEmpty(artistName) && string.IsNullOrEmpty(mbid))
            {
                retval = funName + string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, artistName, "Artist name or mbid must be specified.");
                DBLogger.logError(retval);
            }

            XmlReader xReader = null;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpRootNodeIter;
            XPathNodeIterator locNodeIter;
            XPathNodeIterator newphewNodeIter;
            DataTable albumTracks = new DataTable();

            string xmlfilefolderPath = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_XMLPATH].ToString();
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_ARTIST_GET_INFO_XSDFILE].ToString();

            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_ARTIST_GET_INFO;

            if (!string.IsNullOrEmpty(artistName))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ARTIST_TAG) +
                                System.Web.HttpUtility.UrlEncode(artistName.Trim());
            }

            if (!string.IsNullOrEmpty(username))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_USERNAME_TAG) +
                                System.Web.HttpUtility.UrlEncode(username.Trim());
            }

            if (!string.IsNullOrEmpty(mbid))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_MBID_TAG) +
                                System.Web.HttpUtility.UrlEncode(mbid.Trim());
            }

            if (!string.IsNullOrEmpty(lang))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_LANG_TAG) +
                                System.Web.HttpUtility.UrlEncode(lang.Trim());
            }

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);

                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {

                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                        log.Error(code);
                        log.Error(message);
                        retval = funName + message;
                        DBLogger.logError(retval);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_ARTIST;

                        xpRootNodeIter = xpNav.Select(nodePos);

                        xpRootNodeIter.MoveNext();
                        XPathNavigator node = xpRootNodeIter.Current;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_NAME);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                            lfArtist.name = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_MBID);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                            lfArtist.mbid = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_URL);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                            lfArtist.url = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_STATUS + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_PLAYCOUNT);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_PLAYCOUNT)
                            lfArtist.playcount = locNodeIter.Current.ValueAsInt;


                        locNodeIter = node.Select(Constants.XSD_TAG_LF_TAGS);
                        locNodeIter.MoveNext();
                        XPathNavigator chNode = locNodeIter.Current;

                        XPathNodeIterator chNodes = chNode.SelectDescendants(Constants.XSD_TAG_LF_TAG, "", true);
                        LfTag tag;

                        while (chNodes.MoveNext())
                        {
                            XPathNavigator keyNode = chNodes.Current;
                            tag = new LfTag();
                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                tag.name = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                tag.url = newphewNodeIter.Current.Value;

                            lfArtist.addTag(tag);
                        }

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_BIO + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_SUMMARY);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_SUMMARY)
                            lfArtist.wiki_summary = NetUtils.RemoveHtmlAndRegexTags(locNodeIter.Current.Value, Constants.REGEX_CDATA_STR);// RemoveHtmlAndRegexTags(locNodeIter.Current.Value);

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_BIO + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_CONTENT);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_CONTENT)
                            lfArtist.wiki_content = NetUtils.RemoveHtmlAndRegexTags(locNodeIter.Current.Value, Constants.REGEX_CDATA_STR);// RemoveHtmlAndRegexTags(locNodeIter.Current.Value);

                        retval = Constants.RETVAL_MESSAGE_DONE;
                    }
                }
                else
                {
                    string err = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                    log.Error(err);
                    retval = funName + err;
                    DBLogger.logError(retval);
                }
            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
                retval = funName + Constants.ERROR_LOADING;
                DBLogger.logError(retval);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artist"></param>
        /// <returns></returns>
        public static string getArtistTopTracks(LfArtist artist)
        {
            string retval = string.Empty;
            string funName = "getArtistTopTracks(LfArtist artist) - ";

            //LastfmMethods meths = new LastfmMethods();

            retval = getArtistTopTracks(artist, false, "", Constants.LF_ARTIST_TOP_TRACKS_LIMIT.ToString(), "");
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                retval = (funName + retval);
                DBLogger.logError(retval);
                return retval;
            }

            retval = getAllArtistTracksInfo(artist);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                retval = (funName + retval);
                DBLogger.logError(retval);
                return retval;
            }

            return Constants.RETVAL_MESSAGE_DONE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="album"></param>
        /// <param name="meths"></param>
        /// <returns></returns>
        public static string getAllArtistTracksInfo(LfArtist artist)
        {
            string retval = string.Empty;
            string funName = "getAllArtistTracksInfo(LfArtist artist) - ";


            foreach (LfTrack trackRow in artist.topTracks)
            {
                retval = getTrackInfo(trackRow, false, string.Empty, string.Empty, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }

                retval = DBManager.getTrackIdFromDb(trackRow);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }

                retval = DBManager.getTagIdsFromDb(trackRow.tags);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }

                retval = DBManager.update_track_tagsDB(trackRow);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }

            }

            return Constants.RETVAL_MESSAGE_DONE;
        }

        //public static string getAristsTopAlbums(DataTable dt)
        //{
        //    string retval = string.Empty;
        //    string funName = "getAristsTopAlbums(DataTable dt) - ";
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        //LastfmMethods meths = new LastfmMethods();
        //        DataTable dta = getArtistTopAlbums(row[Constants.XSD_TAG_LF_NAME].ToString(), row[Constants.XSD_TAG_LF_MBID].ToString(), false, "", "10", "");
        //        List<LfAlbum> albumsInfo = getAllArtistAlbumsInfo(dta, row[Constants.XSD_TAG_LF_NAME].ToString(), row[Constants.XSD_TAG_LF_ID].ToString());

        //        retval = DBManager.getAlbumsIdsFromDb(albumsInfo);
        //        if (retval != Constants.RETVAL_MESSAGE_DONE)
        //            return (funName + retval);
        //    }

        //    return Constants.RETVAL_MESSAGE_DONE;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artist"></param>
        /// <returns></returns>
        public static string getAristsSimilarsTopAlbums(LfArtist artist)
        {
            string retval = string.Empty;
            string funName = "getAristsTopAlbums(LfArtist artist) - ";
            foreach (LfArtist simArt in artist.similarArtists)
            {
                //LastfmMethods meths = new LastfmMethods();

                retval = getArtistTopAlbums(simArt, false, "", Constants.LF_ARTIST_TOP_ALBUMS_LIMIT.ToString(), "");
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }

                //List<LfAlbum> albumsInfo = getAllArtistAlbumsInfo(dta, meths, row[Constants.XSD_TAG_LF_NAME].ToString(), row[Constants.XSD_TAG_LF_ID].ToString());
                retval = getAllArtistAlbumsInfo(artist);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }
                retval = DBManager.getAlbumsIdsFromDb(artist.topAlbums);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }
            }

            return Constants.RETVAL_MESSAGE_DONE;
        }

        //public static List<LfAlbum> getAllArtistAlbumsInfo(DataTable dta, string artistname, string artistID)
        //{
        //    List<LfAlbum> albumsInfo = new List<LfAlbum>();
        //    LfAlbum currAlbum = null;
        //    int idArtist = 0;

        //    if (!Int32.TryParse(artistID, out idArtist))
        //    {
        //        log.Error(string.Format(Constants.ERROR_ON_CONVERTING_FSTR, artistID, "Int32.TryParse(artistID, out idArtist)"));
        //    }

        //    foreach (DataRow albumRow in dta.Rows)
        //    {
        //        currAlbum = getAlbumsInfo(artistname, albumRow[Constants.XSD_TAG_LF_NAME].ToString(), albumRow[Constants.XSD_TAG_LF_MBID].ToString(), false, string.Empty, string.Empty, string.Empty);
        //        currAlbum.rank = Int32.Parse(albumRow[Constants.XSD_TAG_LF_RANK].ToString());
        //        currAlbum.idArtist = idArtist;
        //        albumsInfo.Add(currAlbum);
        //    }

        //    return albumsInfo;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artist"></param>
        /// <returns></returns>
        public static string getAllArtistAlbumsInfo(LfArtist artist)
        {
            //List<LfAlbum> albumsInfo = new List<LfAlbum>();
            string retval = string.Empty;
            string funName = "getAllArtistAlbumsInfo(LfArtist artist) - ";
            //int idArtist = 0;
            //string artistname string artistID

            foreach (LfAlbum albumRow in artist.topAlbums)
            {
                if (albumRow != null && string.IsNullOrEmpty(albumRow.artistName))
                {
                    albumRow.artistName = artist.name;
                }

                retval = getAlbumsInfo(albumRow, false, string.Empty, string.Empty, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }

                retval = DBManager.getTagIdsFromDb(albumRow.tags);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                    continue;
                }                //currAlbum.rank = Int32.Parse(trackRow[Constants.XSD_TAG_LF_RANK].ToString());
                //currAlbum.idArtist = tAlbum.id;
                //tAlbum.addAlbum(currAlbum);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artist"></param>
        /// <returns></returns>
        public static string getAristsTopAlbums(LfArtist artist)
        {
            string retval = string.Empty;
            string funName = "getAristsTopAlbums(LfArtist artist) - ";

            //LastfmMethods meths = new LastfmMethods();

            retval = getArtistTopAlbums(artist, false, "", Constants.LF_ARTIST_TOP_ALBUMS_LIMIT.ToString(), "");
            if (retval != Constants.RETVAL_MESSAGE_DONE)
            {
                retval = (funName + retval);
                DBLogger.logError(retval);
                return retval;
            }

            //List<LfAlbum> albumsInfo = getAllArtistAlbumsInfo(dta,row[Constants.XSD_TAG_LF_NAME].ToString(), row[Constants.XSD_TAG_LF_ID].ToString());
            retval = getAllArtistAlbumsInfo(artist);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
                DBLogger.logWarn(funName + retval);

            retval = DBManager.getAlbumsIdsFromDb(artist.topAlbums);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
                DBLogger.logWarn(funName + retval);

            return Constants.RETVAL_MESSAGE_DONE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="srcTrack"></param>
        /// <param name="album"></param>
        /// <param name="doResearchForChildern"></param>
        /// <returns></returns>
        public static string getSimilarTracks(string limit, LfTrack srcTrack, bool doResearchForChildern = false)
        {
            string retval = null;
            string funName = "getSimilarTracks(string limit, string track, bool doAutocorrect, string mbid) - ";
            //LfTrack srcTrack = new LfTrack();

            string artistName = string.Empty;// album.name;
            LfArtist artist = null;

            if (srcTrack.artist != null)
            {
                artist = srcTrack.artist;
            }

            if (!string.IsNullOrEmpty(artistName) && srcTrack.artistName != artistName)
                srcTrack.artistName = artistName;

            //LastfmMethods meths = new LastfmMethods();
            if (srcTrack.tags.Count == 0)
            {
                if (string.IsNullOrEmpty(srcTrack.artistName))
                    srcTrack.artistName = artistName;
                retval = getTrackInfo(srcTrack, false, string.Empty, string.Empty, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logError(funName + retval);
                }
            }

            if (srcTrack.artist != null)
            {
                artistName = srcTrack.artist.name;
                if (srcTrack.artist.id == 0)
                {
                    retval = DBManager.getArtistIdFromDb(srcTrack.artist);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                        DBLogger.logError(funName + retval);
                }
            }
            else
            {
                DBLogger.logWarn(Constants.ERROR_NULL_POINTER);
            }

            //DataTable dt = meths.getSimilarArtistsLF(limit, tAlbum, doAutocorrect, id, null);
            retval = getSimilarTrack(srcTrack, Constants.LF_SIMILAR_TRACKS_LIMIT.ToString(), srcTrack.name, artistName, false, srcTrack.mbid, string.Empty);
            if (retval != Constants.RETVAL_MESSAGE_DONE)
                DBLogger.logWarn(funName + retval);

            if (srcTrack.artist != null)
            {
                if (srcTrack.album != null)
                {
                    if (srcTrack.album.artist != null)
                    {
                        srcTrack.album.artist.id = srcTrack.artist.id;
                    }
                    else
                    {
                        DBLogger.logWarn(Constants.ERROR_NULL_POINTER);
                    }
                    retval = DBManager.getAlbumsIdFromDb(srcTrack.album);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                        DBLogger.logWarn(funName + retval);
                }
                else
                {
                    retval = funName + Constants.ERROR_NULL_POINTER;
                    DBLogger.logWarn(retval);
                }

                bool skip = false;

                retval = DBManager.getTrackIdFromDb(srcTrack);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    skip = true;
                    DBLogger.logWarn(funName + retval);
                }

                if (!skip)
                {
                    retval = DBManager.getTagIdsFromDb(srcTrack.tags);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        skip = true;
                        DBLogger.logWarn(funName + retval);
                    }
                }

                if (!skip)
                {
                    retval = DBManager.update_track_tagsDB(srcTrack);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        skip = true;
                        DBLogger.logWarn(funName + retval);
                    }
                }


                if (doResearchForChildern)
                {
                    foreach (LfTrack trck in srcTrack.similarTrack)
                    {
                        if (trck.artist != null)
                        {
                            retval = getTrackInfo(trck, false, string.Empty, string.Empty, string.Empty);
                            if (retval != Constants.RETVAL_MESSAGE_DONE)
                            {
                                DBLogger.logWarn(funName + retval);
                                continue;
                            }

                            if (trck.artist != null)
                            {
                                if (trck.artist.id <= 0)
                                {
                                    retval = DBManager.getArtistIdFromDb(trck.artist);
                                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                                        DBLogger.logWarn(funName + retval);
                                }
                            }
                            else
                            {
                                DBLogger.logWarn(funName + Constants.ERROR_NULL_POINTER);
                                continue;
                            }

                            if (trck.album != null)
                            {
                                if (trck.album.dbId == 0)
                                {
                                    retval = DBManager.getAlbumsIdFromDb(trck.album);
                                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                                        DBLogger.logWarn(funName + retval);
                                }
                            }
                            else
                            {
                                DBLogger.logWarn(Constants.ERROR_NULL_POINTER);
                            }


                            if (trck.artist != null)
                                artistName = trck.artist.name;

                            retval = getSimilarTrack(trck, Constants.LF_SIMILAR_TRACKS_LIMIT.ToString(), trck.name, artistName, false, trck.mbid, string.Empty);
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
                                retval = funName + retval;
                                DBLogger.logWarn(retval);
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

        public static string getAndUpdateArtistTracksListInfo(List<LfTrack> tracks, string artistname)
        {
            string retval = string.Empty;
            string funName = "getTrackInfo(LfTrack lfTrack, string artistname) - ";

            foreach (LfTrack track in tracks)
            {

                if (string.IsNullOrEmpty(track.artistName))
                    track.artistName = artistname;

                retval = getTrackInfo(track, false, string.Empty, string.Empty, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = DBManager.getTrackIdFromDb(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = DBManager.update_track_tagsDB(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = getSimilarTrack(track, Constants.LF_SIMILAR_TRACKS_LIMIT.ToString(), track.name, artistname, false, track.mbid, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = DBManager.updateTracksMatchDB(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    //retval = DBManager.updateTracksMatchDB(artist);
                }

            }

            return Constants.RETVAL_MESSAGE_DONE;
        }


        public static string getAndUpdateSimilarTracksListInfo(List<LfTrack> simTracks, bool searchChildSimilars = false)
        {
            string retval = string.Empty;
            string funName = "getAndUpdateSimilarTracksListInfo(List<LfTrack> simTracks) - ";
            //string artistname = string.Empty;

            foreach (LfTrack track in simTracks)
            {

                retval = getTrackInfo(track, false, string.Empty, string.Empty, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = DBManager.getTrackIdFromDb(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = DBManager.update_track_tagsDB(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                if (searchChildSimilars)
                {
                    retval = getAndUpdateOnlySimilarTracksList(track.similarTrack, false);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        continue;
                    }

                    retval = DBManager.updateTracksMatchDB(track);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                    }
                }
            }

            return Constants.RETVAL_MESSAGE_DONE;
        }

        public static string getAndUpdateOnlySimilarTracksList(List<LfTrack> simTracks, bool searchChildSimilars = false)
        {
            string retval = string.Empty;
            string funName = "getAndUpdateOnlySimilarTracksList(List<LfTrack> simTracks) - ";
            //string artistname = string.Empty;

            foreach (LfTrack track in simTracks)
            {
                if (track.id == 0)
                {
                    retval = DBManager.getTrackIdFromDb(track);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        continue;
                    }
                }

                //retval = getSimilarTracks(Constants.LF_SIMILAR_TRACKS_LIMIT.ToString(), artist);
                retval = LastfmMethods.getSimilarTrack(track, Constants.LF_SIMILAR_TRACKS_LIMIT.ToString(), track.name, track.artistName, false, track.mbid, string.Empty);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                    continue;
                }

                retval = DBManager.updateTracksMatchDB(track);
                if (retval != Constants.RETVAL_MESSAGE_DONE)
                {
                    DBLogger.logWarn(funName + retval);
                }


                if (searchChildSimilars)
                {
                    retval = getAndUpdateOnlySimilarTracksList(track.similarTrack, false);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                        continue;
                    }

                    retval = DBManager.updateTracksMatchDB(track);
                    if (retval != Constants.RETVAL_MESSAGE_DONE)
                    {
                        DBLogger.logWarn(funName + retval);
                    }
                }
            }

            return Constants.RETVAL_MESSAGE_DONE;
        }

        public static string getTrackInfo(LfTrack lfTrack, bool doAutocorrect, string username, string lang, string key)
        {
            //LfAlbum lfTrack = new LfAlbum();
            string retval = string.Empty;
            string funName = "getTrackInfo(LfTrack lfTrack, string artistName, bool doAutocorrect, string username, string lang, string key) - ";
            string artistname = string.Empty;
            string trackname = string.Empty;
            if (lfTrack == null)
            {
                retval = funName + Constants.ERROR_NULL_POINTER + "lfTrack";
                DBLogger.logWarn(retval);
                return retval;
            }

            if (!string.IsNullOrEmpty(lfTrack.name))
            {
                trackname = lfTrack.name;
            }

            if (lfTrack.artist != null)
            {
                artistname = lfTrack.artist.name;
                if (string.IsNullOrEmpty(lfTrack.artistName))
                    lfTrack.artistName = artistname;
            }
            else
            {
                if (!string.IsNullOrEmpty(lfTrack.artistName))
                {
                    lfTrack.artist = new LfArtist();
                    lfTrack.artist.name = lfTrack.artistName;
                }
            }

            if ((string.IsNullOrEmpty(artistname) || string.IsNullOrEmpty(trackname)) && string.IsNullOrEmpty(lfTrack.mbid))
            {
                retval = funName + string.Format(Constants.ERROR_INPUT_PARAMETER_FSTR, artistname, "Artist and track name must be specified.");
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
            string xsdfilename = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_LF_TRACK_GET_INFO_XSDFILE].ToString();

            string requestUrl = LASTFM_BASE_API_URL +
                                ConfigurationManager.AppSettings[LF_ApiKey_tag].ToString() +
                                string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_METHOD_TAG) +
                                LASTFM_METHOD_TRACK_GET_INFO;



            if (!string.IsNullOrEmpty(artistname) && !string.IsNullOrEmpty(trackname))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_ARTIST_TAG) +
                                System.Web.HttpUtility.UrlEncode(artistname.Trim()) +
                               string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_TRACK_TAG) +
                                System.Web.HttpUtility.UrlEncode(lfTrack.name.Trim());
            }
            else if (!string.IsNullOrEmpty(lfTrack.mbid))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_MBID_TAG) +
                                System.Web.HttpUtility.UrlEncode(lfTrack.mbid.Trim());
            }

            if (!string.IsNullOrEmpty(username))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_USERNAME_TAG) +
                                System.Web.HttpUtility.UrlEncode(username.Trim());
            }

            if (!string.IsNullOrEmpty(lang))
            {
                requestUrl += string.Format(NetUtils.WEB_URL_PARAM_FSTR, LASTFM_PARAM_LANG_TAG) +
                                System.Web.HttpUtility.UrlEncode(lang.Trim());
            }

            try
            {
                string xsdfilePath = Path.Combine(xmlfilefolderPath, xsdfilename);

                XmlReaderSettings vr = new XmlReaderSettings();
                vr.Schemas.Add(string.Empty, xsdfilePath);

                string serviceResponse = NetUtils.GetServiceResponse(requestUrl);

                if (!string.IsNullOrEmpty(serviceResponse) || !serviceResponse.StartsWith(Constants.RETVAL_MESSAGE_ERROR))
                {
                    StringReader sr = new StringReader(serviceResponse);
                    //var xmlResponse = XElement.Parse(serviceResponse);
                    xReader = XmlReader.Create(sr, vr);

                    xpDoc = new XPathDocument(xReader);
                    xpNav = xpDoc.CreateNavigator();
                    string nodePos = Constants.XSD_TAG_LF_LFM;

                    string errorNodePos = Constants.XSD_TAG_LF_LFM +
                                           Constants.XSD_TAG_SEPARATOR +
                                           Constants.XSD_TAG_LF_ERROR;

                    xpRootNodeIter = xpNav.Select(nodePos);

                    if (xpRootNodeIter.Current.HasAttributes &&
                        xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_STATUS, "") == Constants.XSD_TAG_LF_FAILED)
                    {
                        xpRootNodeIter = xpNav.Select(errorNodePos);
                        string code = xpRootNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_CODE, "");
                        string message = xpRootNodeIter.Current.Value.Trim();
                        log.Error(string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl));
                        log.Error(code);
                        log.Error(message);
                        DBLogger.logWarn(message);
                    }
                    else
                    {
                        nodePos += Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_TRACK;

                        xpRootNodeIter = xpNav.Select(nodePos);

                        xpRootNodeIter.MoveNext();
                        XPathNavigator node = xpRootNodeIter.Current;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_NAME);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                            lfTrack.name = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_MBID);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                            lfTrack.mbid = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_URL);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                            lfTrack.url = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_DURATION);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_DURATION)
                            lfTrack.duration = locNodeIter.Current.ValueAsInt;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_PLAYCOUNT);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_PLAYCOUNT)
                            lfTrack.playcount = locNodeIter.Current.ValueAsInt;

                        LfArtist artist;
                        if (lfTrack.artist == null)
                            artist = new LfArtist();
                        else
                            artist = lfTrack.artist;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_ARTIST + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_NAME);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                            artist.name = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_ARTIST + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_MBID);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                            artist.mbid = locNodeIter.Current.Value;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_ARTIST + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_URL);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                            artist.url = locNodeIter.Current.Value;

                        //lfTrack.artist = artist;

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_ALBUM);
                        LfAlbum tAlbum = new LfAlbum();
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_ALBUM)
                            lfTrack.rank = Int32.Parse(locNodeIter.Current.GetAttribute(Constants.XSD_TAG_LF_POSITION, ""));

                        if (artist != null)
                            tAlbum.artist = artist;

                        newphewNodeIter = node.Select(Constants.XSD_TAG_LF_ALBUM + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_ARTIST);
                        if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_ARTIST)
                        {
                            tAlbum.artistName = newphewNodeIter.Current.Value;

                            if (tAlbum.artistName != artist.name)
                                log.Warn(string.Format(WARN_MISMATCH_PARAM_XML_FSTR, funName, tAlbum.artistName, artist.name));
                        }

                        newphewNodeIter = node.Select(Constants.XSD_TAG_LF_ALBUM + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_TITLE);
                        if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_TITLE)
                            tAlbum.name = newphewNodeIter.Current.Value;

                        newphewNodeIter = node.Select(Constants.XSD_TAG_LF_ALBUM + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_MBID);
                        if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_MBID)
                            tAlbum.mbid = newphewNodeIter.Current.Value;

                        newphewNodeIter = node.Select(Constants.XSD_TAG_LF_ALBUM + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_URL);
                        if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                            tAlbum.url = newphewNodeIter.Current.Value;

                        lfTrack.album = tAlbum;


                        LfTag tag;
                        locNodeIter = node.SelectDescendants(Constants.XSD_TAG_LF_TAG, "", true);
                        while (locNodeIter.MoveNext())
                        {
                            XPathNavigator keyNode = locNodeIter.Current;
                            tag = new LfTag();
                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_NAME);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_NAME)
                                tag.name = newphewNodeIter.Current.Value;

                            newphewNodeIter = keyNode.Select(Constants.XSD_TAG_LF_URL);
                            if (newphewNodeIter.MoveNext() && newphewNodeIter.Current.Name == Constants.XSD_TAG_LF_URL)
                                tag.url = newphewNodeIter.Current.Value;

                            lfTrack.addTag(tag);
                        }

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_WIKI + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_SUMMARY);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_SUMMARY)
                            lfTrack.wiki_summary = NetUtils.RemoveHtmlAndRegexTags(locNodeIter.Current.Value, Constants.REGEX_CDATA_STR);//RemoveHtmlAndRegexTags(locNodeIter.Current.Value);

                        locNodeIter = node.Select(Constants.XSD_TAG_LF_WIKI + Constants.XSD_TAG_SEPARATOR + Constants.XSD_TAG_LF_CONTENT);
                        if (locNodeIter.MoveNext() && locNodeIter.Current.Name == Constants.XSD_TAG_LF_CONTENT)
                            lfTrack.wiki_content = NetUtils.RemoveHtmlAndRegexTags(locNodeIter.Current.Value, Constants.REGEX_CDATA_STR);//RemoveHtmlAndRegexTags(locNodeIter.Current.Value);
                    }
                    retval = Constants.RETVAL_MESSAGE_DONE;
                }
                else
                {
                    string err = string.Format(Constants.ERROR_ON_CALLING_WEB_SERVICE_FSTR, requestUrl);
                    log.Error(err);
                    retval = funName + err;
                    DBLogger.logWarn(retval);

                }
            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
                retval = funName + Constants.ERROR_LOADING;
                DBLogger.logWarn(retval);

            }

            return retval;
        }


        /* public static string parseXMLContent(string xmlcontent, out Playlist playlist)
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
                            retval = funName + string.Format(Constants.ERROR_PARSING_XML_FSTR, nodePos, "");
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
 */

        // "tAlbum.getTopTags" "tAlbum.getTags"
    }
}