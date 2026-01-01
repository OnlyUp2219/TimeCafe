namespace Billing.TimeCafe.API.DTOs;

public record GetBalanceDto
{
    public Guid UserId { get; set; }
}

public class GetBalanceDtoExample : IExamplesProvider<GetBalanceDto>
{
    public GetBalanceDto GetExamples()
    {
        return new GetBalanceDto
        {
            UserId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479")
        };
    }
}
