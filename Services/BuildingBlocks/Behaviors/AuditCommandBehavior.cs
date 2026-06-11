namespace BuildingBlocks.Behaviors;

public static class AuditCommandContext
{
    private static readonly AsyncLocal<string?> _currentCommandName = new();
    private static readonly AsyncLocal<Guid?> _currentUserId = new();

    public static string? CurrentCommandName
    {
        get => _currentCommandName.Value;
        set => _currentCommandName.Value = value;
    }

    public static Guid? CurrentUserId
    {
        get => _currentUserId.Value;
        set => _currentUserId.Value = value;
    }
}

public class AuditCommandBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        AuditCommandContext.CurrentCommandName = request.GetType().Name;

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User != null)
        {
            AuditCommandContext.CurrentUserId = httpContext.User.TryGetUserId();
        }

        try
        {
            return await next();
        }
        finally
        {
            AuditCommandContext.CurrentCommandName = null;
            AuditCommandContext.CurrentUserId = null;
        }
    }
}

