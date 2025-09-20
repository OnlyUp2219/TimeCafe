namespace Main.TimeCafe.Application.CQRS.Themes.Get;

public record class GetThemeQuery() : IRequest<IEnumerable<Theme>>;
public class GetThemeHandlery : IRequestHandler<GetThemeQuery, IEnumerable<Theme>>
{
    private readonly IThemeRepository _repository;
    public GetThemeHandlery(IThemeRepository repository)
    {
        _repository = repository;
    }
    public async Task<IEnumerable<Theme>> Handle(GetThemeQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetThemesAsync();
    }
}
