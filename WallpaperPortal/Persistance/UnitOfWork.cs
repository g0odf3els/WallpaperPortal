using WallpaperPortal.Models;
using WallpaperPortal.Repositories;

namespace WallpaperPortal.Persistance
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext _context;
        private RepositoryBase<User> _userRepository;


        public UnitOfWork(ApplicationContext context)
        {
            _context = context;
        }

        public RepositoryBase<User> UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new RepositoryBase<User>(_context);
                }
                return _userRepository;
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
