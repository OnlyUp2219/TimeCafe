namespace UserProfile.TimeCafe.Application.CQRS.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        string payload = string.Empty;
        try
        {
            payload = JsonSerializer.Serialize(request);
        }
        catch { }

        _logger.LogInformation("Handling {RequestName} Payload={Payload}", requestName, payload);
        var response = await next();
        string responseJson = string.Empty;
        try
        {
            responseJson = JsonSerializer.Serialize(response);
        }
        catch { }
        _logger.LogInformation("Handled {RequestName} Response={Response}", requestName, responseJson);
        return response;
    }
}