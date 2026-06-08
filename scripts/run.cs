using System;
using System.Diagnostics;
using System.IO;

string rootPath = Path.GetFullPath(Directory.GetCurrentDirectory());
if (rootPath.EndsWith("scripts") || rootPath.EndsWith("scripts\\") || rootPath.EndsWith("scripts/"))
{
    rootPath = Path.GetFullPath(Path.Combine(rootPath, ".."));
}

while (true)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("========================================");
    Console.WriteLine("       TimeCafe Scripts Launcher        ");
    Console.WriteLine("========================================");
    Console.ResetColor();
    Console.WriteLine("1. Запуск Cloudflare туннелей и AppHost");
    Console.WriteLine("2. Миграции и обновление БД (EF Core)");
    Console.WriteLine("3. Очистка локальных БД (Truncate)");
    Console.WriteLine("4. Очистка БД в Docker контейнере");
    Console.WriteLine("5. Проверка здоровья Prometheus и Grafana");
    Console.WriteLine("0. Выход");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("========================================");
    Console.Write("Выберите пункт меню: ");
    Console.ResetColor();

    var input = Console.ReadLine();
    if (input == "0") break;

    string scriptName = input switch
    {
        "1" => "RunTunnel.cs",
        "2" => "Migration.cs",
        "3" => "DbTruncateLocal.cs",
        "4" => "DbTruncateDocker.cs",
        "5" => "TestPrometheusGrafana.cs",
        _ => null
    };

    if (scriptName == null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nНеверный выбор. Нажмите любую клавишу для повтора...");
        Console.ResetColor();
        Console.ReadKey();
        continue;
    }

    string scriptPath = Path.Combine(rootPath, "scripts", scriptName);
    if (!File.Exists(scriptPath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nФайл скрипта не найден: {scriptPath}");
        Console.ResetColor();
        Console.ReadKey();
        continue;
    }

    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"--- Запуск скрипта: {scriptName} ---\n");
    Console.ResetColor();

    try
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run \"{scriptPath}\"",
            UseShellExecute = false
        };
        using var proc = Process.Start(startInfo);
        proc.WaitForExit();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Ошибка при запуске скрипта: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nВыполнение завершено. Нажмите любую клавишу для продолжения...");
    Console.ReadKey();
}
