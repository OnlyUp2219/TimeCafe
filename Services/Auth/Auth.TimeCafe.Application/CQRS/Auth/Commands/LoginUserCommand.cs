namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResult>;
public record LoginUserResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    bool EmailConfirmed = false,
    TokensDto? TokensDto = null) : ICqrsResultV2
{
    public static LoginUserResult InvalidCredentials() =>
        new(false, Code: "InvalidCredentials", Message: "Неверный email или пароль", StatusCode: 400);

    public static LoginUserResult EmailNotConfirmed() =>
       new(Success: true,
           EmailConfirmed: false);

    public static LoginUserResult LoginSuccess(TokensDto tokensDto) =>
        new(true, Message: "Успешный вход",
            TokensDto: tokensDto, EmailConfirmed: true);
}

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен");
    }
}


public class LoginUserCommandHandler(UserManager<ApplicationUser> userManager, IJwtService jwtService) : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtService _jwtService = jwtService;

    public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return LoginUserResult.InvalidCredentials();

        if (!user.EmailConfirmed)
            return LoginUserResult.EmailNotConfirmed();

        var tokens = await _jwtService.GenerateTokens(user);

        return LoginUserResult.LoginSuccess(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
    }
}
