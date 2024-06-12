namespace WiPapper.DB
{
    public class ImageDetails
    {
        private string _previewUrl;
        private string _wllpaperName;
        private string _wllpaperAutor;

        public string PreviewUrl
        {
            get => _previewUrl;
            set => _previewUrl = value;
        }

        public string WallpaperName
        {
            get => _wllpaperName;
            set => _wllpaperName = value;
        }

        public string WallpaperAutor
        {
            get => _wllpaperAutor;
            set => _wllpaperAutor = value;
        }

    }
}
