using BussinessObjects.Models;
using DataAccess.IRepositories;

namespace DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;

        public IUserRepository Users { get; }
        public IItemRepository Items { get; }
        public IGameNewsRepository GameNews { get; }
        public IUserItemRepository UserItems { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            Users = new UserRepository(_context);
            Items = new ItemRepository(_context);
            GameNews = new GameNewsRepository(_context);
            UserItems = new UserItemRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
