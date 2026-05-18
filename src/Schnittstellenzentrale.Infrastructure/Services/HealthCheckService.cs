#pragma warning disable CS1591
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HealthCheckService> _logger;
    private readonly int _cooldownSeconds;
    private readonly Dictionary<int, DateTime> _lastCheckTimes = new();
    private readonly object _lock = new();

    public HealthCheckService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<HealthCheckService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _cooldownSeconds = configuration.GetValue<int>("HealthCheck:CooldownSeconds", 60);
    }

    public async Task<bool?> CheckAsync(Application application)
    {
        lock (_lock)
        {
            if (_lastCheckTimes.TryGetValue(application.Id, out var lastCheck))
            {
                if ((DateTime.UtcNow - lastCheck).TotalSeconds < _cooldownSeconds)
                    return null;
            }
            _lastCheckTimes[application.Id] = DateTime.UtcNow;
        }

        var url = application.InterfaceUrl ?? application.BaseUrl;
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Health-Check fehlgeschlagen für Anwendung {ApplicationId} ({Url})", application.Id, url);
            return false;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Health-Check Timeout für Anwendung {ApplicationId} ({Url})", application.Id, url);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health-Check unerwarteter Fehler für Anwendung {ApplicationId} ({Url})", application.Id, url);
            return false;
        }
    }
}
