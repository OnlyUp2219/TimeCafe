namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetTariffsPageQuery(int PageNumber, int PageSize) : IQuery<GetTariffsPageResponse>;

public record GetTariffsPageResponse(IEnumerable<TariffWithThemeDto> Tariffs, int TotalCount);

public class GetTariffsPageQueryValidator : AbstractValidator<GetTariffsPageQuery>
{
    public GetTariffsPageQueryValidator()
    {
        RuleFor(x => x.PageNumber).ValidPageNumber();

        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetTariffsPageQueryHandler(ITariffRepository repository) : IQueryHandler<GetTariffsPageQuery, GetTariffsPageResponse>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<Result<GetTariffsPageResponse>> Handle(GetTariffsPageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffs = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
            var totalCount = await _repository.GetTotalCountAsync();

            return Result.Ok(new GetTariffsPageResponse(tariffs, totalCount));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

