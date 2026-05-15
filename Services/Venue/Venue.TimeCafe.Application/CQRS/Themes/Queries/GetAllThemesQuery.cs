namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetAllThemesQuery() : IQuery<IEnumerable<ThemeDto>>;

public class GetAllThemesQueryHandler(IUnitOfWork uow) : IQueryHandler<GetAllThemesQuery, IEnumerable<ThemeDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<ThemeDto>>> Handle(GetAllThemesQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var themes = await _uow.Themes.GetAllAsync(cancellationToken);
            var dtos = themes.Select(t => new ThemeDto(
                t.ThemeId,
                t.Name,
                t.Emoji,
                t.Colors));
            return Result.Ok(dtos);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

