namespace WallpaperPortal.Models
{
    public class UserLikedTag
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public string TagId { get; set; }
        public Tag Tag { get; set; }

        public float Weight { get; set; } = 1;
    }
}
