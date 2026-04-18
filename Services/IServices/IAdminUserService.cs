using BussinessObjects.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IAdminUserService
    {
        // Lấy danh sách người dùng
        Task<ServiceResult<List<AdminUserDto>>> GetAllUsersAsync(string? searchName = null);

        // Xem chi tiết 1 người dùng
        Task<ServiceResult<AdminUserDto>> GetUserByIdAsync(Guid userId);

        // Khóa hoặc Mở khóa người dùng
        Task<ServiceResult<bool>> ToggleBanUserAsync(Guid userId);

        // Điều chỉnh tiền tệ (cộng hoặc trừ)
        Task<ServiceResult<bool>> AdjustUserCurrencyAsync(Guid userId, int amountChange);
    }
}
