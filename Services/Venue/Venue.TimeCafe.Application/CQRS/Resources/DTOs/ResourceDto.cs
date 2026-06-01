namespace Venue.TimeCafe.Application.CQRS.Resources.DTOs;

public record ResourceDto(Guid ResourceId, Guid ResourceGroupId, string Name, int Capacity, bool IsActive);
