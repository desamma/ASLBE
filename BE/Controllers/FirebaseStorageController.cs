using Microsoft.AspNetCore.Mvc;
using Services.IServices;

namespace BE.Controllers;

/// <summary>
/// Example controller demonstrating Firebase Storage usage
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FirebaseStorageController : ControllerBase
{
    private readonly IFirebaseStorageService _firebaseStorageService;
    private readonly ILogger<FirebaseStorageController> _logger;

    public FirebaseStorageController(
        IFirebaseStorageService firebaseStorageService,
        ILogger<FirebaseStorageController> logger)
    {
        _firebaseStorageService = firebaseStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file to Firebase Storage
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(
        [FromForm] IFormFile file,
        [FromForm] string? folderPath = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided");
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var fileUrl = await _firebaseStorageService.UploadFileAsync(
                    stream,
                    file.FileName,
                    folderPath);

                return Ok(new { url = fileUrl, fileName = file.FileName });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Firebase Storage");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Download a file from Firebase Storage
    /// </summary>
    [HttpGet("download")]
    public async Task<IActionResult> DownloadFile([FromQuery] string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return BadRequest("File path is required");
        }

        try
        {
            var fileStream = await _firebaseStorageService.DownloadFileAsync(filePath);
            return File(fileStream, "application/octet-stream", Path.GetFileName(filePath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from Firebase Storage");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a file from Firebase Storage
    /// </summary>
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFile([FromQuery] string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return BadRequest("File path is required");
        }

        try
        {
            var deleted = await _firebaseStorageService.DeleteFileAsync(filePath);

            if (deleted)
            {
                return Ok(new { message = "File deleted successfully" });
            }
            else
            {
                return NotFound(new { message = "File not found" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Firebase Storage");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get the URL of a file in Firebase Storage
    /// </summary>
    [HttpGet("url")]
    public async Task<IActionResult> GetFileUrl([FromQuery] string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return BadRequest("File path is required");
        }

        try
        {
            var url = await _firebaseStorageService.GetFileUrlAsync(filePath);
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file URL from Firebase Storage");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// List all files in a folder
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> ListFiles([FromQuery] string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return BadRequest("Folder path is required");
        }

        try
        {
            var files = await _firebaseStorageService.ListFilesAsync(folderPath);
            return Ok(new { files, count = files.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files from Firebase Storage");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
