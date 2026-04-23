using BussinessObjects.DTOs.Auth;

namespace Services.IServices
{
    public interface IAuthService
    {
        Task<AuthServiceResult> RegisterAsync(string userName, string email, string password, string baseUrl);
        Task<AuthServiceResult> ConfirmEmailAsync(Guid userId, string token);
        Task<AuthServiceResult> LoginAsync(string email, string password);
        Task<AuthServiceResult> ForgotPasswordAsync(string email, string baseUrl);
        Task<AuthServiceResult> ResetPasswordAsync(Guid userId, string token, string newPassword);
    }
}
