using System;
using System.Diagnostics;
using System.IO;

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
Console.ResetColor();

var services = new[]
{
    new { Name = "Auth", Infra = "Services/Auth/Auth.TimeCafe.Infrastructure", Api = "Services/Auth/Auth.TimeCafe.API" },
    new { Name = "Billing", Infra = "Services/Billing/Billing.TimeCafe.Infrastructure", Api = "Services/Billing/Billing.TimeCafe.API" },
    new { Name = "Venue", Infra = "Services/Venue/Venue.TimeCafe.Infrastructure", Api = "Services/Venue/Venue.TimeCafe.API" },
    new { Name = "UserProfile", Infra = "Services/UserProfile/UserProfile.TimeCafe.Infrastructure", Api = "Services/UserProfile/UserProfile.TimeCafe.API" },
    new { Name = "Audit", Infra = "Services/Audit/Audit.TimeCafe.Infrastructure", Api = "Services/Audit/Audit.TimeCafe.API" }
};

var env = new System.Collections.Generic.Dictionary<string, string>();
var currentDir = Directory.GetCurrentDirectory();
var envPath = Path.Combine(currentDir, ".env");
if (!File.Exists(envPath))
{
    envPath = Path.Combine(Directory.GetParent(currentDir)?.FullName ?? currentDir, ".env");
}

if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            env[parts[0].Trim()] = parts[1].Trim().Trim('"');
        }
    }
}

var pgUser = env.TryGetValue("POSTGRES_USER", out var user) ? user : "admin";
var pgPassword = env.TryGetValue("POSTGRES_PASSWORD", out var pass) ? pass : "Admin123!";
var pgPort = "5433"; 

foreach (var service in services)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n--- Checking service: {service.Name} ---");
    Console.ResetColor();

    if (!Directory.Exists(service.Infra))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] Path not found: {service.Infra}");
        Console.ResetColor();
        continue;
    }

    var dbName = service.Name switch
    {
        "Auth" => env.TryGetValue("AUTH_DB", out var d) ? d : "timecafe_auth",
        "UserProfile" => env.TryGetValue("PROFILE_DB", out var d) ? d : "timecafe_profile",
        "Venue" => env.TryGetValue("VENUE_DB", out var d) ? d : "timecafe_venue",
        "Billing" => env.TryGetValue("BILLING_DB", out var d) ? d : "timecafe_billing",
        "Audit" => env.TryGetValue("AUDIT_DB", out var d) ? d : "timecafe_audit",
        _ => $"timecafe_{service.Name.ToLower()}"
    };
    var connectionString = $"Host=localhost;Port={pgPort};Database={dbName};Username={pgUser};Password={pgPassword}";

    Console.WriteLine($"Updating database for {service.Name}...");

    try
    {
        using var process = new Process();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.Arguments = $"ef database update --project {service.Infra} --startup-project {service.Api}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.EnvironmentVariables["ConnectionStrings__DefaultConnection"] = connectionString;
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] Failed to update database for {service.Name}. Process exited with code {process.ExitCode}.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCCESS] Database updated for {service.Name}.");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error running ef database update: {ex.Message}");
        Console.ResetColor();
    }
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\nFinished!");
Console.ResetColor();
