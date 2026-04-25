using BussinessObjects.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IAdminSettingService
    {
        Task<ServiceResult<List<ApiSettingDto>>> GetAllApiSettingsAsync();
        Task<ServiceResult<ApiSettingDto>> CreateApiSettingAsync(ApiSettingDto dto);
        Task<ServiceResult<bool>> DeleteApiSettingAsync(Guid id);
    }
}
