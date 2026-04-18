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
        Task<ServiceResult<List<AdminUserDto>>> GetAllUsersAsync(string? searchName = null);

        Task<ServiceResult<AdminUserDto>> GetUserByIdAsync(Guid userId);

        Task<ServiceResult<bool>> ToggleBanUserAsync(Guid userId);

        Task<ServiceResult<bool>> AdjustUserCurrencyAsync(Guid userId, int amountChange);
    }
}
