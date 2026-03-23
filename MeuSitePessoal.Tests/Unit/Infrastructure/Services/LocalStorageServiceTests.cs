using System.Text;
using MeuSitePessoal.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Unit tests for LocalStorageService, validating file saving and directory creation.
/// </summary>
public class LocalStorageServiceTests : IDisposable
{
    private readonly string _testTempPath;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public LocalStorageServiceTests()
    {
        _testTempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testTempPath);

        _configMock = new Mock<IConfiguration>();
        _envMock = new Mock<IWebHostEnvironment>();

        _envMock.Setup(e => e.ContentRootPath).Returns(_testTempPath);
        _configMock.Setup(c => c["StorageSettings:LocalStoragePath"]).Returns("uploads");
    }

    [Fact]
    public async Task SaveFileAsync_ShouldSaveFileAndReturnRelativeUrl()
    {
        // Arrange
        var service = new LocalStorageService(_configMock.Object, _envMock.Object);
        var content = "test file content";
        var fileName = "test.txt";
        var contentType = "text/plain";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var resultUrl = await service.SaveFileAsync(stream, fileName, contentType);

        // Assert
        Assert.StartsWith("/uploads/", resultUrl);
        
        var savedFileName = Path.GetFileName(resultUrl);
        var fullPath = Path.Combine(_testTempPath, "uploads", savedFileName);
        
        Assert.True(File.Exists(fullPath));
        var savedContent = await File.ReadAllTextAsync(fullPath);
        Assert.Equal(content, savedContent);
    }

    [Fact]
    public void Constructor_ShouldCreateDirectory_IfNotExists()
    {
        // Arrange
        var uploadsPath = Path.Combine(_testTempPath, "new_uploads");
        _configMock.Setup(c => c["StorageSettings:LocalStoragePath"]).Returns("new_uploads");

        // Act
        var service = new LocalStorageService(_configMock.Object, _envMock.Object);

        // Assert
        Assert.True(Directory.Exists(uploadsPath));
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldDeleteFileIfExists()
    {
        // Arrange
        var service = new LocalStorageService(_configMock.Object, _envMock.Object);
        var fileName = "to-delete.txt";
        var fileUrl = $"/uploads/{fileName}";
        var fullPath = Path.Combine(_testTempPath, "uploads", fileName);
        
        await File.WriteAllTextAsync(fullPath, "content");
        Assert.True(File.Exists(fullPath));

        // Act
        await service.DeleteFileAsync(fileUrl);

        // Assert
        Assert.False(File.Exists(fullPath));
    }

    [Fact]
    public async Task DeleteFileAsync_WhenFileDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var service = new LocalStorageService(_configMock.Object, _envMock.Object);
        var fileUrl = "/uploads/non-existent.txt";

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => service.DeleteFileAsync(fileUrl));
        Assert.Null(exception);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testTempPath))
        {
            Directory.Delete(_testTempPath, true);
        }
    }
}
