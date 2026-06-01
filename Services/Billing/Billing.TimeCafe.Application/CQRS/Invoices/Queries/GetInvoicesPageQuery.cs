namespace Billing.TimeCafe.Application.CQRS.Invoices.Queries;

public record GetInvoicesPageQuery(int Page, int PageSize, Guid? UserId = null) : IQuery<GetInvoicesPageResponse>;

public class GetInvoicesPageQueryValidator : AbstractValidator<GetInvoicesPageQuery>
{
    public GetInvoicesPageQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Страница должна быть больше 0");
        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Размер страницы должен быть больше 0");
    }
}

public record GetInvoicesPageResponse(List<Invoice> Invoices, int TotalCount, int TotalPages);

public class GetInvoicesPageQueryHandler(IUnitOfWork uow) : IQueryHandler<GetInvoicesPageQuery, GetInvoicesPageResponse>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<GetInvoicesPageResponse>> Handle(GetInvoicesPageQuery request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _uow.Invoices.GetPageAsync(request.Page, request.PageSize, request.UserId, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return Result.Ok(new GetInvoicesPageResponse(items, totalCount, totalPages));
    }
}
