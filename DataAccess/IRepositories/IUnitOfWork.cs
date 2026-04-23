using BussinessObjects.Models;

namespace DataAccess.IRepositories
{
    public interface IUnitOfWork
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Item> Items { get; }
        IGenericRepository<GameNews> GameNews { get; }
        IGenericRepository<Transaction> Transactions { get; }
        IGenericRepository<GachaBanner> GachaBanners { get; }
        IGenericRepository<GachaItem> GachaItems { get; }

        IGenericRepository<GachaHistory> GachaHistory { get; }
        IGenericRepository<ShopPurchase> ShopPurchases { get; }
        IGenericRepository<BugReport> BugReports { get; }
        IUserItemRepository UserItems { get; }
        IShopItemRepository ShopItems { get; }
        INPCRepository NPCs { get; }
        
        Task SaveChangesAsync();
    }
}