using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq.Expressions;
using WallpaperPortal.Models;
using WallpaperPortal.Repositories.Abstract;

namespace WallpaperPortal.Repositories
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        public ApplicationContext _context { get; set; }

        public RepositoryBase(ApplicationContext context)
        {
            _context = context;
        }

        public IQueryable<T> FindAll(Expression<Func<T, bool>>? expression = null, string[]? include = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (expression != null)
            {
                query = query.Where(expression);
            }

            if (include != null)
            {
                query = include.Aggregate(query,
                          (current, include) => current.Include(include));
            }

            return query;
        }

        public T? FindById(params object?[]? keyValues)
        {
            return _context.Set<T>().Find(keyValues);
        }

        public T? FindFirst(Expression<Func<T, bool>> expression, string[]? include = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (include != null)
            {
                query = include.Aggregate(query,
                          (current, include) => current.Include(include));
            }

            return query.FirstOrDefault(expression);
        }

        public PagedList<T> GetPaged(
            int pageNumber,
            int pageSize,
            string[]? include = null,
            Expression<Func<T, object>>? orderBy = null,
            bool isAscending = true,
            params Expression<Func<T, bool>>[] expressions)
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

            if (orderBy != null)
            {
                query = isAscending
                    ? query.OrderBy(orderBy)
                    : query.OrderByDescending(orderBy);
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
