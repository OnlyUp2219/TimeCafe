using Venue.TimeCafe.Domain.DTOs;

namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetTariffsByBillingTypeQuery(BillingType BillingType) : IRequest<GetTariffsByBillingTypeResult>;

public record GetTariffsByBillingTypeResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<TariffWithThemeDto>? Tariffs = null) : ICqrsResultV2
{
    public static GetTariffsByBillingTypeResult GetFailed() =>
        new(false, Code: "GetTariffsByBillingTypeFailed", Message: "Не удалось получить тарифы по типу биллинга", StatusCode: 500);

    public static GetTariffsByBillingTypeResult GetSuccess(IEnumerable<TariffWithThemeDto> tariffs) =>
        new(true, Tariffs: tariffs);
}

public class GetTariffsByBillingTypeQueryHandler(ITariffRepository repository) : IRequestHandler<GetTariffsByBillingTypeQuery, GetTariffsByBillingTypeResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<GetTariffsByBillingTypeResult> Handle(GetTariffsByBillingTypeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffs = await _repository.GetByBillingTypeAsync(request.BillingType);
            return GetTariffsByBillingTypeResult.GetSuccess(tariffs);
        }
        catch (Exception)
        {
            return GetTariffsByBillingTypeResult.GetFailed();
        }
    }
}
