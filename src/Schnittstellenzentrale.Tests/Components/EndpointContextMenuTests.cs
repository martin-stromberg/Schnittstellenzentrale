using Bunit;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Tests.Components;

public class EndpointContextMenuTests : BunitContext
{
    private static Core.Models.Endpoint CreateEndpoint() => new()
    {
        Id = 42,
        Name = "TestEndpoint",
        ApplicationId = 1
    };

    [Fact]
    public void LöschenEintrag_LöstCallbackAus()
    {
        var endpoint = CreateEndpoint();
        Core.Models.Endpoint? received = null;

        var cut = Render<EndpointContextMenu>(p => p
            .Add(x => x.Endpoint, endpoint)
            .Add(x => x.OnDeleteRequested, (Core.Models.Endpoint e) => received = e));

        cut.Find(".context-menu-toggle").Click();
        cut.FindAll("button.context-menu-item")
            .First(b => b.TextContent.Contains("Endpunkt löschen"))
            .Click();

        Assert.Equal(endpoint, received);
        Assert.Empty(cut.FindAll(".context-menu-dropdown"));
    }
}
