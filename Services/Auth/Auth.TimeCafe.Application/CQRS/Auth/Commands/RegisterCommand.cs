namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<Result<TokensDto>>;

public class RegisterCommandHandler(
    UserManager<IdentityUser> userManager,
    IJwtService jwtService) : IRequestHandler<RegisterCommand, Result<TokensDto>>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IJwtService _jwtService = jwtService;

    public async Task<Result<TokensDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result<TokensDto>.Failure(errors);
        }

        var tokens = await _jwtService.GenerateTokens(user);

        return Result<TokensDto>.Success(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
    }
}
