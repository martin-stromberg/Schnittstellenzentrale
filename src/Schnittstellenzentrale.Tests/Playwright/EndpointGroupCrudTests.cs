using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für das Anlegen, Umbenennen und Löschen von Endpunktordnern.</summary>
[Collection("Playwright")]
public class EndpointGroupCrudTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public EndpointGroupCrudTests(PlaywrightServer server) : base(server) { }

    private ILocator SystemAppRow =>
        Page.Locator(".sz-tree-row", new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "Schnittstellenzentrale" }) });

    private async Task ExpandSystemGroupAsync()
    {
        var groupChevron = Page.Locator(".collapsible-section .sz-tree-chevron-btn").First;
        await groupChevron.ClickAsync();
    }

    private async Task<string> CreateFolderInSystemAppAsync(string folderName)
    {
        var contextMenuToggle = SystemAppRow.Locator("[data-testid=\"context-menu-toggle\"]");
        await contextMenuToggle.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Ordner anlegen" }).ClickAsync();
        var nameInput = Page.GetByLabel("Name");
        await nameInput.FillAsync(folderName);
        await nameInput.PressAsync("Tab");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Anlegen" }).ClickAsync();
        return folderName;
    }

    private async Task ExpandSystemAppAsync()
    {
        var appBtn = SystemAppRow.Locator(".sz-tree-item-btn");
        await appBtn.ClickAsync();
    }

    private ILocator FolderRow(string folderName) =>
        Page.Locator(".sz-tree-row", new() { Has = Page.GetByText(folderName) });

    /// <summary>Ein leerer Ordner wird nach Bestätigung des Löschdialogs aus dem Baum entfernt.</summary>
    [Fact]
    public async Task DeleteEndpointGroup_OhneEndpunkte_LoeschtOrdnerAusBaum()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await ExpandSystemGroupAsync();
        await CreateFolderInSystemAppAsync("Loeschen-Ordner");
        await ExpandSystemAppAsync();

        var folderContextMenu = FolderRow("Loeschen-Ordner").Locator("[data-testid=\"context-menu-toggle\"]");
        await folderContextMenu.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Ordner löschen" }).ClickAsync();

        await Assertions.Expect(Page.GetByText("Loeschen-Ordner wirklich löschen")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator(".sz-alert--warning")).Not.ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Löschen" }).ClickAsync();

        await ExpandSystemAppAsync();
        await Assertions.Expect(Page.GetByText("Loeschen-Ordner")).Not.ToBeVisibleAsync();
    }

    /// <summary>Beim Löschen eines Ordners mit Endpunkten zeigt der Dialog einen Warnhinweis mit der Anzahl.</summary>
    [Fact]
    public async Task DeleteEndpointGroup_MitEndpunkten_ZeigtWarnhinweis()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await ExpandSystemGroupAsync();
        await CreateFolderInSystemAppAsync("Warnung-Ordner");
        await ExpandSystemAppAsync();

        var folderContextMenu = FolderRow("Warnung-Ordner").Locator("[data-testid=\"context-menu-toggle\"]");
        await folderContextMenu.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Endpunkt anlegen" }).ClickAsync();
        await Page.GetByPlaceholder("Relativer Pfad").FillAsync("/api/warnung-test");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await folderContextMenu.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Ordner löschen" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-alert--warning")).ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByText("1")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Löschen" }).ClickAsync();

        await ExpandSystemAppAsync();
        await Assertions.Expect(Page.GetByText("Warnung-Ordner")).Not.ToBeVisibleAsync();
    }

    /// <summary>Nach dem Umbenennen eines Ordners erscheint der neue Name im Baum.</summary>
    [Fact]
    public async Task RenameEndpointGroup_AktualisiertNameImBaum()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await ExpandSystemGroupAsync();
        await CreateFolderInSystemAppAsync("Umbenennen-Ordner-Alt");
        await ExpandSystemAppAsync();

        var folderContextMenu = FolderRow("Umbenennen-Ordner-Alt").Locator("[data-testid=\"context-menu-toggle\"]");
        await folderContextMenu.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Ordner umbenennen" }).ClickAsync();

        await Page.GetByLabel("Name").FillAsync("Umbenennen-Ordner-Neu");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Assertions.Expect(Page.GetByText("Umbenennen-Ordner-Neu")).ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByText("Umbenennen-Ordner-Alt")).Not.ToBeVisibleAsync();
    }

    /// <summary>„Abbrechen" im Umbenennungsdialog lässt den Ordnernamen unverändert.</summary>
    [Fact]
    public async Task RenameEndpointGroup_Abbrechen_BehaeltOriginalenNamen()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await ExpandSystemGroupAsync();
        await CreateFolderInSystemAppAsync("Abbrechen-Ordner");
        await ExpandSystemAppAsync();

        var folderContextMenu = FolderRow("Abbrechen-Ordner").Locator("[data-testid=\"context-menu-toggle\"]");
        await folderContextMenu.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Ordner umbenennen" }).ClickAsync();

        await Page.GetByLabel("Name").FillAsync("Soll-Nicht-Gespeichert-Werden");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Abbrechen" }).ClickAsync();

        await ExpandSystemAppAsync();
        await Assertions.Expect(Page.GetByText("Abbrechen-Ordner")).ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByText("Soll-Nicht-Gespeichert-Werden")).Not.ToBeVisibleAsync();
    }
}
