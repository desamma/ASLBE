using BussinessObjects.DTOs.BugReport;

namespace Services.IServices
{
    public interface IBugReportService
    {
        Task<ServiceResult<Guid>> SubmitBugReportAsync(Guid userId, CreateBugReportRequest request);
        Task<ServiceResult<BugReportDto>> GetBugReportAsync(Guid bugReportId, Guid userId, bool isAdmin);
        Task<ServiceResult<List<BugReportDto>>> GetAllBugReportsAsync();
        Task<ServiceResult<bool>> UpdateBugReportStatusAsync(Guid bugReportId, UpdateBugReportStatusRequest request);
        Task<ServiceResult<bool>> DeleteBugReportAsync(Guid bugReportId, Guid userId, bool isAdmin);
    }
}
