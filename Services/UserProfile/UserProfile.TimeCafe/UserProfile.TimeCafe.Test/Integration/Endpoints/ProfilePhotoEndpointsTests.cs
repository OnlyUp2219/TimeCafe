using Amazon.S3;
using Microsoft.Extensions.Options;
using UserProfile.TimeCafe.Infrastructure.S3.Options;
using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class ProfilePhotoEndpointsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UploadPhoto_Should_Return201_WhenValidPhoto()
    {
        // Arrange
        await SeedProfileAsync("user123", "Иван", "Петров");

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3, 4, 5]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");

        // Act
        var response = await Client.PostAsync("/S3/image/user123", content);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("key", out var key).Should().BeTrue();
            key.GetString()!.Should().Contain("user123");
            json.TryGetProperty("url", out var url).Should().BeTrue();
            url.GetString()!.Should().NotBeNullOrWhiteSpace();
            json.TryGetProperty("contentType", out var ct).Should().BeTrue();
            ct.GetString()!.Should().Be("image/jpeg");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UploadPhoto_Should_Return201_WhenValidPhoto] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetPhoto_Should_ReturnFile_AfterUpload_ToRealS3()
    {
        // Arrange: создаём профиль и загружаем файл (реальный вызов к S3)
        await SeedProfileAsync("user123", "Иван", "Петров");

        using var content = new MultipartFormDataContent();
        var originalData = LoadTestImage();
        var fileContent = new ByteArrayContent(originalData);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "test.png");

        var uploadResponse = await Client.PostAsync("/S3/image/user123", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify S3 object exists via HEAD/metadata
        using var scope = Factory.Services.CreateScope();
        var s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        var s3Options = scope.ServiceProvider.GetRequiredService<IOptions<S3Options>>();
        var objectExists = await VerifyS3ObjectExistsAsync(s3Options.Value.BucketName, "profiles/user123/photo", s3Client);
        objectExists.Should().BeTrue("объект должен существовать в S3 после загрузки");

        // Act: запрашиваем обратно фото из S3 через API
        var getResponse = await Client.GetAsync("/S3/image/user123");

        // Assert: статус OK, корректный content-type и непустое тело
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.Content.Headers.ContentType!.MediaType.Should().Be("image/png");
        var returnedBytes = await getResponse.Content.ReadAsByteArrayAsync();
        returnedBytes.Should().NotBeNull();
        returnedBytes.Length.Should().BeGreaterThan(0);
        returnedBytes.Should().BeEquivalentTo(originalData, "содержимое должно совпадать с загруженным");
    }

    [Fact]
    public async Task Endpoint_UploadPhoto_Should_Return404_WhenProfileNotFound()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3, 4, 5]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");

        // Act
        var response = await Client.PostAsync("/S3/image/nonexistent", content);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ProfileNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UploadPhoto_Should_Return404_WhenProfileNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UploadPhoto_Should_Return400_WhenNoFileProvided()
    {
        // Arrange
        await SeedProfileAsync("user123", "Иван", "Петров");
        using var content = new MultipartFormDataContent();

        // Act
        var response = await Client.PostAsync("/S3/image/user123", content);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UploadPhoto_Should_Return400_WhenNoFileProvided] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("image/bmp")]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    public async Task Endpoint_UploadPhoto_Should_Return422_WhenInvalidContentType(string contentType)
    {
        // Arrange
        await SeedProfileAsync("user123", "Иван", "Петров");

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3, 4, 5]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", "test.file");

        // Act
        var response = await Client.PostAsync("/S3/image/user123", content);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UploadPhoto_Should_Return422_WhenInvalidContentType] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UploadPhoto_Should_Return422_WhenFileTooLarge()
    {
        // Arrange
        await SeedProfileAsync("user123", "Иван", "Петров");

        using var content = new MultipartFormDataContent();
        var largeFile = new byte[6 * 1024 * 1024]; // 6 MB
        var fileContent = new ByteArrayContent(largeFile);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "large.jpg");

        // Act
        var response = await Client.PostAsync("/S3/image/user123", content);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UploadPhoto_Should_Return422_WhenFileTooLarge] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetPhoto_Should_Return404_WhenPhotoNotFound()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/S3/image/user123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Endpoint_DeletePhoto_Should_Return204_WhenPhotoDeleted()
    {
        // Arrange
        await SeedProfileAsync("user123", "Иван", "Петров");

        // First upload a photo
        using var uploadContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3, 4, 5]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        uploadContent.Add(fileContent, "file", "test.jpg");
        await Client.PostAsync("/S3/image/user123", uploadContent);

        // Act - Delete the photo
        var response = await Client.DeleteAsync("/S3/image/user123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Endpoint_DeletePhoto_Should_Return404_WhenProfileNotFound()
    {
        // Act
        var response = await Client.DeleteAsync("/S3/image/nonexistent");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ProfileNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeletePhoto_Should_Return404_WhenProfileNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UploadPhoto_Should_UpdatePhotoUrl_InProfile()
    {
        // Arrange
        await SeedProfileAsync("user123", "Иван", "Петров");

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3, 4, 5]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");

        // Act
        var uploadResponse = await Client.PostAsync("/S3/image/user123", content);
        var jsonString = await uploadResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(jsonString).RootElement;
        var photoUrl = json.GetProperty("url").GetString();

        // Verify profile was updated
        var profileResponse = await Client.GetAsync("/profiles/user123");
        var profileJson = await profileResponse.Content.ReadAsStringAsync();
        var profileData = JsonDocument.Parse(profileJson).RootElement;

        // Assert
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        profileData.GetProperty("photoUrl").GetString().Should().Be(photoUrl);
    }

    [Fact]
    public async Task Endpoint_DeletePhoto_Should_ClearPhotoUrl_InProfile()
    {
        // Arrange
        await SeedProfileAsync("user123", "Иван", "Петров");

        // Upload photo first
        using var uploadContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3, 4, 5]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        uploadContent.Add(fileContent, "file", "test.jpg");
        await Client.PostAsync("/S3/image/user123", uploadContent);

        // Act - Delete photo
        var deleteResponse = await Client.DeleteAsync("/S3/image/user123");

        // Verify profile photoUrl is cleared
        var profileResponse = await Client.GetAsync("/profiles/user123");
        var profileJson = await profileResponse.Content.ReadAsStringAsync();
        var profileData = JsonDocument.Parse(profileJson).RootElement;

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        profileData.TryGetProperty("photoUrl", out var photoUrl).Should().BeTrue();
        (photoUrl.ValueKind == JsonValueKind.Null).Should().BeTrue();
    }

    [Theory]
    [InlineData("image/png", "jpg")]
    [InlineData("image/png", "png")]
    [InlineData("image/webp", "webp")]
    [InlineData("image/gif", "gif")]
    public async Task Endpoint_UploadPhoto_Should_AcceptAllAllowedFormats(string contentType, string extension)
    {
        // Arrange
        var userId = $"user_{extension}";
        await SeedProfileAsync(userId, "Test", "User");

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3, 4, 5]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", $"test.{extension}");

        // Act
        var response = await Client.PostAsync($"/S3/image/{userId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
