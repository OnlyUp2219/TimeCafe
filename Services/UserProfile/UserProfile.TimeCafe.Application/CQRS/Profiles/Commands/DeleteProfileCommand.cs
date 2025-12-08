namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record DeleteProfileCommand(string UserId) : IRequest<DeleteProfileResult>;

public record DeleteProfileResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static DeleteProfileResult ProfileNotFound() =>
        new(false, Code: "ProfileNotFound", Message: "Профиль не найден", StatusCode: 404);

    public static DeleteProfileResult DeleteFailed() =>
        new(false, Code: "DeleteProfileFailed", Message: "Не удалось удалить профиль", StatusCode: 500);

    public static DeleteProfileResult DeleteSuccess() =>
        new(true, Message: "Профиль успешно удалён");
}

public class DeleteProfileCommandValidator : AbstractValidator<DeleteProfileCommand>
{
    public DeleteProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Идентификатор пользователя не указан")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Идентификатор пользователя не указан")
            .Must(x => Guid.TryParse(x, out _)).WithMessage("Некорректный идентификатор пользователя");

    }
}

public class DeleteProfileCommandHandler(IUserRepositories userRepositories) : IRequestHandler<DeleteProfileCommand, DeleteProfileResult>
{
    private readonly IUserRepositories _userRepositories = userRepositories;

    public async Task<DeleteProfileResult> Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(request.UserId);
            var existing = await _userRepositories.GetProfileByIdAsync(userId, cancellationToken);
            if (existing == null)
                return DeleteProfileResult.ProfileNotFound();

            await _userRepositories.DeleteProfileAsync(userId, cancellationToken);
            return DeleteProfileResult.DeleteSuccess();
        }
        catch (Exception)
        {
            return DeleteProfileResult.DeleteFailed();
        }
    }
}