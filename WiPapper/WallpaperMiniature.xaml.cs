using System.IO;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

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
            if (!(Application.Current.MainWindow is MainWindow mainWindow)) return;

            string folderPath = mainWindow.DefaultInstallationPath.Text;

            if (string.IsNullOrEmpty(folderPath))
            {
                if (!Directory.Exists("Wallpapers"))
                {
                    Directory.CreateDirectory("Wallpapers");
                }
                folderPath = "Wallpapers";
            }
            else
            {
                folderPath = Directory.Exists(mainWindow.DefaultInstallationPath.Text) ? mainWindow.DefaultInstallationPath.Text :
                                                                                         "Wallpapers";
            }

            
            DownloadWallpaper(folderPath, sender as Button);
        }

        private async void DownloadWallpaper(string folderPath, Button button)
        {
            Application.Current.Dispatcher.Invoke(() => DowloadProgressGrid.Visibility = Visibility.Visible);

            await DownloadWallpaperAsync(folderPath, button.Tag.ToString());
            //MessageBox.Show("Обои успешно скачаны");

            Application.Current.Dispatcher.Invoke(() => DowloadProgressGrid.Visibility = Visibility.Collapsed);
        }

        private async Task DownloadWallpaperAsync(string folderPath, string supabasePath)
        {
            var preview = await DB.DataBase._supabase.Storage.From("Wallpapers").List(supabasePath);

            foreach (var prew in preview)
            {
                var fullPath = Path.Combine(folderPath, supabasePath, prew.Name);
                var directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!prew.IsFolder)
                {
                    var bytes = await DB.DataBase._supabase.Storage
                        .From("Wallpapers")
                        .Download($"{supabasePath}/{prew.Name}", null);

                    File.WriteAllBytes(fullPath, bytes);
                }
                else if (prew.IsFolder)
                {
                    await DownloadWallpaperAsync(folderPath, $"{supabasePath}/{prew.Name}");
                }
            }
        }
    }
}
