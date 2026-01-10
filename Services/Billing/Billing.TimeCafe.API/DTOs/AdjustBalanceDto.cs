namespace Billing.TimeCafe.API.DTOs;

public record AdjustBalanceDto
{
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Type { get; set; }
    public int Source { get; set; }
    public string? SourceId { get; set; }
    public string? Comment { get; set; }
}

public class AdjustBalanceDtoExample : IExamplesProvider<AdjustBalanceDto>
{
    public AdjustBalanceDto GetExamples()
    {
        return new AdjustBalanceDto
        {
            UserId = "f47ac10b-58cc-4372-a567-0e02b2c3d479",
            Amount = 1000m,
            Type = 1,
            Source = 2,
            SourceId = null,
            Comment = "Пополнение баланса через мобильное приложение"
        };
    }
}
