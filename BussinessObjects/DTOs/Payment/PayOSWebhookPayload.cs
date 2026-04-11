using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Payment
{
    public class PayOSWebhookPayload
    {
        public string Code { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public bool Success { get; set; }
        public PayOSWebhookData? Data { get; set; }
        public string Signature { get; set; } = string.Empty;
    }
}
