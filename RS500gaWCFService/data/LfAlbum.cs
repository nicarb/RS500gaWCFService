using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace RS500gaWCFService.data
{
    public class LfAlbum
    {
        private string _name;
        private int _lf_id;
        private LfArtist _artist;
        private string _artistName;
        private int _id_artist;

        private string _mbid;
        private string _url;
        private DateTime _releasedate;
        //private DataTable _tracks;
        private int _tracks_count;
        private int _playcount;
        private string _wiki_summary;
        private string _wiki_content;
        private int _id;
        private int _rank;
        private List<LfTrack> _tracks = new List<LfTrack>();
        private List<LfTag> _tags = new List<LfTag>();

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

        public LfArtist artist
        {
            get
            {
                return this._artist;
            }
            set
            {
                this._artist = value;
            }
        }

        public int idArtist
        {
            get
            {
                return this._id_artist;
            }
            set
            {
                this._id_artist = value;
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


        public int lf_id
        {
            get
            {
                return this._lf_id;
            }
            set
            {
                this._lf_id = value;
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

        public DateTime releasedate
        {
            get
            {
                return this._releasedate;
            }
            set
            {
                this._releasedate = value;
            }
        }

        public List<LfTrack> tracks
        {
            get
            {
                return this._tracks;
            }
        }

        public void addTrack(LfTrack track)
        {
            this._tracks.Add(track);
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
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        public int tracks_count
        {
            get
            {
                return this._tracks_count;
            }
            set
            {
                this._tracks_count = value;
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

        public override string ToString()
        {
            string retval =
            "<Id:" + this.dbId +
            "><Album:" + this.name +
            "><Artist:" + this.artistName +
            "><AlbumRank:" + this.rank + ">";

            return retval;
        }
    }
}