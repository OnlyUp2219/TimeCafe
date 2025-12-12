using Venue.TimeCafe.Domain.DTOs;

namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetTariffByIdQuery(string TariffId) : IRequest<GetTariffByIdResult>;

public record GetTariffByIdResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    TariffWithThemeDto? Tariff = null) : ICqrsResultV2
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
        RuleFor(x => x.TariffId)
              .NotEmpty().WithMessage("Тариф не найден")
              .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Тариф не найден")
              .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Тариф не найден");
    }
}

public class GetTariffByIdQueryHandler(ITariffRepository repository) : IRequestHandler<GetTariffByIdQuery, GetTariffByIdResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<GetTariffByIdResult> Handle(GetTariffByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffId = Guid.Parse(request.TariffId);    

            var tariff = await _repository.GetByIdAsync(tariffId);

            if (tariff == null)
                return GetTariffByIdResult.TariffNotFound();

            return GetTariffByIdResult.GetSuccess(tariff);
        }
        catch (Exception)
        {
            return GetTariffByIdResult.GetFailed();
        }
    }
}
