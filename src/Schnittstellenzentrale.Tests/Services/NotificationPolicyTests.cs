using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>Unit-Tests für die zentrale <see cref="NotificationPolicy"/> (Team-Gate und Policy-Mapping).</summary>
public class NotificationPolicyTests
{
    private readonly Mock<ISignalRNotificationService> _innerMock = new();
    private readonly NotificationPolicy _policy;

    /// <summary>Initialisiert die Tests mit einem gemockten Transport.</summary>
    public NotificationPolicyTests()
    {
        _policy = new NotificationPolicy(_innerMock.Object);
    }

    /// <summary>NotifyTreeMutationAsync_Team_CallsTreeChanged</summary>
    [Fact]
    public async Task NotifyTreeMutationAsync_Team_CallsTreeChanged()
    {
        await _policy.NotifyTreeMutationAsync(StorageMode.Team);

        _innerMock.Verify(s => s.NotifyTreeChangedAsync(), Times.Once);
        Assert.Single(_innerMock.Invocations);
    }

    /// <summary>NotifyTreeMutationAsync_User_SendsNothing</summary>
    [Fact]
    public async Task NotifyTreeMutationAsync_User_SendsNothing()
    {
        await _policy.NotifyTreeMutationAsync(StorageMode.User);

        Assert.Empty(_innerMock.Invocations);
    }

    /// <summary>NotifyEndpointMutationAsync_Team_CallsEndpointChanged</summary>
    [Fact]
    public async Task NotifyEndpointMutationAsync_Team_CallsEndpointChanged()
    {
        await _policy.NotifyEndpointMutationAsync(StorageMode.Team, 7, 3);

        _innerMock.Verify(s => s.NotifyEndpointChangedAsync(7, 3), Times.Once);
        Assert.Single(_innerMock.Invocations);
    }

    /// <summary>NotifyEndpointMutationAsync_User_SendsNothing</summary>
    [Fact]
    public async Task NotifyEndpointMutationAsync_User_SendsNothing()
    {
        await _policy.NotifyEndpointMutationAsync(StorageMode.User, 7, 3);

        Assert.Empty(_innerMock.Invocations);
    }

    /// <summary>NotifyEndpointGroupMutationAsync_Team_CallsEndpointGroupChanged</summary>
    [Fact]
    public async Task NotifyEndpointGroupMutationAsync_Team_CallsEndpointGroupChanged()
    {
        await _policy.NotifyEndpointGroupMutationAsync(StorageMode.Team, 11, 4);

        _innerMock.Verify(s => s.NotifyEndpointGroupChangedAsync(11, 4), Times.Once);
        Assert.Single(_innerMock.Invocations);
    }

    /// <summary>NotifyEndpointGroupMutationAsync_User_SendsNothing</summary>
    [Fact]
    public async Task NotifyEndpointGroupMutationAsync_User_SendsNothing()
    {
        await _policy.NotifyEndpointGroupMutationAsync(StorageMode.User, 11, 4);

        Assert.Empty(_innerMock.Invocations);
    }

    /// <summary>NotifyBackgroundMutationAsync_AlwaysSendsTreeChanged</summary>
    [Fact]
    public async Task NotifyBackgroundMutationAsync_AlwaysSendsTreeChanged()
    {
        await _policy.NotifyBackgroundMutationAsync();

        _innerMock.Verify(s => s.NotifyTreeChangedAsync(), Times.Once);
        Assert.Single(_innerMock.Invocations);
    }
}
