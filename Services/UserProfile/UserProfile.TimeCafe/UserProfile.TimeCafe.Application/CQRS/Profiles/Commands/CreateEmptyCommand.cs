
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
            .NotEmpty().WithMessage("UserId обязателен")
            .MaximumLength(64).WithMessage("UserId не может превышать 64 символа");
    }
}

public class CreateEmptyCommandHandler(IUserRepositories repositories) : IRequestHandler<CreateEmptyCommand, CreateEmptyResult>
{
    private readonly IUserRepositories _repositories = repositories;

    public async Task<CreateEmptyResult> Handle(CreateEmptyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repositories.GetProfileByIdAsync(request.UserId, cancellationToken);
            if (existing != null)
                return CreateEmptyResult.ProfileAlreadyExists();

            await _repositories.CreateEmptyAsync(request.UserId, cancellationToken);
            return CreateEmptyResult.CreateSuccess();
        }
        catch (Exception)
        {
            return CreateEmptyResult.CreateFailed();
        }
    }
}