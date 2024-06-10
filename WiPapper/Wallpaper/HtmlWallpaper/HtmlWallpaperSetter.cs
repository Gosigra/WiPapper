using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;

namespace WiPapper.Wallpaper.HtmlWallpaper
{
    public static class SetHtmlWallpaper // сделать не статик
    {
        public static bool UsePowerMethod { get; set; }
        public static ChromiumWebBrowser Browser { get; set; }
        public static string FilePath { get; set; }

        public static void SetBrowserAsWallpaper(Window windowList)
        {
            //CefSettings settings = new CefSettings();
            //settings.CefCommandLineArgs.Add("allow-file-access-from-files");
            //settings.CefCommandLineArgs.Add("allow-universal-access-from-files");
            //Cef.Initialize(settings);
            //Browser.BrowserSettings


            Browser = new ChromiumWebBrowser("http://localhost:8723");
            windowList.Content = Browser;

            Browser.IsBrowserInitializedChanged += MediaSessionHandler.IsBrowserInitialized;
        }

        public async static void StartHttpListener()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8723/");
            listener.Start();

            while (true)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    string requestRezult = ProcessRequest(request);
                    if (!string.IsNullOrEmpty(requestRezult))
                    {
                        await ProcessResponse(requestRezult, response);
                    }
                }
                catch (Exception)
                {
                    //MessageBox.Show(ex.ToString());
                }
            }
        }

        private static string ProcessRequest(HttpListenerRequest request)
        {
            string filePath = String.Empty;
            string requestedFile = request.Url.AbsolutePath;

            switch (requestedFile)
            {
                //case "/Use"
                case "/UsePowerMethod":
                    UsePowerMethod = true;
                    break;
                case "/UseStereoRecord":
                    AudioProcessor.Channels = 2;
                    break;
                case "/":
                    filePath = FilePath + "/index.html";
                    break;
                default:
                    filePath = FilePath + requestedFile;
                    break;
            }

            return filePath;
        }

        private static async Task ProcessResponse(string filePath, HttpListenerResponse response)
        {
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
