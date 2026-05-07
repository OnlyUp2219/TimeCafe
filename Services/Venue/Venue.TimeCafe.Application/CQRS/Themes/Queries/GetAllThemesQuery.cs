namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetAllThemesQuery() : IQuery<IEnumerable<Theme>>;

public class GetAllThemesQueryHandler(IUnitOfWork uow) : IQueryHandler<GetAllThemesQuery, IEnumerable<Theme>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<Theme>>> Handle(GetAllThemesQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var themes = await _uow.Themes.GetAllAsync(cancellationToken);
            return Result.Ok(themes);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

