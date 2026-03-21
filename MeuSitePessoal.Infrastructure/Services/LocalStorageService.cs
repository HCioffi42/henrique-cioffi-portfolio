using MeuSitePessoal.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MeuSitePessoal.Infrastructure.Services;

/// <summary>
/// Service implementation for saving files to the local file system.
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly string _storagePath;
    private readonly string _webRootPath;

    public LocalStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var relativePath = configuration["StorageSettings:LocalStoragePath"] ?? "wwwroot/uploads";
        
        // IWebHostEnvironment.WebRootPath typically points to wwwroot
        _webRootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        
        // Ensure we are pointing to the correct uploads folder
        _storagePath = Path.Combine(environment.ContentRootPath, relativePath);

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    /// <summary>
    /// Saves a file to the local uploads directory.
    /// </summary>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="fileName">The original name of the file.</param>
    /// <param name="contentType">The content type (MIME type) of the file.</param>
    /// <returns>The relative URL of the saved file.</returns>
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(_storagePath, uniqueFileName);

        using (var targetStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(targetStream);
        }

        // Return the relative URL (e.g., /uploads/filename.jpg)
        // We assume the static files are served from wwwroot
        return $"/uploads/{uniqueFileName}";
    }

    /// <summary>
    /// Deletes a file from the local uploads directory.
    /// </summary>
    /// <param name="fileUrl">The relative URL of the file (e.g., /uploads/filename.jpg).</param>
    public Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return Task.CompletedTask;

        // Extract the filename from the URL
        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_storagePath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
