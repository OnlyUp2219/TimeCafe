namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetThemeByIdQuery(Guid ThemeId) : IQuery<ThemeDto>;

public class GetThemeByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetThemeByIdQuery, ThemeDto>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<ThemeDto>> Handle(GetThemeByIdQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var theme = await _uow.Themes.GetByIdAsync(request.ThemeId, cancellationToken);

            if (theme == null)
                return Result.Fail(new ThemeNotFoundError());

            return Result.Ok(new ThemeDto(
                theme.ThemeId,
                theme.Name,
                theme.Emoji,
                theme.Colors));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

