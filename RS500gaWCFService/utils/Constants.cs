using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace RS500gaWCFService
{
    public class Constants
    {
        
        public const string ERROR_LOADING = "Error on loading";
        public const string ERROR_ON_CONNECTION = "ERROR ON CONNECTION";
        public const string ERROR_INPUT_PARAMETER_FSTR = "Error on input parameter {0} - {1}";
        public const string ERROR_PARSING_XML_FSTR = "Error on Parsing xml node {0} - {1}";
        public const string ERROR_NULL_POINTER = "Error: null pointer";
        public const string ERROR_NO_ARTISTS_OR_TRACKS = "Error: no artists or tracks to search";
        public const string FORMAT_STRING_2_PARAMS = "{0} - {1}";
        public const string ERROR_ON_CALLING_WEB_SERVICE_FSTR = "ERROR ON CALLING WEB_SERVICE:{0}";
        public const string ERROR_ON_CONVERTING_FSTR = "ERROR ON CONVERTING VALUE '{0}' IN '{1}'";
        

        public const string REGEX_CDATA_STR = "<![CDATA[.+?]]>";
        public const string REGEX_HREF_STR = "<a href=.+?/a>";

        public const string RETVAL_MESSAGE_DONE = "done";
        public const string RETVAL_MESSAGE_ERROR = "Error";
        

        public const string FULLTIME_FORMAT_STRING = "HHmmsstt";

        public const string XSD_KEY_MAJOR_VERSION = "Major Version";
        public const string XSD_KEY_MINOR_VERSION = "Minor Version";
        public const string XSD_KEY_APPLICATION_VERSION = "Application Version";
        public const string XSD_KEY_TRACKS = "Tracks";
        public const string XSD_KEY_TRACK_ID = "Track ID";
        public const string XSD_KEY_NAME = "Name";
        public const string XSD_KEY_ARTIST = "Artist";
        public const string XSD_KEY_ALBUM_ARTIST = "Album Artist";
        public const string XSD_KEY_ALBUM = "Album";
        public const string XSD_KEY_GENRE = "Genre";
        public const string XSD_KEY_KIND = "Kind";
        public const string XSD_KEY_SIZE = "Size";
        public const string XSD_KEY_TOTAL_TIME = "Total Time";
        public const string XSD_KEY_TRACK_NUMBER = "Track Number";
        public const string XSD_KEY_YEAR = "Year";
        public const string XSD_KEY_DATE_MODIFIED = "Date Modified";
        public const string XSD_KEY_DATE_ADDED = "Date Added";
        public const string XSD_KEY_BIT_RATE = "Bit Rate";
        public const string XSD_KEY_DISC_NUMBER = "Disc Number";
        public const string XSD_KEY_DISC_COUNT = "Disc Count";
        public const string XSD_KEY_TRACK_COUNT = "Track Count";
        public const string XSD_KEY_LOCATION = "Location";
        public const string XSD_KEY_FILE_FOLDER_COUNT = "File Folder Count";
        public const string XSD_KEY_LIBRARY_FOLDER_COUNT = "Library Folder Count";
        public const string XSD_TAG_PLIST = "plist";
        public const string XSD_TAG_DICT = "dict";
        public const string XSD_TAG_KEY = "key";
        public const string XSD_TAG_INTEGER = "integer";
        public const string XSD_TAG_STRING = "string";
        public const string XSD_TAG_DATE = "date";
        public const string XSD_TAG_SEPARATOR = "/";

        public const string XSD_TAG_LF_LFM = "lfm";
        public const string XSD_TAG_LF_SIMILARARTISTS = "similarartists";
        public const string XSD_TAG_LF_ARTISTTOPALBUMS = "topalbums";
        public const string XSD_TAG_LF_SIMILARTRACKS = "similartracks";
        public const string XSD_TAG_LF_PLAYLISTS = "playlists";

        public const string XSD_TAG_PLI_RS500GA = "rs500ga";
        public const string XSD_TAG_PLI_PLAYLIST_INPUT = "playlist_input";
        public const string XSD_TAG_PLI_PLAYLIST = "playlist";
        public const string XSD_TAG_PLI_TITLE = "title";
        public const string XSD_TAG_PLI_DESCRIPTION = "description";
        public const string XSD_TAG_PLI_ARTISTS = "artists";
        public const string XSD_TAG_PLI_ARTIST = "artist";
        public const string XSD_TAG_PLI_NAME = "name";
        public const string XSD_TAG_PLI_TRACKS = "tracks";
        public const string XSD_TAG_PLI_TRACK = "track";
        public const string XSD_TAG_PLI_LIMITS = "limits";
        public const string XSD_TAG_PLI_TRACKS_NR = "tracks_nr";
        public const string XSD_TAG_PLI_DURATION = "duration";
        public const string XSD_TAG_PLI_OUTPUT = "output";
        public const string XSD_TAG_PLI_TYPE = "type";

        public const string XSD_TAG_LF_ERROR = "error";
        public const string XSD_TAG_LF_STATUS = "status";
        public const string XSD_TAG_LF_FAILED = "failed";
        public const string XSD_TAG_LF_CODE = "code";

        public const string XSD_TAG_LF_ALBUM = "album";
        public const string XSD_TAG_LF_ARTIST = "artist";
        public const string XSD_TAG_LF_NAME = "name";
        public const string XSD_TAG_LF_MATCH = "match";
        public const string XSD_TAG_LF_MBID = "mbid";
        public const string XSD_TAG_LF_URL = "url";
        public const string XSD_TAG_LF_ID = "id";
        public const string XSD_TAG_LF_RANK = "rank";
        public const string XSD_TAG_LF_PLAYCOUNT = "playcount";
        public const string XSD_TAG_LF_DURATION = "duration";
        public const string XSD_TAG_LF_TRACKS = "tracks";
        public const string XSD_TAG_LF_TRACK = "track";
        public const string XSD_TAG_LF_RELEASEDATE = "releasedate";
        public const string XSD_TAG_LF_WIKI = "wiki";
        public const string XSD_TAG_LF_SUMMARY = "summary";
        public const string XSD_TAG_LF_CONTENT = "content";
        public const string XSD_TAG_LF_TAGS = "tags";
        public const string XSD_TAG_LF_TAG = "tag";
        public const string XSD_TAG_LF_POSITION = "position";
        public const string XSD_TAG_LF_TITLE = "title";
        public const string XSD_TAG_LF_TOPTAGS = "toptags";
        public const string XSD_TAG_LF_TOKEN = "token";
        public const string XSD_TAG_LF_SESSION = "session";
        public const string XSD_TAG_LF_KEY = "key";
        public const string XSD_TAG_LF_PLAYLIST = "playlist";
        public const string XSD_TAG_LF_USER = "user";
        public const string XSD_TAG_LF_DESCRIPTION = "description";
        public const string XSD_TAG_LF_SIZE = "size";
        public const string XSD_TAG_LF_CREATOR = "creator";
        public const string XSD_TAG_LF_DATE = "date";
        public const string XSD_TAG_LF_BIO = "bio";

        public const string LF_TADETIME_FORMAT = "d MMM yyyy, HH:mm";
        public const string LF_TADETIME_FORMAT1 = "yyyy-MM-ddTHH:mm:ss";
        public const string WHITESPACE_STRING = " ";
        public const char GENRES_SEPARATOR_1 = '/';
        public const char GENRES_SEPARATOR_2 = '-';
        public const int SEPARATORS_COUNT = 2;

        public const string APP_SETTINGS_LYRICSURI = "lyricsURI";
        public const string APP_SETTINGS_XMLPATH = "xmlPath";
        public const string APP_SETTINGS_PLAYLIST_XSDFILE = "xsdFile";
        public const string APP_SETTINGS_LF_SIMILAR_ARTIST_XSDFILE = "lf_similar_artists_xsd";
        public const string APP_SETTINGS_LF_SIMILAR_TRACK_XSDFILE = "lf_similar_tracks_xsd";
        public const string APP_SETTINGS_LF_ARTIST_TOP_ALBUMS_XSDFILE = "lf_artist_top_albums_xsd";
        public const string APP_SETTINGS_LF_ARTIST_TOP_TRACKS_XSDFILE = "lf_artist_top_tracks_xsd";
        public const string APP_SETTINGS_LF_ALBUM_GET_INFO_XSDFILE = "lf_album_get_info_xsd";
        public const string APP_SETTINGS_LF_TRACK_GET_INFO_XSDFILE = "lf_track_get_info_xsd";
        public const string APP_SETTINGS_LF_AUTH_GET_TOKEN_XSDFILE = "lf_auth_get_token_xsd";
        public const string APP_SETTINGS_LF_AUTH_GET_SESSION_XSDFILE = "lf_auth_get_session_xsd";
        public const string APP_SETTINGS_PLAYLIST_GEN_INPUT_XSDFILE = "playlist_gen_input_xsd";
        public const string APP_SETTINGS_PLAYLIST_CREATE_XSDFILE = "playlist_create_xsd";
        public const string APP_SETTINGS_LF_ARTIST_GET_INFO_XSDFILE = "lf_artist_get_info_xsd";

        public const string APP_SETTINGS_OUTPUTPATH = "outputPath";
        public const string APP_SETTINGS_OUTPUTFILENAME = "rejectedTracksFN";
        public const string APP_SETTINGS_OUTPUTEXTENSION = "outputExtension";

        public const string APP_SETTINGS_CS_SERVER = "CS_SERVER";
        public const string APP_SETTINGS_CS_DATABASE = "CS_DATABASE";
        public const string APP_SETTINGS_LOG_DATABASE = "LOG_DATABASE";
        public const string APP_SETTINGS_CS_UID = "CS_UID";
        public const string APP_SETTINGS_CS_PASSWORD = "CS_PASSWORD";

        public const string CONNECTION_STRING_FORMATTED = "SERVER={0}; DATABASE={1};UID={2};PASSWORD={3};";

        public const string PLAYLIST_TYPE_MANUAL = "manual";
        public const string PLAYLIST_MANUAL_DESC = "manualy created by 'sendDataToDb' service - 2";
        public const string PLAYLIST_TYPE_MANUAL_DESC = "manualy created by 'sendDataToDb' service - 2";

        public const string PLAYLIST_TYPE_RS500GA = "rs500gaWS";
        public const string PLAYLIST_RS500GA_DESC = "automatically created for generation of a new playlist";
        public const string PLAYLIST_TYPE_RS500GA_DESC = "automatically created for generation of a new playlist";

        public const string STORED_PROC_ADD_NEW_PLAYLIST = "add_new_playlist";
        public const string STORED_PROC_ADD_NEW_PLAYLIST_RS = "add_track_to_playlist_RS";
        public const string STORED_PROC_ADD_TRACK_TO_PLAYLIST = "add_track_to_playlist";
        public const string STORED_PROC_ADD_TRACK_TO_PLAYLIST_RS = "add_track_to_playlist_RS";
        public const string STORED_PROC_GET_OR_ADD_ARTIST_LF = "get_or_add_Artist_LF";
        public const string STORED_PROC_ADD_OR_UPDATE_MATCH_ARTIST_SIMILARS_LF = "add_or_update_match_artist_similars_LF";
        public const string STORED_PROC_GET_OR_ADD_ALBUM_LF = "get_or_add_Album_LF";
        public const string STORED_PROC_GET_OR_ADD_TRACK_LF = "get_or_add_Track_LF";
        public const string STORED_PROC_GET_OR_ADD_TAG = "get_or_add_tag";
        public const string STORED_PROC_ADD_OR_UPDATE_MATCH_TRACK_SIMILARS_LF = "add_or_update_match_track_similars_LF";
        public const string STORED_PROC_UPDATE_TWO_NOT_DIRECT_CONNECTED_TRACKS = "update_two_not_direct_connected_tracks";
        public const string STORED_PROC_UPDATE_TWO_NOT_DIRECT_CONNECTED_ARTISTS = "update_two_not_direct_connected_artists";
        public const string STORED_PROC_ADD_OR_UPDATE_TRACK_TAGS_LF = "add_or_update_match_track_tag";
        public const string STORED_PROC_ADD_OR_UPDATE_ALBUM_TAGS_LF = "add_or_update_match_album_tag";
        public const string STORED_PROC_ADD_OR_UPDATE_ARTIST_TAGS_LF = "add_or_update_match_artist_tag";
        public const string STORED_PROC_GET_ALL_RS500GA_ALBUMS = "get_all_rs500ga_albums";
        public const string STORED_PROC_GET_ALL_RS500GA_TRACKS = "get_all_rs500ga_tracks";
        public const string STORED_PROC_GET_ALL_RS500GA_ARTISTS = "get_all_rs500ga_artists";

        public const string STORED_PROC_LOG_APP_ERROR_PROC = "app_log_error_proc";

        public const string SP_PARAM_LOG_ERR_LOG_LEVEL = "p_log_level";
        public const string SP_PARAM_LOG_ERR_ERR_DESC = "p_err_desc";
        public const string SP_PARAM_LOG_ERR_FILENAME = "p_filename";
        public const string SP_PARAM_LOG_ERR_ERR_FUN_NAME = "p_err_fun_name";
        public const string SP_PARAM_LOG_ERR_ERR_FILENAME = "p_err_filename";
        public const string SP_PARAM_LOG_ERR_ERR_ROW_NR = "p_err_row_nr";

        public const int MYSQL_TINYTEXT_LEN = 255; // 255 bytes
        public const int MYSQL_SHORTTEXT_2048 = 2048; // 2Kbytes
        public const int MYSQL_TEXT_LEN = 65535; // 64 KiB
        public const int MYSQL_MEDIUMTEXT_LEN = 16777215; // 16 MiB
        public const int MYSQL_LONGTEXT_LEN = 255; // 4 GiB


        public const string SP_PARAM_PLAYLIST_TITLE = "playlistTitle";
        public const string SP_PARAM_PLAYLIST_DESC = "playlistDesc";
        public const string SP_PARAM_PLAYLIST_TYPE = "playlistType";
        public const string SP_PARAM_PLAYLIST_TYPE_DESC = "playlistTypeDesc";
        public const string SP_PARAM_PLAYLIST_ID = "playlist_id";
        public const string SP_PARAM_TRACK_ID = "TrackID";
        public const string SP_PARAM_NAME = "tName";
        public const string SP_PARAM_ARTIST = "Artist";
        public const string SP_PARAM_ALBUM_GA_RANK = "AlbumGARank";
        public const string SP_PARAM_ALBUM_ARTIST = "AlbumArtist";
        public const string SP_PARAM_ALBUM = "Album";
        public const string SP_PARAM_GENRE = "Genre";
        public const string SP_PARAM_KIND = "Kind";
        public const string SP_PARAM_SIZE = "tSize";
        public const string SP_PARAM_TOTAL_TIME = "TotalTime";
        public const string SP_PARAM_TRACK_NUMBER = "TrackNumber";
        public const string SP_PARAM_YEAR = "tYear";
        public const string SP_PARAM_DATE_MODIFIED = "DateModified";
        public const string SP_PARAM_DATE_ADDED = "DateAdded";
        public const string SP_PARAM_BIT_RATE = "BitRate";
        public const string SP_PARAM_DISC_NUMBER = "DiscNumber";
        public const string SP_PARAM_DISC_COUNT = "DiscCount";
        public const string SP_PARAM_TRACK_COUNT = "TrackCount";
        public const string SP_PARAM_LOCATION = "Location";
        public const string SP_PARAM_CLEAN_LOC = "CleanLoc";
        public const string SP_PARAM_FILE_FOLDER_COUNT = "FileFolderCount";
        public const string SP_PARAM_LIBRARY_FOLDER_COUNT = "LibraryFolderCount";

        public const string SP_GET_ARTIST_PARAM_NAME = "p_aname";
        public const string SP_GET_ARTIST_PARAM_DESC = "p_adesc";
        public const string SP_GET_ARTIST_PARAM_ID_ARTIST = "p_idArtist";

        public const string SP_PARAM_MATCH_ARTIST_IDARTIST1 = "p_idArtist1";
        public const string SP_PARAM_MATCH_ARTIST_IDARTIST2 = "p_idArtist2";
        public const string SP_PARAM_MATCH_ARTIST_MATCH = "p_match";
        public const string SP_PARAM_MATCH_ARTIST_ASDESC = "p_asdesc";

        public const string SP_PARAM_MATCH_TRACK_IDTRACK1 = "p_idTrack1";
        public const string SP_PARAM_MATCH_TRACK_IDTRACK2 = "p_idTrack2";
        public const string SP_PARAM_MATCH_TRACK_MATCH = "p_match";
        public const string SP_PARAM_MATCH_TRACK_ASDESC = "p_asdesc";

        public const string SP_PARAM_UPDATE_TAG_ID_TRACK = "p_idTrack";
        public const string SP_PARAM_UPDATE_TAG_ID_TAG = "p_idTag";
        public const string SP_PARAM_UPDATE_TAG_ID_DESC = "p_agdesc";
        public const string SP_PARAM_UPDATE_TAG_ID_ARTIST = "p_idArtist";
        public const string SP_PARAM_UPDATE_TAG_ID_ALBUM = "p_idAlbum";

        public const string SP_GET_ALBUM_LF_PARAM_NAME = "p_aname";
        public const string SP_GET_ALBUM_LF_PARAM_SUBTITLE = "p_subtitle";
        public const string SP_GET_ALBUM_LF_PARAM_DESC = "p_adesc";
        public const string SP_GET_ALBUM_LF_PARAM_NOTE = "p_note";
        public const string SP_GET_ALBUM_LF_PARAM_ID_ARTIST = "p_idArtist";
        public const string SP_GET_ALBUM_LF_PARAM_RANK = "p_rank";
        public const string SP_GET_ALBUM_LF_PARAM_YEAR = "p_ayear";
        public const string SP_GET_ALBUM_LF_PARAM_NR_OF_CDS = "p_nr_of_cds";
        public const string SP_GET_ALBUM_LF_PARAM_NR_OF_TRACKS = "p_nr_of_tracks";
        public const string SP_GET_ALBUM_LF_PARAM_PLAYCOUNT = "p_playcount";
        public const string SP_GET_ALBUM_LF_PARAM_ID_ALBUM = "p_idAlbum";

        public const string SP_GET_TRACK_LF_PARAM_TITLE = "p_title";
        public const string SP_GET_TRACK_LF_PARAM_DESC = "p_adesc";
        public const string SP_GET_TRACK_LF_PARAM_NOTE = "p_note";
        public const string SP_GET_TRACK_LF_PARAM_ID_ARTIST = "p_idArtist";
        public const string SP_GET_TRACK_LF_PARAM_ID_ALBUM = "p_idAlbum";
        public const string SP_GET_TRACK_LF_PARAM_RANK = "p_rank";
        public const string SP_GET_TRACK_LF_PARAM_PLAYCOUNT = "p_playcount";
        public const string SP_GET_TRACK_LF_PARAM_ID_TRACK = "p_idTrack";

        public const string SP_GET_ALBUM_LF_PARAM_ID_TAG = "p_idTag";

        public const string SP_PARAM_RESULT = "result";
        public const string SP_PARAM_VRESULT = "vresult";
        public const string SP_PARAM_MBID = "p_mbid";
        public const string SP_PARAM_URL = "p_url";

        public const string SP_TABLE_COLUMN_IDALBUM = "idAlbum";
        public const string SP_TABLE_COLUMN_TITLE = "title";
        public const string SP_TABLE_COLUMN_ATITLE = "atitle";
        public const string SP_TABLE_COLUMN_TTITLE = "ttitle";
        public const string SP_TABLE_COLUMN_RANK_RS_GA = "rank_rs_ga";
        public const string SP_TABLE_COLUMN_ARTIST = "artist";
        public const string SP_TABLE_COLUMN_ANAME = "aname";
        public const string SP_TABLE_COLUMN_IDTRACK = "idtrack";
        public const string SP_TABLE_COLUMN_IDARTIST = "idArtist";
        //, , , , 

        public const int SP_RESULT_OK = 0;

        public const int GA_RANK_DEF_STR_INIT = 0;
        public const int GA_RANK_DEF_STR_LEN = 3;

        public const int LF_ARTIST_TOP_ALBUMS_LIMIT = 10;
        public const int LF_SIMILAR_ARTISTS_LIMIT = 30;
        public const int LF_ALL_SIMILAR_ARTISTS_LIMIT = 50;
        public const int LF_ARTIST_TOP_TRACKS_LIMIT = 10;
        public const int LF_SIMILAR_TRACKS_LIMIT = 50;

        public const char GA_RANK_DEF_SPLIT_CHAR = '#';
        public const string GA_RANK_BLK_INIT_STR = @"file://localhost/";
        public const string GA_RANK_BLK_LIST_1 = "%5C";
        public const string GA_RANK_BLK_LIST_2 = "%5B";
        public const string GA_RANK_BLK_LIST_3 = "%5D";
        public const string GA_RANK_BLK_LIST_4 = "%28";
        public const string GA_RANK_BLK_LIST_5 = "%29";
        public const string GA_RANK_BLK_LIST_6 = "%20";
        public const string GA_RANK_BLK_LIST_7 = "%23";
        public const string GA_RANK_BLK_LIST_8 = "%27";
        public const string GA_RANK_BLK_LIST_9 = "%21";
        public const string GA_RANK_BLK_LIST_10 = "%24";
        public const string GA_RANK_BLK_LIST_11 = "%25";
        public const string GA_RANK_BLK_LIST_12 = "%26";
        public const string GA_RANK_BLK_LIST_13 = "%2B";
        public const string GA_RANK_BLK_LIST_14 = "%2C";
        public const string GA_RANK_BLK_LIST_15 = "%2D";
        public const string GA_RANK_BLK_LIST_16 = "%2E";
        public const string GA_RANK_BLK_LIST_17 = "%2F";
        public const string GA_RANK_BLK_LIST_18 = "%3D";
        public const string GA_RANK_BLK_LIST_19 = "%3C";
        public const string GA_RANK_BLK_LIST_20 = "%3E";
        public const string GA_RANK_BLK_LIST_21 = "%60";
        public const string GA_RANK_BLK_LIST_22 = "%80";
        public const string GA_RANK_BLK_LIST_23 = "%91";
        public const string GA_RANK_BLK_LIST_24 = "%92";
        public const string GA_RANK_BLK_LIST_25 = "%93";
        public const string GA_RANK_BLK_LIST_26 = "%94";

        public const string GA_RANK_WHT_INIT_STR = "";
        public const string GA_RANK_WHT_LIST_1 = @"\";
        public const string GA_RANK_WHT_LIST_2 = "[";
        public const string GA_RANK_WHT_LIST_3 = "]";
        public const string GA_RANK_WHT_LIST_4 = "(";
        public const string GA_RANK_WHT_LIST_5 = ")";
        public const string GA_RANK_WHT_LIST_6 = " ";
        public const string GA_RANK_WHT_LIST_7 = "#";
        public const string GA_RANK_WHT_LIST_8 = "'";
        public const string GA_RANK_WHT_LIST_9 = "!";
        public const string GA_RANK_WHT_LIST_10 = "$";
        public const string GA_RANK_WHT_LIST_11 = "%";
        public const string GA_RANK_WHT_LIST_12 = "&";
        public const string GA_RANK_WHT_LIST_13 = "+";
        public const string GA_RANK_WHT_LIST_14 = ",";
        public const string GA_RANK_WHT_LIST_15 = "-";
        public const string GA_RANK_WHT_LIST_16 = ".";
        public const string GA_RANK_WHT_LIST_17 = "/";
        public const string GA_RANK_WHT_LIST_18 = "=";
        public const string GA_RANK_WHT_LIST_19 = "<";
        public const string GA_RANK_WHT_LIST_20 = ">";
        public const string GA_RANK_WHT_LIST_21 = "`";
        public const string GA_RANK_WHT_LIST_22 = "`";
        public const string GA_RANK_WHT_LIST_23 = "‘";
        public const string GA_RANK_WHT_LIST_24 = "’";
        public const string GA_RANK_WHT_LIST_25 = "“";
        public const string GA_RANK_WHT_LIST_26 = "”";

        public const int GA_RANK_UDEFINED = -1;

        public const string TRANSLATE_RU_W1 = "сатерн";
        public const string TRANSLATE_EN_W1 = "southern";
        public const string TRANSLATE_RU_W2 = "фанк";
        public const string TRANSLATE_EN_W2 = "funk";
        public const string TRANSLATE_RU_W3 = "соул";
        public const string TRANSLATE_EN_W3 = "soul";
        public const string TRANSLATE_RU_W4 = "поп";
        public const string TRANSLATE_EN_W4 = "pop";
        public const string TRANSLATE_RU_W5 = "блюз";
        public const string TRANSLATE_EN_W5 = "blues";
        public const string TRANSLATE_RU_W6 = "рок";
        public const string TRANSLATE_EN_W6 = "rock";
        public const string TRANSLATE_RU_W7 = "арт";
        public const string TRANSLATE_EN_W7 = "art";
        public const string TRANSLATE_RU_W8 = "электронная";
        public const string TRANSLATE_EN_W8 = "electronic";
        public const string TRANSLATE_RU_W9 = "кантри";
        public const string TRANSLATE_EN_W9 = "country";
        public const string TRANSLATE_RU_W10 = "фолк";
        public const string TRANSLATE_EN_W10 = "folk";
        public const string TRANSLATE_RU_W11 = "мировая";
        public const string TRANSLATE_EN_W11 = "world";
        public const string TRANSLATE_RU_W12 = "хип-хоп";
        public const string TRANSLATE_EN_W12 = "hip-hop";
        public const string TRANSLATE_RU_W13 = "рэп";
        public const string TRANSLATE_EN_W13 = "rap";
        public const string TRANSLATE_RU_W14 = "инди";
        public const string TRANSLATE_EN_W14 = "indie";
        public const string TRANSLATE_RU_W15 = "гангста";
        public const string TRANSLATE_EN_W15 = "gangsta";
        public const string TRANSLATE_RU_W16 = "психоделик";
        public const string TRANSLATE_EN_W16 = "psychedelic";
        public const string TRANSLATE_RU_W17 = "джаз";
        public const string TRANSLATE_EN_W17 = "jazz";
        public const string TRANSLATE_RU_W18 = "модальный джаз";
        public const string TRANSLATE_EN_W18 = "modal jazz";
        public const string TRANSLATE_RU_W19 = "фьюжн";
        public const string TRANSLATE_EN_W19 = "fusion";
        public const string TRANSLATE_RU_W20 = "глэм";
        public const string TRANSLATE_EN_W20 = "glam";
        public const string TRANSLATE_RU_W21 = "латинский рок";
        public const string TRANSLATE_EN_W21 = "latin rock";
        public const string TRANSLATE_RU_W22 = "фри джаз";
        public const string TRANSLATE_EN_W22 = "free jazz";
        public const string TRANSLATE_RU_W23 = "гаражный рок";
        public const string TRANSLATE_EN_W23 = "garage rock";
        public const string TRANSLATE_RU_W24 = "гранж";
        public const string TRANSLATE_EN_W24 = "grunge";
        public const string TRANSLATE_RU_W25 = "латинский";
        public const string TRANSLATE_EN_W25 = "latin";
        public const string TRANSLATE_RU_W26 = "синтипоп";
        public const string TRANSLATE_EN_W26 = "synthpop";
        public const string TRANSLATE_RU_W27 = "чикагский блюз";
        public const string TRANSLATE_EN_W27 = "chicago blues";
        public const string TRANSLATE_RU_W28 = "электро";
        public const string TRANSLATE_EN_W28 = "electro";
        public const string TRANSLATE_RU_W29 = "хард";
        public const string TRANSLATE_EN_W29 = "hard";
        public const string TRANSLATE_RU_W30 = "латин";
        public const string TRANSLATE_EN_W30 = "latin";
        //боп   нойз    регги   софт

        
        
        public static List<string> WHITE_LIST_EN_GENRES
        {
            get
            {
                List<string> white_list_genres = new List<string>();
                white_list_genres.Add(TRANSLATE_EN_W12);

                return white_list_genres;
            }
        }

        public static List<string> WHITE_LIST_RU_GENRES
        {
            get
            {
                List<string> white_list_genres = new List<string>();
                white_list_genres.Add(TRANSLATE_RU_W12);

                return white_list_genres;
            }
        }

        public static Dictionary<string, string> TRANSLATE_RU_EN
        {
            get
            {
                Dictionary<string, string> translate_ru_en = new Dictionary<string, string>();
                translate_ru_en.Add(TRANSLATE_RU_W1, TRANSLATE_EN_W1);
                translate_ru_en.Add(TRANSLATE_RU_W2, TRANSLATE_EN_W2);
                translate_ru_en.Add(TRANSLATE_RU_W3, TRANSLATE_EN_W3);
                translate_ru_en.Add(TRANSLATE_RU_W4, TRANSLATE_EN_W4);
                translate_ru_en.Add(TRANSLATE_RU_W5, TRANSLATE_EN_W5);
                translate_ru_en.Add(TRANSLATE_RU_W6, TRANSLATE_EN_W6);
                translate_ru_en.Add(TRANSLATE_RU_W7, TRANSLATE_EN_W7);
                translate_ru_en.Add(TRANSLATE_RU_W8, TRANSLATE_EN_W8);
                translate_ru_en.Add(TRANSLATE_RU_W9, TRANSLATE_EN_W9);
                translate_ru_en.Add(TRANSLATE_RU_W10, TRANSLATE_EN_W10);
                translate_ru_en.Add(TRANSLATE_RU_W11, TRANSLATE_EN_W11);
                translate_ru_en.Add(TRANSLATE_RU_W12, TRANSLATE_EN_W12);
                translate_ru_en.Add(TRANSLATE_RU_W13, TRANSLATE_EN_W13);
                translate_ru_en.Add(TRANSLATE_RU_W14, TRANSLATE_EN_W14);
                translate_ru_en.Add(TRANSLATE_RU_W15, TRANSLATE_EN_W15);
                translate_ru_en.Add(TRANSLATE_RU_W16, TRANSLATE_EN_W16);
                translate_ru_en.Add(TRANSLATE_RU_W17, TRANSLATE_EN_W17);
                translate_ru_en.Add(TRANSLATE_RU_W18, TRANSLATE_EN_W18);
                translate_ru_en.Add(TRANSLATE_RU_W19, TRANSLATE_EN_W19);
                translate_ru_en.Add(TRANSLATE_RU_W20, TRANSLATE_EN_W20);
                translate_ru_en.Add(TRANSLATE_RU_W21, TRANSLATE_EN_W21);
                translate_ru_en.Add(TRANSLATE_RU_W22, TRANSLATE_EN_W22);
                translate_ru_en.Add(TRANSLATE_RU_W23, TRANSLATE_EN_W23);
                translate_ru_en.Add(TRANSLATE_RU_W24, TRANSLATE_EN_W24);
                translate_ru_en.Add(TRANSLATE_RU_W25, TRANSLATE_EN_W25);
                translate_ru_en.Add(TRANSLATE_RU_W26, TRANSLATE_EN_W26);
                //translate_ru_en.Add(TRANSLATE_RU_W27, TRANSLATE_EN_W27);
                translate_ru_en.Add(TRANSLATE_RU_W28, TRANSLATE_EN_W28);
                translate_ru_en.Add(TRANSLATE_RU_W29, TRANSLATE_EN_W29); 
                
                return translate_ru_en;
            }
        }

    }
}