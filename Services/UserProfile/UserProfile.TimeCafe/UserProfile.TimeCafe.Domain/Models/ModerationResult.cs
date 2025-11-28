namespace UserProfile.TimeCafe.Domain.Models;

public record ModerationResult(
    bool IsSafe,
    string? Reason,
    Dictionary<string, double>? Scores
);
