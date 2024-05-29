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

namespace WiPapper.Wallpaper.HtmlWallpaper
{
    internal static class AudioProcessor
    {
        public static int Channels { get; set; } = 1;

        public static void RecordAudioData() // channels, true = 2channels в host определить 
        {
            var capture = new WasapiLoopbackCapture();
            capture.WaveFormat = new WaveFormat(48000, 16, Channels); // переменную для установки кол-ва каналов и обработку закрытия обоев(остановку записи(проверить может сама остановится при закрытии))

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
            double[] spectrumData;
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

                spectrumData = leftChannelSpectrum.Concat(rightChannelSpectrum).ToArray();
            }
            else
            {
                spectrumData = leftChannelSpectrum;
            }

            return spectrumData;
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
            //string jsonAudioData2 = JsonConvert.SerializeObject(spectrumData);
            string jsonAudioData2 = System.Text.Json.JsonSerializer.Serialize(spectrumData);
            SetHtmlWallpaper.Browser.ExecuteScriptAsync("wallpaperAudioListener", jsonAudioData2);
            Console.WriteLine("sended");
        }
    }
}
