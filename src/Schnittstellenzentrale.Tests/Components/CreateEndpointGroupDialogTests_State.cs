using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für Erstellen/Abbrechen und Validierung in <see cref="CreateEndpointGroupDialog"/>.</summary>
public class CreateEndpointGroupDialogTests_State : BunitContext
{
    /// <summary>Registriert Lokalisierung für den Dialog.</summary>
    public CreateEndpointGroupDialogTests_State()
    {
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    /// <summary>Leerer Name zeigt die Validierungsfehlermeldung und ruft keinen Save-Callback auf.</summary>
    [Fact]
    public async Task SaveAsync_WhenNameIsEmpty_ShowsValidationError()
    {
        var savedName = string.Empty;

        var cut = Render<CreateEndpointGroupDialog>(parameters => parameters
            .Add(x => x.Application, new Application { Id = 10, Name = "App", BaseUrl = "https://example.test" })
            .Add(x => x.OnSaved, EventCallback.Factory.Create<string>(this, value => savedName = value)));

        var submitButton = cut.Find("button.sz-btn-primary");
        await cut.InvokeAsync(() => submitButton.Click());

        Assert.Contains("CreateEndpointGroupDialog_Error_NameEmpty", cut.Markup);
        Assert.Equal(string.Empty, savedName);
    }

    /// <summary>Gültiger Name ruft den Save-Callback mit dem eingegebenen Gruppennamen auf.</summary>
    [Fact]
    public async Task SaveAsync_WhenNameIsValid_InvokesOnSaved()
    {
        var savedName = string.Empty;

        var cut = Render<CreateEndpointGroupDialog>(parameters => parameters
            .Add(x => x.Application, new Application { Id = 11, Name = "App", BaseUrl = "https://example.test" })
            .Add(x => x.OnSaved, EventCallback.Factory.Create<string>(this, value => savedName = value)));

        await cut.InvokeAsync(() => cut.Find("input#endpoint-group-name").Change("Neue Gruppe"));
        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());

        Assert.Equal("Neue Gruppe", savedName);
    }

    /// <summary>Der Abbrechen-Button löst den Cancel-Callback aus.</summary>
    [Fact]
    public async Task Cancel_Click_InvokesOnCancel()
    {
        var cancelled = false;

        var cut = Render<CreateEndpointGroupDialog>(parameters => parameters
            .Add(x => x.Application, new Application { Id = 12, Name = "App", BaseUrl = "https://example.test" })
            .Add(x => x.OnCancel, EventCallback.Factory.Create(this, () => cancelled = true)));

        var cancelButton = cut.Find("button.sz-btn-outline");
        await cut.InvokeAsync(() => cancelButton.Click());

        Assert.True(cancelled);
    }
}
