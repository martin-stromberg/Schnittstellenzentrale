using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IHealthCheckService
{
    Task<bool?> CheckAsync(Application application);
}
