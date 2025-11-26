namespace Main.TimeCafe.Application.CQRS.BillingTypes.Get;

public record GetBillingTypeByIdQuery(int BillingTypeId) : IRequest<BillingType?>;

public class GetBillingTypeByIdHandler(
    IBillingTypeRepository repository) : IRequestHandler<GetBillingTypeByIdQuery, BillingType?>
{
    private readonly IBillingTypeRepository _repository = repository;

    public async Task<BillingType?> Handle(GetBillingTypeByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetBillingTypeByIdAsync(request.BillingTypeId);
    }
}