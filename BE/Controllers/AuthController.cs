using Microsoft.AspNetCore.Mvc;
using Services.IServices;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = await _authService.RegisterAsync(
                model.UserName,
                model.Email,
                model.Password,
                baseUrl);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
        {
            var result = await _authService.ConfirmEmailAsync(userId, token);

            if (!result.Success)
            {
                if (result.Message == "User not found")
                    return NotFound(new { message = result.Message });

                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return Ok(new { message = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model.Email, model.Password);

            if (!result.Success)
            {
                if (result.Message == "User is banned")
                    return StatusCode(403, new { message = result.Message });

                return Unauthorized(new { message = result.Message });
            }

            return Ok(new AuthResponse
            {
                Token = result.Data.Token,
                ExpiresIn = result.Data.ExpiresIn,
                UserId = result.Data.UserId,
                UserName = result.Data.UserName,
                Email = result.Data.Email,
                Roles = result.Data.Roles
            });
        }
    }

    public class RegisterRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
    }
}
