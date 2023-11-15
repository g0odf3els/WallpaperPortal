using WallpaperPortal.Models;
using WallpaperPortal.Repositories;
using Color = WallpaperPortal.Models.Color;

namespace WallpaperPortal.Persistance
{
    public interface IUnitOfWork
    {
        RepositoryBase<User> UserRepository { get; }

        RepositoryBase<File> FileRepository { get; }

        RepositoryBase<Tag> TagRepository { get; }

        RepositoryBase<Color> ColorRepository { get; }

        ApplicationContext Context { get; }

        void Save();
    }
}
