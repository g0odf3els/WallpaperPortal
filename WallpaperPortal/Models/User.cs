using Microsoft.AspNetCore.Identity;

namespace WallpaperPortal.Models
{
    public class User : IdentityUser
    {
        public int Year { get; set; }
    }
}
