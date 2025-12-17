using Microsoft.Extensions.Configuration;

using System.Text.Json;

using UserProfile.TimeCafe.Domain.DTOs;

namespace UserProfile.TimeCafe.Infrastructure.Services;

public class SightenginePhotoModerationService : IPhotoModerationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SightenginePhotoModerationService> _logger;
    private readonly string _apiUser;
    private readonly string _apiSecret;
    private readonly string _apiUrl;
    private readonly string _models;

    public SightenginePhotoModerationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SightenginePhotoModerationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiUser = configuration["Sightengine:ApiUser"] ?? throw new ArgumentNullException("Sightengine:ApiUser");
        _apiSecret = configuration["Sightengine:ApiSecret"] ?? throw new ArgumentNullException("Sightengine:ApiSecret");
        _apiUrl = configuration["Sightengine:ApiUrl"] ?? "https://api.sightengine.com/1.0/check.json";
        _models = configuration["Sightengine:Models"] ?? "nudity-2.1,wad,gore,offensive";
    }

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
                return new ModerationResult(true, null, null); // По умолчанию пропускаем
            }

            var scores = new Dictionary<string, double>();
            var reasons = new List<string>();

            // Проверка на обнажённость
            if (result.Nudity?.Raw > 0.5 || result.Nudity?.Partial > 0.5)
            {
                scores["nudity"] = result.Nudity.Raw;
                reasons.Add("Обнаружено нежелательное содержимое");
            }

            // Проверка на оружие/алкоголь/наркотики
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

            // Проверка на насилие/gore
            if (result.Gore?.Prob > 0.5)
            {
                scores["gore"] = result.Gore.Prob;
                reasons.Add("Обнаружен жестокий контент");
            }

            // Проверка на оскорбительные жесты
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
            // В случае ошибки API разрешаем загрузку (fail-open), но можно изменить на fail-closed
            return new ModerationResult(true, null, null);
        }
    }

    private class SightengineResponse
    {
        public NudityInfo? Nudity { get; set; }
        public double Weapon { get; set; }
        public double Alcohol { get; set; }
        public double Drugs { get; set; }
        public GoreInfo? Gore { get; set; }
        public OffensiveInfo? Offensive { get; set; }
    }

    private class NudityInfo
    {
        public double Raw { get; set; }
        public double Partial { get; set; }
        public double Safe { get; set; }
    }

    private class GoreInfo
    {
        public double Prob { get; set; }
    }

    private class OffensiveInfo
    {
        public double Prob { get; set; }
    }
}
