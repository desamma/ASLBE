using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Admin
{
    public class CreateUpdateItemDto
    {
        [Required(ErrorMessage = "Tên vật phẩm không được để trống")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Rarity { get; set; } = string.Empty;

        // Tạm thời nhận đường dẫn ảnh dạng text (Bạn có thể dùng CloudinaryUpload sau nếu cần)
        public string ImagePath { get; set; } = string.Empty;

        public List<string> StatsLines { get; set; } = new List<string>();
    }
}
