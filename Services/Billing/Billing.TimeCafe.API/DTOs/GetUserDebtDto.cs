namespace Billing.TimeCafe.API.DTOs;

public record GetUserDebtDto
{
    public string UserId { get; set; } = string.Empty;
}

public class GetUserDebtDtoExample : IExamplesProvider<GetUserDebtDto>
{
    public GetUserDebtDto GetExamples()
    {
        return new GetUserDebtDto
        {
            UserId = "f47ac10b-58cc-4372-a567-0e02b2c3d479"
        };
    }
}
