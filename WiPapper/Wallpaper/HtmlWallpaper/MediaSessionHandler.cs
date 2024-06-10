using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace WiPapper.Wallpaper.HtmlWallpaper
{
    internal static class MediaSessionHandler
    {
        private static string oldThumbnailUrl;
        private static readonly MediaProperties mediaProperties = new MediaProperties();
        private static readonly PlaybackInfo playbackInfo = new PlaybackInfo();
        private static GlobalSystemMediaTransportControlsSession session;
        static GlobalSystemMediaTransportControlsSessionManager sessionManager = null;

        public static void IsBrowserInitialized(object sender, DependencyPropertyChangedEventArgs e)
        {
            sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult();
            GlobalSystemMediaTransportControlsSession session = sessionManager.GetCurrentSession();

            sessionManager.CurrentSessionChanged += SessionManager_CurrentSessionChanged; // работает между разными приложениями
            SetHtmlWallpaper.Browser.FrameLoadEnd += Browser_FrameLoadEnd;

            //SetHtmlWallpaper.Browser.ShowDevTools();// убрать или сделать для разработчиков
            UpdateSession(session);
        }

        private static void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs args)
        {
            if (args.Frame.IsMain)
            {

                AudioProcessor.RecordAudioData();
                //await RecordAudioData();


                SetHtmlWallpaper.Browser.ShowDevTools();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    //SetHtmlWallpaper.browser.ExecuteScriptAsync("updateInfo", mediaProperties.Artist, mediaProperties.Title, mediaProperties.ThumbnailURL);
                    UpdateInfo();
                });
            }
        }

        private static void SessionManager_CurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager smtc, CurrentSessionChangedEventArgs args) // между разными обектами (браузеры приложения )
        {
            var currentSession = smtc.GetCurrentSession();

            UpdateSession(currentSession);
            UpdateDetails();
        }

        private static void UpdateSession(GlobalSystemMediaTransportControlsSession newSession)
        {
            // Отписка от события старого объекта session
            if (session != null)
            {
                session.MediaPropertiesChanged -= Session_MediaPropertiesChanged;
                session.PlaybackInfoChanged -= Session_PlaybackInfoChanged;
            }

            // Обновление объекта session и подписка на событие нового объекта session
            session = newSession;
            if (session != null)
            {
                session.MediaPropertiesChanged += Session_MediaPropertiesChanged;
                session.PlaybackInfoChanged += Session_PlaybackInfoChanged;
            }
        }

        private static void Session_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession session, MediaPropertiesChangedEventArgs args)
        {
            UpdateDetails();
        }

        private static void Session_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession session, PlaybackInfoChangedEventArgs args)
        {
            UpdateControls();
        }

        private static void UpdateInfo()
        {
            if (session == null) return;

            UpdateControls();
            UpdateDetails();
        }

        private static void UpdateControls() // состояние медиа (пауза и тд) не надо наверное
        {
            if (session == null) return;
            var playback = session.GetPlaybackInfo();

            if (playback == null) return;

            playbackInfo.IsPlaying = playback.PlaybackStatus.ToString();
            Console.WriteLine(playback.PlaybackStatus);
        }

        private static async void UpdateDetails() // информация о медиа
        {
            if (session == null)
                return;

            GlobalSystemMediaTransportControlsSessionMediaProperties properties;
            try
            {
                properties = await session.TryGetMediaPropertiesAsync();
            }
            catch (FileNotFoundException)
            {
                return;
            }
            if (properties == null)
                return;

            mediaProperties.AlbumArtist = properties.AlbumArtist;
            mediaProperties.AlbumTitle = properties.AlbumTitle;
            mediaProperties.AlbumTrackCount = properties.AlbumTrackCount;
            mediaProperties.Artist = properties.Artist;
            mediaProperties.Genres = string.Join(", ", properties.Genres);
            mediaProperties.PlaybackType = properties.PlaybackType?.ToString();
            mediaProperties.Subtitle = properties.Subtitle;
            mediaProperties.ThumbnailURL = await GetThumbnailAsBase64String(properties.Thumbnail);
            mediaProperties.Title = properties.Title;
            mediaProperties.TrackNumber = properties.TrackNumber;

            UpdateWebView();
        }

        private static async Task<string> GetThumbnailAsBase64String(IRandomAccessStreamReference Thumbnail)
        {
            if (Thumbnail == null) return null;

            IRandomAccessStream fileStream = await Thumbnail.OpenReadAsync();

            // 2. Преобразование IRandomAccessStream в массив байтов
            var reader = new DataReader(fileStream.GetInputStreamAt(0));
            var bytes = new byte[fileStream.Size];
            await reader.LoadAsync((uint)fileStream.Size);
            reader.ReadBytes(bytes);

            // 3. Преобразование массива байтов в строку Base64
            string base64String = $"data:image/png;base64,{Convert.ToBase64String(bytes)}";

            mediaProperties.ThumbnailURL = base64String;

            return base64String;
        }

        private static void UpdateWebView() //Отправка данных о медиа (Title, Artist и тд.).
        {
            if (mediaProperties.ThumbnailURL == oldThumbnailUrl) return;

            for (int i = 0; i < MainWindow.windowList.Count; i++)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SetHtmlWallpaper.Browser.IsLoaded)
                    {
                        ChromiumWebBrowser browser = MainWindow.windowList[i].Content as ChromiumWebBrowser;
                        browser.ExecuteScriptAsync("updateInfo", mediaProperties.Artist, mediaProperties.Title, mediaProperties.ThumbnailURL); //Добавить отправку всего.                        
                        oldThumbnailUrl = mediaProperties.ThumbnailURL ?? string.Empty; // проверить
                    }
                });
            }
        }
    }
}
