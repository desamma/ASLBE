using BussinessObjects.DTOs.UserItem;

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
}
