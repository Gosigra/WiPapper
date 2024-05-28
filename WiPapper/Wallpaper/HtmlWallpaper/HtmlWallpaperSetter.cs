using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Net.WebSockets;
using System.Threading;
using System.Numerics;
using CefSharp;
using CefSharp.Wpf;
using Windows.Media.Control;
using Windows.Storage.Streams;
using NAudio.Wave;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Interop;
using Vanara.PInvoke;

namespace HtmlWallpaperSetter
{
    public static class SetHtmlWallpaper
    {
        public static bool usePowerMethod = false;
        public static ChromiumWebBrowser browser = new ChromiumWebBrowser();

        public static void SetMediaAsWallpaper(Window windowList)
        {
            //Host();

            browser.Address = "http://localhost:8723";
            windowList.Content = browser;

            MediaVizualization mediaVizualization = new MediaVizualization();
            browser.IsBrowserInitializedChanged += mediaVizualization.StartMediaVizualization;

        }

        public async static void Host() // надо запускать 1 раз в начале работы приложения 
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8723/");
            listener.Start();

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string filePath;
                string requestedFile = request.Url.AbsolutePath;
                string message = request.QueryString["message"];

                if (message != null)
                {
                    usePowerMethod = bool.Parse(message);
                }

                if (requestedFile == "/") // Указывать путь до папки а не полностью до .html
                {
                    filePath = @"E:/test/test3/Новая папка/index.html";
                    //filePath = @"E:\test\test3\Новая папка\gyahgsdf\index(2).html";
                }
                else
                {
                    filePath = @"E:/test/test3/Новая папка" + requestedFile;
                }

                if (File.Exists(filePath))
                {
                    byte[] buffer = File.ReadAllBytes(filePath);
                    response.ContentLength64 = buffer.Length;

                    // Установка MIME-типа для JavaScript-модулей
                    if (filePath.EndsWith(".js"))
                    {
                        response.ContentType = "application/javascript";
                    }

                    Stream output = response.OutputStream;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                    output.Close();
                }
                else
                {
                    response.StatusCode = 404;
                    response.Close();
                }
            }
        }
    }

    public class MediaVizualization
    {
        private GlobalSystemMediaTransportControlsSession session;
        private GlobalSystemMediaTransportControlsSessionManager sessionManager = null;

        private MediaProperties mediaProperties = new MediaProperties();
        private PlaybackInfo playbackInfo = new PlaybackInfo();

        private string oldThumbnailUrl;

        public void StartMediaVizualization(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetHtmlWallpaper.browser.ShowDevTools();

            sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult();
            sessionManager.CurrentSessionChanged += SessionManager_CurrentSessionChanged; // работает между разными приложениями

            GlobalSystemMediaTransportControlsSession session = sessionManager.GetCurrentSession();

            UpdateSession(session);

            SetHtmlWallpaper.browser.FrameLoadEnd += (s, args) =>
            {
                // Ожидаем, пока весь JavaScript будет загружен
                if (args.Frame.IsMain)
                {
                    RecordAudioData().GetAwaiter().GetResult(); // лучше конечнно await и асинхронный метод

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SetHtmlWallpaper.browser.ExecuteScriptAsync("updateInfo", mediaProperties.Artist, mediaProperties.Title, mediaProperties.ThumbnailURL);
                    });
                }
            };

            UpdateInfo();
        }

        #region обработка звука
        public async Task RecordAudioData()
        {
            var capture = new WasapiLoopbackCapture();
            capture.WaveFormat = new WaveFormat(48000, 16, 1); // переменную для установки кол-ва каналов

            capture.StartRecording();
            capture.DataAvailable += (s, e) =>
            {
                if (e.BytesRecorded != 0)
                {
                    ProcessAudioData(capture, e);
                }
            };
            // Для остановки записи, вы можете вызвать метод StopRecording
            // capture.StopRecording();
        }

        private void ProcessAudioData(WasapiLoopbackCapture capture, WaveInEventArgs e)
        {
            double[] leftChannel;
            double[] rightChannel;

            int bytesPerSample = capture.WaveFormat.BitsPerSample / 8;
            int samplesRecorded = e.BytesRecorded / bytesPerSample;

            if (capture.WaveFormat.Channels == 1)
            {
                leftChannel = GetMonoChannelData(e.Buffer, samplesRecorded, bytesPerSample);
                rightChannel = null;
            }
            else // Предполагается, что количество каналов равно 2
            {
                (leftChannel, rightChannel) = GetStereoChannelData(e.Buffer, samplesRecorded, bytesPerSample);
            }

            double[] spectrumData = GetSpectrum(leftChannel, rightChannel);
            spectrumData = ApplySpectrumFilter(spectrumData);

            SendAudioData(spectrumData);
        }

        private double[] GetMonoChannelData(byte[] buffer, int samplesRecorded, int bytesPerSample)
        {
            double[] channel = new double[samplesRecorded];
            for (int i = 0; i < samplesRecorded; i++)
                channel[i] = BitConverter.ToInt16(buffer, i * bytesPerSample);

            return channel;
        }

        private (double[], double[]) GetStereoChannelData(byte[] buffer, int samplesRecorded, int bytesPerSample)
        {
            double[] leftChannel = new double[samplesRecorded / 2];
            double[] rightChannel = new double[samplesRecorded / 2];

            for (int i = 0; i < samplesRecorded; i++)
            {
                short sample = BitConverter.ToInt16(buffer, i * bytesPerSample);
                if (i % 2 == 0)
                    leftChannel[i / 2] = sample;
                else
                    rightChannel[i / 2] = sample;
            }

            return (leftChannel, rightChannel);
        }

        public double[] GetSpectrum(double[] leftChannel, double[] rightChannel)
        {
            double[] spectrumData;
            var window = new FftSharp.Windows.Hanning();

            // Обработка левого канала
            double[] leftWindowed = window.Apply(leftChannel);
            double[] leftZeroPadded = FftSharp.Pad.ZeroPad(leftWindowed);
            Complex[] leftSpectrum = FftSharp.FFT.Forward(leftZeroPadded);
            double[] leftChannelSpectrum = SetHtmlWallpaper.usePowerMethod ?
                FftSharp.FFT.Power(leftSpectrum) :
                FftSharp.FFT.Magnitude(leftSpectrum);

            if (rightChannel != null)
            {
                // Обработка правого канала
                double[] rightWindowed = window.Apply(rightChannel);
                double[] rightZeroPadded = FftSharp.Pad.ZeroPad(rightWindowed);
                Complex[] rightSpectrum = FftSharp.FFT.Forward(rightZeroPadded);
                double[] rightChannelSpectrum = SetHtmlWallpaper.usePowerMethod ?
                    FftSharp.FFT.Power(rightSpectrum) :
                    FftSharp.FFT.Magnitude(rightSpectrum);

                spectrumData = leftChannelSpectrum.Concat(rightChannelSpectrum).ToArray();
            }
            else
            {
                spectrumData = leftChannelSpectrum;
            }

            return spectrumData;
        }

        private double[] ApplySpectrumFilter(double[] spectrumData)
        {
            for (int i = 0; i < spectrumData.Length; i++)
            {
                if (spectrumData[i] <= 0.1)
                {
                    spectrumData[i] = 0;
                }
            }

            return spectrumData;
        }

        private void SendAudioData(double[] spectrumData)
        {
            //string jsonAudioData2 = JsonConvert.SerializeObject(spectrumData);
            string jsonAudioData2 = System.Text.Json.JsonSerializer.Serialize(spectrumData);
            SetHtmlWallpaper.browser.ExecuteScriptAsync("wallpaperAudioListener", jsonAudioData2);
            Console.WriteLine("sended");
        }
        #endregion

        public void SessionManager_CurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager smtc, CurrentSessionChangedEventArgs args) // между разными обектами (браузеры приложения )
        {
            var currentSession = smtc.GetCurrentSession();

            UpdateSession(currentSession);

            UpdateDetails();
        }

        private void UpdateSession(GlobalSystemMediaTransportControlsSession newSession)
        {
            // Отписка от события старого объекта session
            if (this.session != null)
            {
                this.session.MediaPropertiesChanged -= Session_MediaPropertiesChanged;
                this.session.PlaybackInfoChanged -= Session_PlaybackInfoChanged;
            }

            // Обновление объекта session и подписка на событие нового объекта session
            this.session = newSession;
            if (this.session != null)
            {
                this.session.MediaPropertiesChanged += Session_MediaPropertiesChanged;
                this.session.PlaybackInfoChanged += Session_PlaybackInfoChanged;
            }
        }

        private void Session_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession session, MediaPropertiesChangedEventArgs args)
        {
            //UpdateInfo();
            UpdateDetails();
        }

        private void Session_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession session, PlaybackInfoChangedEventArgs args)
        {
            UpdateControls();
        }

        private void UpdateInfo()
        {
            if (session == null) return;
            UpdateControls();
            UpdateDetails();
        }

        private void UpdateControls() // состояние медиа (пауза и тд)
        {
            if (session == null) return;
            var playback = session.GetPlaybackInfo();

            if (playback == null) return;

            playbackInfo.IsPlaying = playback.PlaybackStatus.ToString();
            Console.WriteLine(playback.PlaybackStatus);
        }

        private async void UpdateDetails() // информация о медиа
        {
            if (session == null) return;
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
            mediaProperties.ThumbnailURL = await pizda(properties.Thumbnail);
            mediaProperties.Title = properties.Title;
            mediaProperties.TrackNumber = properties.TrackNumber;


            object[] abc = new object[] { mediaProperties };
            SetHtmlWallpaper.browser.ExecuteScriptAsync("updateInfoTest", abc);

            UpdateWebView();
            //pizda(properties.Thumbnail); // а ту что выше выпилить
        }

        async Task<string> pizda(IRandomAccessStreamReference Thumbnail)
        {
            if (Thumbnail == null)
                return null;

            IRandomAccessStream fileStream = await Thumbnail.OpenReadAsync();

            // 2. Преобразование IRandomAccessStream в массив байтов
            var reader = new DataReader(fileStream.GetInputStreamAt(0));
            var bytes = new byte[fileStream.Size];
            await reader.LoadAsync((uint)fileStream.Size);
            reader.ReadBytes(bytes);

            // 3. Преобразование массива байтов в строку Base64
            string base64String = $"data:image/png;base64,{Convert.ToBase64String(bytes)}";

            mediaProperties.ThumbnailURL = base64String;
            //UpdateWebView();

            return base64String;
        }

        private void UpdateWebView()
        {
            if (mediaProperties.ThumbnailURL != oldThumbnailUrl)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SetHtmlWallpaper.browser.IsLoaded)
                    {
                        SetHtmlWallpaper.browser.ExecuteScriptAsync("updateInfo", mediaProperties.Artist, mediaProperties.Title, mediaProperties.ThumbnailURL);
                        //CrBrowser.ExecuteScriptAsync("updateInfoTest", new object[] { model });
                        oldThumbnailUrl = mediaProperties.ThumbnailURL;
                    }
                });
            }
            else
            {
                return;
                //UpdateDetails(); //вроде все работало но потом сломалось я добавил return и все теперь тоже работает хз как
            }
        }
    }

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

    public class PlaybackInfo 
        //Не надо наверное так как при 2х медиа сойдет с ума.
    {
        private string _IsPlaying;
        public string IsPlaying
        {
            get { return _IsPlaying; }
            set
            {
                if (_IsPlaying != value)
                {
                    _IsPlaying = value;
                }
            }
        }
    }
}
