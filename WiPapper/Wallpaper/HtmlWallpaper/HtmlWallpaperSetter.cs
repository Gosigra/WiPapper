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
using WiPapper.Wallpaper.HtmlWallpaper;

namespace WiPapper.Wallpaper.HtmlWallpaper
{
    public static class SetHtmlWallpaper // сделать не статик
    {
        public static bool UsePowerMethod { get; set; }
        public static ChromiumWebBrowser Browser { get; set; }
        public static string FilePath { get; set; }


        public static void SetBrowserAsWallpaper(Window windowList)
        {
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
                    await ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private static async Task ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string filePath = String.Empty;
            string requestedFile = request.Url.AbsolutePath;
            string message = request.QueryString["message"]; //без этого и через case

            switch (requestedFile)
            {
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

            //if (message != null)
            //{
            //    usePowerMethod = bool.Parse(message);
            //    return;
            //}

            //if (requestedFile == "/") // Указывать путь до папки а не полностью до .html
            //{
            //    filePath = @"E:/test/test3/Новая папка/index.html";
            //    //filePath = @"E:\test\test3\Новая папка\gyahgsdf\index(2).html";
            //}
            //else
            //{
            //    filePath = @"E:/test/test3/Новая папка" + requestedFile;
            //}

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
