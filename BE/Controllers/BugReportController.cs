using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BussinessObjects.DTOs.BugReport;
using Services.IServices;
using System.Security.Claims;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BugReportController : ControllerBase
    {
        private readonly IBugReportService _bugReportService;

        public BugReportController(IBugReportService bugReportService)
        {
            _bugReportService = bugReportService;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitBugReport([FromBody] CreateBugReportRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "User not authenticated" });

            var result = await _bugReportService.SubmitBugReportAsync(userId, model);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message, bugReportId = result.Data });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBugReport(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim?.Value, out var userId))
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            var result = await _bugReportService.GetBugReportAsync(id, userId, isAdmin);

            if (!result.Success)
            {
                if (result.Message == "Unauthorized access")
                    return Forbid();

                if (result.Message == "Bug report not found")
                    return NotFound(new { message = result.Message });

                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return Ok(result.Data);
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllBugReports()
        {
            var result = await _bugReportService.GetAllBugReportsAsync();

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(result.Data);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateBugReportStatus(Guid id, [FromBody] UpdateBugReportStatusRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _bugReportService.UpdateBugReportStatusAsync(id, model);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBugReport(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim?.Value, out var userId))
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            var result = await _bugReportService.DeleteBugReportAsync(id, userId, isAdmin);

            if (!result.Success)
            {
                if (result.Message == "Unauthorized access")
                    return Forbid();

                if (result.Message == "Bug report not found")
                    return NotFound(new { message = result.Message });

                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return Ok(new { message = result.Message });
        }
    }
}
