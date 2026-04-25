namespace Venue.TimeCafe.Application.DTOs;

public record EndVisitResponse(
    Visit Visit,
    decimal CalculatedCost
);

