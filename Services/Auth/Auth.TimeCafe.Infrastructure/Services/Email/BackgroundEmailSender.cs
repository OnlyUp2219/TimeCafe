namespace Auth.TimeCafe.Infrastructure.Services.Email;

public sealed class BackgroundEmailSender(IEmailSender<ApplicationUser> innerSender, ILogger<BackgroundEmailSender> logger) : IEmailSender<ApplicationUser>
{
    private readonly IEmailSender<ApplicationUser> _innerSender = innerSender;
    private readonly ILogger<BackgroundEmailSender> _logger = logger;

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _innerSender.SendConfirmationLinkAsync(user, email, confirmationLink);
                _logger.LogInformation("Confirmation email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", email);
            }
        });

        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _innerSender.SendPasswordResetLinkAsync(user, email, resetLink);
                _logger.LogInformation("Password reset email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            }
        });

        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _innerSender.SendPasswordResetCodeAsync(user, email, resetCode);
                _logger.LogInformation("Password reset code sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset code to {Email}", email);
            }
        });

        return Task.CompletedTask;
    }
}
