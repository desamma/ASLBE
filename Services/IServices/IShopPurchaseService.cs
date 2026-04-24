using BussinessObjects.DTOs.Shop;

namespace Services.IServices
{
    public interface IShopPurchaseService
    {
        Task<ServiceResult<ShopPurchaseDto>> BuyItemAsync(Guid userId, BuyShopItemRequest request);
        Task<ServiceResult<List<ShopPurchaseDto>>> GetPurchaseHistoryAsync(Guid userId);
    }
}
