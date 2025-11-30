namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfilesPageQuery(int PageNumber, int PageSize) : IRequest<GetProfilesPageResult>;

public record GetProfilesPageResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<Profile>? Profiles = null,
    int? PageNumber = null,
    int? PageSize = null,
    int? TotalCount = null) : ICqrsResultV2
{
    public static GetProfilesPageResult GetFailed() =>
        new(false, Code: "GetProfilesPageFailed", Message: "Не удалось получить страницу профилей", StatusCode: 500);

    public static GetProfilesPageResult GetSuccess(IEnumerable<Profile> profiles, int pageNumber, int pageSize, int totalCount) =>
        new(true, Message: $"Получено профилей: {profiles.Count()}",
            Profiles: profiles, PageNumber: pageNumber, PageSize: pageSize, TotalCount: totalCount);
}

public class GetProfilesPageQueryValidator : AbstractValidator<GetProfilesPageQuery>
{
    public GetProfilesPageQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Номер страницы должен быть больше 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Размер страницы должен быть больше 0")
            .LessThanOrEqualTo(100).WithMessage("Размер страницы не может превышать 100");
        // TODO : добавить ограничение по максимальному размеру страницы в конфигурацию
    }
}

public class GetProfilesPageQueryHandler(IUserRepositories repositories) : IRequestHandler<GetProfilesPageQuery, GetProfilesPageResult>
{
    private readonly IUserRepositories _repositories = repositories;

    public async Task<GetProfilesPageResult> Handle(GetProfilesPageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var profiles = await _repositories.GetProfilesPageAsync(request.PageNumber, request.PageSize, cancellationToken);
            var totalCount = await _repositories.GetTotalPageAsync(cancellationToken);
            var nonNullProfiles = profiles.Where(p => p != null).Cast<Profile>();

            return GetProfilesPageResult.GetSuccess(nonNullProfiles, request.PageNumber, request.PageSize, totalCount);
        }
        catch (Exception)
        {
            return GetProfilesPageResult.GetFailed();
        }
    }
}