using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiPapper.DB
{
    public class ImageDetails
    {
        private string _imageUrl;
        public string ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
            }
        }

        private string _imageDescription;
        public string ImageDescription
        {
            get { return _imageDescription; }
            set
            {
                _imageDescription = value;
            }
        }
    }
}
