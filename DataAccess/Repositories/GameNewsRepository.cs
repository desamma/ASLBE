using BussinessObjects.Models;
using DataAccess.IRepositories;

namespace DataAccess.Repositories
{
    public class GameNewsRepository : GenericRepository<GameNews>, IGameNewsRepository
    {
        public GameNewsRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        // Add specific GameNews-related method implementations here in the future
    }
}

