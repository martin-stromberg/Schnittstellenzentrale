using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für das Umbenennen und Löschen von Sammlungen.</summary>
[Collection("Playwright")]
public class GroupCrudTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public GroupCrudTests(PlaywrightServer server) : base(server) { }

    private async Task<string> CreateGroupWithApplicationAsync(string groupName, string appName)
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ Neue Sammlung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync(groupName);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Page.Locator(".sz-workspaces-sidebar").GetByRole(AriaRole.Button, new() { Name = "+ Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync(appName);
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = appName })).ToBeVisibleAsync();
        return groupName;
    }

    private ILocator GroupRow(string groupName) =>
        Page.Locator(".sz-tree-row", new() { Has = Page.Locator(".sz-tree-node-text", new() { HasText = groupName }) });

    private async Task OpenGroupContextMenuAsync(string groupName)
    {
        var toggle = GroupRow(groupName).Locator("[data-testid=\"context-menu-toggle\"]");
        await toggle.ClickAsync();
    }

    /// <summary>Eine Sammlung mit Anwendung über „Mitlöschen" entfernt beides vollständig aus dem Baum.</summary>
    [Fact]
    public async Task DeleteGroup_Mitloeschen_EntferntGruppeUndAnwendung()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await CreateGroupWithApplicationAsync("Mitloeschen-Gruppe", "Mitloeschen-Anwendung");

        await OpenGroupContextMenuAsync("Mitloeschen-Gruppe");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Löschen" }).ClickAsync();

        await Assertions.Expect(Page.GetByText("Mitloeschen-Gruppe wirklich löschen")).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Mitlöschen" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-node-text", new() { HasText = "Mitloeschen-Gruppe" })).Not.ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Mitloeschen-Anwendung" })).Not.ToBeVisibleAsync();
    }

    /// <summary>„Nur Sammlung löschen" entfernt die Gruppe, die enthaltene Anwendung bleibt im Baum.</summary>
    [Fact]
    public async Task DeleteGroup_NurSammlungLoeschen_EntferntNurGruppe()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await CreateGroupWithApplicationAsync("NurGruppe-Gruppe", "NurGruppe-Anwendung");

        await OpenGroupContextMenuAsync("NurGruppe-Gruppe");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Löschen" }).ClickAsync();

        await Assertions.Expect(Page.GetByText("enthält")).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Nur Sammlung löschen" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-node-text", new() { HasText = "NurGruppe-Gruppe" })).Not.ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "NurGruppe-Anwendung" })).ToBeVisibleAsync();
    }

    /// <summary>„Abbrechen" im Löschdialog lässt die Sammlung unverändert im Baum.</summary>
    [Fact]
    public async Task DeleteGroup_Abbrechen_GruppeBleibtErhalten()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GetByRole(AriaRole.Button, new() { Name = "+ Neue Sammlung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Abbrechen-Gruppe");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await OpenGroupContextMenuAsync("Abbrechen-Gruppe");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Löschen" }).ClickAsync();

        await Assertions.Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Abbrechen" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Abbrechen" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-node-text", new() { HasText = "Abbrechen-Gruppe" })).ToBeVisibleAsync();
    }

    /// <summary>Nach dem Umbenennen einer Sammlung erscheint der neue Name im Baum.</summary>
    [Fact]
    public async Task RenameGroup_AktualisiertNameImBaum()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GetByRole(AriaRole.Button, new() { Name = "+ Neue Sammlung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Umbenennen-Gruppe-Alt");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await OpenGroupContextMenuAsync("Umbenennen-Gruppe-Alt");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Umbenennen" }).ClickAsync();

        await Page.GetByLabel("Name").FillAsync("Umbenennen-Gruppe-Neu");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-node-text", new() { HasText = "Umbenennen-Gruppe-Neu" })).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator(".sz-tree-node-text", new() { HasText = "Umbenennen-Gruppe-Alt" })).Not.ToBeVisibleAsync();
    }
}
