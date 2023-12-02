using Microsoft.AspNetCore.Identity;

namespace WallpaperPortal.Models
{
    public class User : IdentityUser
    {
        public string? ProfileImage { get; set; }
        public List<UserLikedFile> LikedFiles { get; set; } = new List<UserLikedFile>();
        public List<Tag> LikedTags { get; set; } = new List<Tag>();
    }
}
