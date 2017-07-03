using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vision_API_App.Models
{
    public class ImageGallery
    {
        public ImageGallery()
        {
            ImageList = new List<string>();
        }
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public List<string> ImageList { get; set; }
    }
}