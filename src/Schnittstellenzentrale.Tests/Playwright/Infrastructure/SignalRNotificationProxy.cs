using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>
/// Leitet alle Notification-Aufrufe an einen austauschbaren Inner-Service weiter.
/// Ermöglicht es, die PlaywrightApiFactory zunächst mit einem No-Op zu starten
/// und nach dem App-Start auf den echten IHubContext des Kestrel-Servers umzustellen.
/// </summary>
internal sealed class SignalRNotificationProxy : ISignalRNotificationService
{
    private volatile ISignalRNotificationService _inner = NullNotificationService.Instance;

    public void SetInner(ISignalRNotificationService inner) => _inner = inner;

    public Task NotifyTreeChangedAsync() => _inner.NotifyTreeChangedAsync();
    public Task NotifyApplicationChangedAsync(int applicationId) => _inner.NotifyApplicationChangedAsync(applicationId);
    public Task NotifyGroupChangedAsync(int groupId) => _inner.NotifyGroupChangedAsync(groupId);
    public Task NotifyEndpointChangedAsync(int endpointId, int applicationId) => _inner.NotifyEndpointChangedAsync(endpointId, applicationId);
    public Task NotifyEndpointGroupChangedAsync(int endpointGroupId, int applicationId) => _inner.NotifyEndpointGroupChangedAsync(endpointGroupId, applicationId);
    public Task NotifyEnvironmentChangedAsync() => _inner.NotifyEnvironmentChangedAsync();

    private sealed class NullNotificationService : ISignalRNotificationService
    {
        public static readonly NullNotificationService Instance = new();
        public Task NotifyTreeChangedAsync() => Task.CompletedTask;
        public Task NotifyApplicationChangedAsync(int applicationId) => Task.CompletedTask;
        public Task NotifyGroupChangedAsync(int groupId) => Task.CompletedTask;
        public Task NotifyEndpointChangedAsync(int endpointId, int applicationId) => Task.CompletedTask;
        public Task NotifyEndpointGroupChangedAsync(int endpointGroupId, int applicationId) => Task.CompletedTask;
        public Task NotifyEnvironmentChangedAsync() => Task.CompletedTask;
    }
}
