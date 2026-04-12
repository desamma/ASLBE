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

        // GET /api/payment/packages
        [HttpGet("packages")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _paymentService.GetPackagesAsync();
            return Ok(new { success = true, data = packages });
        }

        // POST /api/payment/create
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Guid userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { success = false, message = "User not identified." });

            var result = await _paymentService.CreatePaymentAsync(userId, request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // POST /api/payment/webhook
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromBody] PayOSWebhookPayload payload)
        {
            _logger.LogInformation("PayOS webhook hit. Code={Code}", payload.Code);
            bool ok = await _paymentService.HandleWebhookAsync(payload);
            return Ok(new { success = ok });
        }

        // GET /api/payment/status/{orderCode}
        [HttpGet("status/{orderCode:long}")]
        [Authorize]
        public async Task<IActionResult> GetStatus(long orderCode)
        {
            var transaction = await _paymentService.GetTransactionByOrderCodeAsync(orderCode);
            if (transaction is null)
                return NotFound(new { success = false, message = "Transaction not found." });

            return Ok(new { success = true, data = transaction });
        }

        // GET /api/payment/history
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
        // GET /api/payment/balance
        // Lấy VP hiện tại từ DB — dùng để cập nhật UI header
        // ────────────────────────────────────────────────────────────────────────
        [HttpGet("balance")]
        [Authorize]
        public async Task<IActionResult> GetBalance()
        {
            Guid userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var balance = await _paymentService.GetUserBalanceAsync(userId);
            return Ok(new { success = true, balance });
        }

        // Helpers
        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("sub");

            if (claim is null) return Guid.Empty;
            return Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }
    }
}
