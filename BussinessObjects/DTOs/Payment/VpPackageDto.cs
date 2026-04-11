using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Payment
{
    public class VpPackageDto
    {
        public int VpKey { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int BaseVp { get; set; }
        public int BonusVp { get; set; }
        public int TotalVp { get; set; }
        public decimal PriceVnd { get; set; }
    }
}
