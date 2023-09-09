using System.ComponentModel.DataAnnotations;

namespace WallpaperPortal.ViewModels
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
