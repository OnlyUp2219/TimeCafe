namespace UserProfile.TimeCafe.Domain.Models;

public class UserProfile
{
    public string UserId { get; set; } 
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? MiddleName { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string AccessCardNumber { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum Gender : byte
{
    NotSpecified = 0,
    Male = 1,
    Female = 2
}