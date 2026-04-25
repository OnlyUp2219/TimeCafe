using Serilog;

var builder = DistributedApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());


// Инфраструктура
var redis = builder.AddRedis("redis");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var postgres = builder.AddPostgres("postgres")
    .WithEndpoint(endpointName: "tcp", callback: endpoint => endpoint.Port = 5433)
    .WithPgAdmin();

var elastic = builder.AddElasticsearch("elasticsearch")
    .WithEndpoint("http", endpoint => endpoint.Port = 9200)
    .WithDataBindMount("./elasticsearch_data")
    .WithEnvironment("discovery.type", "single-node")
    .WithEnvironment("xpack.security.enabled", "false");

// Базы данных
var authDb = postgres.AddDatabase("AuthDb", databaseName: "timecafe_auth");
var userProfileDb = postgres.AddDatabase("UserProfileDb", databaseName: "timecafe_userprofile");
var venueDb = postgres.AddDatabase("VenueDb", databaseName: "timecafe_venue");
var billingDb = postgres.AddDatabase("BillingDb", databaseName: "timecafe_billing");

// Вспомогательная функция для проброса конфигов в стиле TimeCafe
void ApplyTimeCafeReferences(IResourceBuilder<ProjectResource> projectBuilder, IResourceBuilder<PostgresDatabaseResource> db)
{
    projectBuilder
        .WithReference(db, "DefaultConnection")
        .WithReference(authDb, "AuthConnection")
        .WithReference(redis)
        .WithReference(rabbitmq)
        .WithReference(elastic)
        .WithEnvironment("Redis__ConnectionString", redis)
        .WithEnvironment("RabbitMQ__ConnectionString", rabbitmq)
        .WithEnvironment("RabbitMQ__Username", "guest")
        .WithEnvironment("RabbitMQ__Password", "guest")
        .WithEnvironment("Elasticsearch__NodeUris", elastic.GetEndpoint("http"));
}

// Auth Service
var authApi = builder.AddProject<Projects.Auth_TimeCafe_API>("auth-api")
    .WithHttpEndpoint(port: 8001, name: "api");
ApplyTimeCafeReferences(authApi, authDb);
authApi
    .WithEnvironment("Authentication__Google__ClientId", builder.Configuration["GOOGLE_CLIENT_ID"])
    .WithEnvironment("Authentication__Google__ClientSecret", builder.Configuration["GOOGLE_CLIENT_SECRET"])
    .WithEnvironment("Authentication__Microsoft__ClientId", builder.Configuration["MICROSOFT_CLIENT_ID"])
    .WithEnvironment("Authentication__Microsoft__ClientSecret", builder.Configuration["MICROSOFT_CLIENT_SECRET"])
    .WithEnvironment("Twilio__AccountSid", builder.Configuration["TWILIO_ACCOUNT_SID"])
    .WithEnvironment("Twilio__AuthToken", builder.Configuration["TWILIO_AUTH_TOKEN"])
    .WithEnvironment("Twilio__TwilioPhoneNumber", builder.Configuration["TWILIO_PHONE_NUMBER"])
    .WithEnvironment("Postmark__ServerToken", builder.Configuration["POSTMARK_SERVER_TOKEN"])
    .WithEnvironment("Postmark__FromEmail", builder.Configuration["POSTMARK_FROM_EMAIL"])
    .WaitFor(authDb).WaitFor(redis).WaitFor(rabbitmq);

// User Profile Service
var userProfileApi = builder.AddProject<Projects.UserProfile_TimeCafe_API>("userprofile-api")
    .WithHttpEndpoint(port: 8002, name: "api");
ApplyTimeCafeReferences(userProfileApi, userProfileDb);
userProfileApi
    .WithEnvironment("S3__AccessKey", builder.Configuration["S3_ACCESS_KEY"])
    .WithEnvironment("S3__SecretKey", builder.Configuration["S3_SECRET_KEY"])
    .WithEnvironment("S3__ServiceUrl", builder.Configuration["S3_SERVICE_URL"])
    .WithEnvironment("S3__BucketName", builder.Configuration["S3_BUCKET_NAME"])
    .WithEnvironment("Sightengine__ApiUser", builder.Configuration["SIGHTENGINE_API_USER"])
    .WithEnvironment("Sightengine__ApiSecret", builder.Configuration["SIGHTENGINE_API_SECRET"])
    .WaitFor(userProfileDb).WaitFor(authDb).WaitFor(redis).WaitFor(rabbitmq);

// Venue Service
var venueApi = builder.AddProject<Projects.Venue_TimeCafe_API>("venue-api")
    .WithHttpEndpoint(port: 8003, name: "api");
ApplyTimeCafeReferences(venueApi, venueDb);
venueApi.WaitFor(venueDb).WaitFor(authDb).WaitFor(redis).WaitFor(rabbitmq);

// Billing Service
var billingApi = builder.AddProject<Projects.Billing_TimeCafe_API>("billing-api")
    .WithHttpEndpoint(port: 8004, name: "api");
ApplyTimeCafeReferences(billingApi, billingDb);
billingApi
    .WithEnvironment("Stripe__PublishableKey", builder.Configuration["STRIPE_PUBLISHABLE_KEY"])
    .WithEnvironment("Stripe__SecretKey", builder.Configuration["STRIPE_SECRET_KEY"])
    .WithEnvironment("Stripe__WebhookSecret", builder.Configuration["STRIPE_WEBHOOK_SECRET"])
    .WaitFor(billingDb).WaitFor(authDb).WaitFor(redis).WaitFor(rabbitmq);

// YARP Proxy
var yarpProxy = builder.AddProject<Projects.YarpProxy>("yarp-proxy")
    .WithReference(authApi)
    .WithReference(userProfileApi)
    .WithReference(venueApi)
    .WithReference(billingApi)
    .WithReference(redis)
    .WithReference(authDb, "AuthConnection")
    .WithEnvironment("Redis__ConnectionString", redis)
    .WithEnvironment("Services__Auth", authApi.GetEndpoint("api"))
    .WithEnvironment("Services__UserProfile", userProfileApi.GetEndpoint("api"))
    .WithEnvironment("Services__Billing", billingApi.GetEndpoint("api"))
    .WithEnvironment("Services__Venue", venueApi.GetEndpoint("api"))
    .WithEnvironment("ReverseProxy__Clusters__auth__Destinations__destination1__Address", authApi.GetEndpoint("api"))
    .WithEnvironment("ReverseProxy__Clusters__userprofile__Destinations__destination1__Address", userProfileApi.GetEndpoint("api"))
    .WithEnvironment("ReverseProxy__Clusters__venue__Destinations__destination1__Address", venueApi.GetEndpoint("api"))
    .WithEnvironment("ReverseProxy__Clusters__billing__Destinations__destination1__Address", billingApi.GetEndpoint("api"))
    .WithHttpEndpoint(port: 8010, name: "api")
    .WithExternalHttpEndpoints()
    .WaitFor(authApi)
    .WaitFor(userProfileApi)
    .WaitFor(venueApi)
    .WaitFor(billingApi);

builder.AddJavaScriptApp("frontend", "../WebApp/timecafe.react.ui")
    .WithReference(yarpProxy)
    .WithHttpEndpoint(port: 9301, isProxied: false)
    .WithExternalHttpEndpoints()
    .WithEnvironment("BROWSER", "none")
    .WithEnvironment("VITE_API_URL", yarpProxy.GetEndpoint("api")); 

await builder.Build().RunAsync();
