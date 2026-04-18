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
        // Xem lịch sử gacha (nếu truyền userId thì xem của 1 người, không truyền thì xem toàn hệ thống)
        Task<ServiceResult<List<AdminGachaHistoryDto>>> GetGachaHistoryAsync(Guid? userId = null);

        // Xem danh sách các vật phẩm hiện có trong game
        Task<ServiceResult<List<AdminItemDto>>> GetAllItemsAsync();

        // Thêm vật phẩm mới
        Task<ServiceResult<AdminItemDto>> CreateItemAsync(CreateUpdateItemDto dto);

        // Cập nhật vật phẩm hiện có
        Task<ServiceResult<AdminItemDto>> UpdateItemAsync(Guid itemId, CreateUpdateItemDto dto);
    }
}
