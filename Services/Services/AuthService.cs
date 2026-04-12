using BussinessObjects.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private const string DefaultRole = "user";

        public AuthService(
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task<AuthServiceResult> RegisterAsync(string userName, string email, string password, string baseUrl)
        {
            var existingByEmail = await _userManager.FindByEmailAsync(email);
            if (existingByEmail != null)
                return new AuthServiceResult
                {
                    Success = false,
                    Message = "Email is already taken"
                };

            var existingByUserName = await _userManager.FindByNameAsync(userName);
            if (existingByUserName != null)
                return new AuthServiceResult
                {
                    Success = false,
                    Message = "UserName is already taken"
                };

            var user = new User
            {
                UserName = userName,
                Email = email,
                Gender = 0,
                EmailConfirmed = false,
                CurrencyAmount = 0,
                CreatedDate = DateTime.UtcNow,
                IsBanned = false,
                PhoneNumber = null,
                UserDOB = null,
                UserAvatar = null
            };

            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                return new AuthServiceResult
                {
                    Success = false,
                    Message = "User creation failed",
                    Errors = createResult.Errors.Select(e => e.Description)
                };
            }

            if (!await _roleManager.RoleExistsAsync(DefaultRole))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = DefaultRole });
            }

            await _userManager.AddToRoleAsync(user, DefaultRole);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{baseUrl}/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var emailBody = $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>";

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email", emailBody);

            return new AuthServiceResult
            {
                Success = true,
                Message = "Registration successful. Please check your email to confirm your account."
            };
        }

        public async Task<AuthServiceResult> ConfirmEmailAsync(Guid userId, string token)
        {
            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
                return new AuthServiceResult
                {
                    Success = false,
                    Message = "Invalid email confirmation request"
                };

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new AuthServiceResult
                {
                    Success = false,
                    Message = "User not found"
                };

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return new AuthServiceResult
                {
                    Success = false,
                    Message = "Email confirmation failed",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }

            return new AuthServiceResult
            {
                Success = true,
                Message = "Email confirmed successfully. You can now login."
            };
        }

        public async Task<AuthServiceResult> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new AuthServiceResult
                {
                    Success = false,
                    Message = "Invalid credentials"
                };

            if (user.IsBanned)
                return new AuthServiceResult
                {
                    Success = false,
                    Message = "User is banned"
                };

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);

            if (!passwordValid)
                return new AuthServiceResult
                {
                    Success = false,
                    Message = "Invalid credentials"
                };

            if (!user.EmailConfirmed)
                return new AuthServiceResult
                {
                    Success = false,
                    Message = "Email not confirmed. Please check your email."
                };

            var token = await GenerateJwtTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthServiceResult
            {
                Success = true,
                Message = "Login successful",
                Data = new AuthData
                {
                    Token = token,
                    ExpiresIn = 7 * 24 * 60 * 60,
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToArray(),
                    Avatar = user.UserAvatar ?? "",
                    CurrencyAmount = user.CurrencyAmount,
                    Gender = user.Gender
                }
            };
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var key = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["JwtSettings:Key"] ?? "MotChuoiKiTuRatDaiVaBaoMatChoJWT123!@#";
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["JwtSettings:Issuer"] ?? "MyLocalServer";
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["JwtSettings:Audience"] ?? "MyLocalClient";

            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("JWT_KEY is not configured");

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("Avatar", user.UserAvatar ?? ""),
                new("CurrencyAmount", user.CurrencyAmount.ToString("F2"))
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expiresInDays = int.TryParse(_configuration["JwtSettings:ExpiresInDays"], out var days) ? days : 7;
            var expires = DateTime.UtcNow.AddDays(expiresInDays);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
