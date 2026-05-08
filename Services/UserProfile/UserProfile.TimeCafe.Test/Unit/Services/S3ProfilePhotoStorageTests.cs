namespace UserProfile.TimeCafe.Test.Unit.Services;

public class S3ProfilePhotoStorageTests
{
    private readonly Mock<IAmazonS3> _s3Mock;
    private readonly S3Options _s3Options;
    private readonly S3ProfilePhotoStorage _storage;

    public S3ProfilePhotoStorageTests()
    {
        _s3Mock = new Mock<IAmazonS3>();
        _s3Options = new S3Options { BucketName = "test-bucket" };
        _storage = new S3ProfilePhotoStorage(_s3Mock.Object, _s3Options);
    }

    [Fact]
    public async Task UploadAsync_Should_ReturnSuccess_WhenS3ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var data = new MemoryStream("fake data"u8.ToArray());
        var contentType = "image/jpeg";
        var fileName = "photo.jpg";

        _s3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });

        var result = await _storage.UploadAsync(userId, data, contentType, fileName);

        result.Success.Should().BeTrue();
        result.Key.Should().Be($"profiles/{userId}/photo");
        _s3Mock.Verify(x => x.PutObjectAsync(It.Is<PutObjectRequest>(r => 
            r.BucketName == "test-bucket" && 
            r.Key == $"profiles/{userId}/photo" &&
            r.ContentType == contentType), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadAsync_Should_ReturnFailed_WhenS3ReturnsNotOk()
    {
        var userId = Guid.NewGuid();
        var data = new MemoryStream("fake data"u8.ToArray());
        var contentType = "image/jpeg";
        var fileName = "photo.jpg";

        _s3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.BadRequest });

        var result = await _storage.UploadAsync(userId, data, contentType, fileName);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_Should_ReturnStream_WhenFound()
    {
        var userId = Guid.NewGuid();
        var responseStream = new MemoryStream("fake image"u8.ToArray());
        var getResponse = new GetObjectResponse
        {
            HttpStatusCode = HttpStatusCode.OK,
            ResponseStream = responseStream
        };
        getResponse.Headers["Content-Type"] = "image/jpeg";

        _s3Mock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(getResponse);

        var result = await _storage.GetAsync(userId);

        result.Should().NotBeNull();
        result!.Stream.Should().BeSameAs(responseStream);
        result.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNull_WhenNotFound()
    {
        var userId = Guid.NewGuid();
        _s3Mock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("Not Found") { StatusCode = HttpStatusCode.NotFound });

        var result = await _storage.GetAsync(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnTrue_WhenDeleted()
    {
        var userId = Guid.NewGuid();
        _s3Mock.Setup(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteObjectResponse { HttpStatusCode = HttpStatusCode.NoContent });

        var result = await _storage.DeleteAsync(userId);

        result.Should().BeTrue();
    }
}
