namespace Venue.TimeCafe.Application.CQRS.ResourceGroups.DTOs;

public record ResourceGroupDto(Guid ResourceGroupId, string Name, string? Description, int Capacity, bool IsActive);
