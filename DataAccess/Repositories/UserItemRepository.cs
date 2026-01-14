using BussinessObjects.Models;
using DataAccess.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class UserItemRepository : GenericRepository<UserItem>, IUserItemRepository
    {
        public UserItemRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<UserItem?> FirstOrDefaultAsync(Guid userId, Guid itemId)
        {
            return await _dbSet
                .Include(ui => ui.Item)
                .FirstOrDefaultAsync(ui =>
                    ui.UserId == userId &&
                    ui.ItemId == itemId);
        }

        public async Task DeleteAsync(Guid userId, Guid itemId)
        {
            var entity = await FirstOrDefaultAsync(userId, itemId);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}