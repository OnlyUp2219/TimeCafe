namespace BuildingBlocks.Behaviors;

public static class AuditCommandContext
{
    private static readonly AsyncLocal<string?> _currentCommandName = new();

    public static string? CurrentCommandName
    {
        get => _currentCommandName.Value;
        set => _currentCommandName.Value = value;
    }
}

public class AuditCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        AuditCommandContext.CurrentCommandName = request.GetType().Name;

        try
        {
            return await next();
        }
        finally
        {
            AuditCommandContext.CurrentCommandName = null;
        }
    }
}
