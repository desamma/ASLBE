namespace BussinessObjects.DTOs.ShopItem
{
    public class ShopItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; }
        public string? ItemType { get; set; }
        public string ImagePath { get; set; }
        public decimal? Price { get; set; }
        public decimal? CurrencyAmount { get; set; }
        public bool IsUsingPremiumCurrency { get; set; }
        public int? ItemQuantity { get; set; }
        public Guid? ItemId { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class CreateShopItemRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; }
        public string? ItemType { get; set; }
        public string ImagePath { get; set; }
        public decimal? Price { get; set; }
        public decimal? CurrencyAmount { get; set; }
        public bool IsUsingPremiumCurrency { get; set; } = false;
        public int? ItemQuantity { get; set; }
        public Guid? ItemId { get; set; }
        public bool IsFeatured { get; set; } = false;
    }

    public class UpdateShopItemRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; }
        public string? ItemType { get; set; }
        public string ImagePath { get; set; }
        public decimal? Price { get; set; }
        public decimal? CurrencyAmount { get; set; }
        public bool IsUsingPremiumCurrency { get; set; }
        public int? ItemQuantity { get; set; }
        public Guid? ItemId { get; set; }
        public bool IsFeatured { get; set; }
    }
}
