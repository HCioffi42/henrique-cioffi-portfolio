using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Images;

/// <summary>
/// Integration tests for the ImagesController, validating the upload workflow and security.
/// </summary>
public class ImagesIntegrationTests : BaseIntegrationTest
{
    private class UploadResponse
    {
        public string url { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Upload_WhenAuthenticated_ShouldReturnOkWithUrl()
    {
        // Arrange
        await AuthenticateAsync();
        
        var content = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }); // Mock PNG header
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        
        using var formData = new MultipartFormDataContent();
        formData.Add(content, "file", "test.png");

        // Act
        var response = await _client.PostAsync("/api/Images/upload", formData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        var uploadResult = JsonSerializer.Deserialize<UploadResponse>(json);
        
        Assert.NotNull(uploadResult);
        Assert.StartsWith("/uploads/", uploadResult.url);
    }

    [Fact]
    public async Task Upload_WhenAnonymous_ShouldReturn401Unauthorized()
    {
        // Arrange
        var content = new ByteArrayContent(new byte[] { 0x00 });
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        
        using var formData = new MultipartFormDataContent();
        formData.Add(content, "file", "test.png");

        // Act
        var response = await _client.PostAsync("/api/Images/upload", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenAuthenticated_ShouldReturnNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        
        // First upload a file to have a valid URL (though the test doesn't check disk, just the endpoint logic)
        var content = new ByteArrayContent(new byte[] { 0x00 });
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        using var formData = new MultipartFormDataContent();
        formData.Add(content, "file", "to-delete.png");
        var uploadResponse = await _client.PostAsync("/api/Images/upload", formData);
        var json = await uploadResponse.Content.ReadAsStringAsync();
        var uploadResult = JsonSerializer.Deserialize<UploadResponse>(json);

        // Act
        var response = await _client.DeleteAsync($"/api/Images?url={uploadResult!.url}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenAnonymous_ShouldReturn401Unauthorized()
    {
        // Act
        var response = await _client.DeleteAsync("/api/Images?url=/uploads/some-image.png");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
