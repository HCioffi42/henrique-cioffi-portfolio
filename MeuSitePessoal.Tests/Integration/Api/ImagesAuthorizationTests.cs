using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Api;

/// <summary>
/// Covers the authorization contract on media management. Upload and delete are administrative
/// operations; an authenticated reader holds no more access to them than an anonymous caller.
/// Follows the precedent in ArticleAuthorizationTests, which asserts the same shape for articles.
/// </summary>
public class ImagesAuthorizationTests : BaseIntegrationTest
{
    private const string ReaderName = "images_reader";
    private const string ReaderEmail = "images_reader@test.com";
    private const string ReaderPassword = "Password123!";

    private async Task AuthenticateAsReaderAsync()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            UserName = ReaderName,
            Email = ReaderEmail,
            Password = ReaderPassword
        });

        await ConfirmUserEmailAsync(ReaderName);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = ReaderName,
            Password = ReaderPassword
        });

        var result = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result?.Token);
    }

    private record AuthResult(string Token, string Username);

    /// <summary>Builds a minimal upload payload carrying an allowed extension.</summary>
    private static MultipartFormDataContent BuildImagePayload()
    {
        // A PNG signature is enough: the controller validates extension and size, not content.
        var content = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        var formData = new MultipartFormDataContent();
        formData.Add(content, "file", "authorization-probe.png");
        return formData;
    }

    private async Task<HttpResponseMessage> UploadAsync()
    {
        // Awaited inside the scope: returning the task would dispose the content mid-flight.
        using var formData = BuildImagePayload();
        return await _client.PostAsync("/api/Images/upload", formData);
    }

    private Task<HttpResponseMessage> DeleteAsync(string url = "/uploads/authorization-probe.png")
        => _client.DeleteAsync($"/api/Images?url={Uri.EscapeDataString(url)}");

    [Fact]
    public async Task Upload_AsReader_ReturnsForbidden()
    {
        await AuthenticateAsReaderAsync();

        var response = await UploadAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsReader_ReturnsForbidden()
    {
        await AuthenticateAsReaderAsync();

        var response = await DeleteAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Upload_AsAnonymous_ReturnsUnauthorized()
    {
        var response = await UploadAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsAnonymous_ReturnsUnauthorized()
    {
        var response = await DeleteAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Upload_AsAdministrator_Succeeds()
    {
        await AuthenticateAsync();

        var response = await UploadAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_AsAdministrator_Succeeds()
    {
        await AuthenticateAsync();

        var response = await DeleteAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
