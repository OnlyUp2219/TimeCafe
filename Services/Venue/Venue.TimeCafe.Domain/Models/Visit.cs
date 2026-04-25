namespace Venue.TimeCafe.Domain.Models;

public class Visit
{
    public Visit()
    {
        VisitId = Guid.NewGuid();
    }

    public Visit(Guid visitId)
    {
        VisitId = visitId;
    }

    public Guid VisitId { get; set; }
    public Guid UserId { get; set; }
    public Guid TariffId { get; set; }
    public DateTimeOffset EntryTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExitTime { get; set; }
    public decimal? CalculatedCost { get; set; }
    public VisitStatus Status { get; set; } = VisitStatus.Active;

    public static Visit Create(Guid? visitId, Guid userId, Guid tariffId, DateTimeOffset entryTime, VisitStatus status, DateTimeOffset? exitTime = null, decimal? calculatedCost = null)
    {
        return new Visit
        {
            VisitId = visitId ?? Guid.NewGuid(),
            UserId = userId,
            TariffId = tariffId,
            EntryTime = entryTime,
            ExitTime = exitTime,
            CalculatedCost = calculatedCost,
            Status = status
        };
    }

    public static Visit Update(Visit existingVisit, Guid? userId = null, Guid? tariffId = null, DateTimeOffset? entryTime = null, DateTimeOffset? exitTime = null, decimal? calculatedCost = null, VisitStatus? status = null)
    {
        return new Visit(existingVisit.VisitId)
        {
            UserId = userId ?? existingVisit.UserId,
            TariffId = tariffId ?? existingVisit.TariffId,
            EntryTime = entryTime ?? existingVisit.EntryTime,
            ExitTime = exitTime ?? existingVisit.ExitTime,
            CalculatedCost = calculatedCost ?? existingVisit.CalculatedCost,
            Status = status ?? existingVisit.Status
        };
    }


    public static decimal CalculateCost(BillingType tariffBillingType, decimal tariffPricePerMinute, DateTimeOffset exitTime, DateTimeOffset entryTime, decimal maxDiscountPercent = 50m, decimal globalDiscount = 0m, decimal tariffDiscount = 0m, decimal personalDiscount = 0m)
    {
        var duration = (exitTime - entryTime).TotalMinutes;

        var baseCost = tariffBillingType == BillingType.Hourly
                ? (decimal)Math.Ceiling(duration / 60) * tariffPricePerMinute * 60
                : (decimal)Math.Ceiling(duration) * tariffPricePerMinute;

        var bestPromotion = Math.Max(globalDiscount, tariffDiscount);
        var totalDiscount = Math.Min(bestPromotion + personalDiscount, maxDiscountPercent);

        return baseCost * (1m - (totalDiscount / 100m));
    }
}

