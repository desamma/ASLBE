using BussinessObjects.DTOs.Admin;
using BussinessObjects.Models;
using DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Services
{
    public class AdminGachaService : IAdminGachaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Inject IWebHostEnvironment vào Constructor
        public AdminGachaService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
                Message = "Successfully retrieved Gacha history."
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
                Message = "Successfully retrieved item list."
            };
        }

        public async Task<ServiceResult<AdminItemDto>> CreateItemAsync(CreateUpdateItemDto dto)
        {
            string finalImagePath = dto.ImagePath ?? string.Empty;

            // XỬ LÝ LƯU ẢNH LOCAL NẾU CÓ FILE ĐƯỢC UPLOAD
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Tạo tên file độc nhất để tránh trùng lặp
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(fileStream);
                }

                // Lưu đường dẫn tương đối vào Database
                finalImagePath = "/images/" + uniqueFileName;
            }

            var newItem = new Item
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                Rarity = dto.Rarity,
                ImagePath = finalImagePath,
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

            return new ServiceResult<AdminItemDto> { Success = true, Data = resultDto, Message = "Item created successfully." };
        }

        public async Task<ServiceResult<AdminItemDto>> UpdateItemAsync(Guid itemId, CreateUpdateItemDto dto)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return new ServiceResult<AdminItemDto> { Success = false, Message = "Item not found." };
            }

            // XỬ LÝ LƯU ẢNH LOCAL NẾU CÓ FILE MỚI ĐƯỢC UPLOAD
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(fileStream);
                }

                // Cập nhật link ảnh mới
                item.ImagePath = "/images/" + uniqueFileName;
            }
            else if (!string.IsNullOrEmpty(dto.ImagePath))
            {
                // Nếu không upload file mới mà truyền link thì dùng link đó
                item.ImagePath = dto.ImagePath;
            }

            item.Name = dto.Name;
            item.Description = dto.Description;
            item.Type = dto.Type;
            item.Rarity = dto.Rarity;
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

            return new ServiceResult<AdminItemDto> { Success = true, Data = resultDto, Message = "Item updated successfully." };
        }
    }
}