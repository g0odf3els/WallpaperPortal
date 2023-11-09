using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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

        public T? FindFirstByCondition(Expression<Func<T, bool>> expression, string[]? include = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (include != null)
            {
                query = include.Aggregate(query,
                          (current, include) => current.Include(include));
            }

            return query.FirstOrDefault(expression);
        }

        public IQueryable<T> FindAllByCondition(Expression<Func<T, bool>> expression) =>
            _context.Set<T>().Where(expression).AsNoTracking();

        public PagedList<T> GetPaged(int pageNumber, int pageSize, string[]? include = null, params Expression<Func<T, bool>>[] expressions)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking();

            if (include != null)
            {
                query = include.Aggregate(query,
                          (current, include) => current.Include(include));
            }

            foreach (var expression in expressions)
            {
                query = query.Where(expression);
            }

            var totalCount = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedList<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public T Create(T entity) => _context.Set<T>().Add(entity).Entity;

        public void Update(T entity) => _context.Entry(entity).State = EntityState.Modified;

        public void Delete(T entity) => _context.Set<T>().Remove(entity);
    }
}
