using BussinessObjects.DTOs.BugReport;
using BussinessObjects.Models;
using DataAccess.IRepositories;
using Services.IServices;

namespace Services.Services
{
    public class BugReportService : IBugReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BugReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<Guid>> SubmitBugReportAsync(Guid userId, CreateBugReportRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Title))
                    return new ServiceResult<Guid>
                    {
                        Success = false,
                        Message = "Bug report title is required",
                        Errors = ["Title cannot be empty"]
                    };

                if (string.IsNullOrWhiteSpace(request.Description))
                    return new ServiceResult<Guid>
                    {
                        Success = false,
                        Message = "Bug report description is required",
                        Errors = ["Description cannot be empty"]
                    };

                // Verify user exists
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    return new ServiceResult<Guid>
                    {
                        Success = false,
                        Message = "User not found"
                    };

                var bugReport = new BugReport
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    Description = request.Description,
                    Steps = request.Steps,
                    ExpectedBehavior = request.ExpectedBehavior,
                    ActualBehavior = request.ActualBehavior,
                    Severity = request.Severity ?? "Medium",
                    Status = "Open",
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.BugReports.AddAsync(bugReport);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<Guid>
                {
                    Success = true,
                    Message = "Bug report submitted successfully",
                    Data = bugReport.Id
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<Guid>
                {
                    Success = false,
                    Message = "Error submitting bug report",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<BugReportDto>> GetBugReportAsync(Guid bugReportId, Guid userId, bool isAdmin)
        {
            try
            {
                if (bugReportId == Guid.Empty)
                    return new ServiceResult<BugReportDto>
                    {
                        Success = false,
                        Message = "Invalid bug report ID"
                    };

                var bugReport = await _unitOfWork.BugReports.FirstOrDefaultAsync(br => br.Id == bugReportId);
                if (bugReport == null)
                    return new ServiceResult<BugReportDto>
                    {
                        Success = false,
                        Message = "Bug report not found"
                    };

                // Users can only view their own reports, admins can view all
                if (bugReport.UserId != userId && !isAdmin)
                    return new ServiceResult<BugReportDto>
                    {
                        Success = false,
                        Message = "Unauthorized access"
                    };

                // Get user info
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == bugReport.UserId);

                var dto = new BugReportDto
                {
                    Id = bugReport.Id,
                    Title = bugReport.Title,
                    Description = bugReport.Description,
                    Steps = bugReport.Steps,
                    ExpectedBehavior = bugReport.ExpectedBehavior,
                    ActualBehavior = bugReport.ActualBehavior,
                    Severity = bugReport.Severity,
                    Status = bugReport.Status,
                    UserId = bugReport.UserId,
                    UserName = user?.UserName,
                    CreatedDate = bugReport.CreatedDate,
                    UpdatedDate = bugReport.UpdatedDate,
                    AdminNotes = bugReport.AdminNotes
                };

                return new ServiceResult<BugReportDto>
                {
                    Success = true,
                    Message = "Bug report retrieved successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<BugReportDto>
                {
                    Success = false,
                    Message = "Error retrieving bug report",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<List<BugReportDto>>> GetUserBugReportsAsync(Guid userId)
        {
            try
            {
                var bugReports = _unitOfWork.BugReports
                    .GetQueryable(asNoTracking: true)
                    .Where(br => br.UserId == userId)
                    .OrderByDescending(br => br.CreatedDate)
                    .ToList();

                if (!bugReports.Any())
                    return new ServiceResult<List<BugReportDto>>
                    {
                        Success = true,
                        Message = "No bug reports found",
                        Data = new List<BugReportDto>()
                    };

                // Get all users in one query for efficiency
                var userIds = bugReports.Select(br => br.UserId).Distinct().ToList();
                var users = _unitOfWork.Users
                    .GetQueryable(asNoTracking: true)
                    .Where(u => userIds.Contains(u.Id))
                    .ToList();

                var dtoList = bugReports.Select(br =>
                {
                    var user = users.FirstOrDefault(u => u.Id == br.UserId);
                    return new BugReportDto
                    {
                        Id = br.Id,
                        Title = br.Title,
                        Description = br.Description,
                        Steps = br.Steps,
                        ExpectedBehavior = br.ExpectedBehavior,
                        ActualBehavior = br.ActualBehavior,
                        Severity = br.Severity,
                        Status = br.Status,
                        UserId = br.UserId,
                        UserName = user?.UserName,
                        CreatedDate = br.CreatedDate,
                        UpdatedDate = br.UpdatedDate,
                        AdminNotes = br.AdminNotes
                    };
                }).ToList();

                return new ServiceResult<List<BugReportDto>>
                {
                    Success = true,
                    Message = "User bug reports retrieved successfully",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<BugReportDto>>
                {
                    Success = false,
                    Message = "Error retrieving user bug reports",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<List<BugReportDto>>> GetAllBugReportsAsync()
        {
            try
            {
                var bugReports = _unitOfWork.BugReports
                    .GetQueryable(asNoTracking: true)
                    .OrderByDescending(br => br.CreatedDate)
                    .ToList();

                if (!bugReports.Any())
                    return new ServiceResult<List<BugReportDto>>
                    {
                        Success = true,
                        Message = "No bug reports found",
                        Data = new List<BugReportDto>()
                    };

                // Get all users in one query for efficiency
                var userIds = bugReports.Select(br => br.UserId).Distinct().ToList();
                var users = _unitOfWork.Users
                    .GetQueryable(asNoTracking: true)
                    .Where(u => userIds.Contains(u.Id))
                    .ToList();

                var dtoList = bugReports.Select(br =>
                {
                    var user = users.FirstOrDefault(u => u.Id == br.UserId);
                    return new BugReportDto
                    {
                        Id = br.Id,
                        Title = br.Title,
                        Description = br.Description,
                        Steps = br.Steps,
                        ExpectedBehavior = br.ExpectedBehavior,
                        ActualBehavior = br.ActualBehavior,
                        Severity = br.Severity,
                        Status = br.Status,
                        UserId = br.UserId,
                        UserName = user?.UserName,
                        CreatedDate = br.CreatedDate,
                        UpdatedDate = br.UpdatedDate,
                        AdminNotes = br.AdminNotes
                    };
                }).ToList();

                return new ServiceResult<List<BugReportDto>>
                {
                    Success = true,
                    Message = "All bug reports retrieved successfully",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<BugReportDto>>
                {
                    Success = false,
                    Message = "Error retrieving bug reports",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<bool>> UpdateBugReportStatusAsync(Guid bugReportId, UpdateBugReportStatusRequest request)
        {
            try
            {
                if (bugReportId == Guid.Empty)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Invalid bug report ID"
                    };

                if (string.IsNullOrWhiteSpace(request.Status))
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Status is required",
                        Errors = ["Status cannot be empty"]
                    };

                var bugReport = await _unitOfWork.BugReports.FirstOrDefaultAsync(br => br.Id == bugReportId);
                if (bugReport == null)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Bug report not found"
                    };

                bugReport.Status = request.Status;
                bugReport.AdminNotes = request.AdminNotes;
                bugReport.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.BugReports.UpdateAsync(bugReport);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "Bug report updated successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Error updating bug report",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<bool>> DeleteBugReportAsync(Guid bugReportId, Guid userId, bool isAdmin)
        {
            try
            {
                if (bugReportId == Guid.Empty)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Invalid bug report ID"
                    };

                var bugReport = await _unitOfWork.BugReports.FirstOrDefaultAsync(br => br.Id == bugReportId);
                if (bugReport == null)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Bug report not found"
                    };

                // Users can only delete their own reports, admins can delete any
                if (bugReport.UserId != userId && !isAdmin)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Unauthorized access"
                    };

                await _unitOfWork.BugReports.DeleteAsync(bugReport);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "Bug report deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Error deleting bug report",
                    Errors = [ex.Message]
                };
            }
        }
    }
}
