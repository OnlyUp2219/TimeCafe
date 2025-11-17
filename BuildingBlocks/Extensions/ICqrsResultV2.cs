namespace BuildingBlocks.Extensions;

public interface ICqrsResultV2
{
    bool Success { get; }
    int? StatusCode { get; }
    string? Code { get; }
    string? Message { get; }
    List<ErrorItem>? Errors { get; }
}

public record ErrorItem(string Code, string Description);