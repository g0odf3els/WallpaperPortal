using WallpaperPortal.Models;
using WallpaperPortal.Repositories;

namespace WallpaperPortal.ViewModels
{
    public class ProfileViewModel
    {
        public User User { get; set; }

        public PagedList<File> UploadedFiles { get; set; }
    }
}
