namespace UserProfile.TimeCafe.Test.Unit.Services;

public class SightenginePhotoModerationServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<SightenginePhotoModerationService>> _loggerMock;
    private readonly SightenginePhotoModerationService _service;

    public SightenginePhotoModerationServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        
        var inMemorySettings = new Dictionary<string, string> {
            {"Sightengine:ApiUser", "test_user"},
            {"Sightengine:ApiSecret", "test_secret"},
            {"Sightengine:ApiUrl", "http://api.sightengine.com/1.0/check.json"},
            {"Sightengine:Models", "nudity,wad"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _loggerMock = new Mock<ILogger<SightenginePhotoModerationService>>();
        
        _service = new SightenginePhotoModerationService(_httpClient, _configuration, _loggerMock.Object);
    }

    [Fact]
    public async Task ModeratePhotoAsync_Should_ReturnSafe_WhenSightengineReturnsSafe()
    {
        var photoStream = new MemoryStream("fake image"u8.ToArray());
        var jsonResponse = "{\"status\":\"success\",\"nudity\":{\"raw\":0.1,\"partial\":0.1,\"safe\":0.8}}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var result = await _service.ModeratePhotoAsync(photoStream);

        result.IsSafe.Should().BeTrue();
        result.Reason.Should().BeNull();
    }

    [Fact]
    public async Task ModeratePhotoAsync_Should_ReturnUnsafe_WhenSightengineReturnsNudity()
    {
        var photoStream = new MemoryStream("fake image"u8.ToArray());
        var jsonResponse = "{\"status\":\"success\",\"nudity\":{\"raw\":0.9,\"partial\":0.1,\"safe\":0.0}}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var result = await _service.ModeratePhotoAsync(photoStream);

        result.IsSafe.Should().BeFalse();
        result.Reason.Should().Contain("Обнаружено нежелательное содержимое");
    }

    [Fact]
    public async Task ModeratePhotoAsync_Should_ReturnSafe_WhenExceptionOccurs()
    {
        var photoStream = new MemoryStream("fake image"u8.ToArray());

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        var result = await _service.ModeratePhotoAsync(photoStream);

        result.IsSafe.Should().BeTrue();
        result.Reason.Should().BeNull();
    }
}
