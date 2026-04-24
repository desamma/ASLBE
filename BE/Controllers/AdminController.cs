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

        public AdminController(
            IAdminGachaService adminGachaService,
            IAdminPaymentService adminPaymentService,
            IAdminUserService adminUserService)
        {
            _adminGachaService = adminGachaService;
            _adminPaymentService = adminPaymentService;
            _adminUserService = adminUserService;
        }

        #region --- QUẢN LÝ NGƯỜI DÙNG (USERS) ---

        // GET: api/admin/users?searchName=abc
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? searchName)
        {
            var result = await _adminUserService.GetAllUsersAsync(searchName);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // GET: api/admin/users/{id}
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var result = await _adminUserService.GetUserByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // PUT: api/admin/users/{id}/toggle-ban
        [HttpPut("users/{id}/toggle-ban")]
        public async Task<IActionResult> ToggleBanUser(Guid id)
        {
            var result = await _adminUserService.ToggleBanUserAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ĐÃ XÓA BỎ API AdjustUserCurrency Ở ĐÂY THEO YÊU CẦU

        #endregion

        #region --- QUẢN LÝ GACHA / ITEMS ---

        // GET: api/admin/gacha/history?userId={id}
        [HttpGet("gacha/history")]
        public async Task<IActionResult> GetGachaHistory([FromQuery] Guid? userId)
        {
            var result = await _adminGachaService.GetGachaHistoryAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // GET: api/admin/gacha/items
        [HttpGet("gacha/items")]
        public async Task<IActionResult> GetAllItems()
        {
            var result = await _adminGachaService.GetAllItemsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // POST: api/admin/gacha/items
        [HttpPost("gacha/items")]
        public async Task<IActionResult> CreateItem([FromForm] CreateUpdateItemDto request) // Đã đổi sang FromForm
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminGachaService.CreateItemAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // PUT: api/admin/gacha/items/{id}
        [HttpPut("gacha/items/{id}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromForm] CreateUpdateItemDto request) // Đã đổi sang FromForm
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminGachaService.UpdateItemAsync(id, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region --- QUẢN LÝ GIAO DỊCH VÀ CỬA HÀNG (PAYMENTS) ---

        // GET: api/admin/payments/transactions?status=Paid&orderCode=123
        [HttpGet("payments/transactions")]
        public async Task<IActionResult> GetAllTransactions([FromQuery] string? status, [FromQuery] string? orderCode)
        {
            // Đã cập nhật truyền thêm orderCode
            var result = await _adminPaymentService.GetAllTransactionsAsync(status, orderCode);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // GET: api/admin/payments/shop-purchases?searchName=abc&quantity=2
        [HttpGet("payments/shop-purchases")]
        public async Task<IActionResult> GetAllShopPurchases([FromQuery] string? searchName, [FromQuery] int? quantity)
        {
            // Đã cập nhật truyền thêm searchName và quantity
            var result = await _adminPaymentService.GetAllShopPurchasesAsync(searchName, quantity);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        #endregion
    }
}