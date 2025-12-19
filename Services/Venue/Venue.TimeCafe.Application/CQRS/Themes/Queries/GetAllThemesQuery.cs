using Venue.TimeCafe.Application.Contracts.Repositories;

namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetAllThemesQuery() : IRequest<GetAllThemesResult>;

public record GetAllThemesResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<Theme>? Themes = null) : ICqrsResultV2
{
    public static GetAllThemesResult GetFailed() =>
        new(false, Code: "GetThemesFailed", Message: "Не удалось получить темы", StatusCode: 500);

    public static GetAllThemesResult GetSuccess(IEnumerable<Theme> themes) =>
        new(true, Themes: themes);
}

public class GetAllThemesQueryHandler(IThemeRepository repository) : IRequestHandler<GetAllThemesQuery, GetAllThemesResult>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<GetAllThemesResult> Handle(GetAllThemesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var themes = await _repository.GetAllAsync();
            return GetAllThemesResult.GetSuccess(themes);
        }
        catch (Exception)
        {
            return GetAllThemesResult.GetFailed();
        }
    }
}
