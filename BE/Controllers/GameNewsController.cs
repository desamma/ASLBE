using BussinessObjects.DTOs.GameNews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IServices;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameNewsController : ControllerBase
    {
        private readonly IGameNewsService _gameNewsService;

        public GameNewsController(IGameNewsService gameNewsService)
        {
            _gameNewsService = gameNewsService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _gameNewsService.GetAll();
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _gameNewsService.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] CreateGameNewsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _gameNewsService.CreateAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Created(nameof(GetById), new { message = result.Message, data = result.Data });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameNewsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _gameNewsService.UpdateAsync(id, request);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _gameNewsService.DeleteAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
