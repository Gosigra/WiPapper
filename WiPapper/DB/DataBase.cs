using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Supabase;
using Supabase.Gotrue;
using System.IO;

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
    }
}
