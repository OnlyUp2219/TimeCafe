
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiConfiguration();
builder.Services.AddCarterConfiguration();
builder.Services.AddVenueCqrs();
builder.Services.AddVenueDatabase(builder.Configuration);

var app = builder.Build();

app.UseScalarConfiguration();

app.MapCarter();
app.MapControllers();

app.Run();


