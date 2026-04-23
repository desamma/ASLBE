using BussinessObjects.DTOs.NPC;

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
}
