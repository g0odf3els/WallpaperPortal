using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WallpaperPortal.Models
{
    public class Color
    {
        [Key, Column(Order = 0)]
        public byte A { get; set; }

        [Key, Column(Order = 1)]
        public byte R { get; set; }

        [Key, Column(Order = 2)]
        public byte G { get; set; }

        [Key, Column(Order = 3)]
        public byte B { get; set; }

        public List<File> Files { get; set; } 
    }
}