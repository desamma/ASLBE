using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects.DTOs.Shop
{
    public class ShopPurchaseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ShopItemId { get; set; }
        public string ShopItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
