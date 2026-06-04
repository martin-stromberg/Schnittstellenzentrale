using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für die <see cref="ApplicationContextMenu"/>-Komponente.</summary>
public class ApplicationContextMenuTests : BunitContext
{
    /// <summary>Registriert den FakeStringLocalizer für IStringLocalizer&lt;SharedResources&gt;.</summary>
    public ApplicationContextMenuTests()
    {
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    private static Application InGroup() =>
        new() { Id = 1, Name = "TestApp", BaseUrl = "http://app", ApplicationGroupId = 5 };

    private static Application WithoutGroup() =>
        new() { Id = 2, Name = "TestApp", BaseUrl = "http://app", ApplicationGroupId = null };

    private static Application SystemApplication() =>
        new() { Id = 3, Name = "Schnittstellenzentrale", BaseUrl = "http://app", IsSystem = true };

    /// <summary>Der Menüeintrag „Aus Sammlung entfernen" ist nur sichtbar, wenn die Anwendung einer Gruppe zugeordnet ist.</summary>
    [Fact]
    public void AusGruppeEntfernen_NurSichtbar_WennAnwendungInGruppe()
    {
        var cut = Render<ApplicationContextMenu>(p => p
            .Add(x => x.Application, InGroup()));

        cut.Find(".context-menu-toggle").Click();

        Assert.Contains(
            cut.FindAll("button.context-menu-item"),
            b => b.TextContent.Contains("ApplicationContextMenu_RemoveFromCollectionButton"));
    }

    /// <summary>Der Menüeintrag „Aus Sammlung entfernen" ist nicht sichtbar, wenn die Anwendung keiner Gruppe zugeordnet ist.</summary>
    [Fact]
    public void AusGruppeEntfernen_NichtSichtbar_WennAnwendungOhneGruppe()
    {
        var cut = Render<ApplicationContextMenu>(p => p
            .Add(x => x.Application, WithoutGroup()));

        cut.Find(".context-menu-toggle").Click();

        Assert.DoesNotContain(
            cut.FindAll("button.context-menu-item"),
            b => b.TextContent.Contains("ApplicationContextMenu_RemoveFromCollectionButton"));
    }

    /// <summary>Klick auf „Aus Sammlung entfernen" löst den Callback aus und schließt das Menü.</summary>
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
            .First(b => b.TextContent.Contains("ApplicationContextMenu_RemoveFromCollectionButton"))
            .Click();

        Assert.Equal(application, received);
        Assert.Empty(cut.FindAll(".context-menu-dropdown"));
    }

    /// <summary>Der „Bearbeiten"-Button ist deaktiviert, wenn die Anwendung eine Systemanwendung ist.</summary>
    [Fact]
    public void Bearbeiten_Deaktiviert_WennIsSystem()
    {
        var cut = Render<ApplicationContextMenu>(p => p
            .Add(x => x.Application, SystemApplication()));

        cut.Find(".context-menu-toggle").Click();

        var bearbeitenButton = cut.FindAll("button.context-menu-item")
            .First(b => b.TextContent.Contains("ApplicationContextMenu_EditButton"));

        Assert.True(bearbeitenButton.HasAttribute("disabled"));
    }

    /// <summary>Der „Löschen"-Button ist deaktiviert, wenn die Anwendung eine Systemanwendung ist.</summary>
    [Fact]
    public void Löschen_Deaktiviert_WennIsSystem()
    {
        var cut = Render<ApplicationContextMenu>(p => p
            .Add(x => x.Application, SystemApplication()));

        cut.Find(".context-menu-toggle").Click();

        var löschenButton = cut.FindAll("button.context-menu-item")
            .First(b => b.TextContent.Contains("ApplicationContextMenu_DeleteButton"));

        Assert.True(löschenButton.HasAttribute("disabled"));
    }
}
