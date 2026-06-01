namespace Venue.TimeCafe.Domain.Models;

public class ResourceGroup
{
    public ResourceGroup()
    {
        ResourceGroupId = Guid.NewGuid();
    }

    public ResourceGroup(Guid resourceGroupId)
    {
        ResourceGroupId = resourceGroupId;
    }

    public Guid ResourceGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;

    public static ResourceGroup Create(Guid? id, string name, string? description, int capacity, bool isActive = true)
    {
        return new ResourceGroup
        {
            ResourceGroupId = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            Capacity = capacity,
            IsActive = isActive
        };
    }
}
