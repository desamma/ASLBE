using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Payment
{
    public partial class CreatePaymentResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? CheckoutUrl { get; set; }
        public long? OrderCode { get; set; }
        public Guid? TransactionId { get; set; }
    }
}
