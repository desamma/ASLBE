using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Services.IServices;

namespace BE.Controllers;

/// <summary>
/// Example controller demonstrating Firebase Storage usage
/// </summary>
[ApiController]
[Authorize]
[EnableRateLimiting("PerUserFirebasePolicy")]
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
    public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
    {
        var file = request.File;
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var fileUrl = await _firebaseStorageService.UploadFileAsync(
                stream,
                file.FileName,
                request.FolderPath);

            return Ok(new { url = fileUrl, fileName = file.FileName });
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

    /// <summary>
    /// Upload multiple files to Firebase Storage
    /// </summary>
    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadMultipleFiles([FromForm] UploadMultipleFilesRequest request)
    {
        var files = request.Files;
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files provided");
        }

        var uploadedFiles = new List<object>();
        var failedFiles = new List<object>();

        try
        {
            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                {
                    failedFiles.Add(new { fileName = file?.FileName, error = "File is empty" });
                    continue;
                }

                try
                {
                    using var stream = file.OpenReadStream();
                    var fileUrl = await _firebaseStorageService.UploadFileAsync(
                        stream,
                        file.FileName,
                        request.FolderPath);

                    uploadedFiles.Add(new { url = fileUrl, fileName = file.FileName });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                    failedFiles.Add(new { fileName = file.FileName, error = ex.Message });
                }
            }

            return Ok(new
            {
                uploadedCount = uploadedFiles.Count,
                failedCount = failedFiles.Count,
                uploadedFiles,
                failedFiles
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch upload to Firebase Storage");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete multiple files from Firebase Storage
    /// </summary>
    [HttpDelete("delete-multiple")]
    public async Task<IActionResult> DeleteMultipleFiles([FromBody] DeleteMultipleFilesRequest request)
    {
        if (request?.FilePaths == null || request.FilePaths.Count == 0)
        {
            return BadRequest("No file paths provided");
        }

        var deletedFiles = new List<string>();
        var failedFiles = new List<object>();

        try
        {
            foreach (var filePath in request.FilePaths)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    failedFiles.Add(new { filePath, error = "File path is empty" });
                    continue;
                }

                try
                {
                    var deleted = await _firebaseStorageService.DeleteFileAsync(filePath);
                    if (deleted)
                    {
                        deletedFiles.Add(filePath);
                    }
                    else
                    {
                        failedFiles.Add(new { filePath, error = "File not found" });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
                    failedFiles.Add(new { filePath, error = ex.Message });
                }
            }

            return Ok(new
            {
                deletedCount = deletedFiles.Count,
                failedCount = failedFiles.Count,
                deletedFiles,
                failedFiles
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch delete from Firebase Storage");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
/// <summary>
/// Request model for deleting multiple files
/// </summary>
public class DeleteMultipleFilesRequest
{
    public List<string> FilePaths { get; set; } = new();
}

/// <summary>
/// Request model for uploading a single file
/// </summary>
public class UploadFileRequest
{
    public IFormFile File { get; set; } = default!;
    public string? FolderPath { get; set; }
}

/// <summary>
/// Request model for uploading multiple files
/// </summary>
public class UploadMultipleFilesRequest
{
    public IFormFileCollection Files { get; set; } = default!;
    public string? FolderPath { get; set; }
}