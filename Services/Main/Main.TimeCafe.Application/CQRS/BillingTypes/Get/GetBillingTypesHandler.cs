namespace Main.TimeCafe.Application.CQRS.BillingTypes.Get;

public record GetBillingTypesQuery : IRequest<IEnumerable<BillingType>>;

public class GetBillingTypesHandler(
    IBillingTypeRepository repository) : IRequestHandler<GetBillingTypesQuery, IEnumerable<BillingType>>
{
    private readonly IBillingTypeRepository _repository = repository;

    public async Task<IEnumerable<BillingType>> Handle(GetBillingTypesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetBillingTypesAsync();
    }
}
