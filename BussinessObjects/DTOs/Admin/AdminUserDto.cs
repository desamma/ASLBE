using System;

namespace BussinessObjects.DTOs.Admin
{
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int CurrencyAmount { get; set; }
        public bool IsBanned { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}