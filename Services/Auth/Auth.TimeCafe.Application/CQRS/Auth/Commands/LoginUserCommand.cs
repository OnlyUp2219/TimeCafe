
using Auth.TimeCafe.Application.DTO;

using BuildingBlocks.Enum;
using BuildingBlocks.Extensions;

namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResult>;
public record LoginUserResult(bool Success, string? Message = null, List<string>? Errors = null, ETypeError? TypeError = null, bool? EmailConfirmed = null, TokensDto? TokensDto = null) : ICqrsResult;

public class LoginUserCommandHandler(UserManager<IdentityUser> userManager, IJwtService jwtService) : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IJwtService _jwtService = jwtService;

    public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return new LoginUserResult(
                Success: false, 
                Message: "Неверный email или пароль", 
                Errors: ["InvalidCredentials"], 
                TypeError: ETypeError.BadRequest);

        if (!user.EmailConfirmed)
            return new LoginUserResult(
                Success: true, 
                EmailConfirmed: false);

        var tokens = await _jwtService.GenerateTokens(user);
       
        return new LoginUserResult(
            Success: true,
            Message: "Успешный вход",
            TokensDto: new TokensDto(tokens.AccessToken, tokens.RefreshToken),
            EmailConfirmed: true
        );
    }
}
