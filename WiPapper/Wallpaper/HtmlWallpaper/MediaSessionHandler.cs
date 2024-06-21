using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using Windows.Media.Control;
using Windows.Storage.Streams;
using static Vanara.PInvoke.User32;

namespace WiPapper.Wallpaper.HtmlWallpaper
{
    internal static class MediaSessionHandler
    {
        public static string oldThumbnailUrl;
        private static readonly MediaProperties mediaProperties = new MediaProperties();
        private static GlobalSystemMediaTransportControlsSession session;
        private static GlobalSystemMediaTransportControlsSessionManager sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult();

        public static void IsBrowserInitialized(object sender, DependencyPropertyChangedEventArgs e)
        {
            //(sender as ChromiumWebBrowser).IsBrowserInitializedChanged -= IsBrowserInitialized;

            sessionManager.CurrentSessionChanged += SessionManager_CurrentSessionChanged;
            SetHtmlWallpaper.Browser.FrameLoadEnd += Browser_FrameLoadEnd;

            GlobalSystemMediaTransportControlsSession session = sessionManager.GetCurrentSession();
            UpdateSession(session);
        }

        private static void ManageExtendedFunctionality()
        {
            try
            {
                if (SetHtmlWallpaper.ShowDevTools)
                {
                    SetHtmlWallpaper.Browser.ShowDevTools();
                    SetHtmlWallpaper.ShowDevTools = false;
                }

                if (SetHtmlWallpaper.RecordAudio)
                {
                    AudioProcessor.RecordAudioData();
                }
            }
            catch { }
        }

        private static void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs args)
        {
            if (args.Frame.IsMain)
            {
                ManageExtendedFunctionality();

                Application.Current.Dispatcher.Invoke(() => UpdateInfo());
            }
        }

        private static void SessionManager_CurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager smtc, CurrentSessionChangedEventArgs args)                // между разными обектами (браузеры приложения )
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
            }

            // Обновление объекта session и подписка на событие нового объекта session
            session = newSession;
            if (session != null)
            {
                session.MediaPropertiesChanged += Session_MediaPropertiesChanged;
            }
        }

        private static void Session_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession session, MediaPropertiesChangedEventArgs args)
        {
            UpdateDetails();
        }

        private static void UpdateInfo()
        {
            if (session == null) return;

            UpdateDetails();
        }

        private static async void UpdateDetails()
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

        private static void UpdateWebView()
        {
            if (mediaProperties.ThumbnailURL == oldThumbnailUrl) return;

            for (int i = 0; i < MainWindow.WindowList.Count; i++)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SetHtmlWallpaper.Browser.IsLoaded)
                    {
                        ChromiumWebBrowser browser = MainWindow.WindowList[i].Content as ChromiumWebBrowser;
                        string jsonMediaProperties = JsonConvert.SerializeObject(mediaProperties);
                        browser.ExecuteScriptAsync("updateInfo", jsonMediaProperties);
                        oldThumbnailUrl = mediaProperties.ThumbnailURL ?? string.Empty;
                    }
                });
            }
        }
    }
}
