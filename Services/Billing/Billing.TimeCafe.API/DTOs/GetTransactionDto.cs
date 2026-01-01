namespace Billing.TimeCafe.API.DTOs;

public record GetTransactionDto
{
    public Guid TransactionId { get; set; }
}

public class GetTransactionDtoExample : IExamplesProvider<GetTransactionDto>
{
    public GetTransactionDto GetExamples()
    {
        return new GetTransactionDto
        {
            TransactionId = Guid.Parse("a47ac10b-58cc-4372-a567-0e02b2c3d480")
        };
    }
}
