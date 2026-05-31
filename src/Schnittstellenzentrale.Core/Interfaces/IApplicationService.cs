using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IApplicationService
{
    Task UpdateNameAsync(int applicationId, string name);
    Task UpdateSubtitleAsync(int applicationId, string? subtitle);
    Task UpdateIconAsync(int applicationId, byte[] iconData);
}
