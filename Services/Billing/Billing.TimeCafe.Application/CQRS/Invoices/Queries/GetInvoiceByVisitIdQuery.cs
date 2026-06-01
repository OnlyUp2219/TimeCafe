namespace Billing.TimeCafe.Application.CQRS.Invoices.Queries;

public record GetInvoiceByVisitIdQuery(Guid VisitId) : IQuery<Invoice>;

public class GetInvoiceByVisitIdQueryValidator : AbstractValidator<GetInvoiceByVisitIdQuery>
{
    public GetInvoiceByVisitIdQueryValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Некорректный VisitId");
    }
}

public class GetInvoiceByVisitIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetInvoiceByVisitIdQuery, Invoice>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<Invoice>> Handle(GetInvoiceByVisitIdQuery request, CancellationToken cancellationToken = default)
    {
        var invoice = await _uow.Invoices.GetByVisitIdAsync(request.VisitId, cancellationToken);
        if (invoice == null)
            return Result.Fail<Invoice>(new InvoiceNotFoundError());

        return Result.Ok(invoice);
    }
}
