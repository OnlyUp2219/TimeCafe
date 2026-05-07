namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetThemeByIdQuery(Guid ThemeId) : IQuery<Theme>;

public class GetThemeByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetThemeByIdQuery, Theme>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<Theme>> Handle(GetThemeByIdQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var theme = await _uow.Themes.GetByIdAsync(request.ThemeId, cancellationToken);

            if (theme == null)
                return Result.Fail(new ThemeNotFoundError());

            return Result.Ok(theme);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

