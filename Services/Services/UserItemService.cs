using BussinessObjects.Models;
using DataAccess.IRepositories;
using Services.IServices;

namespace Services.Services
{
    public class UserItemService : IUserItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<List<UserItemDto>> GetAll()
        {
            try
            {
                var userItems = _unitOfWork.UserItems.GetQueryable(asNoTracking: true)
                    .ToList();

                if (!userItems.Any())
                    return new ServiceResult<List<UserItemDto>>
                    {
                        Success = true,
                        Message = "No user items found",
                        Data = new List<UserItemDto>()
                    };

                var dtoList = userItems.Select(ui => new UserItemDto
                {
                    UserId = ui.UserId,
                    ItemId = ui.ItemId,
                    Quantity = ui.Quantity,
                    QuantityDelivered = ui.QuantityDelivered,
                    IsDeliveredToGame = ui.IsDeliveredToGame,
                    DeliveredToGameAt = ui.DeliveredToGameAt,
                    CreatedAt = ui.CreatedAt,
                    Item = ui.Item != null ? new IItemServiceItemDto
                    {
                        Id = ui.Item.Id,
                        Name = ui.Item.Name,
                        Description = ui.Item.Description,
                        Type = ui.Item.Type,
                        Rarity = ui.Item.Rarity,
                        ImagePath = ui.Item.ImagePath
                    } : null
                }).ToList();

                return new ServiceResult<List<UserItemDto>>
                {
                    Success = true,
                    Message = "User items retrieved successfully",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<UserItemDto>>
                {
                    Success = false,
                    Message = "Error retrieving user items",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<List<UserItemDto>>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return new ServiceResult<List<UserItemDto>>
                    {
                        Success = false,
                        Message = "Invalid user ID"
                    };

                var userItems = _unitOfWork.UserItems.GetQueryable(asNoTracking: true)
                    .Where(ui => ui.UserId == userId)
                    .ToList();

                if (!userItems.Any())
                    return new ServiceResult<List<UserItemDto>>
                    {
                        Success = true,
                        Message = "No items found for this user",
                        Data = new List<UserItemDto>()
                    };

                var dtoList = userItems.Select(ui => new UserItemDto
                {
                    UserId = ui.UserId,
                    ItemId = ui.ItemId,
                    Quantity = ui.Quantity,
                    QuantityDelivered = ui.QuantityDelivered,
                    IsDeliveredToGame = ui.IsDeliveredToGame,
                    DeliveredToGameAt = ui.DeliveredToGameAt,
                    CreatedAt = ui.CreatedAt,
                    Item = ui.Item != null ? new IItemServiceItemDto
                    {
                        Id = ui.Item.Id,
                        Name = ui.Item.Name,
                        Description = ui.Item.Description,
                        Type = ui.Item.Type,
                        Rarity = ui.Item.Rarity,
                        ImagePath = ui.Item.ImagePath
                    } : null
                }).ToList();

                return new ServiceResult<List<UserItemDto>>
                {
                    Success = true,
                    Message = "User items retrieved successfully",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<UserItemDto>>
                {
                    Success = false,
                    Message = "Error retrieving user items",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<UserItemDto>> GetByIdAsync(Guid userId, Guid itemId)
        {
            try
            {
                if (userId == Guid.Empty || itemId == Guid.Empty)
                    return new ServiceResult<UserItemDto>
                    {
                        Success = false,
                        Message = "Invalid user ID or item ID"
                    };

                var userItem = await _unitOfWork.UserItems.FirstOrDefaultAsync(userId, itemId);

                if (userItem == null)
                    return new ServiceResult<UserItemDto>
                    {
                        Success = false,
                        Message = "User item not found"
                    };

                var dto = new UserItemDto
                {
                    UserId = userItem.UserId,
                    ItemId = userItem.ItemId,
                    Quantity = userItem.Quantity,
                    QuantityDelivered = userItem.QuantityDelivered,
                    IsDeliveredToGame = userItem.IsDeliveredToGame,
                    DeliveredToGameAt = userItem.DeliveredToGameAt,
                    CreatedAt = userItem.CreatedAt,
                    Item = userItem.Item != null ? new IItemServiceItemDto
                    {
                        Id = userItem.Item.Id,
                        Name = userItem.Item.Name,
                        Description = userItem.Item.Description,
                        Type = userItem.Item.Type,
                        Rarity = userItem.Item.Rarity,
                        ImagePath = userItem.Item.ImagePath
                    } : null
                };

                return new ServiceResult<UserItemDto>
                {
                    Success = true,
                    Message = "User item retrieved successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<UserItemDto>
                {
                    Success = false,
                    Message = "Error retrieving user item",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<UserItemDto>> AddAsync(Guid userId, AddUserItemRequest request)
        {
            try
            {
                if (userId == Guid.Empty)
                    return new ServiceResult<UserItemDto>
                    {
                        Success = false,
                        Message = "Invalid user ID"
                    };

                if (request.ItemId == Guid.Empty)
                    return new ServiceResult<UserItemDto>
                    {
                        Success = false,
                        Message = "Invalid item ID"
                    };

                if (request.Quantity <= 0)
                    return new ServiceResult<UserItemDto>
                    {
                        Success = false,
                        Message = "Quantity must be greater than 0"
                    };

                var userExists = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (userExists == null)
                    return new ServiceResult<UserItemDto>
                    {
                        Success = false,
                        Message = "User not found"
                    };

                var itemExists = await _unitOfWork.Items.FirstOrDefaultAsync(i => i.Id == request.ItemId);
                if (itemExists == null)
                    return new ServiceResult<UserItemDto>
                    {
                        Success = false,
                        Message = "Item not found"
                    };

                var existingUserItem = await _unitOfWork.UserItems.FirstOrDefaultAsync(userId, request.ItemId);
                if (existingUserItem != null)
                    return new ServiceResult<UserItemDto>
                    {
                        Success = false,
                        Message = "User already has this item"
                    };

                var userItem = new UserItem
                {
                    UserId = userId,
                    ItemId = request.ItemId,
                    Quantity = request.Quantity,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.UserItems.AddAsync(userItem);
                await _unitOfWork.SaveChangesAsync();

                var userItemFromDb = await _unitOfWork.UserItems.FirstOrDefaultAsync(userId, request.ItemId);

                var dto = new UserItemDto
                {
                    UserId = userItemFromDb.UserId,
                    ItemId = userItemFromDb.ItemId,
                    Quantity = userItemFromDb.Quantity,
                    QuantityDelivered = userItemFromDb.QuantityDelivered,
                    IsDeliveredToGame = userItemFromDb.IsDeliveredToGame,
                    DeliveredToGameAt = userItemFromDb.DeliveredToGameAt,
                    CreatedAt = userItemFromDb.CreatedAt,
                    Item = userItemFromDb.Item != null ? new IItemServiceItemDto
                    {
                        Id = userItemFromDb.Item.Id,
                        Name = userItemFromDb.Item.Name,
                        Description = userItemFromDb.Item.Description,
                        Type = userItemFromDb.Item.Type,
                        Rarity = userItemFromDb.Item.Rarity,
                        ImagePath = userItemFromDb.Item.ImagePath
                    } : null
                };

                return new ServiceResult<UserItemDto>
                {
                    Success = true,
                    Message = "User item added successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<UserItemDto>
                {
                    Success = false,
                    Message = "Error adding user item",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<UserItemDto>> UpdateAsync(Guid userId, Guid itemId, UpdateUserItemRequest request)
        {
            try
            {
                if (userId == Guid.Empty || itemId == Guid.Empty)
                    return new ServiceResult<UserItemDto>
                    {
                        Success = false,
                        Message = "Invalid user ID or item ID"
                    };

                if (request.Quantity <= 0)
                    return new ServiceResult<UserItemDto>
                    {
                        Success = false,
                        Message = "Quantity must be greater than 0"
                    };

                var userItem = await _unitOfWork.UserItems.FirstOrDefaultAsync(userId, itemId);
                if (userItem == null)
                    return new ServiceResult<UserItemDto>
                    {
                        Success = false,
                        Message = "User item not found"
                    };

                userItem.Quantity = request.Quantity;
                await _unitOfWork.UserItems.UpdateAsync(userItem);
                await _unitOfWork.SaveChangesAsync();

                var dto = new UserItemDto
                {
                    UserId = userItem.UserId,
                    ItemId = userItem.ItemId,
                    Quantity = userItem.Quantity,
                    QuantityDelivered = userItem.QuantityDelivered,
                    IsDeliveredToGame = userItem.IsDeliveredToGame,
                    DeliveredToGameAt = userItem.DeliveredToGameAt,
                    CreatedAt = userItem.CreatedAt,
                    Item = userItem.Item != null ? new IItemServiceItemDto
                    {
                        Id = userItem.Item.Id,
                        Name = userItem.Item.Name,
                        Description = userItem.Item.Description,
                        Type = userItem.Item.Type,
                        Rarity = userItem.Item.Rarity,
                        ImagePath = userItem.Item.ImagePath
                    } : null
                };

                return new ServiceResult<UserItemDto>
                {
                    Success = true,
                    Message = "User item updated successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<UserItemDto>
                {
                    Success = false,
                    Message = "Error updating user item",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(Guid userId, Guid itemId)
        {
            try
            {
                if (userId == Guid.Empty || itemId == Guid.Empty)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Invalid user ID or item ID"
                    };

                var userItem = await _unitOfWork.UserItems.FirstOrDefaultAsync(userId, itemId);
                if (userItem == null)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "User item not found"
                    };

                await _unitOfWork.UserItems.DeleteAsync(userId, itemId);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "User item deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Error deleting user item",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<List<UserItemDto>>> GetPendingDeliveryAsync(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return new ServiceResult<List<UserItemDto>>
                    {
                        Success = false,
                        Message = "Invalid user ID"
                    };

                var pendingItems = _unitOfWork.UserItems
                    .GetQueryable(asNoTracking: true)
                    .Where(ui => ui.UserId == userId && ui.QuantityDelivered < ui.Quantity)
                    .ToList();

                var dtoList = pendingItems.Select(ui => new UserItemDto
                {
                    UserId = ui.UserId,
                    ItemId = ui.ItemId,
                    Quantity = ui.Quantity,
                    QuantityDelivered = ui.QuantityDelivered,
                    IsDeliveredToGame = ui.IsDeliveredToGame,
                    DeliveredToGameAt = ui.DeliveredToGameAt,
                    CreatedAt = ui.CreatedAt,
                    Item = ui.Item != null ? new IItemServiceItemDto
                    {
                        Id = ui.Item.Id,
                        Name = ui.Item.Name,
                        Description = ui.Item.Description,
                        Type = ui.Item.Type,
                        Rarity = ui.Item.Rarity,
                        ImagePath = ui.Item.ImagePath
                    } : null
                }).ToList();

                return new ServiceResult<List<UserItemDto>>
                {
                    Success = true,
                    Message = pendingItems.Any()
                        ? $"{pendingItems.Count} item type(s) with pending quantities"
                        : "No pending items",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<UserItemDto>>
                {
                    Success = false,
                    Message = "Error retrieving pending items",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<bool>> AcknowledgeDeliveryAsync(Guid userId, AcknowledgeDeliveryRequest request)
        {
            try
            {
                if (userId == Guid.Empty)
                    return new ServiceResult<bool> { Success = false, Message = "Invalid user ID" };

                if (request.Items == null || !request.Items.Any())
                    return new ServiceResult<bool> { Success = false, Message = "No items provided" };

                var itemsToMark = new List<UserItem>();
                foreach (var deliveryItem in request.Items)
                {
                    if (deliveryItem.ItemId == Guid.Empty || deliveryItem.Quantity <= 0)
                        return new ServiceResult<bool>
                        {
                            Success = false,
                            Message = "Invalid item ID or quantity"
                        };

                    var userItem = await _unitOfWork.UserItems.FirstOrDefaultAsync(userId, deliveryItem.ItemId);

                    if (userItem == null)
                        return new ServiceResult<bool>
                        {
                            Success = false,
                            Message = $"UserItem not found for ItemId: {deliveryItem.ItemId}"
                        };

                    // Check if delivery quantity exceeds available quantity
                    int availableQuantity = userItem.Quantity - userItem.QuantityDelivered;
                    if (deliveryItem.Quantity > availableQuantity)
                        return new ServiceResult<bool>
                        {
                            Success = false,
                            Message = $"Cannot deliver {deliveryItem.Quantity} items. Only {availableQuantity} available for ItemId: {deliveryItem.ItemId}"
                        };

                    // Skip if already fully delivered
                    if (userItem.QuantityDelivered >= userItem.Quantity)
                        continue;

                    userItem.QuantityDelivered += deliveryItem.Quantity;
                    
                    // Mark as fully delivered if all quantities have been acknowledged
                    if (userItem.QuantityDelivered >= userItem.Quantity)
                    {
                        userItem.IsDeliveredToGame = true;
                        userItem.DeliveredToGameAt = DateTime.UtcNow;
                    }

                    itemsToMark.Add(userItem);
                }

                foreach (var item in itemsToMark)
                    await _unitOfWork.UserItems.UpdateAsync(item);

                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = $"{itemsToMark.Count} item(s) delivery acknowledged",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Error acknowledging delivery",
                    Errors = [ex.Message]
                };
            }
        }
    }
}
