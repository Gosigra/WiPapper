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
        private string _isPlaying;
        public string IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying != value) //когда одно медиа на паузе а второе играет то приложение покажео второе и можно вызывать чтобы менялось состояние для второго и может с ума не сойдет
                {
                    _isPlaying = value;
                }
            }
        }
    }
}
