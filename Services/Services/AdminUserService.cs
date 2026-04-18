using BussinessObjects.DTOs.Admin;
using BussinessObjects.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly UserManager<User> _userManager;

        public AdminUserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ServiceResult<List<AdminUserDto>>> GetAllUsersAsync(string? searchName = null)
        {
            var query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(u => u.UserName.Contains(searchName) || u.Email.Contains(searchName));
            }

            var users = await query.Select(u => new AdminUserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                CurrencyAmount = u.CurrencyAmount,
                PityCounter = u.PityCounter,
                IsBanned = u.IsBanned,
                CreatedDate = u.CreatedDate
            }).ToListAsync();

            return new ServiceResult<List<AdminUserDto>>
            {
                Success = true,
                Data = users,
                Message = "Lấy danh sách thành công"
            };
        }

        public async Task<ServiceResult<AdminUserDto>> GetUserByIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new ServiceResult<AdminUserDto> { Success = false, Message = "Không tìm thấy người dùng." };
            }

            var userDto = new AdminUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CurrencyAmount = user.CurrencyAmount,
                PityCounter = user.PityCounter,
                IsBanned = user.IsBanned,
                CreatedDate = user.CreatedDate
            };

            return new ServiceResult<AdminUserDto> { Success = true, Data = userDto };
        }

        public async Task<ServiceResult<bool>> ToggleBanUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new ServiceResult<bool> { Success = false, Message = "Không tìm thấy người dùng." };
            }

            user.IsBanned = !user.IsBanned;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                string status = user.IsBanned ? "đã bị khóa" : "đã được mở khóa";
                return new ServiceResult<bool> { Success = true, Data = true, Message = $"Tài khoản {user.UserName} {status}." };
            }

            return new ServiceResult<bool> { Success = false, Message = "Có lỗi xảy ra khi cập nhật trạng thái." };
        }

        public async Task<ServiceResult<bool>> AdjustUserCurrencyAsync(Guid userId, int amountChange)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new ServiceResult<bool> { Success = false, Message = "Không tìm thấy người dùng." };
            }

            if (user.CurrencyAmount + amountChange < 0)
            {
                user.CurrencyAmount = 0;
            }
            else
            {
                user.CurrencyAmount += amountChange;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new ServiceResult<bool> { Success = true, Data = true, Message = $"Đã cập nhật số dư của {user.UserName} thành {user.CurrencyAmount}." };
            }

            return new ServiceResult<bool> { Success = false, Message = "Có lỗi xảy ra khi cập nhật số dư." };
        }
    }
}
