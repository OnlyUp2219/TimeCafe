using Auth.TimeCafe.Application.CQRS.Auth.Commands;

namespace Auth.TimeCafe.Application.CQRS.Auth.Validators;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token обязателен");
    }
}
