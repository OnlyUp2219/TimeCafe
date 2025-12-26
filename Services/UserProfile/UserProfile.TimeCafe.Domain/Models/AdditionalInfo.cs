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
    //TODO: migration db, change DateTimetype on DateTimeOffset

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(450)]
    public string? CreatedBy { get; set; }
}
