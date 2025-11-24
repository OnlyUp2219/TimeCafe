namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record class UpdateProfileCommand(Profile User) : IRequest<Profile?>;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.User)
            .NotNull().WithMessage("Пользователь не найден");
    }
}

public class UpdateProfileCommandHandler(IUserRepositories repositories) : IRequestHandler<UpdateProfileCommand, Profile?>
{
    private readonly IUserRepositories _repositories = repositories;

    public async Task<Profile?> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        return await _repositories.UpdateProfileAsync(request.User,cancellationToken).ConfigureAwait(false);
    }
}