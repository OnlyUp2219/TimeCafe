var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddSharedConfiguration();

var corsPolicyName = builder.Services.AddCorsConfiguration(builder.Configuration);

builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddYarpProxy(builder.Configuration);
builder.Services.AddScalarConfiguration();

builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddRedis(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BuildingBlocks.Utilities.HeaderPropagationHandler>();

builder.Services.AddHttpClient("Auth", client =>
{
    var url = builder.Configuration["Services:Auth"] ?? throw new InvalidOperationException("Services:Auth is missing");
    client.BaseAddress = new Uri(url);
})
.AddHttpMessageHandler<BuildingBlocks.Utilities.HeaderPropagationHandler>();

builder.Services.AddHttpClient("UserProfile", client =>
{
    var url = builder.Configuration["Services:UserProfile"] ?? throw new InvalidOperationException("Services:UserProfile is missing");
    client.BaseAddress = new Uri(url);
})
.AddHttpMessageHandler<BuildingBlocks.Utilities.HeaderPropagationHandler>();

builder.Services.AddHttpClient("Billing", client =>
{
    var url = builder.Configuration["Services:Billing"] ?? throw new InvalidOperationException("Services:Billing is missing");
    client.BaseAddress = new Uri(url);
})
.AddHttpMessageHandler<BuildingBlocks.Utilities.HeaderPropagationHandler>();

builder.Services.AddCarter();

var app = builder.Build();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();
app.MapCarter();

app.UseScalarConfiguration();

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapPost("/dev/admin-token", (HttpContext httpContext, IOptionsSnapshot<JwtOptions> jwtOptionsAccessor) =>
    {
        var jwtOptions = jwtOptionsAccessor.Value;
        var issuer = jwtOptions.Issuer;
        var audience = jwtOptions.Audience;
        var signingKey = jwtOptions.SigningKey;

        const string userId = "00000000-0000-0000-0000-000000000001";
        var now = DateTime.UtcNow;
        var expires = now.AddHours(12);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("sub", userId),
            new(ClaimTypes.Role, "admin")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256));

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        httpContext.Response.Cookies.Append(
            "Access-Token",
            jwt,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = expires
            });

        return Results.Ok(new
        {
            token = jwt,
            userId,
            role = "admin",
            expiresAtUtc = expires
        });
    }).AllowAnonymous();
}


await app.RunAsync();

public partial class Program;
