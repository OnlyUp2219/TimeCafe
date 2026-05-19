using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Billing.TimeCafe.API.Services;

public class StripeCliRunner : IHostedService
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<StripeCliRunner> _logger;
    private Process? _process;

    public StripeCliRunner(IHostEnvironment env, ILogger<StripeCliRunner> logger)
    {
        _env = env;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
        {
            return Task.CompletedTask;
        }

        Task.Run(() =>
        {
            try
            {
                var stripePath = "stripe";
                var specificPath = @"D:\ЗАГРУЗКИ\Stripe\stripe.exe";
                if (File.Exists(specificPath))
                {
                    stripePath = specificPath;
                }

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
}
