using Bunit;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für die <see cref="EndpointGroupContextMenu"/>-Komponente.</summary>
public class EndpointGroupContextMenuTests : BunitContext
{
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
            .First(b => b.TextContent.Contains("Endpunkt anlegen"))
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
            .First(b => b.TextContent.Contains("Ordner umbenennen"))
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
            .First(b => b.TextContent.Contains("Ordner löschen"))
            .Click();

        Assert.Equal(group, received);
    }
}
