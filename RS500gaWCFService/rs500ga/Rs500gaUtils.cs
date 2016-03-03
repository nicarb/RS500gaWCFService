using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RS500gaWCFService.rs500ga
{
    public class Rs500gaUtils
    {
        public static string parseAndTranslateGenres(string genres)
        {
            string retval = string.Empty;
            char[] separators = new char[Constants.SEPARATORS_COUNT] { Constants.GENRES_SEPARATOR_1, Constants.GENRES_SEPARATOR_2 };
            string[] tokens = genres.Split(separators);
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

        public static string normaliseTitle(string title)
        {
            return title.ToLower().Replace(Constants.WHITESPACE_STRING, string.Empty);
        }

        public static string replace_blackListChars(string source)
        {
            return source.Replace(Constants.GA_RANK_BLK_INIT_STR, Constants.GA_RANK_WHT_INIT_STR).
                Replace(Constants.GA_RANK_BLK_LIST_1, Constants.GA_RANK_WHT_LIST_1).
                Replace(Constants.GA_RANK_BLK_LIST_2, Constants.GA_RANK_WHT_LIST_2).
                Replace(Constants.GA_RANK_BLK_LIST_3, Constants.GA_RANK_WHT_LIST_3).
                Replace(Constants.GA_RANK_BLK_LIST_4, Constants.GA_RANK_WHT_LIST_4).
                Replace(Constants.GA_RANK_BLK_LIST_5, Constants.GA_RANK_WHT_LIST_5).
                Replace(Constants.GA_RANK_BLK_LIST_6, Constants.GA_RANK_WHT_LIST_6).
                Replace(Constants.GA_RANK_BLK_LIST_7, Constants.GA_RANK_WHT_LIST_7).
                Replace(Constants.GA_RANK_BLK_LIST_8, Constants.GA_RANK_WHT_LIST_8).
                Replace(Constants.GA_RANK_BLK_LIST_9, Constants.GA_RANK_WHT_LIST_9).
                Replace(Constants.GA_RANK_BLK_LIST_10, Constants.GA_RANK_WHT_LIST_10).
                Replace(Constants.GA_RANK_BLK_LIST_11, Constants.GA_RANK_WHT_LIST_11).
                Replace(Constants.GA_RANK_BLK_LIST_12, Constants.GA_RANK_WHT_LIST_12).
                Replace(Constants.GA_RANK_BLK_LIST_13, Constants.GA_RANK_WHT_LIST_13).
                Replace(Constants.GA_RANK_BLK_LIST_14, Constants.GA_RANK_WHT_LIST_14).
                Replace(Constants.GA_RANK_BLK_LIST_15, Constants.GA_RANK_WHT_LIST_15).
                Replace(Constants.GA_RANK_BLK_LIST_16, Constants.GA_RANK_WHT_LIST_16).
                Replace(Constants.GA_RANK_BLK_LIST_17, Constants.GA_RANK_WHT_LIST_17).
                Replace(Constants.GA_RANK_BLK_LIST_18, Constants.GA_RANK_WHT_LIST_18).
                Replace(Constants.GA_RANK_BLK_LIST_19, Constants.GA_RANK_WHT_LIST_19).
                Replace(Constants.GA_RANK_BLK_LIST_20, Constants.GA_RANK_WHT_LIST_20).
                Replace(Constants.GA_RANK_BLK_LIST_21, Constants.GA_RANK_WHT_LIST_21).
                Replace(Constants.GA_RANK_BLK_LIST_22, Constants.GA_RANK_WHT_LIST_22).
                Replace(Constants.GA_RANK_BLK_LIST_23, Constants.GA_RANK_WHT_LIST_23).
                Replace(Constants.GA_RANK_BLK_LIST_24, Constants.GA_RANK_WHT_LIST_24).
                Replace(Constants.GA_RANK_BLK_LIST_25, Constants.GA_RANK_WHT_LIST_25).
                Replace(Constants.GA_RANK_BLK_LIST_26, Constants.GA_RANK_WHT_LIST_26);
        }

        public static void getAlbumGARank(string albumFullName, Track track)
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

    }
}