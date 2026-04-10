using BussinessObjects.Models;
using DataAccess.IRepositories;

namespace DataAccess.Repositories
{
    public class ShopItemRepository : GenericRepository<ShopItem>, IShopItemRepository
    {
        public ShopItemRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        // Add specific ShopItem-related method implementations here in the future
    }
}
