namespace WallpaperPortal.Models
{
    public class UserLikedFile
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public string FileId { get; set; }
        public File File { get; set; }
    }
}
