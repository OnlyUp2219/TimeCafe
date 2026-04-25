namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetActiveTariffsQuery() : IQuery<IEnumerable<TariffWithThemeDto>>;

public class GetActiveTariffsQueryValidator : AbstractValidator<GetActiveTariffsQuery>
{
    public GetActiveTariffsQueryValidator()
    {

    }
}

public class GetActiveTariffsQueryHandler(ITariffRepository repository) : IQueryHandler<GetActiveTariffsQuery, IEnumerable<TariffWithThemeDto>>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<Result<IEnumerable<TariffWithThemeDto>>> Handle(GetActiveTariffsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffs = await _repository.GetActiveAsync();
            return Result.Ok(tariffs);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

