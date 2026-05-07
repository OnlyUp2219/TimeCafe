namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetTariffByIdQuery(Guid TariffId) : IQuery<TariffWithThemeDto>;

public class GetTariffByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetTariffByIdQuery, TariffWithThemeDto>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<TariffWithThemeDto>> Handle(GetTariffByIdQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tariff = await _uow.Tariffs.GetWithThemeByIdAsync(request.TariffId, cancellationToken);

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

