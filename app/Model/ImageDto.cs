using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuierobesarteApp.Model
{
    public class ImageDto
    {
        public decimal id { get; set; }
        public string originalPath { get; set; }
        public string thumbnailPath { get; set; }
        public DateTime created { get; set; }
    }
}
