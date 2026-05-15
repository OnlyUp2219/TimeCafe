namespace Venue.TimeCafe.Application.DTOs;

public record CostBreakdownDto
{
    public int ActualMinutes { get; init; }
    public int BillableMinutes { get; init; }
    public decimal BaseCost { get; init; }
    public decimal FinalCost { get; init; }
    public decimal OptimizationGain { get; init; }
}
