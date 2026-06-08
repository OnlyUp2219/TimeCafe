using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

Console.WriteLine("Testing Prometheus and Grafana...");
using var httpClient = new HttpClient();
httpClient.Timeout = TimeSpan.FromSeconds(5);

string promUrl = "http://localhost:9090/api/v1/targets";
Console.WriteLine($"Fetching Prometheus targets from {promUrl}...");
try
{
    var response = await httpClient.GetStringAsync(promUrl);
    using var doc = JsonDocument.Parse(response);
    var root = doc.RootElement;
    if (root.TryGetProperty("data", out var data) && data.TryGetProperty("activeTargets", out var activeTargets))
    {
        int upCount = 0;
        int downCount = 0;
        int count = activeTargets.GetArrayLength();
        
        foreach (var target in activeTargets.EnumerateArray())
        {
            if (target.TryGetProperty("health", out var health) && health.GetString() == "up")
            {
                upCount++;
            }
            else
            {
                downCount++;
            }
        }
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Prometheus: Found {count} targets. {upCount} UP, {downCount} DOWN.");
        Console.ResetColor();
        
        if (downCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warning: Some targets are down. Prometheus might still be starting or scraping failed:");
            Console.ResetColor();
            
            foreach (var target in activeTargets.EnumerateArray())
            {
                if (target.TryGetProperty("health", out var health) && health.GetString() != "up")
                {
                    string url = target.TryGetProperty("scrapeUrl", out var scrapeUrl) ? scrapeUrl.GetString() : "unknown";
                    string error = target.TryGetProperty("lastError", out var lastError) ? lastError.GetString() : "none";
                    Console.WriteLine($"- Target: {url} | Error: {error}");
                }
            }
        }
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Failed to reach Prometheus: {ex.Message}");
    Console.ResetColor();
}

string grafanaUrl = "http://localhost:3000/api/health";
Console.WriteLine($"Checking Grafana health at {grafanaUrl}...");
try
{
    var response = await httpClient.GetStringAsync(grafanaUrl);
    using var doc = JsonDocument.Parse(response);
    var root = doc.RootElement;
    if (root.TryGetProperty("database", out var dbStatus) && dbStatus.GetString() == "ok")
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Grafana: UP and connected to its database.");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Grafana: Responded but status is not fully ok.");
        Console.ResetColor();
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Failed to reach Grafana: {ex.Message}");
    Console.ResetColor();
}

Console.WriteLine("Test finished.");
