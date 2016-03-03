using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RS500gaWCFService.RSPlaylistTypeNS
{
    public class RSPlaylistType
    {
        public enum PlType
        {
            UNDEF, SPOTIFY, LASTFM, XSPF, M3U
        }

        private PlType _type;

        private const string T_UNDEF = "UNDEF";
        private const string T_SPOTIFY = "SPOTIFY";
        private const string T_LASTFM = "LASTFM";
        private const string T_XSPF = "XSPF";
        private const string T_M3U = "M3U";

        public RSPlaylistType()
        {
            this._type = PlType.UNDEF;
        }

        public RSPlaylistType(string type)
        {
            switch (type)
            {
                case T_SPOTIFY: 
                    this._type = PlType.SPOTIFY; break;
                case T_LASTFM: 
                    this._type = PlType.LASTFM; break; 
                case T_XSPF: 
                    this._type = PlType.XSPF; break;
                case T_M3U: 
                    this._type = PlType.M3U; break;
                default: 
                    this._type = PlType.UNDEF; break;
            }
        }


        public PlType type
        {
            get
            {
                return this._type;
            }
            set
            {
                this._type = value;
            }
        }

        public string getString(PlType type) {
            switch (type)
            {
                case PlType.SPOTIFY: return T_SPOTIFY;
                case PlType.LASTFM: return T_LASTFM;
                case PlType.XSPF: return T_XSPF;
                case PlType.M3U: return T_M3U;
                default: return T_UNDEF;
            }
        }

    }
}