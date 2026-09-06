using Microsoft.AspNetCore.SignalR;
using Moq;
using Schnittstellenzentrale.Hubs;

namespace Schnittstellenzentrale.Tests.Hubs;

/// <summary>Unit-Tests für Gruppen-Subscriptions im <see cref="EndpointHub"/>.</summary>
public class EndpointHubTests_Subscriptions
{
    /// <summary>Abonnieren/Abmelden für anwendungs- und gruppenbezogene IDs verwendet die erwarteten Gruppennamen.</summary>
    /// <param name="methodName">Name der aufzurufenden Hub-Methode.</param>
    /// <param name="id">Die an die Hub-Methode übergebene ID.</param>
    /// <param name="expectedGroup">Der erwartete SignalR-Gruppenname.</param>
    /// <param name="isSubscribe"><c>true</c> für Abonnieren, <c>false</c> für Abmelden.</param>
    [Theory]
    [InlineData(nameof(EndpointHub.SubscribeToApplication), 42, "application:42", true)]
    [InlineData(nameof(EndpointHub.UnsubscribeFromApplication), 42, "application:42", false)]
    [InlineData(nameof(EndpointHub.SubscribeToGroup), 7, "group:7", true)]
    [InlineData(nameof(EndpointHub.UnsubscribeFromGroup), 7, "group:7", false)]
    public async Task GroupSubscriptions_UseExpectedGroupNames(string methodName, int id, string expectedGroup, bool isSubscribe)
    {
        var contextMock = new Mock<HubCallerContext>();
        contextMock.SetupGet(c => c.ConnectionId).Returns("conn-1");

        var groupManagerMock = new Mock<IGroupManager>();
        groupManagerMock
            .Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        groupManagerMock
            .Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var hub = new EndpointHub
        {
            Context = contextMock.Object,
            Groups = groupManagerMock.Object
        };

        switch (methodName)
        {
            case nameof(EndpointHub.SubscribeToApplication):
                await hub.SubscribeToApplication(id);
                break;
            case nameof(EndpointHub.UnsubscribeFromApplication):
                await hub.UnsubscribeFromApplication(id);
                break;
            case nameof(EndpointHub.SubscribeToGroup):
                await hub.SubscribeToGroup(id);
                break;
            case nameof(EndpointHub.UnsubscribeFromGroup):
                await hub.UnsubscribeFromGroup(id);
                break;
        }

        if (isSubscribe)
        {
            groupManagerMock.Verify(g => g.AddToGroupAsync("conn-1", expectedGroup, default), Times.Once);
            groupManagerMock.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        else
        {
            groupManagerMock.Verify(g => g.RemoveFromGroupAsync("conn-1", expectedGroup, default), Times.Once);
            groupManagerMock.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    /// <summary>Abonnieren/Abmelden für statische Gruppen verwendet die erwarteten Gruppennamen.</summary>
    /// <param name="methodName">Name der aufzurufenden Hub-Methode.</param>
    /// <param name="expectedGroup">Der erwartete SignalR-Gruppenname.</param>
    /// <param name="isSubscribe"><c>true</c> für Abonnieren, <c>false</c> für Abmelden.</param>
    [Theory]
    [InlineData(nameof(EndpointHub.SubscribeToWorkspace), "workspace", true)]
    [InlineData(nameof(EndpointHub.UnsubscribeFromWorkspace), "workspace", false)]
    [InlineData(nameof(EndpointHub.SubscribeToEnvironments), "environments", true)]
    [InlineData(nameof(EndpointHub.UnsubscribeFromEnvironments), "environments", false)]
    public async Task StaticGroupSubscriptions_UseExpectedGroupNames(string methodName, string expectedGroup, bool isSubscribe)
    {
        var contextMock = new Mock<HubCallerContext>();
        contextMock.SetupGet(c => c.ConnectionId).Returns("conn-2");

        var groupManagerMock = new Mock<IGroupManager>();
        groupManagerMock
            .Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        groupManagerMock
            .Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var hub = new EndpointHub
        {
            Context = contextMock.Object,
            Groups = groupManagerMock.Object
        };

        switch (methodName)
        {
            case nameof(EndpointHub.SubscribeToWorkspace):
                await hub.SubscribeToWorkspace();
                break;
            case nameof(EndpointHub.UnsubscribeFromWorkspace):
                await hub.UnsubscribeFromWorkspace();
                break;
            case nameof(EndpointHub.SubscribeToEnvironments):
                await hub.SubscribeToEnvironments();
                break;
            case nameof(EndpointHub.UnsubscribeFromEnvironments):
                await hub.UnsubscribeFromEnvironments();
                break;
        }

        if (isSubscribe)
        {
            groupManagerMock.Verify(g => g.AddToGroupAsync("conn-2", expectedGroup, default), Times.Once);
            groupManagerMock.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        else
        {
            groupManagerMock.Verify(g => g.RemoveFromGroupAsync("conn-2", expectedGroup, default), Times.Once);
            groupManagerMock.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
