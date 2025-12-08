namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfileByIdQuery(string UserId) : IRequest<GetProfileByIdResult>;

public record GetProfileByIdResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Profile? Profile = null) : ICqrsResultV2
{
    public static GetProfileByIdResult ProfileNotFound() =>
        new(false, Code: "ProfileNotFound", Message: "Профиль не найден", StatusCode: 404);

    public static GetProfileByIdResult GetFailed() =>
        new(false, Code: "GetProfileFailed", Message: "Не удалось получить профиль", StatusCode: 500);

    public static GetProfileByIdResult GetSuccess(Profile profile) =>
        new(true, Message: "Профиль найден", Profile: profile);
}

public class GetProfileByIdQueryValidator : AbstractValidator<GetProfileByIdQuery>
{
    public GetProfileByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Идентификатор пользователя не указан")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Идентификатор пользователя не указан")
            .Must(x => Guid.TryParse(x, out _)).WithMessage("Некорректный идентификатор пользователя");
    }
}

public class GetProfileByIdQueryHandler(IUserRepositories repository) : IRequestHandler<GetProfileByIdQuery, GetProfileByIdResult>
{
    private readonly IUserRepositories _repository = repository;

    public async Task<GetProfileByIdResult> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(request.UserId);
            var profile = await _repository.GetProfileByIdAsync(userId, cancellationToken);

            if (profile == null)
                return GetProfileByIdResult.ProfileNotFound();

            return GetProfileByIdResult.GetSuccess(profile);
        }
        catch (Exception)
        {
            return GetProfileByIdResult.GetFailed();
        }
    }
}