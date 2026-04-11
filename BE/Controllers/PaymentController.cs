using BussinessObjects.DTOs.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Services.IServices;
using System.Security.Claims;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // ────────────────────────────────────────────────────────────────────────
        // GET /api/payment/packages
        // Lấy danh sách gói VP để hiển thị UI (không cần đăng nhập)
        // ────────────────────────────────────────────────────────────────────────
        [HttpGet("packages")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _paymentService.GetPackagesAsync();
            return Ok(new { success = true, data = packages });
        }

        // ────────────────────────────────────────────────────────────────────────
        // POST /api/payment/create
        // Tạo payment link — cần đăng nhập
        // Body: { "vpPackage": 110 }
        // ────────────────────────────────────────────────────────────────────────
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Guid userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { success = false, message = "Không xác định được user." });

            var result = await _paymentService.CreatePaymentAsync(userId, request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ────────────────────────────────────────────────────────────────────────
        // POST /api/payment/webhook
        // PayOS gọi endpoint này sau khi thanh toán — KHÔNG cần auth
        // ────────────────────────────────────────────────────────────────────────
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromBody] PayOSWebhookPayload payload)
        {
            _logger.LogInformation("PayOS webhook hit. Code={Code}", payload.Code);

            bool ok = await _paymentService.HandleWebhookAsync(payload);

            // PayOS yêu cầu trả 200 OK để dừng retry
            return Ok(new { success = ok });
        }

        // ────────────────────────────────────────────────────────────────────────
        // GET /api/payment/status/{orderCode}
        // FE gọi sau khi user quay về từ trang PayOS để kiểm tra kết quả
        // ────────────────────────────────────────────────────────────────────────
        [HttpGet("status/{orderCode:long}")]
        [Authorize]
        public async Task<IActionResult> GetStatus(long orderCode)
        {
            var transaction = await _paymentService.GetTransactionByOrderCodeAsync(orderCode);
            if (transaction is null)
                return NotFound(new { success = false, message = "Không tìm thấy giao dịch." });

            return Ok(new { success = true, data = transaction });
        }

        // ────────────────────────────────────────────────────────────────────────
        // GET /api/payment/history
        // Lịch sử nạp tiền của user hiện tại
        // ────────────────────────────────────────────────────────────────────────
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetHistory()
        {
            Guid userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var transactions = await _paymentService.GetUserTransactionsAsync(userId);
            return Ok(new { success = true, data = transactions });
        }

        // ────────────────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────────────────
        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("sub");

            if (claim is null) return Guid.Empty;
            return Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }
    }
}