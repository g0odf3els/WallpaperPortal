using WallpaperPortal.Models;
using WallpaperPortal.Repositories;

namespace WallpaperPortal.Persistance
{
    public interface IUnitOfWork
    {
        RepositoryBase<User> UserRepository { get; }

        RepositoryBase<File> FileRepository { get; }

        RepositoryBase<Tag> TagRepository { get; }

        ApplicationContext Context { get; }

        void Save();
    }
}
