using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IServices;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopItemController : ControllerBase
    {
        private readonly IShopItemService _shopItemService;

        public ShopItemController(IShopItemService shopItemService)
        {
            _shopItemService = shopItemService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _shopItemService.GetAll();
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpGet("active")]
        public IActionResult GetActive()
        {
            var result = _shopItemService.GetActiveOnly();
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _shopItemService.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] CreateShopItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _shopItemService.CreateAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Created(nameof(GetById), new { message = result.Message, data = result.Data });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShopItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _shopItemService.UpdateAsync(id, request);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpPut("{id}/disable")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Disable(Guid id)
        {
            var result = await _shopItemService.DisableAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpPut("{id}/enable")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Enable(Guid id)
        {
            var result = await _shopItemService.EnableAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
