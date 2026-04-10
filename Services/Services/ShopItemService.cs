using BussinessObjects.Models;
using DataAccess.IRepositories;
using Services.IServices;

namespace Services.Services
{
    public class ShopItemService : IShopItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShopItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<List<ShopItemDto>> GetAll()
        {
            try
            {
                var shopItems = _unitOfWork.ShopItems.GetQueryable(asNoTracking: true).ToList();

                if (!shopItems.Any())
                    return new ServiceResult<List<ShopItemDto>>
                    {
                        Success = true,
                        Message = "No shop items found",
                        Data = new List<ShopItemDto>()
                    };

                var dtoList = MapToDto(shopItems);

                return new ServiceResult<List<ShopItemDto>>
                {
                    Success = true,
                    Message = "Shop items retrieved successfully",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<ShopItemDto>>
                {
                    Success = false,
                    Message = "Error retrieving shop items",
                    Errors = [ex.Message]
                };
            }
        }

        public ServiceResult<List<ShopItemDto>> GetActiveOnly()
        {
            try
            {
                var shopItems = _unitOfWork.ShopItems.GetQueryable(asNoTracking: true)
                    .Where(si => si.IsActive)
                    .ToList();

                if (!shopItems.Any())
                    return new ServiceResult<List<ShopItemDto>>
                    {
                        Success = true,
                        Message = "No active shop items found",
                        Data = new List<ShopItemDto>()
                    };

                var dtoList = MapToDto(shopItems);

                return new ServiceResult<List<ShopItemDto>>
                {
                    Success = true,
                    Message = "Active shop items retrieved successfully",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<ShopItemDto>>
                {
                    Success = false,
                    Message = "Error retrieving active shop items",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<ShopItemDto>> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<ShopItemDto>
                    {
                        Success = false,
                        Message = "Invalid shop item ID"
                    };

                var shopItem = await _unitOfWork.ShopItems.FirstOrDefaultAsync(si => si.Id == id);

                if (shopItem == null)
                    return new ServiceResult<ShopItemDto>
                    {
                        Success = false,
                        Message = "Shop item not found"
                    };

                var dto = MapToDto(shopItem);

                return new ServiceResult<ShopItemDto>
                {
                    Success = true,
                    Message = "Shop item retrieved successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ShopItemDto>
                {
                    Success = false,
                    Message = "Error retrieving shop item",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<ShopItemDto>> CreateAsync(CreateShopItemRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name) ||
                    string.IsNullOrWhiteSpace(request.Category) ||
                    string.IsNullOrWhiteSpace(request.ImagePath))
                {
                    return new ServiceResult<ShopItemDto>
                    {
                        Success = false,
                        Message = "Required fields are missing",
                        Errors = ["Name, Category, and ImagePath are required"]
                    };
                }

                var existingShopItem = await _unitOfWork.ShopItems.FirstOrDefaultAsync(si => si.Name == request.Name);
                if (existingShopItem != null)
                    return new ServiceResult<ShopItemDto>
                    {
                        Success = false,
                        Message = "A shop item with this name already exists"
                    };

                // Validate ItemId if provided
                if (request.ItemId.HasValue && request.ItemId.Value != Guid.Empty)
                {
                    var item = await _unitOfWork.Items.FirstOrDefaultAsync(i => i.Id == request.ItemId.Value);
                    if (item == null)
                        return new ServiceResult<ShopItemDto>
                        {
                            Success = false,
                            Message = "The specified item does not exist"
                        };
                }

                var shopItem = new ShopItem
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    Category = request.Category,
                    ItemType = request.ItemType,
                    ImagePath = request.ImagePath,
                    Price = request.Price,
                    CurrencyAmount = request.CurrencyAmount,
                    IsUsingPremiumCurrency = request.IsUsingPremiumCurrency,
                    ItemQuantity = request.ItemQuantity,
                    ItemId = request.ItemId,
                    IsFeatured = request.IsFeatured,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = null
                };

                await _unitOfWork.ShopItems.AddAsync(shopItem);
                await _unitOfWork.SaveChangesAsync();

                var dto = MapToDto(shopItem);

                return new ServiceResult<ShopItemDto>
                {
                    Success = true,
                    Message = "Shop item created successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ShopItemDto>
                {
                    Success = false,
                    Message = "Error creating shop item",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<ShopItemDto>> UpdateAsync(Guid id, UpdateShopItemRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<ShopItemDto>
                    {
                        Success = false,
                        Message = "Invalid shop item ID"
                    };

                var shopItem = await _unitOfWork.ShopItems.FirstOrDefaultAsync(si => si.Id == id);
                if (shopItem == null)
                    return new ServiceResult<ShopItemDto>
                    {
                        Success = false,
                        Message = "Shop item not found"
                    };

                if (string.IsNullOrWhiteSpace(request.Name) ||
                    string.IsNullOrWhiteSpace(request.Category) ||
                    string.IsNullOrWhiteSpace(request.ImagePath))
                {
                    return new ServiceResult<ShopItemDto>
                    {
                        Success = false,
                        Message = "Required fields are missing",
                        Errors = ["Name, Category, and ImagePath are required"]
                    };
                }

                // Check if name is changed and if new name already exists
                if (shopItem.Name != request.Name)
                {
                    var existingShopItem = await _unitOfWork.ShopItems.FirstOrDefaultAsync(si => si.Name == request.Name);
                    if (existingShopItem != null)
                        return new ServiceResult<ShopItemDto>
                        {
                            Success = false,
                            Message = "A shop item with this name already exists"
                        };
                }

                // Validate ItemId if provided and changed
                if (request.ItemId.HasValue && request.ItemId.Value != Guid.Empty)
                {
                    if (shopItem.ItemId != request.ItemId.Value)
                    {
                        var item = await _unitOfWork.Items.FirstOrDefaultAsync(i => i.Id == request.ItemId.Value);
                        if (item == null)
                            return new ServiceResult<ShopItemDto>
                            {
                                Success = false,
                                Message = "The specified item does not exist"
                            };
                    }
                }

                shopItem.Name = request.Name;
                shopItem.Description = request.Description;
                shopItem.Category = request.Category;
                shopItem.ItemType = request.ItemType;
                shopItem.ImagePath = request.ImagePath;
                shopItem.Price = request.Price;
                shopItem.CurrencyAmount = request.CurrencyAmount;
                shopItem.IsUsingPremiumCurrency = request.IsUsingPremiumCurrency;
                shopItem.ItemQuantity = request.ItemQuantity;
                shopItem.ItemId = request.ItemId;
                shopItem.IsFeatured = request.IsFeatured;
                shopItem.UpdatedDate = DateTime.Now;

                await _unitOfWork.ShopItems.UpdateAsync(shopItem);
                await _unitOfWork.SaveChangesAsync();

                var dto = MapToDto(shopItem);

                return new ServiceResult<ShopItemDto>
                {
                    Success = true,
                    Message = "Shop item updated successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ShopItemDto>
                {
                    Success = false,
                    Message = "Error updating shop item",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<bool>> DisableAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Invalid shop item ID"
                    };

                var shopItem = await _unitOfWork.ShopItems.FirstOrDefaultAsync(si => si.Id == id);
                if (shopItem == null)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Shop item not found"
                    };

                if (!shopItem.IsActive)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Shop item is already disabled"
                    };

                shopItem.IsActive = false;
                shopItem.UpdatedDate = DateTime.Now;

                await _unitOfWork.ShopItems.UpdateAsync(shopItem);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "Shop item disabled successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Error disabling shop item",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<bool>> EnableAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Invalid shop item ID"
                    };

                var shopItem = await _unitOfWork.ShopItems.FirstOrDefaultAsync(si => si.Id == id);
                if (shopItem == null)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Shop item not found"
                    };

                if (shopItem.IsActive)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Shop item is already enabled"
                    };

                shopItem.IsActive = true;
                shopItem.UpdatedDate = DateTime.Now;

                await _unitOfWork.ShopItems.UpdateAsync(shopItem);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "Shop item enabled successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Error enabling shop item",
                    Errors = [ex.Message]
                };
            }
        }

        private ShopItemDto MapToDto(ShopItem shopItem)
        {
            return new ShopItemDto
            {
                Id = shopItem.Id,
                Name = shopItem.Name,
                Description = shopItem.Description,
                Category = shopItem.Category,
                ItemType = shopItem.ItemType,
                ImagePath = shopItem.ImagePath,
                Price = shopItem.Price,
                CurrencyAmount = shopItem.CurrencyAmount,
                IsUsingPremiumCurrency = shopItem.IsUsingPremiumCurrency,
                ItemQuantity = shopItem.ItemQuantity,
                ItemId = shopItem.ItemId,
                IsActive = shopItem.IsActive,
                IsFeatured = shopItem.IsFeatured,
                CreatedDate = shopItem.CreatedDate,
                UpdatedDate = shopItem.UpdatedDate
            };
        }

        private List<ShopItemDto> MapToDto(List<ShopItem> shopItems)
        {
            return shopItems.Select(MapToDto).ToList();
        }
    }
}
