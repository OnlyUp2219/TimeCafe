namespace UserProfile.TimeCafe.Domain.DTOs;

public record ModerationResult(
    bool IsSafe,
    string? Reason,
    Dictionary<string, double>? Scores
);
