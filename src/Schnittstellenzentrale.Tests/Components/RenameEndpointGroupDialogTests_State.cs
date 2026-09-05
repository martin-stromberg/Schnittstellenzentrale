using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für Umbenennen/Abbrechen und Fehlerpfade in <see cref="RenameEndpointGroupDialog"/>.</summary>
public class RenameEndpointGroupDialogTests_State : BunitContext
{
    /// <summary>Registriert Lokalisierung für den Dialog.</summary>
    public RenameEndpointGroupDialogTests_State()
    {
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    /// <summary>Leerer Name blockiert das Speichern und zeigt die Fehlermeldung an.</summary>
    [Fact]
    public async Task SaveAsync_WhenNameIsEmpty_ShowsValidationError()
    {
        var saved = false;
        var cut = Render<RenameEndpointGroupDialog>(parameters => parameters
            .Add(x => x.Group, new EndpointGroup { Id = 2, Name = "Alt", ApplicationId = 1 })
            .Add(x => x.OnSaved, EventCallback.Factory.Create<EndpointGroup>(this, _ => saved = true)));

        await cut.InvokeAsync(() => cut.Find("input#rename-endpoint-group-name").Change(string.Empty));
        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());

        Assert.False(saved);
        Assert.Contains("RenameEndpointGroupDialog_Error_NameEmpty", cut.Markup);
    }

    /// <summary>Gültiges Speichern ruft den Callback mit der aktualisierten Gruppe auf.</summary>
    [Fact]
    public async Task SaveAsync_WhenNameIsValid_InvokesOnSaved()
    {
        EndpointGroup? saved = null;
        var cut = Render<RenameEndpointGroupDialog>(parameters => parameters
            .Add(x => x.Group, new EndpointGroup { Id = 3, Name = "Alt", ApplicationId = 5, RowVersion = [1] })
            .Add(x => x.OnSaved, EventCallback.Factory.Create<EndpointGroup>(this, value => saved = value)));

        await cut.InvokeAsync(() => cut.Find("input#rename-endpoint-group-name").Change("Neu"));
        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());

        Assert.NotNull(saved);
        Assert.Equal(3, saved!.Id);
        Assert.Equal("Neu", saved.Name);
        Assert.Equal(5, saved.ApplicationId);
    }

    /// <summary>Exceptions im Save-Callback werden als Fehlermeldung angezeigt.</summary>
    [Fact]
    public async Task SaveAsync_WhenCallbackThrows_ShowsSaveError()
    {
        var cut = Render<RenameEndpointGroupDialog>(parameters => parameters
            .Add(x => x.Group, new EndpointGroup { Id = 4, Name = "Alt", ApplicationId = 2 })
            .Add(x => x.OnSaved, EventCallback.Factory.Create<EndpointGroup>(this, _ => throw new InvalidOperationException("save failed"))));

        await cut.InvokeAsync(() => cut.Find("input#rename-endpoint-group-name").Change("Neu"));
        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());

        Assert.Contains("RenameEndpointGroupDialog_Error_Save", cut.Markup);
    }
}
