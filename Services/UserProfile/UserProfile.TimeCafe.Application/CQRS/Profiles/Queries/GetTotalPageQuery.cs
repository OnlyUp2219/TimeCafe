namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetTotalPagesQuery() : IRequest<GetTotalPagesResult>;

public record GetTotalPagesResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    int? TotalCount = null) : ICqrsResultV2
{
    public static GetTotalPagesResult GetFailed() =>
        new(false, Code: "GetTotalPagesFailed", Message: "Не удалось получить общее количество", StatusCode: 500);

    public static GetTotalPagesResult GetSuccess(int totalCount) =>
        new(true, Message: $"Всего профилей: {totalCount}", TotalCount: totalCount);
}

public class GetTotalPagesQueryValidator : AbstractValidator<GetTotalPagesQuery>
{
    public GetTotalPagesQueryValidator()
    {
    }
}

public class GetTotalPagesQueryHandler(IUserRepositories repositories) : IRequestHandler<GetTotalPagesQuery, GetTotalPagesResult>
{
    private readonly IUserRepositories _repositories = repositories;

    public async Task<GetTotalPagesResult> Handle(GetTotalPagesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var totalCount = await _repositories.GetTotalPageAsync(cancellationToken);
            return GetTotalPagesResult.GetSuccess(totalCount);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(GetTotalPagesResult.GetFailed(), ex);
        }
    }
}