using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Services.IServices;

namespace Services.Services;

/// <summary>
/// Service for handling Firebase Storage operations using Google Cloud Storage
/// </summary>
public class FirebaseStorageService : IFirebaseStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    private readonly IConfiguration _configuration;

    public FirebaseStorageService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        // Get bucket name from configuration
        _bucketName = _configuration["Firebase:StorageBucket"] 
            ?? throw new InvalidOperationException("Firebase:StorageBucket configuration is missing");

        // Initialize Google Cloud Storage client
        // Note: Ensure GOOGLE_APPLICATION_CREDENTIALS environment variable is set
        // or credentials file path is configured
        _storageClient = StorageClient.Create();
    }

    /// <summary>
    /// Uploads a file to Firebase Storage
    /// </summary>
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string? folderPath = null)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));
        
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        try
        {
            // Build the full path including folder if provided
            var fullPath = string.IsNullOrEmpty(folderPath) 
                ? fileName 
                : $"{folderPath.TrimEnd('/')}/{fileName}";

            // Upload the file
            var gcsObject = await _storageClient.UploadObjectAsync(
                _bucketName,
                fullPath,
                "application/octet-stream",
                fileStream);

            // Return the public URL
            return GetPublicUrl(gcsObject.Name);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to upload file '{fileName}' to Firebase Storage", ex);
        }
    }

    /// <summary>
    /// Downloads a file from Firebase Storage
    /// </summary>
    public async Task<Stream> DownloadFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        try
        {
            var memoryStream = new MemoryStream();
            await _storageClient.DownloadObjectAsync(_bucketName, filePath, memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to download file '{filePath}' from Firebase Storage", ex);
        }
    }

    /// <summary>
    /// Deletes a file from Firebase Storage
    /// </summary>
    public async Task<bool> DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        try
        {
            await _storageClient.DeleteObjectAsync(_bucketName, filePath);
            return true;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete file '{filePath}' from Firebase Storage", ex);
        }
    }

    /// <summary>
    /// Gets the URL of a file in Firebase Storage
    /// </summary>
    public async Task<string> GetFileUrlAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        try
        {
            // Check if file exists
            var gcsObject = await _storageClient.GetObjectAsync(_bucketName, filePath);
            return GetPublicUrl(gcsObject.Name);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"File '{filePath}' not found in Firebase Storage");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get URL for file '{filePath}'", ex);
        }
    }

    /// <summary>
    /// Lists all files in a specific folder
    /// </summary>
    public async Task<List<string>> ListFilesAsync(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Folder path cannot be empty", nameof(folderPath));

        try
        {
            var files = new List<string>();
            var folderPrefix = folderPath.TrimEnd('/') + "/";

            await foreach (var gcsObject in _storageClient.ListObjectsAsync(_bucketName, folderPrefix))
            {
                files.Add(gcsObject.Name);
            }

            return files;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to list files in folder '{folderPath}'", ex);
        }
    }

    /// <summary>
    /// Generates the public URL for a file in Firebase Storage
    /// </summary>
    private string GetPublicUrl(string objectName)
    {
        return $"https://storage.googleapis.com/{_bucketName}/{Uri.EscapeDataString(objectName)}";
    }
}
