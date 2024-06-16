using System;
using System.Linq;
using System.Windows;
using System.Numerics;
using CefSharp;
using CefSharp.Wpf;
using NAudio.Wave;
using Newtonsoft.Json;

namespace WiPapper.Wallpaper.HtmlWallpaper
{
    internal static class AudioProcessor
    {
        public static int Channels { get; set; } = 1;
        private static WasapiLoopbackCapture Capture = new WasapiLoopbackCapture();

        public static void ChangeWaweFormat()
        {
            Capture.StopRecording();
            Channels = 2;
            RecordAudioData();
        }

        public static void RecordAudioData() // channels, true = 2channels в host определить
        {
            //var Capture = new WasapiLoopbackCapture();
            Capture.WaveFormat = new WaveFormat(48000, 16, Channels); // переменную для установки кол-ва каналов и обработку закрытия обоев(остановку записи(проверить может сама остановится при закрытии))

            Capture.StartRecording();
            Capture.DataAvailable += (s, e) =>
            {
                if (e.BytesRecorded != 0)
                {
                    ProcessAudioData(Capture, e);
                }
            };
            // Для остановки записи, вы можете вызвать метод StopRecording
            // capture.StopRecording();
        }

        private static void ProcessAudioData(WasapiLoopbackCapture capture, WaveInEventArgs e)
        {
            double[] leftChannel;
            double[] rightChannel;

            int bytesPerSample = capture.WaveFormat.BitsPerSample / 8;
            int samplesRecorded = e.BytesRecorded / bytesPerSample;

            if (Channels == 1)
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

        private static double[] GetMonoChannelData(byte[] buffer, int samplesRecorded, int bytesPerSample)
        {
            double[] channel = new double[samplesRecorded];
            for (int i = 0; i < samplesRecorded; i++)
                channel[i] = BitConverter.ToInt16(buffer, i * bytesPerSample);

            return channel;
        }

        private static (double[], double[]) GetStereoChannelData(byte[] buffer, int samplesRecorded, int bytesPerSample)
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

        public static double[] GetSpectrum(double[] leftChannel, double[] rightChannel)
        {
            var window = new FftSharp.Windows.Hanning();

            // Обработка левого канала
            double[] leftWindowed = window.Apply(leftChannel);
            double[] leftZeroPadded = FftSharp.Pad.ZeroPad(leftWindowed);
            Complex[] leftSpectrum = FftSharp.FFT.Forward(leftZeroPadded);
            double[] leftChannelSpectrum = SetHtmlWallpaper.UsePowerMethod ?
                FftSharp.FFT.Power(leftSpectrum) :
                FftSharp.FFT.Magnitude(leftSpectrum);

            if (rightChannel != null)
            {
                // Обработка правого канала
                double[] rightWindowed = window.Apply(rightChannel);
                double[] rightZeroPadded = FftSharp.Pad.ZeroPad(rightWindowed);
                Complex[] rightSpectrum = FftSharp.FFT.Forward(rightZeroPadded);
                double[] rightChannelSpectrum = SetHtmlWallpaper.UsePowerMethod ?
                    FftSharp.FFT.Power(rightSpectrum) :
                    FftSharp.FFT.Magnitude(rightSpectrum);

                return leftChannelSpectrum.Concat(rightChannelSpectrum).ToArray();
            }
            else
            {
                return leftChannelSpectrum;
            }
        }

        private static double[] ApplySpectrumFilter(double[] spectrumData)
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

        private static void SendAudioData(double[] spectrumData)
        {
            //string jsonAudioData = JsonConvert.SerializeObject(spectrumData);
            string jsonAudioData = System.Text.Json.JsonSerializer.Serialize(spectrumData);
            //SetHtmlWallpaper.Browser.ExecuteScriptAsync("wallpaperAudioListener", jsonAudioData);

            for (int i = 0; i < MainWindow.WindowList.Count; i++)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChromiumWebBrowser browser = MainWindow.WindowList[i].Content as ChromiumWebBrowser;
                    browser.ExecuteScriptAsync("wallpaperAudioListener", jsonAudioData); //надоедливая ошибка при закрытии обоев                    
                });
            }
        }
    }
}
