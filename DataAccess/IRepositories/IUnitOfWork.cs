using BussinessObjects.Models;

namespace DataAccess.IRepositories
{
    public interface IUnitOfWork
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Item> Items { get; }
        IGenericRepository<GameNews> GameNews { get; }
        IUserItemRepository UserItems { get; }
    }
}