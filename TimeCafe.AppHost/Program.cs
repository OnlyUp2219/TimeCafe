using Microsoft.Extensions.DependencyInjection;
using Serilog;

var currentDir = Directory.GetCurrentDirectory();
var envFilePath = Path.Combine(currentDir, ".env");
if (!File.Exists(envFilePath))
{
    var rootDir = Directory.GetParent(currentDir)?.FullName;
    if (rootDir != null) envFilePath = Path.Combine(rootDir, ".env");
}

if (File.Exists(envFilePath))
{
    foreach (var line in File.ReadAllLines(envFilePath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim().Trim('"');
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

var builder = DistributedApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.Services.AddHealthChecks();

// Параметры из конфига
var pgUser = builder.AddParameter("pg-user", builder.Configuration["POSTGRES_USER"] ?? throw new InvalidOperationException("POSTGRES_USER not found in configuration"));
var pgPassword = builder.AddParameter("pg-password", builder.Configuration["POSTGRES_PASSWORD"] ?? throw new InvalidOperationException("POSTGRES_PASSWORD not found in configuration"), secret: true);
var rabbitUser = builder.AddParameter("rabbit-user", builder.Configuration["RABBITMQ_USER"] ?? throw new InvalidOperationException("RABBITMQ_USER not found in configuration"));
var rabbitPass = builder.AddParameter("rabbit-password", builder.Configuration["RABBITMQ_PASSWORD"] ?? throw new InvalidOperationException("RABBITMQ_PASSWORD not found in configuration"), secret: true);

// Инфраструктура
var redis = builder.AddRedis("redis")
    .WithImageTag("8.6-alpine")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
redis.WithEndpoint("tcp", e => e.Port = 6379);

var rabbitmq = builder.AddRabbitMQ("rabbitmq", rabbitUser, rabbitPass)
    .WithImageTag("4.2.2-management-alpine")
    .WithManagementPlugin()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
rabbitmq.WithEndpoint("tcp", e => e.Port = 5672);
rabbitmq.WithEndpoint("management", e => e.Port = 15672);

var postgres = builder.AddPostgres("postgres", pgUser, pgPassword)
    .WithImageTag("17.6-alpine")
    .WithPgAdmin()
    .WithDataVolume()
    .WithBindMount("../scripts/init-databases.sql", "/docker-entrypoint-initdb.d/init-databases.sql")
    .WithLifetime(ContainerLifetime.Persistent);
postgres.WithEndpoint("tcp", e => e.Port = 5433);

var elastic = builder.AddElasticsearch("elasticsearch")
    .WithImage("elasticsearch", "docker.elastic.co/elasticsearch/elasticsearch")
    .WithImageTag("9.2.3")
    .WithEnvironment("discovery.type", "single-node")
    .WithEnvironment("xpack.security.enabled", "false")
    .WithEnvironment("ES_JAVA_OPTS", "-Xms256m -Xmx256m")
    .WithContainerRuntimeArgs("--ulimit", "nofile=65536:65536", "--memory", "768m")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
elastic.WithEndpoint("http", e => e.Port = 9200);

builder.AddContainer("kibana", "docker.elastic.co/kibana/kibana", "9.2.3")
    .WithReference(elastic)
    .WithEnvironment("ELASTICSEARCH_HOSTS", elastic.GetEndpoint("http"))
    .WithEnvironment("CSP_STRICT", "false")
    .WithEnvironment("NODE_OPTIONS", "--max-old-space-size=512")
    .WithContainerRuntimeArgs("--memory", "512m")
    .WithHttpEndpoint(port: 5601, targetPort: 5601)
    .WaitFor(elastic);



// Базы данных
var authDb = postgres.AddDatabase("AuthDb", databaseName: "timecafe_auth");
var userProfileDb = postgres.AddDatabase("UserProfileDb", databaseName: "timecafe_userprofile");
var venueDb = postgres.AddDatabase("VenueDb", databaseName: "timecafe_venue");
var billingDb = postgres.AddDatabase("BillingDb", databaseName: "timecafe_billing");
var auditDb = postgres.AddDatabase("AuditDb", databaseName: "timecafe_audit");

// Auth Service
var authApi = builder.AddProject<Projects.Auth_TimeCafe_API>("auth-api")
    .WithHttpEndpoint(name: "api")
    .WithHttpEndpoint(name: "grpc")
    .WithHttpHealthCheck("/health", endpointName: "api");

authApi
    .WithEnvironment("ASPNETCORE_URLS", ReferenceExpression.Create($"http://+:{authApi.GetEndpoint("api").Property(EndpointProperty.TargetPort)};http://+:{authApi.GetEndpoint("grpc").Property(EndpointProperty.TargetPort)}"))
    .WithEnvironment("ASPNETCORE_KESTREL__ENDPOINTS__api__Url", 
        ReferenceExpression.Create($"http://+:{authApi.GetEndpoint("api").Property(EndpointProperty.TargetPort)}"))
    .WithEnvironment("ASPNETCORE_KESTREL__ENDPOINTS__api__Protocols", "Http1")
    .WithEnvironment("ASPNETCORE_KESTREL__ENDPOINTS__grpc__Url", 
        ReferenceExpression.Create($"http://+:{authApi.GetEndpoint("grpc").Property(EndpointProperty.TargetPort)}"))
    .WithEnvironment("ASPNETCORE_KESTREL__ENDPOINTS__grpc__Protocols", "Http2");

ApplyTimeCafeReferences(authApi, authDb, rabbitUser, rabbitPass, authApi);
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
    .WithHttpEndpoint(name: "api")
    .WithHttpHealthCheck("/health", endpointName: "api");
userProfileApi.WithEnvironment("ASPNETCORE_URLS", ReferenceExpression.Create($"http://+:{userProfileApi.GetEndpoint("api").Property(EndpointProperty.TargetPort)}"));
ApplyTimeCafeReferences(userProfileApi, userProfileDb, rabbitUser, rabbitPass, authApi);
userProfileApi
    .WithEnvironment("S3__AccessKey", builder.Configuration["S3_ACCESS_KEY"])
    .WithEnvironment("S3__SecretKey", builder.Configuration["S3_SECRET_KEY"])
    .WithEnvironment("S3__ServiceUrl", builder.Configuration["S3_SERVICE_URL"])
    .WithEnvironment("S3__BucketName", builder.Configuration["S3_BUCKET_NAME"])
    .WithEnvironment("Sightengine__ApiUser", builder.Configuration["SIGHTENGINE_API_USER"])
    .WithEnvironment("Sightengine__ApiSecret", builder.Configuration["SIGHTENGINE_API_SECRET"])
    .WaitFor(userProfileDb).WaitFor(authApi).WaitFor(redis).WaitFor(rabbitmq);

// Billing Service
var billingApi = builder.AddProject<Projects.Billing_TimeCafe_API>("billing-api")
    .WithHttpEndpoint(name: "api")
    .WithHttpHealthCheck("/health", endpointName: "api");
billingApi.WithEnvironment("ASPNETCORE_URLS", ReferenceExpression.Create($"http://+:{billingApi.GetEndpoint("api").Property(EndpointProperty.TargetPort)}"));
ApplyTimeCafeReferences(billingApi, billingDb, rabbitUser, rabbitPass, authApi);
billingApi
    .WithEnvironment("Stripe__PublishableKey", builder.Configuration["STRIPE_PUBLISHABLE_KEY"])
    .WithEnvironment("Stripe__SecretKey", builder.Configuration["STRIPE_SECRET_KEY"])
    .WithEnvironment("Stripe__WebhookSecret", builder.Configuration["STRIPE_WEBHOOK_SECRET"])
    .WaitFor(billingDb).WaitFor(authApi).WaitFor(redis).WaitFor(rabbitmq);

// Venue Service
var venueApi = builder.AddProject<Projects.Venue_TimeCafe_API>("venue-api")
    .WithHttpEndpoint(name: "api")
    .WithHttpHealthCheck("/health", endpointName: "api");
venueApi.WithEnvironment("ASPNETCORE_URLS", ReferenceExpression.Create($"http://+:{venueApi.GetEndpoint("api").Property(EndpointProperty.TargetPort)}"));
ApplyTimeCafeReferences(venueApi, venueDb, rabbitUser, rabbitPass, authApi);
venueApi
    .WithReference(billingApi)
    .WithEnvironment("Services__Billing__BaseUrl", billingApi.GetEndpoint("api"))
    .WaitFor(venueDb).WaitFor(authApi).WaitFor(redis).WaitFor(rabbitmq).WaitFor(billingApi);

// Audit Service
var auditApi = builder.AddProject<Projects.Audit_TimeCafe_API>("audit-api")
    .WithHttpEndpoint(name: "api")
    .WithHttpHealthCheck("/health", endpointName: "api");
auditApi.WithEnvironment("ASPNETCORE_URLS", ReferenceExpression.Create($"http://+:{auditApi.GetEndpoint("api").Property(EndpointProperty.TargetPort)}"));
ApplyTimeCafeReferences(auditApi, auditDb, rabbitUser, rabbitPass, authApi);
auditApi.WaitFor(auditDb).WaitFor(authApi).WaitFor(redis).WaitFor(rabbitmq);

// YARP Proxy
var yarpProxy = builder.AddProject<Projects.YarpProxy>("yarp-proxy")
    .WithReference(authApi)
    .WithReference(userProfileApi)
    .WithReference(venueApi)
    .WithReference(auditApi)
    .WithReference(billingApi)
    .WithReference(redis)
    .WithEnvironment("Redis__ConnectionString", redis)
    .WithEnvironment("Services__Auth", authApi.GetEndpoint("api"))
    .WithEnvironment("Services__AuthGrpc", authApi.GetEndpoint("grpc"))
    .WithEnvironment("Services__UserProfile", userProfileApi.GetEndpoint("api"))
    .WithEnvironment("Services__Venue", venueApi.GetEndpoint("api"))
    .WithEnvironment("Services__Billing", billingApi.GetEndpoint("api"))
    .WithEnvironment("Services__Audit", auditApi.GetEndpoint("api"))
    .WithEnvironment("ReverseProxy__Clusters__auth__Destinations__destination1__Address", authApi.GetEndpoint("api"))
    .WithEnvironment("ReverseProxy__Clusters__userprofile__Destinations__destination1__Address", userProfileApi.GetEndpoint("api"))
    .WithEnvironment("ReverseProxy__Clusters__venue__Destinations__destination1__Address", venueApi.GetEndpoint("api"))
    .WithEnvironment("ReverseProxy__Clusters__billing__Destinations__destination1__Address", billingApi.GetEndpoint("api"))
    .WithEnvironment("ReverseProxy__Clusters__audit__Destinations__destination1__Address", auditApi.GetEndpoint("api"))
    .WithHttpEndpoint(port: 8010, name: "api")
    .WithHttpsEndpoint(port: 8011, name: "api-secure")
    .WithHttpHealthCheck("/health", endpointName: "api")
    .WithExternalHttpEndpoints()
    .WaitFor(authApi)
    .WaitFor(userProfileApi)
    .WaitFor(venueApi)
    .WaitFor(billingApi)
    .WaitFor(auditApi);
yarpProxy.WithEnvironment("ASPNETCORE_URLS", ReferenceExpression.Create($"http://+:{yarpProxy.GetEndpoint("api").Property(EndpointProperty.TargetPort)}"));

var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "latest")
    .WithBindMount("../infra/prometheus/prometheus.yml", "/etc/prometheus/prometheus.yml.tmpl")
    .WithEntrypoint("sh")
    .WithArgs("-c", "sed -e \"s/\\${AUTH_PORT}/$AUTH_PORT/g\" -e \"s/\\${PROFILE_PORT}/$PROFILE_PORT/g\" -e \"s/\\${VENUE_PORT}/$VENUE_PORT/g\" -e \"s/\\${BILLING_PORT}/$BILLING_PORT/g\" -e \"s/\\${AUDIT_PORT}/$AUDIT_PORT/g\" -e \"s/\\${YARP_PORT}/$YARP_PORT/g\" /etc/prometheus/prometheus.yml.tmpl > /etc/prometheus/prometheus.yml && /bin/prometheus --config.file=/etc/prometheus/prometheus.yml --storage.tsdb.path=/prometheus")
    .WithEnvironment("AUTH_PORT", authApi.GetEndpoint("api").Property(EndpointProperty.Port))
    .WithEnvironment("PROFILE_PORT", userProfileApi.GetEndpoint("api").Property(EndpointProperty.Port))
    .WithEnvironment("VENUE_PORT", venueApi.GetEndpoint("api").Property(EndpointProperty.Port))
    .WithEnvironment("BILLING_PORT", billingApi.GetEndpoint("api").Property(EndpointProperty.Port))
    .WithEnvironment("AUDIT_PORT", auditApi.GetEndpoint("api").Property(EndpointProperty.Port))
    .WithEnvironment("YARP_PORT", yarpProxy.GetEndpoint("api").Property(EndpointProperty.Port))
    .WithHttpEndpoint(port: 9090, targetPort: 9090);

builder.AddContainer("grafana", "grafana/grafana", "latest")
    .WithHttpEndpoint(port: 3000, targetPort: 3000)
    .WithBindMount("../infra/grafana/provisioning", "/etc/grafana/provisioning")
    .WithEnvironment("GF_AUTH_ANONYMOUS_ENABLED", "true")
    .WithEnvironment("GF_AUTH_ANONYMOUS_ORG_ROLE", "Viewer")
    .WithEnvironment("GF_SECURITY_ALLOW_EMBEDDING", "true")
    .WithEnvironment("PROMETHEUS_URL", "http://host.docker.internal:9090")
    .WaitFor(prometheus);

var customApiUrl = Environment.GetEnvironmentVariable("VITE_API_BASE_URL");
var frontend = builder.AddJavaScriptApp("frontend", "../WebApp/timecafe.react.ui")
    .WithReference(yarpProxy)
    .WithHttpEndpoint(port: 9301, isProxied: false)
    .WithHttpsEndpoint(port: 9302)
    .WithExternalHttpEndpoints()
    .WithEnvironment("BROWSER", "none");

if (!string.IsNullOrEmpty(customApiUrl))
{
    frontend.WithEnvironment("VITE_API_BASE_URL", customApiUrl);
}
else
{
    frontend.WithEnvironment("VITE_API_BASE_URL", yarpProxy.GetEndpoint("api"));
}

await builder.Build().RunAsync();

// Вспомогательная функция для проброса конфигов в стиле TimeCafe
void ApplyTimeCafeReferences(
    IResourceBuilder<ProjectResource> projectBuilder,
    IResourceBuilder<PostgresDatabaseResource> db,
    IResourceBuilder<ParameterResource> rabbitUserParam,
    IResourceBuilder<ParameterResource> rabbitPassParam,
    IResourceBuilder<ProjectResource>? authApiRef = null)
{
    projectBuilder
        .WithReference(db, "DefaultConnection")
        .WithReference(redis)
        .WithReference(rabbitmq)
        .WithEnvironment("Redis__ConnectionString", redis)
        .WithEnvironment("RabbitMQ__ConnectionString", rabbitmq)
        .WithEnvironment("RabbitMQ__Host", rabbitmq.GetEndpoint("tcp").Property(EndpointProperty.Host))
        .WithEnvironment("RabbitMQ__Port", rabbitmq.GetEndpoint("tcp").Property(EndpointProperty.Port))
        .WithEnvironment("RabbitMQ__Username", rabbitUserParam)
        .WithEnvironment("RabbitMQ__Password", rabbitPassParam)
        .WithEnvironment("Elasticsearch__NodeUris", elastic.GetEndpoint("http"));

    if (authApiRef != null)
    {
        projectBuilder
            .WithReference(authApiRef)
            .WithEnvironment("Services__Auth", authApiRef.GetEndpoint("api"))
            .WithEnvironment("Services__AuthGrpc", authApiRef.GetEndpoint("grpc"));
    }
}
