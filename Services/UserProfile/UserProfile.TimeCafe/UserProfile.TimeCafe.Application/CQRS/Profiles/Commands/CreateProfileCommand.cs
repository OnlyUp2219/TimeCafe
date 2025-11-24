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
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId обязателен")
            .MaximumLength(64).WithMessage("UserId не может превышать 64 символа");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно")
            .MaximumLength(128).WithMessage("Имя не может превышать 128 символов");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия обязательна")
            .MaximumLength(128).WithMessage("Фамилия не может превышать 128 символов");
    }
}

public class CreateProfileCommandHandler(IUserRepositories repository) : IRequestHandler<CreateProfileCommand, CreateProfileResult>
{
    private readonly IUserRepositories _repository = repository;

    public async Task<CreateProfileResult> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetProfileByIdAsync(request.UserId, cancellationToken);
            if (existing != null)
                return CreateProfileResult.ProfileAlreadyExists();

            var profile = new Profile
            {
                UserId = request.UserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Gender = request.Gender,
                ProfileStatus = ProfileStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateProfileAsync(profile, cancellationToken);

            if (created == null)
                return CreateProfileResult.CreateFailed();

            return CreateProfileResult.CreateSuccess(created);
        }
        catch (Exception)
        {
            return CreateProfileResult.CreateFailed();
        }
    }
}