using System;
using System.Diagnostics;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=== Truncating local databases ===");
Console.ResetColor();

string dbUser = "admin";
string dbPass = "Admin123!";
string dbHost = "localhost";
string dbPort = "5433";
string[] databases = { "timecafe_auth", "timecafe_profile", "timecafe_venue", "timecafe_billing", "timecafe_audit" };

string truncateSql = @"
DO $$ 
DECLARE 
    r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public' AND tablename <> '__EFMigrationsHistory') LOOP
        EXECUTE 'TRUNCATE TABLE ' || quote_ident(r.tablename) || ' CASCADE';
    END FOR;
END $$;
";

foreach (var db in databases)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"Truncating database: {db}...");
    Console.ResetColor();

    try
    {
        using var process = new Process();
        process.StartInfo.FileName = "psql";
        process.StartInfo.Arguments = $"-h {dbHost} -p {dbPort} -U {dbUser} -d {db} -c \"{truncateSql.Replace("\"", "\\\"")}\"";
        process.StartInfo.EnvironmentVariables["PGPASSWORD"] = dbPass;
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
            Console.WriteLine($"Error truncating {db}: {error}");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error executing psql: {ex.Message}");
    }
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\nAll databases truncated locally!");
Console.ResetColor();
