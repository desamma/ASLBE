using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Gacha
{
    
        // ── REQUEST ──────────────────────────────────────────────

        public class GachaSinglePullRequest
        {
            [Required]
            public Guid BannerId { get; set; }
        }

        public class GachaMultiPullRequest
        {
            [Required]
            public Guid BannerId { get; set; }
        }

        public class CreateGachaBannerRequest
        {
            [Required, MaxLength(100)]
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string BannerImagePath { get; set; } = string.Empty;
            public int CostPerSinglePull { get; set; } = 100;
            public int CostPerMultiPull { get; set; } = 1000;
            public int PityThreshold { get; set; } = 10;
            public int HardPityThreshold { get; set; } = 90;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<AddGachaItemRequest> Items { get; set; } = new();
        }

        public class AddGachaItemRequest
        {
            [Required]
            public Guid ItemId { get; set; }
            [Range(0.001, 100.0)]
            public double DropRate { get; set; }
            [Range(1, 5)]
            public int StarRating { get; set; }
            [Required]
            public string ItemCategory { get; set; } = string.Empty;
            public bool IsFeatured { get; set; } = false;
        }

        public class UpdateGachaBannerRequest
        {
            [Required, MaxLength(100)]
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string BannerImagePath { get; set; } = string.Empty;
            public int CostPerSinglePull { get; set; }
            public int CostPerMultiPull { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        // ── RESPONSE ─────────────────────────────────────────────

        public class GachaPullResultDto
        {
            public Guid ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public string ItemCategory { get; set; } = string.Empty;  // "Character" | "Weapon"
            public string ImagePath { get; set; } = string.Empty;
            public int StarRating { get; set; }                        // 3, 4, 5
            public bool IsFeatured { get; set; }
            public bool WasPityTriggered { get; set; }
            public bool IsNew { get; set; }                            // chưa có trong inventory
            public int PullNumber { get; set; }                        // thứ tự trong batch
        }

        public class GachaPullResponseDto
        {
            public List<GachaPullResultDto> Results { get; set; } = new();
            public int GemsSpent { get; set; }
            public int RemainingGems { get; set; }
            public int CurrentPityCounter { get; set; }
            public bool HadGuaranteedPity { get; set; }  // batch này có kích hoạt pity không
        }

        public class GachaBannerDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string BannerImagePath { get; set; } = string.Empty;
            public int CostPerSinglePull { get; set; }
            public int CostPerMultiPull { get; set; }
            public int PityThreshold { get; set; }
            public int HardPityThreshold { get; set; }
            public bool IsActive { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<GachaBannerItemDto> FeaturedItems { get; set; } = new();
            public List<GachaBannerItemDto> AllItems { get; set; } = new();
        }

        public class GachaBannerItemDto
        {
            public Guid ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public string ItemCategory { get; set; } = string.Empty;
            public string ImagePath { get; set; } = string.Empty;
            public int StarRating { get; set; }
            public double DropRate { get; set; }
            public bool IsFeatured { get; set; }
        }

        public class GachaHistoryDto
        {
            public Guid Id { get; set; }
            public string BannerName { get; set; } = string.Empty;
            public string ItemName { get; set; } = string.Empty;
            public string ItemImagePath { get; set; } = string.Empty;
            public string ItemCategory { get; set; } = string.Empty;
            public int StarRating { get; set; }
            public bool IsFeatured { get; set; }
            public bool WasPityTriggered { get; set; }
            public string PullType { get; set; } = string.Empty;
            public int GemsCost { get; set; }
            public DateTime PulledAt { get; set; }
        }

        public class UserGachaStatusDto
        {
            public int CurrentGems { get; set; }
            public int PityCounter { get; set; }
            public int PullsUntilGuaranteed4Star { get; set; }
            public int PullsUntilGuaranteed5Star { get; set; }
        }
    }

