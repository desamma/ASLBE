using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Services.IServices;

namespace BE.Controllers
{
    [Route("api/admin/payments")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminPaymentController : ControllerBase
    {
        private readonly IAdminPaymentService _adminPaymentService;

        public AdminPaymentController(IAdminPaymentService adminPaymentService)
        {
            _adminPaymentService = adminPaymentService;
        }

        // GET: api/admin/payments/transactions?status=Paid
        [HttpGet("transactions")]
        public async Task<IActionResult> GetAllTransactions([FromQuery] string? status)
        {
            var result = await _adminPaymentService.GetAllTransactionsAsync(status);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // GET: api/admin/payments/shop-purchases
        [HttpGet("shop-purchases")]
        public async Task<IActionResult> GetAllShopPurchases()
        {
            var result = await _adminPaymentService.GetAllShopPurchasesAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}