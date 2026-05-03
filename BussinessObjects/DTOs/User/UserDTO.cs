namespace BussinessObjects.DTOs.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateOnly? UserDOB { get; set; }
        public byte Gender { get; set; }
        public string UserAvatar { get; set; }
        public string SaveFilePath { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsBanned { get; set; }
        public decimal CurrencyAmount { get; set; }
        public int PityCounter { get; set; }
    }

    public class CreateUserRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public DateOnly? UserDOB { get; set; }
        public byte Gender { get; set; }
        public string UserAvatar { get; set; }
        public string SaveFilePath { get; set; }
    }

    public class UpdateUserRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateOnly? UserDOB { get; set; }
        public byte Gender { get; set; }
        public string UserAvatar { get; set; }
        public string SaveFilePath { get; set; }
        public bool IsBanned { get; set; }
        public int CurrencyAmount { get; set; }
        public int PityCounter { get; set; }
    }
}
