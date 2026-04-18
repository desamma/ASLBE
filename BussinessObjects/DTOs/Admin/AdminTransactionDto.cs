using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Admin
{
    public class AdminTransactionDto
    {
        public Guid Id { get; set; }
        public long OrderCode { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }

        public string Name { get; set; } = string.Empty; // Tên gói nạp
        public string Type { get; set; } = string.Empty; // TopUp
        public int Amount { get; set; } // Số VND
        public int CurrencyAwarded { get; set; } // Số VP nhận được

        public string Status { get; set; } = string.Empty; // Pending | Paid | Cancelled
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
