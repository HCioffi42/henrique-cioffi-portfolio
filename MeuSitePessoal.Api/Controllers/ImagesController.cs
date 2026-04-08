using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuSitePessoal.Api.Controllers;

/// <summary>
/// Controller for handling image uploads and media operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly ILogger<ImagesController> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public ImagesController(IStorageService storageService, ILogger<ImagesController> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Uploads an image file to the storage system with enhanced error handling and logging.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        // Checks if the file exists and has content to avoid processing empty requests.
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Upload attempt with null or empty file.");
            return BadRequest("No file uploaded.");
        }

        // Validates file size to protect the server from storage abuse.
        if (file.Length > MaxFileSize)
        {
            _logger.LogWarning("File exceeds the 5MB limit: {FileName} ({Size} bytes)", file.FileName, file.Length);
            return BadRequest("File size exceeds the limit of 5MB.");
        }

        // Validates the extension to ensure only supported image formats are saved.
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            _logger.LogWarning("File extension not allowed: {Extension}", extension);
            return BadRequest("Invalid file type. Only images are allowed.");
        }

        try
        {
            _logger.LogInformation("Starting upload for file: {FileName}", file.FileName);

            // Opens the read stream of the file sent by the client.
            await using var stream = file.OpenReadStream();
            
            // Persists the file via the storage service (Local or Cloud).
            var url = await _storageService.SaveFileAsync(stream, file.FileName, file.ContentType);

            _logger.LogInformation("Upload completed successfully. Generated URL: {Url}", url);
            
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            // Logs the full exception to ease debugging via the IDE or log files.
            _logger.LogError(ex, "Critical failure during upload of file {FileName}", file.FileName);
            return StatusCode(500, "An internal error occurred while saving the file.");
        }
    }

    /// <summary>
    /// Deletes an uploaded image from the storage system.
    /// </summary>
    /// <param name="url">The relative URL of the image to delete.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
            return BadRequest("URL is required.");

        await _storageService.DeleteFileAsync(url);
        return NoContent();
    }
}
