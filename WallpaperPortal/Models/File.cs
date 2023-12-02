namespace WallpaperPortal.Models
{
    public class File
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public float Lenght { get; set; }
        public string Path { get; set; }
        public string PreviewPath { get; set; }
        public DateTime CreationTime { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public List<Tag> Tags { get; set; } = new List<Tag>();
        public List<Color> Colors { get; set; } = new List<Color>();
        public List<UserLikedFile> LikedByUsers { get; set; } = new List<UserLikedFile>();
    }
}
