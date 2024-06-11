using System.Net;
using System.Windows;
using System.Windows.Controls;
using static WiPapper.MainWindow;

namespace WiPapper
{
    /// <summary>
    /// Логика взаимодействия для WallpaperMiniature.xaml
    /// </summary>
    public partial class WallpaperMiniature : UserControl
    {
        public WallpaperMiniature()
        {
            InitializeComponent();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            //if (this.DataContext is CustomImage customImage)
            //{
            //    // Скачивание изображения по URL
            //    using (var client = new WebClient())
            //    {
            //        string fileName = System.IO.Path.GetFileName(customImage.ImageUrl);
            //        client.DownloadFile(customImage.ImageUrl, fileName);
            //        MessageBox.Show($"Изображение сохранено как {fileName}", "Скачивание завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            //    }
            //}
        }
    }
}
