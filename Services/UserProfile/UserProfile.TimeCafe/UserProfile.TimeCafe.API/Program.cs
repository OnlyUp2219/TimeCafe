var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddRabbitMqMessaging(builder.Configuration);
builder.Services.AddRedis(builder.Configuration);

builder.Services.AddDbContext<ApplicationDbContext>(op => op.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepositories, UserRepositories>();
builder.Services.AddScoped<IAdditionalInfoRepository, AdditionalInfoRepository>();

// CQRS + behaviors
builder.Services.AddUserProfileCqrs();

// S3 storage (photos)
builder.Services.AddS3(builder.Configuration);

// Photo moderation
builder.Services.AddHttpClient<IPhotoModerationService, SightenginePhotoModerationService>();

// Swagger & Carter
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration(builder.Configuration);
builder.Services.AddCarter();


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeCafe UserProfile API v1");
        c.RoutePrefix = string.Empty;
    });
    app.UseScalarConfiguration();
}

app.MapCarter();

await app.RunAsync();


public partial class Program { }


