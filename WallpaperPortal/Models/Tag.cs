namespace WallpaperPortal.Models
{
    public class Tag
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<File> Files { get; set; }
        public List<UserLikedTag> UserLikedTag { get; set;}
    }
}
