using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Admin
{
    public class AdminGachaHistoryDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }

        // Thông tin vật phẩm quay ra
        public string? ItemName { get; set; }
        public string? ItemRarity { get; set; }

        public DateTime Date { get; set; }
        public bool IsSuccess { get; set; }
        public decimal NewUserBalance { get; set; }
    }
}
