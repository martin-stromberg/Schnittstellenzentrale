using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für Umbenennen und Fehlerbehandlung in <see cref="RenameGroupDialog"/>.</summary>
public class RenameGroupDialogTests_State : BunitContext
{
    /// <summary>Registriert Lokalisierung für den Dialog.</summary>
    public RenameGroupDialogTests_State()
    {
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    /// <summary>Der Save-Callback erhält die aktualisierte Gruppe.</summary>
    [Fact]
    public async Task SaveAsync_WhenValid_InvokesOnSaved()
    {
        ApplicationGroup? saved = null;

        var cut = Render<RenameGroupDialog>(parameters => parameters
            .Add(x => x.Group, new ApplicationGroup { Id = 9, Name = "Alt", RowVersion = [5] })
            .Add(x => x.OnSaved, EventCallback.Factory.Create<ApplicationGroup>(this, group => saved = group)));

        await cut.InvokeAsync(() => cut.Find("input#rename-group-name").Change("Neu"));
        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());

        Assert.NotNull(saved);
        Assert.Equal(9, saved!.Id);
        Assert.Equal("Neu", saved.Name);
    }

    /// <summary>Fehler im Save-Callback werden als Fehlermeldung angezeigt.</summary>
    [Fact]
    public async Task SaveAsync_WhenCallbackThrows_ShowsError()
    {
        var cut = Render<RenameGroupDialog>(parameters => parameters
            .Add(x => x.Group, new ApplicationGroup { Id = 10, Name = "Alt" })
            .Add(x => x.OnSaved, EventCallback.Factory.Create<ApplicationGroup>(this, _ => throw new InvalidOperationException("boom"))));

        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());

        Assert.Contains("RenameGroupDialog_Error_Save", cut.Markup);
    }
}
