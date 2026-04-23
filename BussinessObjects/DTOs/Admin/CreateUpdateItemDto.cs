using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.DTOs.Admin
{
    public class CreateUpdateItemDto
    {
        [Required(ErrorMessage = "Item name is required")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Rarity { get; set; } = string.Empty;

        // Bỏ Required để tránh lỗi Validation khi người dùng chọn Upload File
        public string? ImagePath { get; set; }

        // Property mới để nhận file ảnh từ giao diện (FE)
        public IFormFile? ImageFile { get; set; }

        public List<string> StatsLines { get; set; } = new List<string>();
    }
}