using BussinessObjects.Models;

namespace DataAccess.IRepositories
{
    public interface IUserItemRepository : IGenericRepository<UserItem>
    {
        Task<UserItem?> FirstOrDefaultAsync(Guid userId, Guid itemId);
        Task DeleteAsync(Guid userId, Guid itemId);
    }
}
