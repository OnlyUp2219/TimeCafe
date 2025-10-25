namespace UserProfile.TimeCafe.Application.CQRS.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly TimeSpan _warningThreshold = TimeSpan.FromMilliseconds(500);

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.Elapsed > _warningThreshold)
        {
            _logger.LogWarning("Performance: {RequestName} took {Elapsed}ms", typeof(TRequest).Name, sw.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogDebug("Performance: {RequestName} {Elapsed}ms", typeof(TRequest).Name, sw.ElapsedMilliseconds);
        }
        return response;
    }
}