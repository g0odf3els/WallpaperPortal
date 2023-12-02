using System.Linq.Expressions;

namespace WallpaperPortal.Repositories.Abstract
{
    public interface IRepositoryBase<T>
    {
        IQueryable<T> FindAll(Expression<Func<T, bool>>? expression = null, string[]? include = null);

        T? FindFirst(Expression<Func<T, bool>> expression, string[]? include = null);

        PagedList<T> GetPaged(
           int pageNumber,
           int pageSize,
           string[]? include = null,
           Expression<Func<T, object>>? orderBy = null,
           bool isAscending = true,
           params Expression<Func<T, bool>>[] expressions);
        
        T Create(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
