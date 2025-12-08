namespace UserProfile.TimeCafe.Domain.DTOs;

public record PhotoUploadDto(bool Success, string? Key = null, string? Url = null, long? Size = null, string? ContentType = null);
public record PhotoStreamDto(Stream Stream, string ContentType);
