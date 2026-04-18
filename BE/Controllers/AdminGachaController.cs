using BussinessObjects.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IServices;

namespace BE.Controllers
{
    [Route("api/admin/gacha")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminGachaController : ControllerBase
    {
        private readonly IAdminGachaService _adminGachaService;

        public AdminGachaController(IAdminGachaService adminGachaService)
        {
            _adminGachaService = adminGachaService;
        }

        // GET: api/admin/gacha/history
        // GET: api/admin/gacha/history?userId={id}
        [HttpGet("history")]
        public async Task<IActionResult> GetGachaHistory([FromQuery] Guid? userId)
        {
            var result = await _adminGachaService.GetGachaHistoryAsync(userId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // GET: api/admin/gacha/items
        [HttpGet("items")]
        public async Task<IActionResult> GetAllItems()
        {
            var result = await _adminGachaService.GetAllItemsAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // POST: api/admin/gacha/items
        [HttpPost("items")]
        public async Task<IActionResult> CreateItem([FromBody] CreateUpdateItemDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminGachaService.CreateItemAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // PUT: api/admin/gacha/items/{id}
        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromBody] CreateUpdateItemDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminGachaService.UpdateItemAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}