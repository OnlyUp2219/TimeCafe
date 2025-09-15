namespace TimeCafe.Core.Models;

public partial class Tariff
{
    public int TariffId { get; set; }
    public string TariffName { get; set; }
    public string? DescriptionTitle { get; set; }
    public string? Description { get; set; }
    public byte[] Icon { get; set; }
    public int? ThemeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public decimal Price { get; set; }
    public int BillingTypeId { get; set; }
    public virtual BillingType BillingType { get; set; }
    public virtual Theme Theme { get; set; }
    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
