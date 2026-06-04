using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für die <see cref="EndpointGroupContextMenu"/>-Komponente.</summary>
public class EndpointGroupContextMenuTests : BunitContext
{
    /// <summary>Registriert den FakeStringLocalizer für IStringLocalizer&lt;SharedResources&gt;.</summary>
    public EndpointGroupContextMenuTests()
    {
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    private static EndpointGroup CreateGroup() => new()
    {
        Id = 7,
        Name = "TestGroup",
        ApplicationId = 1
    };

    /// <summary>Klick auf „Endpunkt anlegen" löst den entsprechenden Callback aus.</summary>
    [Fact]
    public void EndpunktAnlegen_LöstCallbackAus()
    {
        var group = CreateGroup();
        EndpointGroup? received = null;

        var cut = Render<EndpointGroupContextMenu>(p => p
            .Add(x => x.Group, group)
            .Add(x => x.OnCreateEndpointRequested, (EndpointGroup g) => received = g));

        cut.Find(".context-menu-toggle").Click();
        cut.FindAll("button.context-menu-item")
            .First(b => b.TextContent.Contains("EndpointGroupContextMenu_CreateEndpointButton"))
            .Click();

        Assert.Equal(group, received);
    }

    /// <summary>Klick auf „Ordner umbenennen" löst den entsprechenden Callback aus.</summary>
    [Fact]
    public void OrdnerUmbenennen_LöstCallbackAus()
    {
        var group = CreateGroup();
        EndpointGroup? received = null;

        var cut = Render<EndpointGroupContextMenu>(p => p
            .Add(x => x.Group, group)
            .Add(x => x.OnRenameEndpointGroupRequested, (EndpointGroup g) => received = g));

        cut.Find(".context-menu-toggle").Click();
        cut.FindAll("button.context-menu-item")
            .First(b => b.TextContent.Contains("EndpointGroupContextMenu_RenameButton"))
            .Click();

        Assert.Equal(group, received);
    }

    /// <summary>Klick auf „Ordner löschen" löst den entsprechenden Callback aus.</summary>
    [Fact]
    public void OrdnerLöschen_LöstCallbackAus()
    {
        var group = CreateGroup();
        EndpointGroup? received = null;

        var cut = Render<EndpointGroupContextMenu>(p => p
            .Add(x => x.Group, group)
            .Add(x => x.OnDeleteEndpointGroupRequested, (EndpointGroup g) => received = g));

        cut.Find(".context-menu-toggle").Click();
        cut.FindAll("button.context-menu-item")
            .First(b => b.TextContent.Contains("EndpointGroupContextMenu_DeleteButton"))
            .Click();

        Assert.Equal(group, received);
    }
}
