using BussinessObjects.Models;

namespace DataAccess.IRepositories
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IItemRepository Items { get; }
        IGameNewsRepository GameNews { get; }
        IUserItemRepository UserItems { get; }
    }
}