using BussinessObjects.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
