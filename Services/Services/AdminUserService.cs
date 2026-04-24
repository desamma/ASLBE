using BussinessObjects.DTOs.Admin;
using BussinessObjects.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
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
                IsBanned = u.IsBanned,
                CreatedDate = u.CreatedDate
            }).ToListAsync();

            return new ServiceResult<List<AdminUserDto>>
            {
                Success = true,
                Data = users,
                Message = "Successfully retrieved user list."
            };
        }

        public async Task<ServiceResult<AdminUserDto>> GetUserByIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new ServiceResult<AdminUserDto> { Success = false, Message = "User not found." };
            }

            var userDto = new AdminUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CurrencyAmount = user.CurrencyAmount,
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
                return new ServiceResult<bool> { Success = false, Message = "User not found." };
            }

            user.IsBanned = !user.IsBanned;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                string status = user.IsBanned ? "has been banned" : "has been unbanned";
                return new ServiceResult<bool> { Success = true, Data = true, Message = $"Account {user.UserName} {status}." };
            }

            return new ServiceResult<bool> { Success = false, Message = "An error occurred while updating the account status." };
        }
    }
}