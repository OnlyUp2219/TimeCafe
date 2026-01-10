namespace Billing.TimeCafe.API.DTOs;

public record GetTransactionHistoryDto
{
    public string UserId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetTransactionHistoryDtoExample : IExamplesProvider<GetTransactionHistoryDto>
{
    public GetTransactionHistoryDto GetExamples()
    {
        return new GetTransactionHistoryDto
        {
            UserId = "f47ac10b-58cc-4372-a567-0e02b2c3d479",
            Page = 1,
            PageSize = 10
        };
    }
}
