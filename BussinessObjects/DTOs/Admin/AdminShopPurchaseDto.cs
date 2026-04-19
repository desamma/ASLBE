using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Admin
{
    public class AdminShopPurchaseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }

        public string? ShopItemName { get; set; } // Tên vật phẩm trong shop

        public int Quantity { get; set; }
        public string PaymentType { get; set; } = string.Empty; // RegularCurrency / PremiumCurrency
        public int AmountPaid { get; set; } // Số tiền ảo đã trả

        public DateTime PurchaseDate { get; set; }
    }
}
