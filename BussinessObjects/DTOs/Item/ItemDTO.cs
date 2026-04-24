namespace BussinessObjects.DTOs.Item
{
    public class ItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public string ImagePath { get; set; }
        public bool IsGachaOnly { get; set; }
        public bool IsActive { get; set; }
        public List<string> StatsLines { get; set; } = new List<string>();
    }

    public class CreateItemRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public string ImagePath { get; set; }
        public bool IsGachaOnly { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public List<string> StatsLines { get; set; } = new List<string>();
    }

    public class UpdateItemRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public string ImagePath { get; set; }
        public bool IsGachaOnly { get; set; }
        public bool IsActive { get; set; }
        public List<string> StatsLines { get; set; } = new List<string>();
    }
}
