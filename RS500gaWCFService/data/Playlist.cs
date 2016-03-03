using RS500gaWCFService.data;
using RS500gaWCFService.RSPlaylistTypeNS;
using System;
using System.Collections.Generic;

namespace RS500gaWCFService.rs500ga
{
    public class Playlist
    {
        private string _title;
        private string _description;
        private int _db_id;
        private string _lf_id;
        private DateTime _date;
        //private int _size;
        //private int _duration;
        private List<LfArtist> _artists = new List<LfArtist>();
        private List<LfTrack> _tracks = new List<LfTrack>();
        private List<LfTrack> _candidate_tracks = new List<LfTrack>();
        private List<LfTrack> _source_tracks = new List<LfTrack>();
        private int _lim_track_nr;
        private int _lim_duration;
        private string _creator_url;
        private string _url;
        private string _username;
        private RSPlaylistType _type;

        public string title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
            }
        }

        public string description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }

        public List<LfArtist> artists
        {
            get
            {
                return this._artists;
            }
        }

        public void addArtist(LfArtist artist)
        {
            this._artists.Add(artist);
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

        public List<LfTrack> candidate_tracks
        {
            get
            {
                return this._candidate_tracks;
            }
        }

        public void addCandidateTracks(List<LfTrack> tracks)
        {
            foreach (LfTrack track in tracks)
            {
                this._candidate_tracks.Add(track);
            }
        }

        public List<LfTrack> source_tracks
        {
            get
            {
                return this._source_tracks;
            }
        }

        public void addSourceTracks(List<LfTrack> tracks)
        {
            foreach (LfTrack track in tracks)
            {
                this._source_tracks.Add(track);
            }
        }

        public int lim_track_nr
        {
            get
            {
                return this._lim_track_nr;
            }
            set
            {
                this._lim_track_nr = value;
            }
        }

        public int lim_duration
        {
            get
            {
                return this._lim_duration;
            }
            set
            {
                this._lim_duration = value;
            }
        }

        public DateTime date
        {
            get
            {
                return this._date;
            }
            set
            {
                this._date = value;
            }
        }

        public int db_id
        {
            get
            {
                return this._db_id;
            }
            set
            {
                this._db_id = value;
            }
        }

        public string lf_id
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

        public string creator_url
        {
            get
            {
                return this._creator_url;
            }
            set
            {
                this._creator_url = value;
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

        public string username
        {
            get
            {
                return this._username;
            }
            set
            {
                this._username = value;
            }
        }

        public RSPlaylistType type
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
    }
}