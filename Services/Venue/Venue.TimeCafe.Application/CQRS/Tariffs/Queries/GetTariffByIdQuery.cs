namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetTariffByIdQuery(Guid TariffId) : IRequest<GetTariffByIdResult>;

public record GetTariffByIdResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    TariffWithThemeDto? Tariff = null) : ICqrsResult
{
    public static GetTariffByIdResult TariffNotFound() =>
        new(false, Code: "TariffNotFound", Message: "Тариф не найден", StatusCode: 404);

    public static GetTariffByIdResult GetFailed() =>
        new(false, Code: "GetTariffFailed", Message: "Не удалось получить тариф", StatusCode: 500);

    public static GetTariffByIdResult GetSuccess(TariffWithThemeDto tariff) =>
        new(true, Tariff: tariff);
}

public class GetTariffByIdQueryValidator : AbstractValidator<GetTariffByIdQuery>
{
    public GetTariffByIdQueryValidator()
    {
        RuleFor(x => x.TariffId).ValidGuidEntityId("Тариф не найден");
    }
}

public class GetTariffByIdQueryHandler(ITariffRepository repository) : IRequestHandler<GetTariffByIdQuery, GetTariffByIdResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<GetTariffByIdResult> Handle(GetTariffByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariff = await _repository.GetByIdAsync(request.TariffId);

            if (tariff == null)
                return GetTariffByIdResult.TariffNotFound();

            return GetTariffByIdResult.GetSuccess(tariff);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(GetTariffByIdResult.GetFailed(), ex);
        }
    }
}
