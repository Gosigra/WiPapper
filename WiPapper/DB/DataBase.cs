using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;

namespace WiPapper.DB
{
    public class DataBase
    {
        const string SUPABASE_URL = "https://kswefyoocehyihrohpqj.supabase.co";
        const string SUPABASE_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imtzd2VmeW9vY2VoeWlocm9ocHFqIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MTc2Nzg1MjgsImV4cCI6MjAzMzI1NDUyOH0.8b_eNchjg7RdcE_7lw8qp4u9YxMW6j2YgHAOK58hioE";

        public static Client _supabase;

        public async void Start()
        {
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            _supabase = new Client(SUPABASE_URL, SUPABASE_KEY, options);
            await _supabase.InitializeAsync();
        }

    }
}
