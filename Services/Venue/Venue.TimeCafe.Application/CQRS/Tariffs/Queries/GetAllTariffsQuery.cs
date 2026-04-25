namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetAllTariffsQuery() : IQuery<IEnumerable<TariffWithThemeDto>>;

public class GetAllTariffsQueryValidator : AbstractValidator<GetAllTariffsQuery>
{
    public GetAllTariffsQueryValidator()
    {
    }
}

public class GetAllTariffsQueryHandler(ITariffRepository repository) : IQueryHandler<GetAllTariffsQuery, IEnumerable<TariffWithThemeDto>>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<Result<IEnumerable<TariffWithThemeDto>>> Handle(GetAllTariffsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffs = await _repository.GetAllAsync();
            return Result.Ok(tariffs);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

