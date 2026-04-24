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
    public class AdminSettingService : IAdminSettingService
    {
        private readonly ApplicationDbContext _context;

        public AdminSettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<ApiSettingDto>> GetApiSettingAsync()
        {
            // Lấy dòng config đầu tiên trong DB (vì ta chỉ cần 1 dòng duy nhất để lưu cấu hình)
            var setting = await _context.ApiSettings.FirstOrDefaultAsync();
            if (setting == null)
            {
                return new ServiceResult<ApiSettingDto> { Success = true, Data = new ApiSettingDto() };
            }

            return new ServiceResult<ApiSettingDto>
            {
                Success = true,
                Data = new ApiSettingDto
                {
                    GeminiApiKey = setting.GeminiApiKey,
                    ColabApiUrl = setting.ColabApiUrl
                }
            };
        }

        public async Task<ServiceResult<ApiSettingDto>> UpdateApiSettingAsync(ApiSettingDto dto)
        {
            var setting = await _context.ApiSettings.FirstOrDefaultAsync();

            if (setting == null)
            {
                // Nếu chưa có thì tạo mới
                setting = new ApiSetting
                {
                    Id = Guid.NewGuid(),
                    GeminiApiKey = dto.GeminiApiKey,
                    ColabApiUrl = dto.ColabApiUrl,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ApiSettings.Add(setting);
            }
            else
            {
                // Nếu có rồi thì cập nhật
                setting.GeminiApiKey = dto.GeminiApiKey;
                setting.ColabApiUrl = dto.ColabApiUrl;
                setting.UpdatedAt = DateTime.UtcNow;
                _context.ApiSettings.Update(setting);
            }

            await _context.SaveChangesAsync();

            return new ServiceResult<ApiSettingDto>
            {
                Success = true,
                Message = "Lưu API Key thành công!",
                Data = dto
            };
        }
    }
}
