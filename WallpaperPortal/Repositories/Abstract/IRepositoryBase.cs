using System.Linq.Expressions;

namespace WallpaperPortal.Repositories.Abstract
{
    public interface IRepositoryBase<T>
    {
        IQueryable<T> FindAll();
        T? FindFirstByCondition(Expression<Func<T, bool>> expression, string[]? include = null);
        IQueryable<T> FindAllByCondition(Expression<Func<T, bool>> expression);
        PagedList<T> GetPaged(int pageNumber, int pageSize, string[]? include = null, params Expression<Func<T, bool>>[] expressions);
		void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
