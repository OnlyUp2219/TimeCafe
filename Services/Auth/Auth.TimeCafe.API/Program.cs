using Auth.TimeCafe.Infrastructure.Data;
using Auth.TimeCafe.Infrastructure.Services;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity
builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        options.User.RequireUniqueEmail = true;

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 0;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<RussianIdentityErrorDescriber>();

builder.Services.AddScoped<IPasswordValidator<IdentityUser>, CustomPasswordValidator>();

//builder.Services.Configure<PostmarkOptions>(builder.Configuration.GetSection("Postmark"));
//builder.Services.AddHttpClient();
builder.Services.AddSingleton<IEmailSender<IdentityUser>, NullEmailSender>();



// Authentication: JWT + external providers
builder.Services.AddScoped<IJwtService, JwtService>();
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var signingKey = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey missing");
var keyBytes = Encoding.UTF8.GetBytes(signingKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    })
    .AddCookie(IdentityConstants.ExternalScheme)
    .AddGoogle(op =>
    {
        var google = builder.Configuration.GetSection("Authentication:Google");
        op.ClientId = google["ClientId"];
        op.ClientSecret = google["ClientSecret"];
        op.CallbackPath = "/signin-google";
    })
    .AddMicrosoftAccount(op =>
    {
        var ms = builder.Configuration.GetSection("Authentication:Microsoft");
        op.ClientId = ms["ClientId"];
        op.ClientSecret = ms["ClientSecret"];
        op.CallbackPath = "/signin-microsoft";
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TimeCafe Auth API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS 
var corsPolicyName = "react-client";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, p =>
        p.AllowAnyHeader().
        AllowAnyMethod().
        AllowCredentials().
        WithOrigins("http://127.0.0.1:9301", 
        "http://localhost:9301"));
});

// Carter
builder.Services.AddCarter();

var app = builder.Build();



// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeCafe Auth API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();

// Встроенные Identity API эндпоинты
app.MapIdentityApi<IdentityUser>();

// Внешние логины
app.MapControllers();


app.Run();


