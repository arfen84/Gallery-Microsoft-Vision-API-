using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vision_API_App.Models
{
    public class IGContainer
    {
        public ImageGallery imageGalery { get; set; }
        public string json { get; set; }
        public List<categories> categories { get; set; }
        public List<string> tags { get; set; }
    }
}