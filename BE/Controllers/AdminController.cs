using BussinessObjects.DTOs;
using BussinessObjects.DTOs.Admin;
using BussinessObjects.DTOs.Gacha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IServices;

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
        private readonly IAdminSettingService _adminSettingService;
        private readonly IGachaService _gachaService;

        public AdminController(
            IAdminGachaService adminGachaService,
            IAdminPaymentService adminPaymentService,
            IAdminUserService adminUserService,
            IAdminSettingService adminSettingService,
            IGachaService gachaService)
        {
            _adminGachaService = adminGachaService;
            _adminPaymentService = adminPaymentService;
            _adminUserService = adminUserService;
            _adminSettingService = adminSettingService;
            _gachaService = gachaService;
        }

        // ════════════════════════════════════════════════════
        // QUẢN LÝ NGƯỜI DÙNG (USERS)
        // ════════════════════════════════════════════════════

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? searchName)
        {
            var result = await _adminUserService.GetAllUsersAsync(searchName);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("users/{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var result = await _adminUserService.GetUserByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPut("users/{id:guid}/toggle-ban")]
        public async Task<IActionResult> ToggleBanUser(Guid id)
        {
            var result = await _adminUserService.ToggleBanUserAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ════════════════════════════════════════════════════
        // QUẢN LÝ ITEMS (dùng IAdminGachaService)
        // ════════════════════════════════════════════════════

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

        [HttpPut("gacha/items/{id:guid}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromForm] CreateUpdateItemDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminGachaService.UpdateItemAsync(id, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ════════════════════════════════════════════════════
        // QUẢN LÝ BANNER (dùng IGachaService)
        // ════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/admin/gacha/items-available?search=sword
        /// Danh sách Item trong DB để admin chọn khi tạo/sửa banner.
        /// </summary>
        [HttpGet("gacha/items-available")]
        public async Task<IActionResult> GetAvailableItems([FromQuery] string? search = null)
        {
            var result = await _gachaService.GetAvailableItemsAsync(search);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// GET /api/admin/gacha/banners
        /// Danh sách tất cả banner (kể cả inactive) dành cho admin quản lý.
        /// </summary>
        [HttpGet("gacha/banners")]
        public async Task<IActionResult> GetAllBanners()
        {
            var result = await _gachaService.GetActiveBannersAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// GET /api/admin/gacha/banners/{bannerId}
        /// </summary>
        [HttpGet("gacha/banners/{bannerId:guid}")]
        public async Task<IActionResult> GetBannerById(Guid bannerId)
        {
            var result = await _gachaService.GetBannerByIdAsync(bannerId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// POST /api/admin/gacha/banners
        /// Tạo banner mới. Dùng multipart/form-data (có thể kèm file ảnh).
        /// </summary>
        [HttpPost("gacha/banners")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateBanner([FromForm] CreateGachaBannerRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _gachaService.CreateBannerAsync(request);
            return result.Success
                ? CreatedAtAction(nameof(GetBannerById), new { bannerId = result.Data?.Id }, result)
                : BadRequest(result);
        }

        /// <summary>
        /// PUT /api/admin/gacha/banners/{bannerId}
        /// Cập nhật banner. Dùng multipart/form-data.
        /// </summary>
        [HttpPut("gacha/banners/{bannerId:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateBanner(Guid bannerId, [FromForm] UpdateGachaBannerRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _gachaService.UpdateBannerAsync(bannerId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// PATCH /api/admin/gacha/banners/{bannerId}/toggle
        /// Bật/tắt trạng thái active của banner.
        /// </summary>
        [HttpPatch("gacha/banners/{bannerId:guid}/toggle")]
        public async Task<IActionResult> ToggleBanner(Guid bannerId)
        {
            var result = await _gachaService.ToggleBannerAsync(bannerId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// POST /api/admin/gacha/banners/{bannerId}/items
        /// Thêm một item vào banner đã có.
        /// </summary>
        [HttpPost("gacha/banners/{bannerId:guid}/items")]
        public async Task<IActionResult> AddItemToBanner(Guid bannerId, [FromBody] AddGachaItemRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _gachaService.AddItemToBannerAsync(bannerId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// DELETE /api/admin/gacha/banners/{bannerId}/items/{itemId}
        /// Xoá một item khỏi banner.
        /// </summary>
        [HttpDelete("gacha/banners/{bannerId:guid}/items/{itemId:guid}")]
        public async Task<IActionResult> RemoveItemFromBanner(Guid bannerId, Guid itemId)
        {
            var result = await _gachaService.RemoveItemFromBannerAsync(bannerId, itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // ════════════════════════════════════════════════════
        // QUẢN LÝ GIAO DỊCH VÀ CỬA HÀNG (PAYMENTS)
        // ════════════════════════════════════════════════════

        [HttpGet("payments/transactions")]
        public async Task<IActionResult> GetAllTransactions(
            [FromQuery] string? status,
            [FromQuery] string? orderCode)
        {
            var result = await _adminPaymentService.GetAllTransactionsAsync(status, orderCode);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("payments/shop-purchases")]
        public async Task<IActionResult> GetAllShopPurchases(
            [FromQuery] string? searchName,
            [FromQuery] int? quantity)
        {
            var result = await _adminPaymentService.GetAllShopPurchasesAsync(searchName, quantity);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ════════════════════════════════════════════════════
        // QUẢN LÝ CẤU HÌNH API (SETTINGS)
        // ════════════════════════════════════════════════════

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

        [HttpDelete("settings/api-keys/{id:guid}")]
        public async Task<IActionResult> DeleteApiSetting(Guid id)
        {
            var result = await _adminSettingService.DeleteApiSettingAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}