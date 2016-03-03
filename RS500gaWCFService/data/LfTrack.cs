using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RS500gaWCFService.data
{
    public class LfTrack
    {
        private string _name;
        private int _id;
        private int _rank;
        private int _dbId;
        private string _mbid;
        private string _url;
        private int _duration;
        private int _playcount;
        private double _parent_match;
        private string _artistName;
        private string _albumName;
        private string _parent_name;
        private LfArtist _artist;
        private LfAlbum _album;

        private List<LfTag> _tags = new List<LfTag>();
        private List<LfTrack> _similarTracks = new List<LfTrack>();
        
        private string _wiki_summary;
        private string _wiki_content;

        public string name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public string albumName
        {
            get
            {
                return this._albumName;
            }
            set
            {
                this._albumName = value;
            }
        }

        public string artistName
        {
            get
            {
                return this._artistName;
            }
            set
            {
                this._artistName = value;
            }
        }
        public int id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        public string mbid
        {
            get
            {
                return this._mbid;
            }
            set
            {
                this._mbid = value;
            }
        }

        public string url
        {
            get
            {
                return this._url;
            }
            set
            {
                this._url = value;
            }
        }

        public int playcount
        {
            get
            {
                return this._playcount;
            }
            set
            {
                this._playcount = value;
            }
        }

        public string parent_name
        {
            get
            {
                return this._parent_name;
            }
            set
            {
                this._parent_name = value;
            }
        }

        public string wiki_summary
        {
            get
            {
                return this._wiki_summary;
            }
            set
            {
                this._wiki_summary = value;
            }
        }

        public string wiki_content
        {
            get
            {
                return this._wiki_content;
            }
            set
            {
                this._wiki_content = value;
            }
        }

        public int dbId
        {
            get
            {
                return this._dbId;
            }
            set
            {
                this._dbId = value;
            }
        }

        public int duration
        {
            get
            {
                return this._duration;
            }

            set
            {
                this._duration = value;
            }
        }

        public int rank
        {
            get
            {
                return this._rank;
            }
            set
            {
                this._rank = value;
            }
        }

        public double parent_match
        {
            get
            {
                return this._parent_match;
            }
            set
            {
                this._parent_match = value;
            }
        }

        public LfArtist artist
        {
            get
            {
                return _artist;
            }
            set
            {
                this._artist = value;
            }
        }

        public LfAlbum album
        {
            get
            {
                return _album;
            }
            set
            {
                this._album = value;
            }
        }

        public List<LfTag> tags
        {
            get
            {
                return this._tags;
            }
        }

        public void addTag(LfTag tag)
        {
            this._tags.Add(tag);
        }


        public List<LfTrack> similarTrack
        {
            get
            {
                return this._similarTracks;
            }
        }

        public void addSimilarTrack(LfTrack track)
        {
            this.similarTrack.Add(track);
        }

        public override string ToString()
        {
            string retval =
            "<Id:" + this.dbId +
            "><Name:" + this.name +
            "><Artist:" + this.artistName +
            "><Album:" + this.albumName +">";

            return retval;
        }
    }
}