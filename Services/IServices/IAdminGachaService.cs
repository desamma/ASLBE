using BussinessObjects.DTOs.Admin;

namespace Services.IServices
{
    public interface IAdminGachaService
    {
        Task<ServiceResult<List<AdminGachaHistoryDto>>> GetGachaHistoryAsync(Guid? userId = null);

        Task<ServiceResult<List<AdminItemDto>>> GetAllItemsAsync();

        Task<ServiceResult<AdminItemDto>> CreateItemAsync(CreateUpdateItemDto dto);

        Task<ServiceResult<AdminItemDto>> UpdateItemAsync(Guid itemId, CreateUpdateItemDto dto);
    }
}
