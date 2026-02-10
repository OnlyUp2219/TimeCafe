using Auth.TimeCafe.Domain.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

using System.Text;

namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record ConfirmEmailChangeCommand(string UserId, string NewEmail, string Token) : IRequest<ConfirmEmailChangeResult>;

public record ConfirmEmailChangeResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static ConfirmEmailChangeResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 401);

    public static ConfirmEmailChangeResult InvalidTokenFormat() =>
        new(false, Code: "InvalidTokenFormat", Message: "Неверный формат токена", StatusCode: 400);

    public static ConfirmEmailChangeResult InvalidToken() =>
        new(false, Code: "InvalidToken", Message: "Токен недействителен или уже использован", StatusCode: 400);

    public static ConfirmEmailChangeResult ConfirmEmailChangeSuccess() =>
        new(true, Message: "Email изменен и подтвержден");
}

public class ConfirmEmailChangeCommandValidator : AbstractValidator<ConfirmEmailChangeCommand>
{
    public ConfirmEmailChangeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .NotNull().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");

        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("Email не может быть пустым")
            .EmailAddress().WithMessage("Неверный формат Email");
    }
}

public class ConfirmEmailChangeCommandHandler(
    UserManager<ApplicationUser> userManager) : IRequestHandler<ConfirmEmailChangeCommand, ConfirmEmailChangeResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<ConfirmEmailChangeResult> Handle(ConfirmEmailChangeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return ConfirmEmailChangeResult.UserNotFound();

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch
        {
            return ConfirmEmailChangeResult.InvalidTokenFormat();
        }

        var result = await _userManager.ChangeEmailAsync(user, request.NewEmail, decodedToken);
        if (!result.Succeeded)
        {
            return ConfirmEmailChangeResult.InvalidToken();
        }

        return ConfirmEmailChangeResult.ConfirmEmailChangeSuccess();
    }
}
