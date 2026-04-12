using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Payment
{
    public static class VpPackageCatalog
    {
        public static readonly Dictionary<int, VpPackage> Packages = new()
        {
            [52] = new() { Vp = 52, BonusVp = 0, PriceVnd = 10_000, DisplayName = "Gói 52 VP" },
            [110] = new() { Vp = 110, BonusVp = 0, PriceVnd = 20_000, DisplayName = "Gói 110 VP" },
            [275] = new() { Vp = 275, BonusVp = 0, PriceVnd = 50_000, DisplayName = "Gói 275 VP" },
            [610] = new() { Vp = 500, BonusVp = 110, PriceVnd = 100_000, DisplayName = "Gói 610 VP" },
            [1220] = new() { Vp = 1000, BonusVp = 220, PriceVnd = 200_000, DisplayName = "Gói 1220 VP" },
            [3040] = new() { Vp = 2750, BonusVp = 290, PriceVnd = 500_000, DisplayName = "Gói 3040 VP" },
            [6550] = new() { Vp = 5500, BonusVp = 1050, PriceVnd = 1_000_000, DisplayName = "Gói 6550 VP" },
            [13250] = new() { Vp = 11000, BonusVp = 2250, PriceVnd = 2_000_000, DisplayName = "Gói 13250 VP" },
        };

        public static bool TryGet(int vpKey, out VpPackage? package)
            => Packages.TryGetValue(vpKey, out package);
    }
}
