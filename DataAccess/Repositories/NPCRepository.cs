using BussinessObjects.Models;
using DataAccess.IRepositories;

namespace DataAccess.Repositories
{
    public class NPCRepository : GenericRepository<NPC>, INPCRepository
    {
        public NPCRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        // Add specific NPC-related method implementations here in the future
    }
}
