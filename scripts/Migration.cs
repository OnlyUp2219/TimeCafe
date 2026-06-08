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

    Console.WriteLine($"Creating migration for {service.Name}...");

    try
    {
        using var process = new Process();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.Arguments = $"ef migrations add AddMassTransitOutbox --project {service.Infra} --startup-project {service.Api}";
        process.StartInfo.UseShellExecute = false;
        process.Start();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            Console.WriteLine($"Updating database for {service.Name}...");
            using var updateProcess = new Process();
            updateProcess.StartInfo.FileName = "dotnet";
            updateProcess.StartInfo.Arguments = $"ef database update --project {service.Infra} --startup-project {service.Api}";
            updateProcess.StartInfo.UseShellExecute = false;
            updateProcess.Start();
            updateProcess.WaitForExit();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Failed to create migration for {service.Name}.");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error running ef tools: {ex.Message}");
        Console.ResetColor();
    }
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\nFinished!");
Console.ResetColor();
