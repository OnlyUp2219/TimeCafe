var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddRabbitMqMessaging(builder.Configuration);
builder.Services.AddRedis(builder.Configuration);

builder.Services.AddDbContext<ApplicationDbContext>(op => op.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepositories, UserRepositories>();


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.RunAsync();


