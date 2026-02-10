namespace UserProfile.TimeCafe.Infrastructure.Services;

public class SightenginePhotoModerationService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<SightenginePhotoModerationService> logger) : IPhotoModerationService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<SightenginePhotoModerationService> _logger = logger;
    private readonly string _apiUser = configuration["Sightengine:ApiUser"] ?? throw new InvalidOperationException("Configuration value 'Sightengine:ApiUser' is missing.");
    private readonly string _apiSecret = configuration["Sightengine:ApiSecret"] ?? throw new InvalidOperationException("Configuration value 'Sightengine:ApiSecret' is missing.");
    private readonly string _apiUrl = configuration["Sightengine:ApiUrl"] ?? throw new InvalidOperationException("Configuration value 'Sightengine:ApiUrl' is missing.");
    private readonly string _models = configuration["Sightengine:Models"] ?? throw new InvalidOperationException("Configuration value 'Sightengine:Models' is missing.");

    public async Task<ModerationResult> ModeratePhotoAsync(Stream photoStream, CancellationToken cancellationToken = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(photoStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            content.Add(streamContent, "media", "photo.jpg");
            content.Add(new StringContent(_models), "models");
            content.Add(new StringContent(_apiUser), "api_user");
            content.Add(new StringContent(_apiSecret), "api_secret");

            var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<SightengineResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                _logger.LogWarning("Не удалось десериализовать ответ от Sightengine");
                return new ModerationResult(true, null, null);
            }

            var scores = new Dictionary<string, double>();
            var reasons = new List<string>();


            if (result.Nudity?.Raw > 0.5 || result.Nudity?.Partial > 0.5)
            {
                scores["nudity"] = result.Nudity.Raw;
                reasons.Add("Обнаружено нежелательное содержимое");
            }


            if (result.Weapon > 0.5)
            {
                scores["weapon"] = result.Weapon;
                reasons.Add("Обнаружено оружие");
            }

            if (result.Alcohol > 0.5)
            {
                scores["alcohol"] = result.Alcohol;
                reasons.Add("Обнаружен алкоголь");
            }

            if (result.Drugs > 0.5)
            {
                scores["drugs"] = result.Drugs;
                reasons.Add("Обнаружены наркотики");
            }

            if (result.Gore?.Prob > 0.5)
            {
                scores["gore"] = result.Gore.Prob;
                reasons.Add("Обнаружен жестокий контент");
            }

            if (result.Offensive?.Prob > 0.5)
            {
                scores["offensive"] = result.Offensive.Prob;
                reasons.Add("Обнаружены оскорбительные жесты");
            }

            bool isSafe = reasons.Count == 0;
            string? reason = reasons.Count > 0 ? string.Join(", ", reasons) : null;

            _logger.LogInformation("Модерация фото: IsSafe={IsSafe}, Reason={Reason}", isSafe, reason);

            return new ModerationResult(isSafe, reason, scores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при модерации фото через Sightengine");
            return new ModerationResult(true, null, null);
        }
    }

    private sealed class SightengineResponse
    {
        public NudityInfo? Nudity { get; set; } = null;
        public double Weapon { get; set; } = 0;
        public double Alcohol { get; set; } = 0;
        public double Drugs { get; set; } = 0;
        public GoreInfo? Gore { get; set; } = null;
        public OffensiveInfo? Offensive { get; set; } = null;
    }

    private sealed class NudityInfo
    {
        public double Raw { get; set; } = 0;
        public double Partial { get; set; } = 0;
        public double Safe { get; set; } = 0;
    }

    private sealed class GoreInfo
    {
        public double Prob { get; set; } = 0;
    }

    private sealed class OffensiveInfo
    {
        public double Prob { get; set; } = 0;
    }
}
