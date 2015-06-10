using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RS500gaWCFService
{
    public class Track
    {
        private int TrackID;
        private string Name;
        private string Artist;
        private int AlbumGARank;
        private string AlbumArtist;
        private string Album;
        private string Genre;
        private string Kind;
        private int Size;
        private int TotalTime;
        private int TrackNumber;
        private int Year;
        private DateTime DateModified;
        private DateTime DateAdded;
        private int BitRate;
        private int DiscNumber;
        private int DiscCount;
        private int TrackCount;
        private string Location;
        private int FileFolderCount;
        private int LibraryFolderCount;

        public int TRACK_ID
        {
            get
            {
                return this.TrackID;
            }
            set
            {
                this.TrackID = value;
            }
        }


        public string NAME
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }

        public string ARTIST
        {
            get
            {
                return this.Artist;
            }
            set
            {
                this.Artist = value;
            }
        }

        public int ALBUM_GA_RANK
        {
            get
            {
                return this.AlbumGARank;
            }
            set
            {
                this.AlbumGARank = value;
            }
        }

        public string ALBUM_ARTIST
        {
            get
            {
                return this.AlbumArtist;
            }
            set
            {
                this.AlbumArtist = value;
            }
        }

        public string ALBUM
        {
            get
            {
                return this.Album;
            }
            set
            {
                this.Album = value;
            }
        }

        public string GENRE
        {
            get
            {
                return this.Genre;
            }
            set
            {
                this.Genre = value;
            }
        }

        public string KIND
        {
            get
            {
                return this.Kind;
            }
            set
            {
                this.Kind = value;
            }
        }
        public int SIZE
        {
            get
            {
                return this.Size;
            }
            set
            {
                this.Size = value;
            }
        }
        public int TOTAL_TIME
        {
            get
            {
                return this.TotalTime;
            }
            set
            {
                this.TotalTime = value;
            }
        }
        public int TRACK_NUMBER
        {
            get
            {
                return this.TrackNumber;
            }
            set
            {
                this.TrackNumber = value;
            }
        }
        public int YEAR
        {
            get
            {
                return this.Year;
            }
            set
            {
                this.Year = value;
            }
        }
        public DateTime DATE_MODIFIED
        {
            get
            {
                return this.DateModified;
            }
            set
            {
                this.DateModified = value;
            }
        }
        public DateTime DATE_ADDED
        {
            get
            {
                return this.DateAdded;
            }
            set
            {
                this.DateAdded = value;
            }
        }
        public int BIT_RATE
        {
            get
            {
                return this.BitRate;
            }
            set
            {
                this.BitRate = value;
            }
        }
        public int DISC_NUMBER
        {
            get
            {
                return this.DiscNumber;
            }
            set
            {
                this.DiscNumber = value;
            }
        }

        public int DISC_COUNT
        {
            get
            {
                return this.DiscCount;
            }
            set
            {
                this.DiscCount = value;
            }
        }

        public int TRACK_COUNT
        {
            get
            {
                return this.TrackCount;
            }
            set
            {
                this.TrackCount = value;
            }
        }
        public string LOCATION
        {
            get
            {
                return this.Location;
            }
            set
            {
                this.Location = value;
            }
        }
        public int FILE_FOLDER_COUNT
        {
            get
            {
                return this.FileFolderCount;
            }
            set
            {
                this.FileFolderCount = value;
            }
        }

        public int LIBRARY_FOLDER_COUNT
        {
            get
            {
                return this.LibraryFolderCount;
            }
            set
            {
                this.LibraryFolderCount = value;
            }
        }

        public override string ToString()
        {
            string retval =
            "<TrackID:" + this.TRACK_ID +
            "><Name:" + this.NAME +
            "><Artist:" + this.ARTIST +
            "><AlbumGARank:" + this.ALBUM_GA_RANK +
            "><AlbumArtist:" + this.ALBUM_ARTIST +
            "><Album:" + this.ALBUM +
                "><Genre:" + this.GENRE +
                "><Kind:" + this.KIND +
                "><Size:" + this.SIZE +
                "><TotalTime:" + this.TOTAL_TIME +
                "><TrackNumber:" + this.TRACK_NUMBER +
                "><Year:" + this.YEAR +
                "><DateModified:" + this.DATE_MODIFIED.ToString() +
                "><DateAdded:" + this.DATE_ADDED.ToString() +
                "><BitRate:" + this.BIT_RATE +
                "><DiscNumber:" + this.DISC_NUMBER +
                "><DiscCount:" + this.DISC_COUNT +
                "><TrackCount:" + this.TRACK_COUNT +
                "><Location:" + this.LOCATION +
                "><FileFolderCount:" + this.FILE_FOLDER_COUNT +
                "><LibraryFolderCount:" + this.LIBRARY_FOLDER_COUNT + ">";
            return retval;
        }
    }
}