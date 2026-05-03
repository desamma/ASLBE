using BussinessObjects.DTOs.User;

namespace Services.IServices
{
    public interface IUserService
    {
        ServiceResult<List<UserDto>> GetAll();
        Task<ServiceResult<UserDto>> GetByIdAsync(Guid id);
        Task<ServiceResult<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request);
        Task<ServiceResult<bool>> DisableAsync(Guid id);
    }
}
