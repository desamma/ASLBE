using BussinessObjects.DTOs.Admin;
using BussinessObjects.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class AdminGachaService : IAdminGachaService
    {
        private readonly ApplicationDbContext _context;

        public AdminGachaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<List<AdminGachaHistoryDto>>> GetGachaHistoryAsync(Guid? userId = null)
        {
            var query = _context.GachaHistories
                .Include(h => h.User)
                .Include(h => h.Item)
                .AsNoTracking();

            if (userId.HasValue)
            {
                query = query.Where(h => h.UserId == userId.Value);
            }

            var history = await query.OrderByDescending(h => h.PulledAt)
                .Select(h => new AdminGachaHistoryDto
                {
                    Id = h.Id,
                    UserId = h.UserId,
                    UserName = h.User != null ? h.User.UserName : "Unknown",

                    ItemName = h.Item != null ? h.Item.Name : "Unknown Item",
                    ItemRarity = h.Item != null ? h.Item.Rarity : "Unknown",

                    Date = h.PulledAt,

                    IsSuccess = true,
                    NewUserBalance = 0
                }).ToListAsync();

            return new ServiceResult<List<AdminGachaHistoryDto>>
            {
                Success = true,
                Data = history,
                Message = "Lấy lịch sử Gacha thành công"
            };
        }

        public async Task<ServiceResult<List<AdminItemDto>>> GetAllItemsAsync()
        {
            var items = await _context.Items.AsNoTracking()
                .Select(i => new AdminItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Type = i.Type,
                    Rarity = i.Rarity,
                    ImagePath = i.ImagePath,
                    StatsLines = i.StatsLines
                }).ToListAsync();

            return new ServiceResult<List<AdminItemDto>>
            {
                Success = true,
                Data = items,
                Message = "Lấy danh sách vật phẩm thành công"
            };
        }

        public async Task<ServiceResult<AdminItemDto>> CreateItemAsync(CreateUpdateItemDto dto)
        {
            var newItem = new Item
            {
                Id = Guid.NewGuid(), 
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                Rarity = dto.Rarity,
                ImagePath = dto.ImagePath,
                StatsLines = dto.StatsLines ?? new List<string>()
            };

            _context.Items.Add(newItem);
            await _context.SaveChangesAsync();

            var resultDto = new AdminItemDto
            {
                Id = newItem.Id,
                Name = newItem.Name,
                Description = newItem.Description,
                Type = newItem.Type,
                Rarity = newItem.Rarity,
                ImagePath = newItem.ImagePath,
                StatsLines = newItem.StatsLines
            };

            return new ServiceResult<AdminItemDto> { Success = true, Data = resultDto, Message = "Tạo vật phẩm thành công." };
        }

        public async Task<ServiceResult<AdminItemDto>> UpdateItemAsync(Guid itemId, CreateUpdateItemDto dto)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return new ServiceResult<AdminItemDto> { Success = false, Message = "Không tìm thấy vật phẩm này." };
            }

            item.Name = dto.Name;
            item.Description = dto.Description;
            item.Type = dto.Type;
            item.Rarity = dto.Rarity;
            item.ImagePath = dto.ImagePath;
            item.StatsLines = dto.StatsLines ?? new List<string>();

            _context.Items.Update(item);
            await _context.SaveChangesAsync();

            var resultDto = new AdminItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Type = item.Type,
                Rarity = item.Rarity,
                ImagePath = item.ImagePath,
                StatsLines = item.StatsLines
            };

            return new ServiceResult<AdminItemDto> { Success = true, Data = resultDto, Message = "Cập nhật vật phẩm thành công." };
        }
    }
}
