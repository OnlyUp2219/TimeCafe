namespace Billing.TimeCafe.Application.CQRS.Invoices.Queries;

public record GetInvoiceByIdQuery(Guid InvoiceId) : IQuery<Invoice>;

public class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator()
    {
        RuleFor(x => x.InvoiceId).ValidGuidEntityId("Инвойс не найден");
    }
}

public class GetInvoiceByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetInvoiceByIdQuery, Invoice>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<Invoice>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken = default)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice == null)
            return Result.Fail<Invoice>(new InvoiceNotFoundError());

        return Result.Ok(invoice);
    }
}
