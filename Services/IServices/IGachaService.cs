using BussinessObjects.Models;

namespace Services.IServices
{
    public interface IGachaService
    {
        Task<UserItem> RollSingleAsync(Guid userId);
        Task<List<UserItem>> RollMultiplesAsync(Guid userId, int count);
    }
}

