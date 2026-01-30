namespace Services.IServices
{
    public interface IAuthService
    {
        Task<AuthServiceResult> RegisterAsync(string userName, string email, string password, string baseUrl);
        Task<AuthServiceResult> ConfirmEmailAsync(Guid userId, string token);
        Task<AuthServiceResult> LoginAsync(string email, string password);
    }

    public class AuthServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public AuthData? Data { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }

    public class AuthData
    {
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
    }
}
