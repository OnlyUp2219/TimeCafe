namespace BuildingBlocks.Utilities;

public class HeaderPropagationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
        {
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader!);
        }
        else if (context.Request.Cookies.TryGetValue("Access-Token", out var cookieToken) && !string.IsNullOrEmpty(cookieToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookieToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
