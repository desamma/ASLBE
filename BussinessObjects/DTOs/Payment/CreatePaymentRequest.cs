using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Payment
{
    public partial class CreatePaymentRequest
    {
        /// <summary>
        /// Gói VP muốn mua. Các giá trị hợp lệ:
        /// 52, 110, 275, 610, 1220, 3040, 6550, 13250
        /// </summary>
        public int VpPackage { get; set; }
    }
}
