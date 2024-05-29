using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiPapper.Wallpaper.HtmlWallpaper
{
    public class PlaybackInfo // пауза 
                              //Не надо наверное так как при 2х медиа сойдет с ума.
    {
        private string _IsPlaying;
        public string IsPlaying
        {
            get { return _IsPlaying; }
            set
            {
                if (_IsPlaying != value)
                {
                    _IsPlaying = value;
                }
            }
        }
    }
}
