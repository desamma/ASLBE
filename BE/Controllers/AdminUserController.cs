using BussinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization; // Đảm bảo có thư viện này
using Microsoft.AspNetCore.Mvc;
using Services.IServices;
using System;
using System.Threading.Tasks;

namespace BE.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUserController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        // GET: api/admin/users?searchName=abc
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? searchName)
        {
            var result = await _adminUserService.GetAllUsersAsync(searchName);

            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // GET: api/admin/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var result = await _adminUserService.GetUserByIdAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        // PUT: api/admin/users/{id}/toggle-ban
        [HttpPut("{id}/toggle-ban")]
        public async Task<IActionResult> ToggleBanUser(Guid id)
        {
            var result = await _adminUserService.ToggleBanUserAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // PUT: api/admin/users/{id}/adjust-currency
        [HttpPut("{id}/adjust-currency")]
        public async Task<IActionResult> AdjustUserCurrency(Guid id, [FromBody] AdjustCurrencyDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ." });
            }

            var result = await _adminUserService.AdjustUserCurrencyAsync(id, request.AmountChange);

            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}