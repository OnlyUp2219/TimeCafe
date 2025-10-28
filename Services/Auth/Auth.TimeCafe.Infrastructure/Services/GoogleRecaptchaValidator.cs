using System.Text.Json;
using Auth.TimeCafe.Domain.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auth.TimeCafe.Infrastructure.Services;

public class GoogleRecaptchaValidator : ICaptchaValidator
{
    private readonly HttpClient _httpClient;
    private readonly string _secretKey;
    private readonly ILogger<GoogleRecaptchaValidator> _logger;

    public GoogleRecaptchaValidator(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GoogleRecaptchaValidator> logger)
    {
        _httpClient = httpClient;
        _secretKey = configuration["GoogleRecaptcha:SecretKey"]
            ?? throw new InvalidOperationException("GoogleRecaptcha:SecretKey не настроен в конфигурации");
        _logger = logger;
    }

    public async Task<bool> ValidateAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Попытка валидации пустого токена reCAPTCHA");
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = _secretKey,
                    ["response"] = token
                })
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google reCAPTCHA API вернул статус {StatusCode}", response.StatusCode);
                return false;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RecaptchaResponse>(jsonResponse);

            if (result?.Success == true)
            {
                _logger.LogInformation("reCAPTCHA успешно пройдена");
                return true;
            }

            _logger.LogWarning("reCAPTCHA не пройдена. Ошибки: {Errors}",
                string.Join(", ", result?.ErrorCodes ?? Array.Empty<string>()));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при валидации reCAPTCHA");
            return false;
        }
    }

    private class RecaptchaResponse
    {
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}
