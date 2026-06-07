using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für <see cref="ImportDialog"/> und <see cref="ODataImportDialog"/>.</summary>
public class ImportDialogTests : BunitContext
{
    private readonly Mock<IODataImportService> _odataImportMock = new();

    /// <summary>Initialisiert die Test-Services.</summary>
    public ImportDialogTests()
    {
        Services.AddSingleton(_odataImportMock.Object);
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    private static Application CreateApplication() => new()
    {
        Id = 1,
        Name = "Test App",
        BaseUrl = "http://example.com"
    };

    private static ImportDiff CreateDiff(Dictionary<string, string>? bearerTokens = null) => new()
    {
        NewEndpoints = [],
        ChangedEndpoints = [],
        RemovedEndpoints = [],
        BearerTokens = bearerTokens ?? new Dictionary<string, string>()
    };

    /// <summary>
    /// Eine Exception aus ODataImportService.ApplyDiffAsync wird als Fehlermeldung im Dialog angezeigt,
    /// da ODataImportDialog die Exception nicht mehr fängt und ImportDialog sie verarbeitet.
    /// </summary>
    [Fact]
    public async Task ODataImportDialog_ApplyAsync_OnException_ErrorIsDisplayedInImportDialog()
    {
        var app = CreateApplication();
        var diff = CreateDiff();

        _odataImportMock
            .Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var cut = Render<ODataImportDialog>(p =>
        {
            p.Add(x => x.Diff, diff);
            p.Add(x => x.Application, app);
            p.Add(x => x.OnClose, EventCallback.Empty);
        });

        var applyButton = cut.Find("button.sz-btn-primary");
        await cut.InvokeAsync(() => applyButton.Click());

        Assert.Contains("ImportDialog_Error_Apply", cut.Markup);
    }

    /// <summary>
    /// BearerTokens aus dem Diff werden beim Erstellen von selectedDiff in ImportDialog übernommen,
    /// sodass ApplyDiffAsync die Credentials erhält.
    /// </summary>
    [Fact]
    public async Task ImportDialog_ApplyAsync_BearerTokensCopiedToSelectedDiff()
    {
        var app = CreateApplication();
        var bearerTokens = new Dictionary<string, string> { { "http://example.com", "my-token" } };
        var diff = CreateDiff(bearerTokens);

        ImportDiff? capturedDiff = null;
        _odataImportMock
            .Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .Callback<ImportDiff>(d => capturedDiff = d)
            .Returns(Task.CompletedTask);

        var onCloseCalled = false;

        var cut = Render<ODataImportDialog>(p =>
        {
            p.Add(x => x.Diff, diff);
            p.Add(x => x.Application, app);
            p.Add(x => x.OnClose, EventCallback.Factory.Create(this, () => onCloseCalled = true));
        });

        var applyButton = cut.Find("button.sz-btn-primary");
        await cut.InvokeAsync(() => applyButton.Click());

        Assert.NotNull(capturedDiff);
        Assert.Contains("http://example.com", capturedDiff.BearerTokens.Keys);
        Assert.Equal("my-token", capturedDiff.BearerTokens["http://example.com"]);
        Assert.True(onCloseCalled);
    }
}
