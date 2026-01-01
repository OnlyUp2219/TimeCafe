namespace Billing.TimeCafe.API.DTOs;

public record GetUserDebtDto
{
    public Guid UserId { get; set; }
}

public class GetUserDebtDtoExample : IExamplesProvider<GetUserDebtDto>
{
    public GetUserDebtDto GetExamples()
    {
        return new GetUserDebtDto
        {
            UserId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479")
        };
    }
}
