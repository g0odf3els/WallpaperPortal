namespace WallpaperPortal.ViewModels
{
    public class FileViewModel
    {
        public File File { get; set; }
        public bool isFavorite { get; set; }   
        public IEnumerable<File> SimilarFiles { get; set; } 
    }
}
