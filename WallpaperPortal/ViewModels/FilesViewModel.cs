using WallpaperPortal.Queries;
using WallpaperPortal.Repositories;

namespace WallpaperPortal.ViewModels
{
    public class FilesViewModel
    {
       public FilesQuery FilesQuery { get; set; }
       public PagedList<File> PagedList { get; set; }
    }
}
