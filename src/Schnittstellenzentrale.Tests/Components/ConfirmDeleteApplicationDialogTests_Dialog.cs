using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für Dialogbestätigung und -abbruch von <see cref="ConfirmDeleteApplicationDialog"/>.</summary>
public class ConfirmDeleteApplicationDialogTests_Dialog : BunitContext
{
    /// <summary>Registriert Lokalisierung für Komponententests.</summary>
    public ConfirmDeleteApplicationDialogTests_Dialog()
    {
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    /// <summary>Der Lösch-Button übergibt die erwartete Anwendung an den Confirm-Callback.</summary>
    [Fact]
    public async Task Confirmed_Click_InvokesOnConfirmedWithApplication()
    {
        var application = new Application { Id = 15, Name = "Delete me", BaseUrl = "https://example.test" };
        Application? received = null;

        var cut = Render<ConfirmDeleteApplicationDialog>(parameters => parameters
            .Add(x => x.Application, application)
            .Add(x => x.OnConfirmed, EventCallback.Factory.Create<Application>(this, app => received = app)));

        var confirmButton = cut.Find("button.sz-btn-destructive");
        await cut.InvokeAsync(() => confirmButton.Click());

        Assert.Equal(application, received);
    }

    /// <summary>Der Abbrechen-Button löst den Cancel-Callback aus.</summary>
    [Fact]
    public async Task Cancelled_Click_InvokesOnCancel()
    {
        var application = new Application { Id = 16, Name = "Cancel me", BaseUrl = "https://example.test" };
        var cancelled = false;

        var cut = Render<ConfirmDeleteApplicationDialog>(parameters => parameters
            .Add(x => x.Application, application)
            .Add(x => x.OnCancel, EventCallback.Factory.Create(this, () => cancelled = true)));

        var cancelButton = cut.Find("button.sz-btn-outline");
        await cut.InvokeAsync(() => cancelButton.Click());

        Assert.True(cancelled);
    }
}
