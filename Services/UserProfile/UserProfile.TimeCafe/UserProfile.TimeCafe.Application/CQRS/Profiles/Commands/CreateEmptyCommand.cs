
namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record class CreateEmptyCommand(string UserId) : IRequest;

public class CreateEmptyCommandValidator : AbstractValidator<CreateEmptyCommand>
{
    public CreateEmptyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден");
    }
}

public class CreateEmptyCommandHandler(IUserRepositories repositories) : IRequestHandler<CreateEmptyCommand>
{
    private readonly IUserRepositories _repositories = repositories;
    public async Task<Unit> Handle(CreateEmptyCommand request, CancellationToken cancellationToken)
    {
        await _repositories.CreateEmptyAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}