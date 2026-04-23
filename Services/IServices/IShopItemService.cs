using BussinessObjects.DTOs.ShopItem;

namespace Services.IServices
{
    public interface IShopItemService
    {
        ServiceResult<List<ShopItemDto>> GetAll();
        ServiceResult<List<ShopItemDto>> GetActiveOnly();
        Task<ServiceResult<ShopItemDto>> GetByIdAsync(Guid id);
        Task<ServiceResult<ShopItemDto>> CreateAsync(CreateShopItemRequest request);
        Task<ServiceResult<ShopItemDto>> UpdateAsync(Guid id, UpdateShopItemRequest request);
        Task<ServiceResult<bool>> DisableAsync(Guid id);
        Task<ServiceResult<bool>> EnableAsync(Guid id);
    }
}
