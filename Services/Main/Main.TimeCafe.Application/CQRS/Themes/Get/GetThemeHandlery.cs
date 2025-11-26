namespace Main.TimeCafe.Application.CQRS.Themes.Get;

public record class GetThemeQuery() : IRequest<IEnumerable<Theme>>;
public class GetThemeHandlery(IThemeRepository repository) : IRequestHandler<GetThemeQuery, IEnumerable<Theme>>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<IEnumerable<Theme>> Handle(GetThemeQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetThemesAsync();
    }
}
