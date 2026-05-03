namespace BussinessObjects.DTOs.Auth
{
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
        public string Avatar { get; set; }
        public decimal CurrencyAmount { get; set; }
        public int Gender { get; set; }
    }
}
