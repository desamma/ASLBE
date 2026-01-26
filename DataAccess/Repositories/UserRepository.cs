using BussinessObjects.Models;
using DataAccess.IRepositories;

namespace DataAccess.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        // Add specific User-related method implementations here in the future
    }
}

