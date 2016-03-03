using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RS500gaWCFService.data
{
    public class LfArtist
    {
        private string _name;
        private int _id;
        private string _mbid;
        private string _url;
        private int _playcount;
        private string _parent_name = string.Empty;
        private double _parent_match = 0;
        //private Item<string, double> _match_with_parent = new Item<string, double>();
        private List<LfArtist> _similarArtists = new List<LfArtist>();
        private List<LfAlbum> _topAlbums = new List<LfAlbum>();
        private List<LfTrack> _topTracks = new List<LfTrack>();
        private List<LfTag> _tags = new List<LfTag>();
        private string _wiki_summary;
        private string _wiki_content;

        public void addSimilarArtists(LfArtist simArtist)
        {
            this._similarArtists.Add(simArtist);
        }

        public List<LfArtist> similarArtists
        {
            get
            {
                return this._similarArtists;
            }
        }

        public void addTopTrack(LfTrack track)
        {
            this._topTracks.Add(track);
        }

        public List<LfTrack> topTracks
        {
            get
            {
                return this._topTracks;
            }
        }

        public void addTag(LfTag tag) {
            this._tags.Add(tag);
        }

        public List<LfTag> tags
        {
            get
            {
                return this._tags;
            }
        }

        public void addAlbum(LfAlbum topAlbum)
        {
            this._topAlbums.Add(topAlbum);
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

        public List<LfAlbum> topAlbums
        {
            get
            {
                return this._topAlbums;
            }
        }

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

        public override string ToString()
        {
            string retval =
            "<Id:" + this.id +
            "><Name:" + this.name + ">";

            return retval;
        }
    }
}