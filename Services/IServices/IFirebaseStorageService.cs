namespace Services.IServices;

/// <summary>
/// Interface for Firebase Storage operations
/// </summary>
public interface IFirebaseStorageService
{
    /// <summary>
    /// Uploads a file to Firebase Storage
    /// </summary>
    /// <param name="fileStream">The file stream to upload</param>
    /// <param name="fileName">The name of the file</param>
    /// <param name="folderPath">Optional folder path within the bucket</param>
    /// <returns>The URL of the uploaded file</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string? folderPath = null);

    /// <summary>
    /// Downloads a file from Firebase Storage
    /// </summary>
    /// <param name="filePath">The path of the file in storage</param>
    /// <returns>The file stream</returns>
    Task<Stream> DownloadFileAsync(string filePath);

    /// <summary>
    /// Deletes a file from Firebase Storage
    /// </summary>
    /// <param name="filePath">The path of the file to delete</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteFileAsync(string filePath);

    /// <summary>
    /// Gets the URL of a file in Firebase Storage
    /// </summary>
    /// <param name="filePath">The path of the file</param>
    /// <returns>The file URL</returns>
    Task<string> GetFileUrlAsync(string filePath);

    /// <summary>
    /// Lists all files in a specific folder
    /// </summary>
    /// <param name="folderPath">The folder path</param>
    /// <returns>List of file names</returns>
    Task<List<string>> ListFilesAsync(string folderPath);
}
