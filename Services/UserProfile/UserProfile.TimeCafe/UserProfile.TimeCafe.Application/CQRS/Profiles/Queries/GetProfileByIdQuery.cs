namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfileByIdQuery(string UserId) : IRequest<Profile?>;

public class GetProfileByIdQueryValidator : AbstractValidator<GetProfileByIdQuery>
{
    public GetProfileByIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(64);
    }
}
public class GetProfileByIdQueryHandler(IUserRepositories repository) : IRequestHandler<GetProfileByIdQuery, Profile?>
{
    private readonly IUserRepositories _repository = repository;

    public async Task<Profile?> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetProfileByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
    }
}