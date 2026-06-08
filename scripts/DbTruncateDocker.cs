using System;
using System.Diagnostics;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=== Truncating databases inside Docker ===");
Console.ResetColor();

string containerName = "timecafe-postgres";
string dbUser = "admin";
string dbPass = "Admin123!";
string[] databases = { "Auth.TimeCafe", "UserProfile.TimeCafe", "Venue.TimeCafe", "Billing.TimeCafe" };

string truncateSql = "DO $$ DECLARE r RECORD; BEGIN FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public' AND tablename <> '__EFMigrationsHistory') LOOP EXECUTE 'TRUNCATE TABLE ' || quote_ident(r.tablename) || ' CASCADE'; END FOR; END $$;";

foreach (var db in databases)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"Truncating database in Docker: {db}...");
    Console.ResetColor();

    try
    {
        using var process = new Process();
        process.StartInfo.FileName = "docker";
        process.StartInfo.Arguments = $"exec -e PGPASSWORD={dbPass} {containerName} psql -U {dbUser} -d {db} -c \"{truncateSql}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Success: {db}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error truncating {db} in Docker: {error}");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error executing docker command: {ex.Message}");
    }
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\nAll databases truncated in Docker!");
Console.ResetColor();
