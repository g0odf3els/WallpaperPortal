using System.ComponentModel.DataAnnotations;

namespace WallpaperPortal.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Rember?")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
