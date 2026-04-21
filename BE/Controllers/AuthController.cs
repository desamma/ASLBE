using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Services.IServices;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
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
                Roles = result.Data.Roles,
                Avatar = result.Data.Avatar,
                CurrencyAmount = result.Data.CurrencyAmount,
                Gender = result.Data.Gender
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var frontendBaseUrl = _configuration["FrontendUrl"] ?? "https://localhost:7032";
            var result = await _authService.ForgotPasswordAsync(model.Email, frontendBaseUrl);

            return Ok(new { message = result.Message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(model.UserId, model.Token, model.NewPassword);

            if (!result.Success)
            {
                if (result.Message == "User not found")
                    return NotFound(new { message = result.Message });

                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return Ok(new { message = result.Message });
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
        public string Avatar { get; set; }
        public decimal CurrencyAmount { get; set; }
        public int Gender { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}

