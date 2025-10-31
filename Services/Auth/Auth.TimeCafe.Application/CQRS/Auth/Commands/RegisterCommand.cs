namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;

public record RegisterCommand(string Username, string Email, string Password, bool SendEmail = true) : IRequest<Result<RegisterResponseDto>>;

public record RegisterResponseDto(string Message, string? ConfirmLink = null);

public class RegisterCommandHandler(
    UserManager<IdentityUser> userManager,
    IEmailSender<IdentityUser> emailSender,
    IConfiguration configuration) : IRequestHandler<RegisterCommand, Result<RegisterResponseDto>>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IEmailSender<IdentityUser> _emailSender = emailSender;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Result<RegisterResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e =>
            {
                // Определяем поле на основе кода ошибки Identity
                string? field = e.Code.ToLower() switch
                {
                    var code when code.Contains("email") => "email",
                    var code when code.Contains("password") => "password",
                    var code when code.Contains("username") => "username",
                    _ => null
                };

                // Определяем тип ошибки
                var errorType = e.Code switch
                {
                    "DuplicateUserName" or "DuplicateEmail" => ErrorType.Conflict,
                    _ => ErrorType.Validation
                };

                return new ErrorDetail(errorType, $"Identity.{e.Code}", e.Description, field);
            }).ToList();

            return Result<RegisterResponseDto>.Failure(errors);
        }
        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var frontendBase = _configuration["Frontend:BaseUrl"] ?? "http://127.0.0.1:9301";
        var confirmLink = $"{frontendBase}/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(confirmationToken)}";

        if (request.SendEmail)
        {
            await _emailSender.SendConfirmationLinkAsync(user, user.Email!, confirmLink);
            return Result<RegisterResponseDto>.Success(new RegisterResponseDto("Пользователь создан. Проверьте почту для подтверждения email."));
        }

        return Result<RegisterResponseDto>.Success(new RegisterResponseDto(
            "Пользователь создан. Проверьте почту для подтверждения email.",
            confirmLink));
    }
}
