using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiPapper.Wallpaper.HtmlWallpaper
{
    public class MediaProperties
    {
        private string _AlbumArtist;
        public string AlbumArtist
        {
            get => _AlbumArtist;
            set => _AlbumArtist = value;
        }

        private string _AlbumTitle;
        public string AlbumTitle
        {
            get => _AlbumTitle;
            set => _AlbumTitle = value;
        }

        private int _AlbumTrackCount;
        public int AlbumTrackCount
        {
            get => _AlbumTrackCount;
            set => _AlbumTrackCount = value;
        }

        private string _Artist;
        public string Artist
        {
            get => _Artist;
            set => _Artist = value;
        }

        private string _Genres;
        public string Genres
        {
            get => _Genres;
            set => _Genres = value;
        }

        private string _PlaybackType;
        public string PlaybackType
        {
            get => _PlaybackType;
            set => _PlaybackType = value;
        }

        private string _Subtitle;
        public string Subtitle
        {
            get => _Subtitle;
            set => _Subtitle = value;
        }

        private string _ThumbnailURL;
        public string ThumbnailURL // изменить, не забыть
        {
            get { return _ThumbnailURL; }
            set
            {
                //if (value != _ThumbnailURL)
                //{
                _ThumbnailURL = value;
                //}
            }
        }

        private string _Title;
        public string Title
        {
            get => _Title;
            set => _Title = value;
        }

        private int _TrackNumber;
        public int TrackNumber
        {
            get => _TrackNumber;
            set => _TrackNumber = value;
        }
    }
}
