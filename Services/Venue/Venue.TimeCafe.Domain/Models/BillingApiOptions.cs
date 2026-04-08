using System.ComponentModel.DataAnnotations;

namespace Venue.TimeCafe.Domain.Models;

public sealed class BillingApiOptions
{
    [Required]
    [Url]
    public string BaseUrl { get; set; } = null!;
}