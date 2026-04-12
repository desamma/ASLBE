using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Payment
{
    public class VpPackage
    {
        public int Vp { get; set; }          // VP base
        public int BonusVp { get; set; }     // VP bonus
        public int TotalVp => Vp + BonusVp;
        public int PriceVnd { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}