using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Billing.TimeCafe.API.Services;

public class StripeCliRunner : IHostedService
{
    private readonly IHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<StripeCliRunner> _logger;
    private Process? _process;

    public StripeCliRunner(IHostEnvironment env, IConfiguration config, ILogger<StripeCliRunner> logger)
    {
        _env = env;
        _config = config;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment() || _config.GetValue<bool>("Stripe:DisableCliRunner") || _config.GetValue<bool>("Stripe__DisableCliRunner"))
        {
            _logger.LogInformation("Stripe CLI automatic listener is disabled (Non-development mode or disabled via configuration).");
            return Task.CompletedTask;
        }

        Task.Run(() =>
        {
            try
            {
                var stripePath = ResolveStripePath();
                _logger.LogInformation("Starting Stripe CLI automatic listener from {Path}...", stripePath);

                var startInfo = new ProcessStartInfo
                {
                    FileName = stripePath,
                    Arguments = "listen --forward-to http://localhost:8010/billing/payments/webhook/stripe",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _process = new Process { StartInfo = startInfo };
                _process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _logger.LogInformation("[Stripe CLI] {Log}", e.Data);
                    }
                };
                _process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _logger.LogWarning("[Stripe CLI Warning] {Log}", e.Data);
                    }
                };

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                _logger.LogInformation("Stripe CLI listener started successfully in background.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Stripe CLI automatic listener.");
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_process != null && !_process.HasExited)
        {
            try
            {
                _logger.LogInformation("Stopping Stripe CLI automatic listener...");
                _process.Kill(entireProcessTree: true);
                _process.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while stopping Stripe CLI process.");
            }
        }

        return Task.CompletedTask;
    }

    private string ResolveStripePath()
    {
        // 1. Попытка взять из конфигурации
        var configPath = _config["Stripe:CliPath"] ?? _config["Stripe__CliPath"];
        if (!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath))
        {
            return configPath;
        }

        // 2. Попытка найти scripts/stripe.exe относительно корня решения
        var baseDir = AppContext.BaseDirectory;
        var dir = new DirectoryInfo(baseDir);
        while (dir != null)
        {
            var exePath = Path.Combine(dir.FullName, "scripts", "stripe.exe");
            if (File.Exists(exePath))
            {
                return exePath;
            }

            var exePathNoFolder = Path.Combine(dir.FullName, "stripe.exe");
            if (File.Exists(exePathNoFolder))
            {
                return exePathNoFolder;
            }

            if (dir.GetFiles("*.slnx").Any() || dir.GetFiles("*.sln").Any())
            {
                break;
            }

            dir = dir.Parent;
        }

        // 3. Резервный захардкоженный путь из оригинальной версии
        var defaultOldPath = @"D:\ЗАГРУЗКИ\Stripe\stripe.exe";
        if (File.Exists(defaultOldPath))
        {
            return defaultOldPath;
        }

        // 4. Попытка вызвать глобальный stripe из PATH
        return "stripe";
    }
}
