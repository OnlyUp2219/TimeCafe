namespace Auth.TimeCafe.Infrastructure.Services.Email;

public sealed class PostmarkEmailSender(IHttpClientFactory httpClientFactory, IOptions<PostmarkOptions> options) : IEmailSender<ApplicationUser>
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly PostmarkOptions _options = options.Value;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(_options.ServerToken))
        {
            throw new InvalidOperationException("Postmark ServerToken is not configured.");
        }
        if (string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            throw new InvalidOperationException("Postmark FromEmail is not configured.");
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://api.postmarkapp.com/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("X-Postmark-Server-Token", _options.ServerToken);

        var payload = new Dictionary<string, object>
        {
            ["From"] = _options.FromEmail,
            ["To"] = email,
            ["Subject"] = subject,
            ["HtmlBody"] = htmlMessage
        };

        if (!string.IsNullOrWhiteSpace(_options.MessageStream))
        {
            payload["MessageStream"] = _options.MessageStream!;
        }

        var textBody = StripHtml(htmlMessage);
        if (!string.IsNullOrWhiteSpace(textBody))
        {
            payload["TextBody"] = textBody;
        }

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("email", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var subject = "Подтвердите ваш email";
        var htmlMessage = EmailTemplates.GetConfirmationEmailTemplate(confirmationLink);
        await SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        if (string.IsNullOrWhiteSpace(_options.FrontendBaseUrl))
            throw new InvalidOperationException("Postmark FrontendBaseUrl is not configured.");

        var subject = "Сброс пароля";
        var htmlMessage = EmailTemplates.GetPasswordResetLinkTemplate(resetLink);
        await SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var subject = "Код для сброса пароля";
        var htmlMessage = EmailTemplates.GetPasswordResetCodeTemplate(resetCode);
        await SendEmailAsync(email, subject, htmlMessage);
    }

    private static string StripHtml(string html)
    {
        var array = new char[html.Length];
        var arrayIndex = 0;
        var inside = false;
        foreach (var @let in html)
        {
            if (@let == '<')
            {
                inside = true;
                continue;
            }
            if (@let == '>')
            {
                inside = false;
                continue;
            }
            if (!inside)
            {
                array[arrayIndex] = @let;
                arrayIndex++;
            }
        }
        return new string(array, 0, arrayIndex).Trim();
    }
}

