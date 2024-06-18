using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Supabase;
using Supabase.Gotrue;
using System.IO;
using Supabase.Gotrue.Exceptions;
using System.Windows;
using System;

namespace WiPapper.DB
{
    public class DataBase
    {
        const string SUPABASE_URL = "https://kswefyoocehyihrohpqj.supabase.co";
        const string SUPABASE_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imtzd2VmeW9vY2VoeWlocm9ocHFqIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MTc2Nzg1MjgsImV4cCI6MjAzMzI1NDUyOH0.8b_eNchjg7RdcE_7lw8qp4u9YxMW6j2YgHAOK58hioE";

        public static Session session;
        public static Supabase.Client _supabase;

        private ObservableCollection<ImageDetails> Images { get; set; }

        public void Start()
        {
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            _supabase = new Supabase.Client(SUPABASE_URL, SUPABASE_KEY, options);
            _supabase.InitializeAsync().GetAwaiter();
        }

        public async Task<ObservableCollection<ImageDetails>> LoadImageDetailsFromSupabase()
        {
            Images = new ObservableCollection<ImageDetails>();

            var result = await _supabase
                                   .From<UserInfo>()
                                   .Select(x => new object[] { x.Name, x.UrlToWallpaper })
                                   .Get();

            var content = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(result.Content);
            foreach (var userInfo in content)
            {
                if (userInfo.ContainsKey("URLToWallpaper") && userInfo["URLToWallpaper"] is JArray wallpaperArray && wallpaperArray.Count > 0)
                {
                    //string name = userInfo["name"].ToString();
                    foreach (var item in wallpaperArray)
                    {
                        ImageDetails imageDetails = new ImageDetails
                        {
                            PreviewUrl = item.ToString(),
                            WallpaperName = Path.GetFileName(Path.GetDirectoryName(item.ToString())),
                            WallpaperAutor = userInfo["name"].ToString()
                        };
                        Images.Add(imageDetails);
                    }
                }
            }
            
            return Images;
        }

        public static async Task<bool> CreateAccount(string name, string email, string password)
        {
            try
            {
                DataBase.session = await DataBase._supabase.Auth.SignUp(email, password);
                var model = new UserInfo
                {
                    Id = DataBase.session.User.Id,
                    Name = name,
                };
                await DataBase._supabase.From<UserInfo>().Insert(model);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> Authorize(string email, string password)
        {
            try
            {
                DataBase.session = await _supabase.Auth.SignIn(email, password);

                if (DataBase.session != null)
                {
                    return true;
                }
                return false;
            }
            catch (GotrueException ex)
            {
                if (ex.Message.Contains("Invalid login credentials"))
                {
                    System.Windows.MessageBox.Show("Неверные учетные данные или такой пользователь не существует", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    System.Windows.MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Произошла неизвестная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static async Task UploadFolderAsync(string localFolderPath, string supabaseFolderPath, Guid userId)
        {
            string[] preview = Directory.GetFiles(localFolderPath, "preview.*", SearchOption.TopDirectoryOnly);
            if (preview.Length == 0)
            {
                System.Windows.MessageBox.Show("Добавте превью в папку с обоями");
                return;
            }
            var files = Directory.GetFiles(localFolderPath);
            var directories = Directory.GetDirectories(localFolderPath);
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                var response = await DataBase._supabase.Storage
                    .From($"Wallpapers/{supabaseFolderPath}")
                    .Upload(file, fileName, null);                                              //onProgress: (s, progress) => Debug.WriteLine($"{progress}%")

                if (file.Contains("preview"))
                {
                    var publicUrl = DataBase._supabase.Storage
                        .From("Wallpapers")
                        .GetPublicUrl($"{supabaseFolderPath}/{fileName}");

                    var result = await DataBase._supabase.Rpc("append_to_array", new { idd = userId, newelement = publicUrl });
                }
            }

            // Рекурсивная обработка подкаталогов
            foreach (var directory in directories)
            {
                string folderName = Path.GetFileName(directory);
                await UploadFolderAsync(directory, $"{supabaseFolderPath}/{folderName}", userId);
            }
        }
    }
}
