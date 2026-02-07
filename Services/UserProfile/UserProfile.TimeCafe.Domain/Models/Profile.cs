using System.ComponentModel.DataAnnotations;

namespace UserProfile.TimeCafe.Domain.Models;

public class Profile
{

    [Key]
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    public DateOnly? BirthDate { get; set; }
    //TODO: migration db, change DateTimetype on DateTimeOffset
    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    public Gender Gender { get; set; } = Gender.NotSpecified;

    [Required]
    public ProfileStatus ProfileStatus { get; set; }

    [MaxLength(500)]
    public string? BanReason { get; set; }
}

public enum Gender : byte
{
    NotSpecified = 0,
    Male = 1,
    Female = 2
}

public enum ProfileStatus : byte
{
    Pending = 0,
    Completed = 1,
    Banned = 2
}