namespace WiPapper.Wallpaper.HtmlWallpaper
{
    public class MediaProperties
    {
        private string _albumArtist;

        private string _albumTitle;

        private int _albumTrackCount;

        private string _artist;

        private string _genres;

        private string _playbackType;

        private string _subtitle;

        private string _thumbnailURL;

        private string _title;

        private int _trackNumber;

        public string AlbumArtist
        {
            get => _albumArtist;
            set => _albumArtist = value;
        }

        public string AlbumTitle
        {
            get => _albumTitle;
            set => _albumTitle = value;
        }

        public int AlbumTrackCount
        {
            get => _albumTrackCount;
            set => _albumTrackCount = value;
        }

        public string Artist
        {
            get => _artist;
            set => _artist = value;
        }

        public string Genres
        {
            get => _genres;
            set => _genres = value;
        }

        public string PlaybackType
        {
            get => _playbackType;
            set => _playbackType = value;
        }

        public string Subtitle
        {
            get => _subtitle;
            set => _subtitle = value;
        }

        public string ThumbnailURL // изменить, не забыть
        {
            get { return _thumbnailURL; }
            set
            {
                //if (value != _ThumbnailURL)
                //{
                _thumbnailURL = value;
                //}
            }
        }

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public int TrackNumber
        {
            get => _trackNumber;
            set => _trackNumber = value;
        }
    }
}
