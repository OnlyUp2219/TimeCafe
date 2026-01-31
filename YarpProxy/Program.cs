using BuildingBlocks.Authorization;

var builder = WebApplication.CreateBuilder(args);

var sharedSettingsPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "appsettings.shared.json"));
builder.Configuration.AddJsonFile(sharedSettingsPath, optional: true, reloadOnChange: true);

builder.Services.AddSpaCorsConfiguration();

builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddYarpProxy(builder.Configuration);
builder.Services.AddScalarConfiguration();

builder.Services.AddAuthenticationConfiguration(builder.Configuration);
// Register permission authorization in proxy so YARP can evaluate permission-based policies on routes.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddPermissionAuthorizationDev();
}
else
{
    // TODO : Register real IPermissionService here
}


var app = builder.Build();

app.UseSpaCorsConfiguration();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.UseScalarConfiguration();

app.MapGet("/", () => "Hello World!");
app.MapGet("/health", () => Results.Ok("OK"));

app.MapPost("/dev/admin-token", (HttpContext httpContext, IConfiguration configuration) =>
{
    var jwtSection = configuration.GetSection("Jwt");
    var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
    var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
    var signingKey = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

    var userId = "00000000-0000-0000-0000-000000000001";
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

await app.RunAsync();

public partial class Program { }
