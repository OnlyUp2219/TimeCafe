namespace Venue.TimeCafe.Domain.Models;

public class Resource
{
    public Resource()
    {
        ResourceId = Guid.NewGuid();
    }

    public Resource(Guid resourceId)
    {
        ResourceId = resourceId;
    }

    public Guid ResourceId { get; set; }
    public Guid ResourceGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;

    public static Resource Create(Guid? id, Guid resourceGroupId, string name, int capacity, bool isActive = true)
    {
        return new Resource
        {
            ResourceId = id ?? Guid.NewGuid(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Capacity = capacity,
            IsActive = isActive
        };
    }
}
