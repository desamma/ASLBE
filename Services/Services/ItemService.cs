using BussinessObjects.DTOs.Item;
using BussinessObjects.Models;
using DataAccess.IRepositories;
using Services.IServices;

namespace Services.Services
{
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<List<ItemDto>> GetAll()
        {
            try
            {
                var items = _unitOfWork.Items.GetQueryable(asNoTracking: true).ToList();

                if (!items.Any())
                    return new ServiceResult<List<ItemDto>>
                    {
                        Success = true,
                        Message = "No items found",
                        Data = new List<ItemDto>()
                    };

                var dtoList = items.Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Type = i.Type,
                    Rarity = i.Rarity,
                    ImagePath = i.ImagePath,
                    IsGachaOnly = i.IsGachaOnly,
                    IsActive = i.IsActive,
                    StatsLines = i.StatsLines
                }).ToList();

                return new ServiceResult<List<ItemDto>>
                {
                    Success = true,
                    Message = "Items retrieved successfully",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<ItemDto>>
                {
                    Success = false,
                    Message = "Error retrieving items",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<ItemDto>> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<ItemDto>
                    {
                        Success = false,
                        Message = "Invalid item ID"
                    };

                var item = await _unitOfWork.Items.FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                    return new ServiceResult<ItemDto>
                    {
                        Success = false,
                        Message = "Item not found"
                    };

                var dto = new ItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Type = item.Type,
                    Rarity = item.Rarity,
                    ImagePath = item.ImagePath,
                    IsGachaOnly = item.IsGachaOnly,
                    IsActive = item.IsActive,
                    StatsLines = item.StatsLines
                };

                return new ServiceResult<ItemDto>
                {
                    Success = true,
                    Message = "Item retrieved successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ItemDto>
                {
                    Success = false,
                    Message = "Error retrieving item",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<ItemDto>> CreateAsync(CreateItemRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name) ||
                    string.IsNullOrWhiteSpace(request.Description) ||
                    string.IsNullOrWhiteSpace(request.Type) ||
                    string.IsNullOrWhiteSpace(request.Rarity) ||
                    string.IsNullOrWhiteSpace(request.ImagePath))
                {
                    return new ServiceResult<ItemDto>
                    {
                        Success = false,
                        Message = "All fields are required",
                        Errors = ["Name, Description, Type, Rarity, and ImagePath cannot be empty"]
                    };
                }

                var existingItem = await _unitOfWork.Items.FirstOrDefaultAsync(i => i.Name == request.Name);
                if (existingItem != null)
                    return new ServiceResult<ItemDto>
                    {
                        Success = false,
                        Message = "An item with this name already exists"
                    };

                var item = new Item
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    Type = request.Type,
                    Rarity = request.Rarity,
                    ImagePath = request.ImagePath,
                    IsGachaOnly = request.IsGachaOnly,
                    IsActive = request.IsActive,
                    StatsLines = request.StatsLines ?? new List<string>()
                };

                await _unitOfWork.Items.AddAsync(item);
                await _unitOfWork.SaveChangesAsync();

                var dto = new ItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Type = item.Type,
                    Rarity = item.Rarity,
                    ImagePath = item.ImagePath,
                    IsGachaOnly = item.IsGachaOnly,
                    IsActive = item.IsActive,
                    StatsLines = item.StatsLines
                };

                return new ServiceResult<ItemDto>
                {
                    Success = true,
                    Message = "Item created successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ItemDto>
                {
                    Success = false,
                    Message = "Error creating item",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<ItemDto>> UpdateAsync(Guid id, UpdateItemRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<ItemDto>
                    {
                        Success = false,
                        Message = "Invalid item ID"
                    };

                var item = await _unitOfWork.Items.FirstOrDefaultAsync(i => i.Id == id);
                if (item == null)
                    return new ServiceResult<ItemDto>
                    {
                        Success = false,
                        Message = "Item not found"
                    };

                if (string.IsNullOrWhiteSpace(request.Name) ||
                    string.IsNullOrWhiteSpace(request.Description) ||
                    string.IsNullOrWhiteSpace(request.Type) ||
                    string.IsNullOrWhiteSpace(request.Rarity) ||
                    string.IsNullOrWhiteSpace(request.ImagePath))
                {
                    return new ServiceResult<ItemDto>
                    {
                        Success = false,
                        Message = "All fields are required",
                        Errors = ["Name, Description, Type, Rarity, and ImagePath cannot be empty"]
                    };
                }

                // Check if name is changed and if new name already exists
                if (item.Name != request.Name)
                {
                    var existingItem = await _unitOfWork.Items.FirstOrDefaultAsync(i => i.Name == request.Name);
                    if (existingItem != null)
                        return new ServiceResult<ItemDto>
                        {
                            Success = false,
                            Message = "An item with this name already exists"
                        };
                }

                item.Name = request.Name;
                item.Description = request.Description;
                item.Type = request.Type;
                item.Rarity = request.Rarity;
                item.ImagePath = request.ImagePath;
                item.IsGachaOnly = request.IsGachaOnly;
                item.IsActive = request.IsActive;
                item.StatsLines = request.StatsLines ?? new List<string>();

                await _unitOfWork.Items.UpdateAsync(item);
                await _unitOfWork.SaveChangesAsync();

                var dto = new ItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Type = item.Type,
                    Rarity = item.Rarity,
                    ImagePath = item.ImagePath,
                    IsGachaOnly = item.IsGachaOnly,
                    IsActive = item.IsActive,
                    StatsLines = item.StatsLines
                };

                return new ServiceResult<ItemDto>
                {
                    Success = true,
                    Message = "Item updated successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ItemDto>
                {
                    Success = false,
                    Message = "Error updating item",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Invalid item ID"
                    };

                var item = await _unitOfWork.Items.FirstOrDefaultAsync(i => i.Id == id);
                if (item == null)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Item not found"
                    };

                await _unitOfWork.Items.DeleteAsync(item);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "Item deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Error deleting item",
                    Errors = [ex.Message]
                };
            }
        }
    }
}
