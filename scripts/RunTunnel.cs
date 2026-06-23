using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

string rootPath = Path.GetFullPath(Directory.GetCurrentDirectory());
if (rootPath.EndsWith("scripts") || rootPath.EndsWith("scripts\\") || rootPath.EndsWith("scripts/"))
{
    rootPath = Path.GetFullPath(Path.Combine(rootPath, ".."));
}

string envPath = Path.Combine(rootPath, ".env");
string cloudflaredPath = Path.Combine(rootPath, "scripts", "cloudflared.exe");
string backendLog = Path.Combine(rootPath, "scripts", "cf_backend.log");
string frontendLog = Path.Combine(rootPath, "scripts", "cf_frontend.log");

if (File.Exists(backendLog)) File.Delete(backendLog);
if (File.Exists(frontendLog)) File.Delete(frontendLog);

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Запуск туннеля для backend (порт 8010)...");
Console.ResetColor();

var backendStartInfo = new ProcessStartInfo
{
    FileName = cloudflaredPath,
    Arguments = "tunnel --url http://127.0.0.1:8010",
    UseShellExecute = false,
    RedirectStandardError = true,
    CreateNoWindow = true
};
var backendProc = new Process { StartInfo = backendStartInfo };
var backendWriter = new StreamWriter(backendLog);
backendProc.ErrorDataReceived += (s, e) => { if (e.Data != null) { backendWriter.WriteLine(e.Data); backendWriter.Flush(); } };
backendProc.Start();
backendProc.BeginErrorReadLine();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Запуск туннеля для frontend (порт 9301)...");
Console.ResetColor();

var frontendStartInfo = new ProcessStartInfo
{
    FileName = cloudflaredPath,
    Arguments = "tunnel --url http://127.0.0.1:9301",
    UseShellExecute = false,
    RedirectStandardError = true,
    CreateNoWindow = true
};
var frontendProc = new Process { StartInfo = frontendStartInfo };
var frontendWriter = new StreamWriter(frontendLog);
frontendProc.ErrorDataReceived += (s, e) => { if (e.Data != null) { frontendWriter.WriteLine(e.Data); frontendWriter.Flush(); } };
frontendProc.Start();
frontendProc.BeginErrorReadLine();

string backendUrl = null;
string frontendUrl = null;
var rx = new Regex(@"https://[a-zA-Z0-9\-]+\.trycloudflare\.com");

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Ожидание создания туннелей Cloudflare...");
Console.ResetColor();

for (int i = 0; i < 30; i++)
{
    Thread.Sleep(1000);
    
    string ReadLogContent(string path)
    {
        try
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                return sr.ReadToEnd();
            }
        }
        catch { return ""; }
    }

    if (backendUrl == null && File.Exists(backendLog))
    {
        string content = ReadLogContent(backendLog);
        var match = rx.Match(content);
        if (match.Success)
        {
            backendUrl = match.Value;
        }
    }
    
    if (frontendUrl == null && File.Exists(frontendLog))
    {
        string content = ReadLogContent(frontendLog);
        var match = rx.Match(content);
        if (match.Success)
        {
            frontendUrl = match.Value;
        }
    }
    
    if (backendUrl != null && frontendUrl != null)
    {
        break;
    }
}

if (backendUrl == null || frontendUrl == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Не удалось получить ссылки на туннели Cloudflare за 30 секунд.");
    Console.ResetColor();
    Cleanup();
    Environment.Exit(1);
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"\nТуннели успешно созданы!");
Console.WriteLine($"Бэкенд URL:  {backendUrl}");
Console.WriteLine($"Фронтенд URL: {frontendUrl}");
Console.ResetColor();

if (File.Exists(envPath))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\nОбновление .env параметров для туннелей...");
    Console.ResetColor();
    
    var lines = File.ReadAllLines(envPath);
    var list = new System.Collections.Generic.List<string>(lines);
    
    void UpdateOrAdd(string key, string value)
    {
        bool found = false;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].StartsWith($"{key}="))
            {
                list[i] = $"{key}={value}";
                found = true;
                break;
            }
        }
        if (!found)
        {
            list.Add($"{key}={value}");
        }
    }
    
    UpdateOrAdd("VITE_API_BASE_URL", backendUrl);
    UpdateOrAdd("CORS_EXTRA_ORIGINS", frontendUrl);
    
    File.WriteAllLines(envPath, list);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Файл .env обновлен!");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Файл .env не найден по пути: {envPath}");
    Console.ResetColor();
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\nВыберите режим запуска платформы:");
Console.WriteLine("1. Debug   (быстрый запуск, HMR для фронтенда, отладка бэкенда) [По умолчанию]");
Console.WriteLine("2. Release (оптимизированный бэкенд, сжатый production-бандл фронтенда)");
Console.Write("Ваш выбор (1/2): ");
Console.ResetColor();

var modeInput = Console.ReadLine();
bool isRelease = modeInput == "2";

if (isRelease)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n[Release] Сборка production-бандла фронтенда (npm run build)...");
    Console.ResetColor();

    try
    {
        var npmBuildInfo = new ProcessStartInfo
        {
            FileName = "cmd",
            Arguments = "/c npm run build",
            WorkingDirectory = Path.Combine(rootPath, "WebApp", "timecafe.react.ui"),
            UseShellExecute = false
        };
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            npmBuildInfo.FileName = "npm";
            npmBuildInfo.Arguments = "run build";
        }

        using var npmProc = Process.Start(npmBuildInfo);
        npmProc?.WaitForExit();
    }
    catch (Exception buildEx)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Не удалось собрать фронтенд: {buildEx.Message}");
        Console.ResetColor();
    }
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(isRelease ? "\nЗапуск проекта TimeCafe.AppHost в режиме [Release]..." : "\nЗапуск проекта TimeCafe.AppHost в режиме [Debug]...");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Туннели будут работать в фоне. Для остановки нажмите Ctrl + C\n");
Console.ResetColor();

Console.CancelKeyPress += (s, e) => {
    Cleanup();
};

try
{
    var appHostStartInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = isRelease
            ? $"run -c Release --project \"{Path.Combine(rootPath, "TimeCafe.AppHost")}\""
            : $"run --project \"{Path.Combine(rootPath, "TimeCafe.AppHost")}\"",
        UseShellExecute = false
    };

    if (isRelease)
    {
        appHostStartInfo.EnvironmentVariables["TimeCafe__AppHostRelease"] = "true";
    }

    using var appHostProc = Process.Start(appHostStartInfo);
    appHostProc.WaitForExit();
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка запуска AppHost: {ex.Message}");
}
finally
{
    Cleanup();
}

void Cleanup()
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\nОстановка туннелей Cloudflare...");
    Console.ResetColor();
    
    try { backendWriter.Close(); } catch {}
    try { frontendWriter.Close(); } catch {}
    
    try { if (!backendProc.HasExited) backendProc.Kill(); } catch {}
    try { if (!frontendProc.HasExited) frontendProc.Kill(); } catch {}
    
    try { if (File.Exists(backendLog)) File.Delete(backendLog); } catch {}
    try { if (File.Exists(frontendLog)) File.Delete(frontendLog); } catch {}
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Все процессы остановлены!");
    Console.ResetColor();
}
