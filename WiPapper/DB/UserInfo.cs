using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Collections.Generic;

namespace WiPapper.DB
{
    [Table("client")]
    class UserInfo : BaseModel
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("URLToWallpaper")]
        public List<string> UrlToWallpaper { get; set; }
    }

}
