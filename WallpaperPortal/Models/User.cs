using Microsoft.AspNetCore.Identity;

namespace WallpaperPortal.Models
{
    public class User : IdentityUser
    {
        public string? ProfileImage { get; set; }
    }
}
