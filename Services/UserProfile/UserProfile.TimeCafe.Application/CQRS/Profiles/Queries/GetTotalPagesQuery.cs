namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetTotalPagesQuery() : IQuery<int>;

public class GetTotalPagesQueryHandler(IUnitOfWork uow) : IQueryHandler<GetTotalPagesQuery, int>
{
    public async Task<Result<int>> Handle(GetTotalPagesQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Ok(await uow.Profiles.GetTotalPageAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
