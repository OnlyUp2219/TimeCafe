namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResult>;
public record LoginUserResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    bool EmailConfirmed = false,
    TokensDto? TokensDto = null) : ICqrsResult
{
    public static LoginUserResult InvalidCredentials() =>
        new(false, Code: "InvalidCredentials", Message: "Неверный email или пароль", StatusCode: 400);

    public static LoginUserResult LockedOut() =>
        new(false, Code: "LockedOut", Message: "Учетная запись заблокирована из-за слишком большого количества неудачных попыток входа. Пожалуйста, попробуйте позже.", StatusCode: 400);

    public static LoginUserResult EmailNotConfirmed() =>
       new(Success: true,
           EmailConfirmed: false);

    public static LoginUserResult LoginSuccess(TokensDto tokensDto) =>
        new(true, Message: "Успешный вход",
 EmailConfirmed: true, TokensDto: tokensDto);
}

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен");
    }
}


public class LoginUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtService jwtService) : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtService _jwtService = jwtService;

    public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return LoginUserResult.InvalidCredentials();

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
            return LoginUserResult.LockedOut();

        if (signInResult.IsNotAllowed)
        {
            if (!user.EmailConfirmed)
                return LoginUserResult.EmailNotConfirmed();

            return LoginUserResult.InvalidCredentials();
        }

        if (!signInResult.Succeeded)
            return LoginUserResult.InvalidCredentials();

        var tokens = await _jwtService.GenerateTokens(user);

        return LoginUserResult.LoginSuccess(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
    }
}
