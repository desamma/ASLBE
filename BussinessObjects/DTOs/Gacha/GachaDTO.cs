using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BussinessObjects.DTOs.Gacha
{
    // ══════════════════════════════════════════════════════════════
    // REQUEST DTOs
    // ══════════════════════════════════════════════════════════════

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

    // ── Create Banner ─────────────────────────────────────────────

    public class CreateGachaBannerRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Upload file trực tiếp.
        /// Ưu tiên cao hơn <see cref="BannerImagePath"/>.
        /// Request phải dùng multipart/form-data.
        /// </summary>
        public IFormFile? ImageFile { get; set; }

        /// <summary>
        /// URL tuyệt đối (https://...) hoặc server path (/images/banners/...).
        /// Chỉ dùng khi <see cref="ImageFile"/> là null.
        /// </summary>
        public string? BannerImagePath { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CostPerSinglePull must be at least 1")]
        public int CostPerSinglePull { get; set; } = 100;

        [Range(1, int.MaxValue, ErrorMessage = "CostPerMultiPull must be at least 1")]
        public int CostPerMultiPull { get; set; } = 1000;

        [Range(1, 100, ErrorMessage = "PityThreshold must be between 1 and 100")]
        public int PityThreshold { get; set; } = 10;

        [Range(1, 200, ErrorMessage = "HardPityThreshold must be between 1 and 200")]
        public int HardPityThreshold { get; set; } = 90;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Danh sách item (từ DB) được gắn vào banner.
        /// Tổng DropRate phải bằng 100%.
        /// Mọi ItemId phải tồn tại trong database.
        /// </summary>
        public List<AddGachaItemRequest> Items { get; set; } = [];
    }

    // ── Update Banner ─────────────────────────────────────────────

    public class UpdateGachaBannerRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Upload file mới để thay ảnh cũ.
        /// Ưu tiên cao hơn <see cref="BannerImagePath"/>.
        /// </summary>
        public IFormFile? ImageFile { get; set; }

        /// <summary>
        /// URL hoặc server path mới.
        /// Nếu cả hai null/empty thì giữ nguyên ảnh cũ của banner.
        /// </summary>
        public string? BannerImagePath { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CostPerSinglePull must be at least 1")]
        public int CostPerSinglePull { get; set; } = 100;

        [Range(1, int.MaxValue, ErrorMessage = "CostPerMultiPull must be at least 1")]
        public int CostPerMultiPull { get; set; } = 1000;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // ── Add / Remove Item ─────────────────────────────────────────

    public class AddGachaItemRequest
    {
        [Required]
        public Guid ItemId { get; set; }

        [Range(0.001, 100.0, ErrorMessage = "DropRate must be between 0.001 and 100")]
        public double DropRate { get; set; }

        [Range(1, 5, ErrorMessage = "StarRating must be between 1 and 5")]
        public int StarRating { get; set; }

        [Required, MaxLength(50)]
        public string ItemCategory { get; set; } = string.Empty;    // "Character" | "Weapon"

        public bool IsFeatured { get; set; } = false;
    }

    // ══════════════════════════════════════════════════════════════
    // RESPONSE DTOs — Pull
    // ══════════════════════════════════════════════════════════════

    public class GachaPullResultDto
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCategory { get; set; } = string.Empty;    // "Character" | "Weapon"
        public string ImagePath { get; set; } = string.Empty;
        public int StarRating { get; set; }                         // 3 | 4 | 5
        public bool IsFeatured { get; set; }
        public bool WasPityTriggered { get; set; }
        public bool IsNew { get; set; }                             // chưa có trong inventory
        public int PullNumber { get; set; }                         // thứ tự trong batch
    }

    public class GachaPullResponseDto
    {
        public List<GachaPullResultDto> Results { get; set; } = [];
        public int GemsSpent { get; set; }
        public int RemainingGems { get; set; }
        public int CurrentPityCounter { get; set; }
        public bool HadGuaranteedPity { get; set; }                 // batch này có kích hoạt pity không
    }

    // ══════════════════════════════════════════════════════════════
    // RESPONSE DTOs — Banner
    // ══════════════════════════════════════════════════════════════

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
        public List<GachaBannerItemDto> FeaturedItems { get; set; } = [];
        public List<GachaBannerItemDto> AllItems { get; set; } = [];
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

    // ══════════════════════════════════════════════════════════════
    // RESPONSE DTOs — History & Status
    // ══════════════════════════════════════════════════════════════

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
        public string PullType { get; set; } = string.Empty;       // "SinglePull" | "MultiPull"
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

    // ══════════════════════════════════════════════════════════════
    // RESPONSE DTOs — Admin Utilities
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Dùng cho GET /api/admin/gacha/items-available.
    /// Cung cấp danh sách Item từ DB để admin chọn khi tạo/sửa banner.
    /// </summary>
    public class AvailableItemDto
    {
        public Guid ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;           // "Character" | "Weapon"
    }
}