using BussinessObjects.DTOs;
using BussinessObjects.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IServices;
using System;
using System.Threading.Tasks;

namespace BE.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminGachaService _adminGachaService;
        private readonly IAdminPaymentService _adminPaymentService;
        private readonly IAdminUserService _adminUserService;
        // 1. Khai báo Setting Service
        private readonly IAdminSettingService _adminSettingService;

        // 2. Inject vào Constructor
        public AdminController(
            IAdminGachaService adminGachaService,
            IAdminPaymentService adminPaymentService,
            IAdminUserService adminUserService,
            IAdminSettingService adminSettingService)
        {
            _adminGachaService = adminGachaService;
            _adminPaymentService = adminPaymentService;
            _adminUserService = adminUserService;
            _adminSettingService = adminSettingService;
        }

        #region --- QUẢN LÝ NGƯỜI DÙNG (USERS) ---

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? searchName)
        {
            var result = await _adminUserService.GetAllUsersAsync(searchName);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var result = await _adminUserService.GetUserByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPut("users/{id}/toggle-ban")]
        public async Task<IActionResult> ToggleBanUser(Guid id)
        {
            var result = await _adminUserService.ToggleBanUserAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region --- QUẢN LÝ GACHA / ITEMS ---

        [HttpGet("gacha/history")]
        public async Task<IActionResult> GetGachaHistory([FromQuery] Guid? userId)
        {
            var result = await _adminGachaService.GetGachaHistoryAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("gacha/items")]
        public async Task<IActionResult> GetAllItems()
        {
            var result = await _adminGachaService.GetAllItemsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("gacha/items")]
        public async Task<IActionResult> CreateItem([FromForm] CreateUpdateItemDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminGachaService.CreateItemAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("gacha/items/{id}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromForm] CreateUpdateItemDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminGachaService.UpdateItemAsync(id, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region --- QUẢN LÝ GIAO DỊCH VÀ CỬA HÀNG (PAYMENTS) ---

        [HttpGet("payments/transactions")]
        public async Task<IActionResult> GetAllTransactions([FromQuery] string? status, [FromQuery] string? orderCode)
        {
            var result = await _adminPaymentService.GetAllTransactionsAsync(status, orderCode);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("payments/shop-purchases")]
        public async Task<IActionResult> GetAllShopPurchases([FromQuery] string? searchName, [FromQuery] int? quantity)
        {
            var result = await _adminPaymentService.GetAllShopPurchasesAsync(searchName, quantity);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region --- QUẢN LÝ CẤU HÌNH API (SETTINGS) ---

        [HttpGet("settings/api-keys")]
        public async Task<IActionResult> GetApiSettings()
        {
            var result = await _adminSettingService.GetAllApiSettingsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("settings/api-keys")]
        public async Task<IActionResult> CreateApiSettings([FromBody] ApiSettingDto request)
        {
            var result = await _adminSettingService.CreateApiSettingAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("settings/api-keys/{id}")]
        public async Task<IActionResult> DeleteApiSetting(Guid id)
        {
            var result = await _adminSettingService.DeleteApiSettingAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        #endregion
    }
}