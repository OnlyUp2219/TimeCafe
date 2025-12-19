using Venue.TimeCafe.Application.Contracts.Repositories;

namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetTariffsPageQuery(int PageNumber, int PageSize) : IRequest<GetTariffsPageResult>;

public record GetTariffsPageResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<TariffWithThemeDto>? Tariffs = null,
    int TotalCount = 0) : ICqrsResultV2
{
    public static GetTariffsPageResult GetFailed() =>
        new(false, Code: "GetTariffsPageFailed", Message: "Не удалось получить страницу тарифов", StatusCode: 500);

    public static GetTariffsPageResult GetSuccess(IEnumerable<TariffWithThemeDto> tariffs, int totalCount) =>
        new(true, Tariffs: tariffs, TotalCount: totalCount);
}

public class GetTariffsPageQueryValidator : AbstractValidator<GetTariffsPageQuery>
{
    public GetTariffsPageQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Номер страницы должен быть больше 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Размер страницы должен быть больше 0")
            .LessThanOrEqualTo(100).WithMessage("Размер страницы не может превышать 100");
    }
}

public class GetTariffsPageQueryHandler(ITariffRepository repository) : IRequestHandler<GetTariffsPageQuery, GetTariffsPageResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<GetTariffsPageResult> Handle(GetTariffsPageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffs = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
            var totalCount = await _repository.GetTotalCountAsync();

            return GetTariffsPageResult.GetSuccess(tariffs, totalCount);
        }
        catch (Exception)
        {
            return GetTariffsPageResult.GetFailed();
        }
    }
}
