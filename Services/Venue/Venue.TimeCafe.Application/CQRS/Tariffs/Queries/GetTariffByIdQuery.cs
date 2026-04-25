namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetTariffByIdQuery(Guid TariffId) : IQuery<TariffWithThemeDto>;

public class GetTariffByIdQueryValidator : AbstractValidator<GetTariffByIdQuery>
{
    public GetTariffByIdQueryValidator()
    {
        RuleFor(x => x.TariffId).ValidGuidEntityId("Тариф не найден");
    }
}

public class GetTariffByIdQueryHandler(ITariffRepository repository) : IQueryHandler<GetTariffByIdQuery, TariffWithThemeDto>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<Result<TariffWithThemeDto>> Handle(GetTariffByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariff = await _repository.GetByIdAsync(request.TariffId);

            if (tariff == null)
                return Result.Fail(new TariffNotFoundError());

            return Result.Ok(tariff);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

