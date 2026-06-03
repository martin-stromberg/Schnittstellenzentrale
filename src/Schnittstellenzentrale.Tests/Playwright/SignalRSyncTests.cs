using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für SignalR-Echtzeitsynchronisation im Team-Modus mit zwei Browser-Kontexten.</summary>
[Collection("PlaywrightSignalR")]
public class SignalRSyncTests : PlaywrightTestBase
{
    private IBrowserContext _contextB = null!;
    private IPage _pageB = null!;

    /// <summary>Initialisiert den Test mit der SignalR-Factory.</summary>
    public SignalRSyncTests(PlaywrightSignalRServer server) : base(server) { }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        _contextB = await CreateAdditionalContextAsync();
        _pageB = await _contextB.NewPageAsync();
    }

    /// <inheritdoc/>
    protected override async Task OnDisposingAsync()
    {
        Directory.CreateDirectory("playwright-traces");
        await _contextB.Tracing.StopAsync(new TracingStopOptions
        {
            Path = Path.Combine("playwright-traces", $"{nameof(SignalRSyncTests)}-B.zip")
        });
        await _contextB.DisposeAsync();
    }

    /// <summary>Browser B zeigt eine neue Anwendung ohne Reload, nachdem Browser A sie angelegt hat.</summary>
    [Fact]
    public async Task BrowserA_CreatesApp_BrowserB_ReceivesViaSignalR()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _pageB.GotoAsync(BaseUrl);
        await _pageB.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator(".sz-topbar-select").SelectOptionAsync("Team");
        await _pageB.Locator(".sz-topbar-select").SelectOptionAsync("Team");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("SignalR-Sync-Test");
        await Page.GetByLabel("Basis-URL").FillAsync("http://signalr-test.example.com");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "SignalR-Sync-Test" })).ToBeVisibleAsync();

        await Assertions.Expect(_pageB.Locator(".sz-tree-item-btn", new() { HasText = "SignalR-Sync-Test" })).ToBeVisibleAsync();
    }
}
