using BussinessObjects.DTOs.Admin;
using BussinessObjects.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Services
{
    public class AdminSettingService : IAdminSettingService
    {
        private readonly ApplicationDbContext _context;

        public AdminSettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<List<ApiSettingDto>>> GetAllApiSettingsAsync()
        {
            var settings = await _context.ApiSettings
                .OrderByDescending(s => s.UpdatedAt)
                .Select(s => new ApiSettingDto
                {
                    Id = s.Id,
                    GeminiApiKey = s.GeminiApiKey,
                    ColabApiUrl = s.ColabApiUrl,
                    UpdatedAt = s.UpdatedAt
                }).ToListAsync();

            return new ServiceResult<List<ApiSettingDto>> { Success = true, Data = settings };
        }

        public async Task<ServiceResult<ApiSettingDto>> CreateApiSettingAsync(ApiSettingDto dto)
        {
            var newSetting = new ApiSetting
            {
                Id = Guid.NewGuid(),
                GeminiApiKey = dto.GeminiApiKey,
                ColabApiUrl = dto.ColabApiUrl,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ApiSettings.Add(newSetting);
            await _context.SaveChangesAsync();

            dto.Id = newSetting.Id;
            dto.UpdatedAt = newSetting.UpdatedAt;

            return new ServiceResult<ApiSettingDto> { Success = true, Message = "Thêm cấu hình API thành công!", Data = dto };
        }

        public async Task<ServiceResult<bool>> DeleteApiSettingAsync(Guid id)
        {
            var setting = await _context.ApiSettings.FindAsync(id);
            if (setting != null)
            {
                _context.ApiSettings.Remove(setting);
                await _context.SaveChangesAsync();
                return new ServiceResult<bool> { Success = true, Message = "Xóa API thành công!" };
            }
            return new ServiceResult<bool> { Success = false, Message = "Không tìm thấy API!" };
        }
    }
}