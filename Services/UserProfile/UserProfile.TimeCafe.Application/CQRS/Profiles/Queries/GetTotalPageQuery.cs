namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetTotalPagesQuery() : IQuery<int>;

public class GetTotalPagesQueryValidator : AbstractValidator<GetTotalPagesQuery>
{
    public GetTotalPagesQueryValidator()
    {
    }
}

public class GetTotalPagesQueryHandler(IUserRepositories repositories) : IQueryHandler<GetTotalPagesQuery, int>
{
    private readonly IUserRepositories _repositories = repositories;

    public async Task<Result<int>> Handle(GetTotalPagesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var totalCount = await _repositories.GetTotalPageAsync(cancellationToken);
            return Result.Ok(totalCount);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
