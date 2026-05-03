using BussinessObjects.DTOs.Shop;
using BussinessObjects.Models;
using DataAccess.IRepositories;
using Services.IServices;

namespace Services.Services
{
    public class ShopPurchaseService : IShopPurchaseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShopPurchaseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<ShopPurchaseDto>> BuyItemAsync(Guid userId, BuyShopItemRequest request)
        {
            try
            {
                if (request.Quantity <= 0)
                    return Fail<ShopPurchaseDto>("Quantity must be greater than 0");

                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    return Fail<ShopPurchaseDto>("User not found");

                if (user.IsBanned)
                    return Fail<ShopPurchaseDto>("Your account has been banned");

                var shopItem = await _unitOfWork.ShopItems.FirstOrDefaultAsync(si => si.Id == request.ShopItemId);
                if (shopItem == null)
                    return Fail<ShopPurchaseDto>("Shop item not found");

                if (!shopItem.IsActive)
                    return Fail<ShopPurchaseDto>("This item is no longer available");

                if (!shopItem.IsUsingPremiumCurrency)
                    return Fail<ShopPurchaseDto>("This item must be purchased via payment flow");

                if (!shopItem.Price.HasValue || shopItem.Price.Value <= 0)
                    return Fail<ShopPurchaseDto>("Invalid item price");

                var totalCostDecimal = shopItem.Price.Value * request.Quantity;
                var totalCost = (int)Math.Ceiling(totalCostDecimal);

                if (user.CurrencyAmount < totalCost)
                    return Fail<ShopPurchaseDto>(
                        $"Insufficient currency. Required: {totalCost}, Available: {user.CurrencyAmount}");

                user.CurrencyAmount -= totalCost;

                var purchase = new ShopPurchase
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ShopItemId = shopItem.Id,
                    Quantity = request.Quantity,
                    PaymentType = "PremiumCurrency",
                    AmountPaid = totalCost,
                    PurchaseDate = DateTime.Now
                };

                await _unitOfWork.ShopPurchases.AddAsync(purchase);

                if (shopItem.ItemId.HasValue && shopItem.ItemId.Value != Guid.Empty)
                {
                    var quantityToAdd = (shopItem.ItemQuantity ?? 1) * request.Quantity;

                    var userItem = await _unitOfWork.UserItems
                        .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ItemId == shopItem.ItemId.Value);

                    if (userItem != null)
                    {
                        userItem.Quantity += quantityToAdd;
                        await _unitOfWork.UserItems.UpdateAsync(userItem);
                    }
                    else
                    {
                        await _unitOfWork.UserItems.AddAsync(new UserItem
                        {
                            UserId = userId,
                            ItemId = shopItem.ItemId.Value,
                            Quantity = quantityToAdd
                        });
                    }
                }

                await _unitOfWork.Users.UpdateAsync(user);      
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<ShopPurchaseDto>
                {
                    Success = true,
                    Message = $"Successfully purchased {shopItem.Name} x{request.Quantity}",
                    Data = new ShopPurchaseDto
                    {
                        Id = purchase.Id,
                        UserId = purchase.UserId,
                        ShopItemId = purchase.ShopItemId,
                        ShopItemName = shopItem.Name,
                        Quantity = purchase.Quantity,
                        PaymentType = purchase.PaymentType,
                        AmountPaid = purchase.AmountPaid,
                        PurchaseDate = purchase.PurchaseDate
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ShopPurchaseDto>
                {
                    Success = false,
                    Message = "Error processing purchase",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<List<ShopPurchaseDto>>> GetPurchaseHistoryAsync(Guid userId)
        {
            try
            {
                var purchases = _unitOfWork.ShopPurchases
                    .GetQueryable(asNoTracking: true)
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToList();

                var dtoList = purchases.Select(p => new ShopPurchaseDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    ShopItemId = p.ShopItemId,
                    ShopItemName = p.ShopItem?.Name ?? string.Empty,
                    Quantity = p.Quantity,
                    PaymentType = p.PaymentType,
                    AmountPaid = p.AmountPaid,
                    PurchaseDate = p.PurchaseDate
                }).ToList();

                return new ServiceResult<List<ShopPurchaseDto>>
                {
                    Success = true,
                    Message = "Purchase history retrieved successfully",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<ShopPurchaseDto>>
                {
                    Success = false,
                    Message = "Error retrieving purchase history",
                    Errors = [ex.Message]
                };
            }
        }

        private static ServiceResult<T> Fail<T>(string message) =>
            new() { Success = false, Message = message };
    }
}
