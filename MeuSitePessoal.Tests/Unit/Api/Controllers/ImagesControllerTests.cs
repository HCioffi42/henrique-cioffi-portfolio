using System.Text;
using MeuSitePessoal.Api.Controllers;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Api.Controllers;

/// <summary>
/// Unit tests for ImagesController, validating file upload validation and processing.
/// </summary>
public class ImagesControllerTests
{
    private readonly Mock<IStorageService> _storageMock;
    private readonly Mock<ILogger<ImagesController>> _loggerMock;
    private readonly ImagesController _controller;

    public ImagesControllerTests()
    {
        _storageMock = new Mock<IStorageService>();
        _loggerMock = new Mock<ILogger<ImagesController>>();
        _controller = new ImagesController(_storageMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Upload_ValidFile_ReturnsOkWithUrl()
    {
        // Arrange
        var content = "test image";
        var fileName = "test.png";
        var contentType = "image/png";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        var expectedUrl = "/uploads/test.png";
        _storageMock.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), fileName, contentType))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _controller.Upload(fileMock.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var urlProperty = okResult.Value!.GetType().GetProperty("url");
        var urlValue = urlProperty!.GetValue(okResult.Value);
        Assert.Equal(expectedUrl, urlValue);
    }

    [Fact]
    public async Task Upload_NoFile_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Upload(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No file uploaded.", badRequestResult.Value);
    }

    [Fact]
    public async Task Upload_OversizedFile_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB

        // Act
        var result = await _controller.Upload(fileMock.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File size exceeds the limit of 5MB.", badRequestResult.Value);
    }

    [Fact]
    public async Task Upload_InvalidExtension_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("test.exe");

        // Act
        var result = await _controller.Upload(fileMock.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid file type. Only images are allowed.", badRequestResult.Value);
    }

    [Fact]
    public async Task Delete_ValidUrl_ReturnsNoContent()
    {
        // Arrange
        var url = "/uploads/test.png";

        // Act
        var result = await _controller.Delete(url);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _storageMock.Verify(s => s.DeleteFileAsync(url), Times.Once);
    }

    [Fact]
    public async Task Delete_EmptyUrl_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Delete("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("URL is required.", badRequestResult.Value);
    }
}
