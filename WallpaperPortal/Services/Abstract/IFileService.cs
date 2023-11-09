using Microsoft.AspNetCore.Mvc;
using WallpaperPortal.Queries;
using WallpaperPortal.Repositories;

namespace WallpaperPortal.Services.Abstract
{
    public interface IFileService
    {
        PagedList<File> Files(FilesQuery query);
        File? File(string id);
        List<File> SimilarFiles(File file);
        void Upload(IFormFile upload, string userId, string[] tags);
        void Delete(File file);
        void AddTagsToFile(File file, string[] tags);
        void RemoveTagsFromFile(File file, string[] tags);
    }
}
