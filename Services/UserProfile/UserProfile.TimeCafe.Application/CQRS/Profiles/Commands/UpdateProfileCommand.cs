namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;
//TODO: rewrite dto
public record UpdateProfileCommand(Profile User) : IRequest<UpdateProfileResult>;

public record UpdateProfileResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Profile? Profile = null) : ICqrsResultV2
{
    public static UpdateProfileResult ProfileNotFound() =>
        new(false, Code: "ProfileNotFound", Message: "Профиль не найден", StatusCode: 404);

    public static UpdateProfileResult UpdateFailed() =>
        new(false, Code: "UpdateProfileFailed", Message: "Не удалось обновить профиль", StatusCode: 500);

    public static UpdateProfileResult UpdateSuccess(Profile profile) =>
        new(true, Message: "Профиль успешно обновлён", Profile: profile);
}

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.User)
            .NotNull().WithMessage("Профиль обязателен");

        RuleFor(x => x.User.UserId)
            .NotEmpty().WithMessage("Такого пользователя не существует")
            .Must(id => id != Guid.Empty).WithMessage("Такого пользователя не существует");

        RuleFor(x => x.User.FirstName)
            .NotEmpty().WithMessage("Имя обязательно")
            .MaximumLength(100).WithMessage("Имя не может превышать 100 символов");

        RuleFor(x => x.User.LastName)
            .NotEmpty().WithMessage("Фамилия обязательна")
            .MaximumLength(100).WithMessage("Фамилия не может превышать 100 символов");

        RuleFor(x => x.User.Gender)
            .IsInEnum().WithMessage("Пол указан некорректно");

        RuleFor(x => x.User.BirthDate)
            .Must(date => date == null || date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Дата рождения не может быть в будущем");
    }
}

public class UpdateProfileCommandHandler(IUserRepositories repositories) : IRequestHandler<UpdateProfileCommand, UpdateProfileResult>
{
    private readonly IUserRepositories _repositories = repositories;

    public async Task<UpdateProfileResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repositories.GetProfileByIdAsync(request.User.UserId, cancellationToken);
            if (existing == null)
                return UpdateProfileResult.ProfileNotFound();

            request.User.AccessCardNumber = existing.AccessCardNumber;

            if (existing.ProfileStatus == ProfileStatus.Banned)
            {
                request.User.ProfileStatus = ProfileStatus.Banned;
                request.User.BanReason = existing.BanReason;
            }
            else
            {
                var isCompleted =
                    !string.IsNullOrWhiteSpace(request.User.FirstName)
                    && !string.IsNullOrWhiteSpace(request.User.LastName);

                request.User.ProfileStatus = isCompleted ? ProfileStatus.Completed : ProfileStatus.Pending;
            }

            var updated = await _repositories.UpdateProfileAsync(request.User, cancellationToken);

            if (updated == null)
                return UpdateProfileResult.UpdateFailed();

            return UpdateProfileResult.UpdateSuccess(updated);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(UpdateProfileResult.UpdateFailed(), ex);
        }
    }
}