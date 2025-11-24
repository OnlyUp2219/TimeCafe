namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record class DeleteProfileCommand(string UserId) : IRequest;

public class DeleteProfileCommandValidator : AbstractValidator<DeleteProfileCommand>
{
    public DeleteProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден.");
    }
}
public class DeleteProfileCommandHandler(IUserRepositories userRepositories) : IRequestHandler<DeleteProfileCommand>
{
    public async Task<Unit> Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
    {
        await userRepositories.DeleteProfileAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}