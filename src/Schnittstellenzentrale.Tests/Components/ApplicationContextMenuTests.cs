using Bunit;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Tests.Components;

public class ApplicationContextMenuTests : BunitContext
{
    private static Application InGroup() =>
        new() { Id = 1, Name = "TestApp", BaseUrl = "http://app", ApplicationGroupId = 5 };

    private static Application WithoutGroup() =>
        new() { Id = 2, Name = "TestApp", BaseUrl = "http://app", ApplicationGroupId = null };

    [Fact]
    public void AusGruppeEntfernen_NurSichtbar_WennAnwendungInGruppe()
    {
        var cut = Render<ApplicationContextMenu>(p => p
            .Add(x => x.Application, InGroup()));

        cut.Find(".context-menu-toggle").Click();

        Assert.Contains(
            cut.FindAll("button.context-menu-item"),
            b => b.TextContent.Contains("Aus Gruppe entfernen"));
    }

    [Fact]
    public void AusGruppeEntfernen_NichtSichtbar_WennAnwendungOhneGruppe()
    {
        var cut = Render<ApplicationContextMenu>(p => p
            .Add(x => x.Application, WithoutGroup()));

        cut.Find(".context-menu-toggle").Click();

        Assert.DoesNotContain(
            cut.FindAll("button.context-menu-item"),
            b => b.TextContent.Contains("Aus Gruppe entfernen"));
    }

    [Fact]
    public void AusGruppeEntfernen_LöstCallbackAus_UndSchliestMenu()
    {
        var application = InGroup();
        Application? received = null;

        var cut = Render<ApplicationContextMenu>(p => p
            .Add(x => x.Application, application)
            .Add(x => x.OnRemoveFromGroupRequested, (Application app) => received = app));

        cut.Find(".context-menu-toggle").Click();
        cut.FindAll("button.context-menu-item")
            .First(b => b.TextContent.Contains("Aus Gruppe entfernen"))
            .Click();

        Assert.Equal(application, received);
        Assert.Empty(cut.FindAll(".context-menu-dropdown"));
    }
}
