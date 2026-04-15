using BussinessObjects.DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IShopPurchaseService
    {
        Task<ServiceResult<ShopPurchaseDto>> BuyItemAsync(Guid userId, BuyShopItemRequest request);
        Task<ServiceResult<List<ShopPurchaseDto>>> GetPurchaseHistoryAsync(Guid userId);
    }
}
