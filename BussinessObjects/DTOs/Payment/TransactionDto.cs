using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Payment
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public long OrderCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int CurrencyAwarded { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CheckoutUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
