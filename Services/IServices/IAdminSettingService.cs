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
        Task<ServiceResult<ApiSettingDto>> GetApiSettingAsync();
        Task<ServiceResult<ApiSettingDto>> UpdateApiSettingAsync(ApiSettingDto dto);
    }
}
