namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record CreateProfileCommand(string UserId, string FirstName, string LastName, Gender Gender) : IRequest<CreateProfileResult>;

public record CreateProfileResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Profile? Profile = null) : ICqrsResultV2
{
    public static CreateProfileResult ProfileAlreadyExists() =>
        new(false, Code: "ProfileAlreadyExists", Message: "Профиль для пользователя уже существует", StatusCode: 409);

    public static CreateProfileResult CreateFailed() =>
        new(false, Code: "CreateProfileFailed", Message: "Не удалось создать профиль", StatusCode: 500);

    public static CreateProfileResult CreateSuccess(Profile profile) =>
        new(true, Message: "Профиль успешно создан", StatusCode: 201, Profile: profile);
}

public class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(x => x.UserId).ValidEntityId("Такого пользователя не существует");

        RuleFor(x => x.FirstName).ValidName("Имя");

        RuleFor(x => x.LastName).ValidName("Фамилия");
    }
}

public class CreateProfileCommandHandler(IUserRepositories repository) : IRequestHandler<CreateProfileCommand, CreateProfileResult>
{
    private readonly IUserRepositories _repository = repository;

    public async Task<CreateProfileResult> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(request.UserId);

            var existing = await _repository.GetProfileByIdAsync(userId, cancellationToken);
            if (existing != null)
                return CreateProfileResult.ProfileAlreadyExists();

            var profile = new Profile
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Gender = request.Gender,
                ProfileStatus = ProfileStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var created = await _repository.CreateProfileAsync(profile, cancellationToken);

            if (created == null)
                return CreateProfileResult.CreateFailed();

            return CreateProfileResult.CreateSuccess(created);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(CreateProfileResult.CreateFailed(), ex);
        }
    }
}
