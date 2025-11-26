namespace Auth.TimeCafe.Domain.Models;

public class PhoneVerificationResult
{
    public string PhoneNumber { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

