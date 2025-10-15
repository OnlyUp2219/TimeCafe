using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Auth.TimeCafe.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace Auth.TimeCafe.Infrastructure.Services;

public sealed class PostmarkEmailSender : IEmailSender<IdentityUser>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PostmarkOptions _options;

    public PostmarkEmailSender(IHttpClientFactory httpClientFactory, IOptions<PostmarkOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

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

    public async Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink)
    {
        var subject = "Подтвердите ваш email";
        var htmlMessage = $"<p>Подтвердите ваш email, перейдя по <a href='{confirmationLink}'>этой ссылке</a>.</p>";
        await SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink)
    {
        if (string.IsNullOrWhiteSpace(_options.FrontendBaseUrl))
            throw new InvalidOperationException("Postmark FrontendBaseUrl is not configured.");

        var token = Uri.EscapeDataString(resetLink.Split("code=").Last());


        var frontendLink = $"{_options.FrontendBaseUrl}/forgotPassword?email={Uri.EscapeDataString(email)}&code={token}";

        var subject = "Сброс пароля";
        var htmlMessage = $"<p>Для сброса пароля перейдите по <a href='{resetLink}'>этой ссылке</a>.</p>";
        await SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode)
    {
        var subject = "Код для сброса пароля";
        var htmlMessage = $"<p>Ваш код для сброса пароля: <strong>{resetCode}</strong></p>";
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
