
using BussinessObjects.DTOs.Shop;
using BussinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IServices;
using System.Security.Claims;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShopPurchaseController : ControllerBase
    {
        private readonly IShopPurchaseService _shopPurchaseService;

        public ShopPurchaseController(IShopPurchaseService shopPurchaseService)
        {
            _shopPurchaseService = shopPurchaseService;
        }

        /// <summary>
        /// Mua item từ shop bằng currency của user hiện tại
        /// </summary>
        [HttpPost("buy")]
        public async Task<IActionResult> Buy([FromBody] BuyShopItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var result = await _shopPurchaseService.BuyItemAsync(userId.Value, request);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message, data = result.Data });
        }

        /// <summary>
        /// Xem lịch sử mua hàng của user hiện tại
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var result = await _shopPurchaseService.GetPurchaseHistoryAsync(userId.Value);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message, data = result.Data });
        }

        private Guid? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}