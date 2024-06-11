using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Supabase;
using static WiPapper.MainWindow;

namespace WiPapper.DB
{
    public class DataBase
    {
        const string SUPABASE_URL = "https://kswefyoocehyihrohpqj.supabase.co";
        const string SUPABASE_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imtzd2VmeW9vY2VoeWlocm9ocHFqIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MTc2Nzg1MjgsImV4cCI6MjAzMzI1NDUyOH0.8b_eNchjg7RdcE_7lw8qp4u9YxMW6j2YgHAOK58hioE";
        //const string SUPABASE_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imtzd2VmeW9vY2VoeWlocm9ocHFqIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTcxNzY3ODUyOCwiZXhwIjoyMDMzMjU0NTI4fQ.AinO71T0-bbxsZLOpQGjqc2lr4k8Wop_8gL6OBaqRVQ";

        public static Client _supabase;

        private ObservableCollection<ImageDetails> Images { get; set; }

        public async void Start()
        {
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            _supabase = new Client(SUPABASE_URL, SUPABASE_KEY, options);
            await _supabase.InitializeAsync();
        }


        private async Task<ObservableCollection<ImageDetails>> LoadImagesFromSupabase()
        {
            Images = new ObservableCollection<ImageDetails>
            {
                new ImageDetails { ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSXqPjgMIQB8GV-Jm4yUMtRTHN5F6vc0WTzAA&s", ImageDescription = "Описание 1" },
                new ImageDetails { ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSXqPjgMIQB8GV-Jm4yUMtRTHN5F6vc0WTzAA&s", ImageDescription = "Описание 2" }
                

                



            };

            return Images;
        }
    }
}
