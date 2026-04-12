using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Payment
{
    public class PayOSWebhookData
    {
        public long OrderCode { get; set; }
        public long Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string TransactionDateTime { get; set; } = string.Empty;
        public string? PaymentLinkId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string? CounterAccountBankId { get; set; }
        public string? CounterAccountBankName { get; set; }
        public string? CounterAccountName { get; set; }
        public string? CounterAccountNumber { get; set; }
        public string? VirtualAccountName { get; set; }
        public string? VirtualAccountNumber { get; set; }
        public string Currency { get; set; } = "VND";
    }
}
