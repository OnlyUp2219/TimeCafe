namespace Auth.TimeCafe.Infrastructure.Services;

public class GoogleRecaptchaValidator(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<GoogleRecaptchaValidator> logger) : ICaptchaValidator
{
    private const string VerifyEndpoint = "https://www.google.com/recaptcha/api/siteverify";

    private readonly HttpClient _httpClient = httpClient;
    private readonly string _secretKey = configuration["GoogleRecaptcha:SecretKey"]
            ?? throw new InvalidOperationException("GoogleRecaptcha:SecretKey не настроен в конфигурации");
    private readonly ILogger<GoogleRecaptchaValidator> _logger = logger;

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
                VerifyEndpoint,
                new FormUrlEncodedContent([
                    new KeyValuePair<string, string>("secret", _secretKey),
                    new KeyValuePair<string, string>("response", token)
                ])
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google reCAPTCHA API вернул статус {StatusCode}", response.StatusCode);
                return false;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            using var jsonDocument = JsonDocument.Parse(jsonResponse);
            var root = jsonDocument.RootElement;
            var isSuccess = root.TryGetProperty("success", out var successElement) && successElement.GetBoolean();

            if (isSuccess)
            {
                _logger.LogInformation("reCAPTCHA успешно пройдена");
                return true;
            }

            string[] errorCodes = [];
            if (root.TryGetProperty("error-codes", out var errorCodesElement) && errorCodesElement.ValueKind == JsonValueKind.Array)
            {
                errorCodes = [.. errorCodesElement
                    .EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString() ?? string.Empty)
                    .Where(x => !string.IsNullOrWhiteSpace(x))];
            }

            _logger.LogWarning("reCAPTCHA не пройдена. Ошибки: {Errors}",
                string.Join(", ", errorCodes));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при валидации reCAPTCHA");
            return false;
        }
    }
}
