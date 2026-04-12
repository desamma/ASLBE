using Services;

namespace Services.IServices
{
    public interface IUserItemService
    {
        ServiceResult<List<UserItemDto>> GetAll();
        Task<ServiceResult<List<UserItemDto>>> GetByUserIdAsync(Guid userId);
        Task<ServiceResult<UserItemDto>> GetByIdAsync(Guid userId, Guid itemId);
        Task<ServiceResult<UserItemDto>> AddAsync(Guid userId, AddUserItemRequest request);
        Task<ServiceResult<UserItemDto>> UpdateAsync(Guid userId, Guid itemId, UpdateUserItemRequest request);
        Task<ServiceResult<bool>> DeleteAsync(Guid userId, Guid itemId);
    }

    public class UserItemDto
    {
        public Guid UserId { get; set; }
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }
        public IItemServiceItemDto Item { get; set; }
    }

    public class IItemServiceItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public string ImagePath { get; set; }
    }

    public class AddUserItemRequest
    {
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateUserItemRequest
    {
        public int Quantity { get; set; }
    }
}
