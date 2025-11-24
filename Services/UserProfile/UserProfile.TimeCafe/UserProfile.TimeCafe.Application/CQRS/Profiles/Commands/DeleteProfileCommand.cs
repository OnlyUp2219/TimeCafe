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
            .NotEmpty().WithMessage("UserId обязателен")
            .MaximumLength(64).WithMessage("UserId не может превышать 64 символа");
    }
}

public class DeleteProfileCommandHandler(IUserRepositories userRepositories) : IRequestHandler<DeleteProfileCommand, DeleteProfileResult>
{
    private readonly IUserRepositories _userRepositories = userRepositories;

    public async Task<DeleteProfileResult> Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _userRepositories.GetProfileByIdAsync(request.UserId, cancellationToken);
            if (existing == null)
                return DeleteProfileResult.ProfileNotFound();

            await _userRepositories.DeleteProfileAsync(request.UserId, cancellationToken);
            return DeleteProfileResult.DeleteSuccess();
        }
        catch (Exception)
        {
            return DeleteProfileResult.DeleteFailed();
        }
    }
}