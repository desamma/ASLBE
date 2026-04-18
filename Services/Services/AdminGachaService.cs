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
                .Include(h => h.GachaItem)
                    .ThenInclude(gi => gi.Item) // Giả định GachaItem có Navigation Property trỏ tới Item
                .AsNoTracking();

            // Nếu Admin muốn lọc theo 1 User cụ thể
            if (userId.HasValue)
            {
                query = query.Where(h => h.UserId == userId.Value);
            }

            // Sắp xếp lịch sử mới nhất lên đầu
            var history = await query.OrderByDescending(h => h.Date)
                .Select(h => new AdminGachaHistoryDto
                {
                    Id = h.Id,
                    UserId = h.UserId,
                    UserName = h.User != null ? h.User.UserName : "Unknown",
                    // Lấy tên và độ hiếm từ Item thông qua GachaItem
                    ItemName = h.GachaItem != null && h.GachaItem.Item != null ? h.GachaItem.Item.Name : "Unknown Item",
                    ItemRarity = h.GachaItem != null && h.GachaItem.Item != null ? h.GachaItem.Item.Rarity : "Unknown",
                    Date = h.Date,
                    IsSuccess = h.IsSuccess,
                    NewUserBalance = h.NewUserBalance
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
            // 1. Khởi tạo Object Item mới
            var newItem = new Item
            {
                Id = Guid.NewGuid(), // Tạo ID mới
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                Rarity = dto.Rarity,
                ImagePath = dto.ImagePath,
                StatsLines = dto.StatsLines ?? new List<string>()
            };

            // 2. Lưu vào Database
            _context.Items.Add(newItem);
            await _context.SaveChangesAsync();

            // 3. Map sang DTO để trả về cho Web hiển thị luôn mà không cần gọi lại API get
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
            // 1. Tìm vật phẩm trong database
            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return new ServiceResult<AdminItemDto> { Success = false, Message = "Không tìm thấy vật phẩm này." };
            }

            // 2. Cập nhật các trường thông tin
            item.Name = dto.Name;
            item.Description = dto.Description;
            item.Type = dto.Type;
            item.Rarity = dto.Rarity;
            item.ImagePath = dto.ImagePath;
            item.StatsLines = dto.StatsLines ?? new List<string>();

            // 3. Lưu thay đổi
            _context.Items.Update(item);
            await _context.SaveChangesAsync();

            // 4. Trả về kết quả
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
