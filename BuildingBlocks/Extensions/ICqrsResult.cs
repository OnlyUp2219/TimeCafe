namespace BuildingBlocks.Extensions;

public interface ICqrsResult
{
    bool Success { get; }
    string? Message { get; }
    List<string>? Errors { get; }
    ETypeError? TypeError { get; }
}
