using BussinessObjects.DTOs.Admin;

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
