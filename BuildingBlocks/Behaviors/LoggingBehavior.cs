namespace BuildingBlocks.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        jsonOptions.Converters.Add(new StreamLoggingConverter());
        string payload = string.Empty;
        try
        {
            payload = JsonSerializer.Serialize(request, jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to serialize request payload for {RequestName}", requestName);
        }

        _logger.LogInformation("Handling {RequestName} Payload={Payload}", requestName, payload);
        var response = await next();
        string responseJson = string.Empty;
        try
        {
            responseJson = JsonSerializer.Serialize(response, jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to serialize response for {RequestName}", requestName);
        }
        _logger.LogInformation("Handled {RequestName} Response={Response}", requestName, responseJson);
        return response;
    }
}

public class StreamLoggingConverter : JsonConverter<Stream>
{
    public override Stream? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => null;

    public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
        => writer.WriteStringValue("<stream>");
}