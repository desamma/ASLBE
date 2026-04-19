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
        Task<ServiceResult<List<UserItemDto>>> GetPendingDeliveryAsync(Guid userId);
        Task<ServiceResult<bool>> AcknowledgeDeliveryAsync(Guid userId, AcknowledgeDeliveryRequest request);

    }
    public class AcknowledgeDeliveryRequest
    {
        public List<DeliveryItem> Items { get; set; } = new();
    }

    public class DeliveryItem
    {
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class UserItemDto
    {
        public Guid UserId { get; set; }
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }
        public int QuantityDelivered { get; set; }
        public int QuantityPending => Quantity - QuantityDelivered;
        public bool IsDeliveredToGame { get; set; }
        public DateTime? DeliveredToGameAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public IItemServiceItemDto? Item { get; set; }
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
