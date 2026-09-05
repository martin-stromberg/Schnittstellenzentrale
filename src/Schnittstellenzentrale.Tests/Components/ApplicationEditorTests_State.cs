using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für Lade-, Speichern- und Interface-URL-Pfade in <see cref="ApplicationEditor"/>.</summary>
public class ApplicationEditorTests_State : BunitContext
{
    private readonly Mock<IApplicationApiClient> _apiClientMock = new();
    private readonly Mock<IStorageModeService> _storageModeServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    /// <summary>Registriert Mocks und Lokalisierung für den Editor.</summary>
    public ApplicationEditorTests_State()
    {
        _storageModeServiceMock.SetupGet(s => s.CurrentMode).Returns(StorageMode.Team);
        _currentUserServiceMock.Setup(s => s.GetCurrentUserName()).Returns("tester");
        _apiClientMock
            .Setup(a => a.GetGroupsAsync(It.IsAny<StorageMode>(), It.IsAny<string>()))
            .ReturnsAsync(new List<ApplicationGroup>());

        Services.AddSingleton(_apiClientMock.Object);
        Services.AddSingleton(_storageModeServiceMock.Object);
        Services.AddSingleton(_currentUserServiceMock.Object);
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    /// <summary>Fehler beim Laden der Gruppen wird als Fehlermeldung dargestellt.</summary>
    [Fact]
    public void OnInitialized_WhenLoadingGroupsFails_ShowsError()
    {
        _apiClientMock
            .Setup(a => a.GetGroupsAsync(It.IsAny<StorageMode>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("load failed"));

        var cut = Render<ApplicationEditor>();

        Assert.Contains("ApplicationEditor_Error_LoadGroups", cut.Markup);
    }

    /// <summary>Im Edit-Modus wird die bestehende Anwendung geladen und per Update gespeichert.</summary>
    [Fact]
    public async Task SaveAsync_InEditMode_UpdatesApplication()
    {
        Application? saved = null;
        _apiClientMock
            .Setup(a => a.UpdateApplicationAsync(It.IsAny<Application>()))
            .ReturnsAsync((Application app) => app);

        var existing = new Application
        {
            Id = 77,
            Name = "Alt",
            BaseUrl = "https://alt.example.test",
            InterfaceUrl = "https://alt.example.test/swagger",
            InterfaceType = InterfaceType.Rest
        };

        var cut = Render<ApplicationEditor>(parameters => parameters
            .Add(x => x.ExistingApplication, existing)
            .Add(x => x.OnSaved, EventCallback.Factory.Create<Application?>(this, value => saved = value)));

        await cut.InvokeAsync(() => cut.Find("input#app-name").Change("Neu"));
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        _apiClientMock.Verify(a => a.UpdateApplicationAsync(It.Is<Application>(app => app.Id == 77 && app.Name == "Neu")), Times.Once);
        Assert.NotNull(saved);
        Assert.Equal("Neu", saved!.Name);
    }

    /// <summary>Im Create-Modus mit User-Storage wird Owner gesetzt und Add aufgerufen.</summary>
    [Fact]
    public async Task SaveAsync_InCreateModeForUser_AddsApplicationWithOwner()
    {
        _storageModeServiceMock.SetupGet(s => s.CurrentMode).Returns(StorageMode.User);
        Application? saved = null;
        _apiClientMock
            .Setup(a => a.AddApplicationAsync(It.IsAny<Application>()))
            .ReturnsAsync((Application app) => app);

        var cut = Render<ApplicationEditor>(parameters => parameters
            .Add(x => x.OnSaved, EventCallback.Factory.Create<Application?>(this, value => saved = value)));

        await cut.InvokeAsync(() => cut.Find("input#app-name").Change("Neu"));
        await cut.InvokeAsync(() => cut.Find("input#app-base-url").Change("https://new.example.test"));
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        _apiClientMock.Verify(a => a.AddApplicationAsync(It.Is<Application>(app =>
            app.Name == "Neu" &&
            app.BaseUrl == "https://new.example.test" &&
            app.Owner == "tester")), Times.Once);
        Assert.NotNull(saved);
    }

    /// <summary>Änderung der Interface-URL erkennt den OData-Typ und rendert den passenden Hint.</summary>
    [Fact]
    public async Task OnInterfaceUrlChanged_WithMetadata_SetsODataHint()
    {
        var cut = Render<ApplicationEditor>();

        await cut.InvokeAsync(() => cut.Find("input#app-interface-url").Input("https://example.test/$metadata"));

        Assert.Contains("ApplicationEditor_Hint_OData", cut.Markup);
    }
}
