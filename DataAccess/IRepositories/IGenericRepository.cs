using System.Linq.Expressions;

namespace DataAccess.IRepositories
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetQueryable(bool asNoTracking = true);

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }

}
