using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WallpaperPortal.Models;
using WallpaperPortal.Repositories.Abstract;

namespace WallpaperPortal.Repositories
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected ApplicationContext _context { get; set; }

        public RepositoryBase(ApplicationContext context)
        {
            _context = context;
        }

        public IQueryable<T> FindAll() => _context.Set<T>().AsNoTracking();

        public T? FindFirstByCondition(Expression<Func<T, bool>> expression) =>
            _context.Set<T>().FirstOrDefault(expression);

        public IQueryable<T> FindAllByCondition(Expression<Func<T, bool>> expression) =>
            _context.Set<T>().Where(expression).AsNoTracking();

        public void Create(T entity) => _context.Set<T>().Add(entity);

        public void Update(T entity) => _context.Set<T>().Update(entity);

        public void Delete(T entity) => _context.Set<T>().Remove(entity);
    }
}
