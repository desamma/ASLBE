using BussinessObjects.DTOs.UserItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IServices;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserItemController : ControllerBase
    {
        private readonly IUserItemService _userItemService;

        public UserItemController(IUserItemService userItemService)
        {
            _userItemService = userItemService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll()
        {
            var result = _userItemService.GetAll();
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var result = await _userItemService.GetByUserIdAsync(userId);

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpGet("{userId}/{itemId}")]
        public async Task<IActionResult> GetById(Guid userId, Guid itemId)
        {
            var result = await _userItemService.GetByIdAsync(userId, itemId);

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpPost("user/{userId}")]
        public async Task<IActionResult> Add(Guid userId, [FromBody] AddUserItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userItemService.AddAsync(userId, request);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return CreatedAtAction(nameof(GetById), new { userId, itemId = result.Data.ItemId }, result.Data);
        }

        [HttpPut("{userId}/{itemId}")]
        public async Task<IActionResult> Update(Guid userId, Guid itemId, [FromBody] UpdateUserItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userItemService.UpdateAsync(userId, itemId, request);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpDelete("{userId}/{itemId}")]
        public async Task<IActionResult> Delete(Guid userId, Guid itemId)
        {
            var result = await _userItemService.DeleteAsync(userId, itemId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Game client polls this to get items not yet delivered.
        /// </summary>
        [HttpGet("pending-delivery/{userId}")]
        public async Task<IActionResult> GetPendingDelivery(Guid userId)
        {
            var result = await _userItemService.GetPendingDeliveryAsync(userId);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message, data = result.Data });
        }

        /// <summary>
        /// Game client calls this after it has received and applied the items.
        /// Marks them delivered so they won't be returned again.
        /// </summary>
        [HttpPost("acknowledge-delivery/{userId}")]
        public async Task<IActionResult> AcknowledgeDelivery(Guid userId, [FromBody] AcknowledgeDeliveryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userItemService.AcknowledgeDeliveryAsync(userId, request);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
