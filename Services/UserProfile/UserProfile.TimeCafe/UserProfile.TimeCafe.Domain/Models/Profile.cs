using System.ComponentModel.DataAnnotations;

namespace UserProfile.TimeCafe.Domain.Models;

public class Profile
{
    [Key]
    public string UserId { get; set; } = null!;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }


    public string? AccessCardNumber { get; set; }
    public string? PhotoUrl { get; set; }
    public DateOnly? BirthDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Gender Gender { get; set; } = Gender.NotSpecified;
    public ProfileStatus ProfileStatus { get; set; }
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