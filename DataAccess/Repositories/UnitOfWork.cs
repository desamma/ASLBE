using BussinessObjects.Models;
using DataAccess;
using DataAccess.IRepositories;
using DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    public IGenericRepository<User> Users { get; }
    public IGenericRepository<Item> Items { get; }
    public IGenericRepository<Transaction> Transactions { get; }
    public IGenericRepository<GameNews> GameNews { get; }
    public IGenericRepository<ShopPurchase> ShopPurchases { get; }

   
    public IGenericRepository<GachaBanner> GachaBanners { get; }
    public IGenericRepository<GachaItem> GachaItems { get; }
    public IGenericRepository<GachaHistory> GachaHistory { get; }

    public IUserItemRepository UserItems { get; }
    public IShopItemRepository ShopItems { get; }
    public INPCRepository NPCs { get; }
    public IGenericRepository<BugReport> BugReports { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Users = new GenericRepository<User>(_context);
        Items = new GenericRepository<Item>(_context);
        Transactions = new GenericRepository<Transaction>(_context);
        GameNews = new GenericRepository<GameNews>(_context);
        UserItems = new UserItemRepository(_context);
        ShopItems = new ShopItemRepository(_context);
        NPCs = new NPCRepository(_context);
        ShopPurchases = new GenericRepository<ShopPurchase>(_context);

        // ✅ Initialize them in the constructor
        GachaBanners = new GenericRepository<GachaBanner>(_context);
        GachaItems = new GenericRepository<GachaItem>(_context);
        GachaHistory = new GenericRepository<GachaHistory>(_context);
        BugReports = new GenericRepository<BugReport>(_context);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}