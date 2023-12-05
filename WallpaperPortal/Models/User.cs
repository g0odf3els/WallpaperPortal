using Microsoft.AspNetCore.Identity;

namespace WallpaperPortal.Models
{
    public class User : IdentityUser
    {
        public string? ProfileImage { get; set; }
        public List<UserLikedFile> LikedFiles { get; set; } = new List<UserLikedFile>();
        public List<UserLikedTag> LikedTags { get; set; } = new List<UserLikedTag>();
    }
}
