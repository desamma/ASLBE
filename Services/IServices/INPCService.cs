using Services;

namespace Services.IServices
{
    public interface INPCService
    {
        ServiceResult<List<NPCDto>> GetAll();
        Task<ServiceResult<NPCDto>> GetByIdAsync(Guid id);
        Task<ServiceResult<NPCDto>> CreateAsync(CreateNPCRequest request);
        Task<ServiceResult<NPCDto>> UpdateAsync(Guid id, UpdateNPCRequest request);
        Task<ServiceResult<bool>> DeleteAsync(Guid id);
    }

    public class NPCDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string Location { get; set; }
        public string NPCType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class CreateNPCRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string Location { get; set; }
        public string NPCType { get; set; }
    }

    public class UpdateNPCRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string Location { get; set; }
        public string NPCType { get; set; }
    }
}
