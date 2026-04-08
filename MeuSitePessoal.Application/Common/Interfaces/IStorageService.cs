namespace MeuSitePessoal.Application.Common.Interfaces;

/// <summary>
/// Interface for storage operations.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Saves a file to the storage system.
    /// </summary>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="fileName">The original name of the file.</param>
    /// <param name="contentType">The content type (MIME type) of the file.</param>
    /// <returns>The URL or path of the saved file.</returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType);

    /// <summary>
    /// Deletes a file from the storage system.
    /// </summary>
    /// <param name="fileUrl">The relative URL or path of the file to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFileAsync(string fileUrl);
}
