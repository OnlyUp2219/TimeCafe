using Microsoft.EntityFrameworkCore;
using UserProfile.TimeCafe.API.Middleware;
using UserProfile.TimeCafe.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRabbitMqMessaging(builder.Configuration);

builder.Services.AddDbContext<ApplicationDbContext>(op => op.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepositories, UserRepositories>();


var app = builder.Build();

app.Run();


