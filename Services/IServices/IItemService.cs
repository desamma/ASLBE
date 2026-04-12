using Services;

namespace Services.IServices
{
    public interface IItemService
    {
        ServiceResult<List<ItemDto>> GetAll();
        Task<ServiceResult<ItemDto>> GetByIdAsync(Guid id);
        Task<ServiceResult<ItemDto>> CreateAsync(CreateItemRequest request);
        Task<ServiceResult<ItemDto>> UpdateAsync(Guid id, UpdateItemRequest request);
        Task<ServiceResult<bool>> DeleteAsync(Guid id);
    }

    public class ItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public string ImagePath { get; set; }
        public List<string> StatsLines { get; set; } = new List<string>();
    }

    public class CreateItemRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public string ImagePath { get; set; }
        public List<string> StatsLines { get; set; } = new List<string>();
    }

    public class UpdateItemRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public string ImagePath { get; set; }
        public List<string> StatsLines { get; set; } = new List<string>();
    }
}
