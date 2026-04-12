using BussinessObjects.Models;
using DataAccess.IRepositories;
using Services.IServices;
using Microsoft.AspNetCore.Identity;

namespace Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public UserService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public ServiceResult<List<UserDto>> GetAll()
        {
            try
            {
                var users = _unitOfWork.Users.GetQueryable(asNoTracking: true).ToList();

                if (!users.Any())
                    return new ServiceResult<List<UserDto>>
                    {
                        Success = true,
                        Message = "No users found",
                        Data = new List<UserDto>()
                    };

                var dtoList = users.Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    UserDOB = u.UserDOB,
                    Gender = u.Gender,
                    UserAvatar = u.UserAvatar,
                    SaveFilePath = u.SaveFilePath,
                    CreatedDate = u.CreatedDate,
                    IsBanned = u.IsBanned,
                    CurrencyAmount = u.CurrencyAmount,
                    PityCounter = u.PityCounter
                }).ToList();

                return new ServiceResult<List<UserDto>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<UserDto>>
                {
                    Success = false,
                    Message = "Error retrieving users",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<UserDto>> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<UserDto>
                    {
                        Success = false,
                        Message = "Invalid user ID"
                    };

                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    return new ServiceResult<UserDto>
                    {
                        Success = false,
                        Message = "User not found"
                    };

                var dto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    UserDOB = user.UserDOB,
                    Gender = user.Gender,
                    UserAvatar = user.UserAvatar,
                    SaveFilePath = user.SaveFilePath,
                    CreatedDate = user.CreatedDate,
                    IsBanned = user.IsBanned,
                    CurrencyAmount = user.CurrencyAmount,
                    PityCounter = user.PityCounter
                };

                return new ServiceResult<UserDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<UserDto>
                {
                    Success = false,
                    Message = "Error retrieving user",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<UserDto>
                    {
                        Success = false,
                        Message = "Invalid user ID"
                    };

                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    return new ServiceResult<UserDto>
                    {
                        Success = false,
                        Message = "User not found"
                    };

                if (string.IsNullOrWhiteSpace(request.UserName) ||
                    string.IsNullOrWhiteSpace(request.Email))
                {
                    return new ServiceResult<UserDto>
                    {
                        Success = false,
                        Message = "Username and Email are required",
                        Errors = ["Username and Email cannot be empty"]
                    };
                }

                // Check if username is changed and if new username already exists
                if (user.UserName != request.UserName)
                {
                    var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
                    if (existingUser != null)
                        return new ServiceResult<UserDto>
                        {
                            Success = false,
                            Message = "A user with this username already exists"
                        };
                }

                user.UserName = request.UserName;
                user.Email = request.Email;
                user.PhoneNumber = request.PhoneNumber;
                user.UserDOB = request.UserDOB;
                user.Gender = request.Gender;
                user.UserAvatar = request.UserAvatar;
                user.SaveFilePath = request.SaveFilePath;
                user.IsBanned = request.IsBanned;
                user.CurrencyAmount = request.CurrencyAmount;
                user.PityCounter = request.PityCounter;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var dto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    UserDOB = user.UserDOB,
                    Gender = user.Gender,
                    UserAvatar = user.UserAvatar,
                    SaveFilePath = user.SaveFilePath,
                    CreatedDate = user.CreatedDate,
                    IsBanned = user.IsBanned,
                    CurrencyAmount = user.CurrencyAmount,
                    PityCounter = user.PityCounter
                };

                return new ServiceResult<UserDto>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<UserDto>
                {
                    Success = false,
                    Message = "Error updating user",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<bool>> DisableAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Invalid user ID"
                    };

                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "User not found"
                    };

                user.IsBanned = true;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "User disabled successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Error disabling user",
                    Errors = [ex.Message]
                };
            }
        }
    }
}
