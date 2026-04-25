namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfileByIdQuery(Guid UserId) : IQuery<Profile>;

public class GetProfileByIdQueryValidator : AbstractValidator<GetProfileByIdQuery>
{
    public GetProfileByIdQueryValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");
    }
}

public class GetProfileByIdQueryHandler(IUserRepositories repository) : IQueryHandler<GetProfileByIdQuery, Profile>
{
    private readonly IUserRepositories _repository = repository;

    public async Task<Result<Profile>> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _repository.GetProfileByIdAsync(request.UserId, cancellationToken);

            if (profile == null)
                return Result.Fail(new ProfileNotFoundError());

            var responseProfile = ProfilePhotoUrlMapper.WithApiUrl(profile);
            return Result.Ok(responseProfile);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
