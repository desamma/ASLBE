using Microsoft.AspNetCore.Mvc;

namespace BE.Controllers
{
    using BussinessObjects.DTOs.Gacha;
    // BE/Controllers/GachaController.cs
    using BussinessObjects.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Services.IServices;
    using System.Security.Claims;

    namespace BE.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class GachaController : ControllerBase
        {
            private readonly IGachaService _gachaService;
            public GachaController(IGachaService gachaService) => _gachaService = gachaService;

            // ── PUBLIC ───────────────────────────────────────────

            /// <summary>Lấy danh sách banner đang active (hiển thị trên Gacha page)</summary>
            [HttpGet("banners")]
            public async Task<IActionResult> GetActiveBanners()
            {
                var result = await _gachaService.GetActiveBannersAsync();
                if (!result.Success) return BadRequest(new { message = result.Message });
                return Ok(new { message = result.Message, data = result.Data });
            }

            /// <summary>Chi tiết 1 banner: featured items, tỷ lệ, cost</summary>
            [HttpGet("banners/{bannerId}")]
            public async Task<IActionResult> GetBanner(Guid bannerId)
            {
                var result = await _gachaService.GetBannerByIdAsync(bannerId);
                if (!result.Success) return NotFound(new { message = result.Message });
                return Ok(new { message = result.Message, data = result.Data });
            }

            // ── USER (đăng nhập) ─────────────────────────────────

            /// <summary>Wish x1 — tốn 100 Gems</summary>
            [HttpPost("wish/single")]
            [Authorize]
            public async Task<IActionResult> WishSingle([FromBody] GachaSinglePullRequest request)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized(new { message = "Invalid token" });

                var result = await _gachaService.SinglePullAsync(userId.Value, request);
                if (!result.Success) return BadRequest(new { message = result.Message, errors = result.Errors });
                return Ok(new { message = result.Message, data = result.Data });
            }

            /// <summary>Wish x10 — tốn 1000 Gems, guaranteed 4★+ trong batch</summary>
            [HttpPost("wish/multi")]
            [Authorize]
            public async Task<IActionResult> WishMulti([FromBody] GachaMultiPullRequest request)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized(new { message = "Invalid token" });

                var result = await _gachaService.MultiPullAsync(userId.Value, request);
                if (!result.Success) return BadRequest(new { message = result.Message, errors = result.Errors });
                return Ok(new { message = result.Message, data = result.Data });
            }

            /// <summary>Gems balance + pity counter + pulls until guarantee</summary>
            [HttpGet("status/{bannerId}")]
            [Authorize]
            public async Task<IActionResult> GetStatus(Guid bannerId)
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized(new { message = "Invalid token" });

                var result = await _gachaService.GetUserGachaStatusAsync(userId.Value, bannerId);
                if (!result.Success) return BadRequest(new { message = result.Message });
                return Ok(new { message = result.Message, data = result.Data });
            }

            /// <summary>Lịch sử pull của user (phân trang)</summary>
            [HttpGet("history")]
            [Authorize]
            public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized(new { message = "Invalid token" });

                var result = await _gachaService.GetUserHistoryAsync(userId.Value, page, pageSize);
                if (!result.Success) return BadRequest(new { message = result.Message });
                return Ok(new { message = result.Message, data = result.Data });
            }

            // ── ADMIN ────────────────────────────────────────────

            [HttpPost("banners")]
            [Authorize(Roles = "admin")]
            public async Task<IActionResult> CreateBanner([FromBody] CreateGachaBannerRequest request)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var result = await _gachaService.CreateBannerAsync(request);
                if (!result.Success) return BadRequest(new { message = result.Message, errors = result.Errors });
                return Created(nameof(GetBanner), new { message = result.Message, data = result.Data });
            }

            [HttpPut("banners/{bannerId}")]
            [Authorize(Roles = "admin")]
            public async Task<IActionResult> UpdateBanner(Guid bannerId, [FromBody] UpdateGachaBannerRequest request)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var result = await _gachaService.UpdateBannerAsync(bannerId, request);
                if (!result.Success) return BadRequest(new { message = result.Message });
                return Ok(new { message = result.Message, data = result.Data });
            }

            [HttpPut("banners/{bannerId}/toggle")]
            [Authorize(Roles = "admin")]
            public async Task<IActionResult> ToggleBanner(Guid bannerId)
            {
                var result = await _gachaService.ToggleBannerAsync(bannerId);
                if (!result.Success) return BadRequest(new { message = result.Message });
                return Ok(new { message = result.Message, data = result.Data });
            }

            [HttpPost("banners/{bannerId}/items")]
            [Authorize(Roles = "admin")]
            public async Task<IActionResult> AddItem(Guid bannerId, [FromBody] AddGachaItemRequest request)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var result = await _gachaService.AddItemToBannerAsync(bannerId, request);
                if (!result.Success) return BadRequest(new { message = result.Message });
                return Ok(new { message = result.Message });
            }

            [HttpDelete("banners/{bannerId}/items/{itemId}")]
            [Authorize(Roles = "admin")]
            public async Task<IActionResult> RemoveItem(Guid bannerId, Guid itemId)
            {
                var result = await _gachaService.RemoveItemFromBannerAsync(bannerId, itemId);
                if (!result.Success) return BadRequest(new { message = result.Message });
                return Ok(new { message = result.Message });
            }

            // ── HELPER ──────────────────────────────────────────
            private Guid? GetCurrentUserId()
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;
                return Guid.TryParse(claim, out var id) ? id : null;
            }
        }
    }
}
