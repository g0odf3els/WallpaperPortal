using WallpaperPortal.Models;
using WallpaperPortal.Repositories;

namespace WallpaperPortal.Persistance
{
    public interface IUnitOfWork
    {
        RepositoryBase<User> UserRepository { get; }
        void Save();
    }
}
