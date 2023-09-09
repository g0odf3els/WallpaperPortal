using System.Linq.Expressions;

namespace WallpaperPortal.Repositories.Abstract
{
    public interface IRepositoryBase<T>
    {
        IQueryable<T> FindAll();
        T? FindFirstByCondition(Expression<Func<T, bool>> expression);
        IQueryable<T> FindAllByCondition(Expression<Func<T, bool>> expression);
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
