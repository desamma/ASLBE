using BussinessObjects.DTOs.Item;

namespace Services.IServices
{
    public interface IItemService
    {
        ServiceResult<List<ItemDto>> GetAll();
        Task<ServiceResult<ItemDto>> GetByIdAsync(Guid id);
        Task<ServiceResult<ItemDto>> CreateAsync(CreateItemRequest request);
        Task<ServiceResult<ItemDto>> UpdateAsync(Guid id, UpdateItemRequest request);
        Task<ServiceResult<bool>> DeleteAsync(Guid id);
    }
}
