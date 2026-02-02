namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetAllProfilesQuery() : IRequest<GetAllProfilesResult>;

public record GetAllProfilesResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<Profile>? Profiles = null) : ICqrsResultV2
{
    public static GetAllProfilesResult GetFailed() =>
        new(false, Code: "GetAllProfilesFailed", Message: "Не удалось получить профили", StatusCode: 500);

    public static GetAllProfilesResult GetSuccess(IEnumerable<Profile> profiles) =>
        new(true, Message: $"Получено профилей: {profiles.Count()}", Profiles: profiles);
}

public class GetAllProfilesQueryValidator : AbstractValidator<GetAllProfilesQuery>
{
    public GetAllProfilesQueryValidator()
    {
        // Нет параметров для валидации
    }
}

public class GetAllProfilesQueryHandler(IUserRepositories userRepositories) : IRequestHandler<GetAllProfilesQuery, GetAllProfilesResult>
{
    private readonly IUserRepositories _userRepositories = userRepositories;

    public async Task<GetAllProfilesResult> Handle(GetAllProfilesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var profiles = await _userRepositories.GetAllProfilesAsync(cancellationToken);
            var nonNullProfiles = profiles.Where(p => p != null).Cast<Profile>();
            return GetAllProfilesResult.GetSuccess(nonNullProfiles);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(GetAllProfilesResult.GetFailed(), ex);
        }
    }
}