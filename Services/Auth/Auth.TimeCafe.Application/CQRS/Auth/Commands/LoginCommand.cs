namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<Result<TokensDto>>;

public class LoginCommandHandler(
    UserManager<IdentityUser> userManager,
    IJwtService jwtService) : IRequestHandler<LoginCommand, Result<TokensDto>>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IJwtService _jwtService = jwtService;

    public async Task<Result<TokensDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            // Унифицированное сообщение для предотвращения перечисления пользователей
            return Result<TokensDto>.ValidationError("email", "Неверный email или пароль");
        }

        if (!user.EmailConfirmed)
        {
            return Result<TokensDto>.BusinessLogic(
                "Email.NotConfirmed",
                "Подтвердите email перед входом. Проверьте почту."
            );
        }

        var tokens = await _jwtService.GenerateTokens(user);
        return Result<TokensDto>.Success(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
    }
}
