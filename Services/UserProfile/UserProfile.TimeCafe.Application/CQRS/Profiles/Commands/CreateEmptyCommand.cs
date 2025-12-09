
namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record CreateEmptyCommand(string UserId) : IRequest<CreateEmptyResult>;

public record CreateEmptyResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static CreateEmptyResult ProfileAlreadyExists() =>
        new(false, Code: "ProfileAlreadyExists", Message: "Профиль уже существует", StatusCode: 409);

    public static CreateEmptyResult CreateFailed() =>
        new(false, Code: "CreateEmptyFailed", Message: "Не удалось создать профиль", StatusCode: 500);

    public static CreateEmptyResult CreateSuccess() =>
        new(true, Message: "Пустой профиль создан", StatusCode: 201);
}

public class CreateEmptyCommandValidator : AbstractValidator<CreateEmptyCommand>
{
    public CreateEmptyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Такого пользователя не существует")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Такого пользователя не существует")
            .Must(x => Guid.TryParse(x, out _)).WithMessage("Такого пользователя не существует");
    }
}

public class CreateEmptyCommandHandler(IUserRepositories repositories) : IRequestHandler<CreateEmptyCommand, CreateEmptyResult>
{
    private readonly IUserRepositories _repositories = repositories;

    public async Task<CreateEmptyResult> Handle(CreateEmptyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(request.UserId);

            var existing = await _repositories.GetProfileByIdAsync(userId, cancellationToken);
            if (existing != null)
                return CreateEmptyResult.ProfileAlreadyExists();

            await _repositories.CreateEmptyAsync(userId, cancellationToken);
            return CreateEmptyResult.CreateSuccess();
        }
        catch (Exception)
        {
            return CreateEmptyResult.CreateFailed();
        }
    }
}