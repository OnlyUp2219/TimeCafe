using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserProfile.TimeCafe.Domain.Models;

public class AdditionalInfo
{
    [Key]
    public Guid InfoId { get; set; }

    [Required]
    [ForeignKey(nameof(Profile))]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string InfoText { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? CreatedBy { get; set; }
}
