namespace TimeCafe.Application.CQRS.BillingTypes.Get;
public record GetBillingTypeByIdQuery(int BillingTypeId) : IRequest<BillingType?>;

public class GetBillingTypeByIdHandler : IRequestHandler<GetBillingTypeByIdQuery, BillingType?>
{
    private readonly IBillingTypeRepository _repository;

    public GetBillingTypeByIdHandler(
        IBillingTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<BillingType?> Handle(GetBillingTypeByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetBillingTypeByIdAsync(request.BillingTypeId);
    }
}