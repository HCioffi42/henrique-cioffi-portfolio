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
        // Verifica se o arquivo existe e tem conteúdo para evitar processar requisições vazias.
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Tentativa de upload com arquivo nulo ou vazio.");
            return BadRequest("No file uploaded.");
        }

        // Valida o tamanho do arquivo para proteger o servidor contra abusos de armazenamento.
        if (file.Length > MaxFileSize)
        {
            _logger.LogWarning("Arquivo excede o limite de 5MB: {FileName} ({Size} bytes)", file.FileName, file.Length);
            return BadRequest("File size exceeds the limit of 5MB.");
        }

        // Valida a extensão para garantir que apenas formatos de imagem suportados sejam salvos.
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Extensão de arquivo não permitida: {Extension}", extension);
            return BadRequest("Invalid file type. Only images are allowed.");
        }

        try
        {
            _logger.LogInformation("Iniciando upload do arquivo: {FileName}", file.FileName);

            // Abre o stream de leitura do arquivo enviado pelo cliente.
            await using var stream = file.OpenReadStream();
            
            // Persiste o arquivo através do serviço de armazenamento (Local ou Cloud).
            var url = await _storageService.SaveFileAsync(stream, file.FileName, file.ContentType);

            _logger.LogInformation("Upload concluído com sucesso. URL gerada: {Url}", url);
            
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            // Registra a exceção completa no Log para facilitar a depuração via IDE ou arquivos de log.
            _logger.LogError(ex, "Falha crítica ao processar upload do arquivo {FileName}", file.FileName);
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
