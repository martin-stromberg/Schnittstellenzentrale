using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Schnittstellenzentrale.Components.Layout;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für die <see cref="MainLayout"/>-Komponente.</summary>
public class MainLayoutTests : BunitContext
{
    private readonly Mock<IStorageModeService> _storageMock = new();
    private readonly Mock<IThemeService> _themeMock = new();
    private readonly Mock<IActiveEnvironmentService> _activeEnvMock = new();
    private readonly Mock<ISystemEnvironmentRepository> _envRepoMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ISignalRNotificationService> _signalRMock = new();
    private readonly Mock<IActivityLogService> _activityLogMock = new();

    /// <summary>Initialisiert die Test-Services und JS-Interop-Mocks.</summary>
    public MainLayoutTests()
    {
        _storageMock.Setup(s => s.CurrentMode).Returns(StorageMode.Team);
        _activeEnvMock.Setup(s => s.ActiveEnvironment).Returns((SystemEnvironment?)null);
        _activeEnvMock.Setup(s => s.ActiveVariables).Returns(new Dictionary<string, string>());
        _envRepoMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync([]);
        _currentUserMock.Setup(s => s.GetCurrentUserName()).Returns("DOMAIN\\testuser");
        _activityLogMock.Setup(s => s.Entries).Returns([]);

        Services.AddSingleton(_storageMock.Object);
        Services.AddSingleton(_themeMock.Object);
        Services.AddSingleton(_activeEnvMock.Object);
        Services.AddSingleton(_envRepoMock.Object);
        Services.AddSingleton(_currentUserMock.Object);
        Services.AddSingleton(_signalRMock.Object);
        Services.AddSingleton(_activityLogMock.Object);
        Services.AddSingleton<ILogger<MainLayout>>(NullLogger<MainLayout>.Instance);

        JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);
        JSInterop.SetupVoid("localStorage.removeItem", _ => true).SetVoidResult();
        JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();
    }

    /// <summary>Das Layout rendert den Modus-Selektor im Header.</summary>
    [Fact]
    public void Layout_RendertModusSelektor()
    {
        var cut = Render<MainLayout>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        Assert.NotEmpty(cut.FindAll("select.form-select"));
    }

    /// <summary>Das Layout rendert das Zahnrad-Icon für die Umgebungsverwaltung.</summary>
    [Fact]
    public void Layout_RendertZahnradIcon()
    {
        var cut = Render<MainLayout>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        var button = cut.FindAll("button[title='Umgebungen verwalten']");
        Assert.NotEmpty(button);
    }

    /// <summary>DisposeAsync kann ohne Fehler aufgerufen werden, wenn keine HubConnection aufgebaut wurde.</summary>
    [Fact]
    public async Task DisposeAsync_OhneHubConnection_WirftKeinenFehler()
    {
        var cut = Render<MainLayout>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        var exception = await Record.ExceptionAsync(() => cut.Instance.DisposeAsync().AsTask());

        Assert.Null(exception);
    }

    /// <summary>SetActiveEnvironment wird mit der aus der DB geladenen Umgebung aufgerufen, wenn eine ID im localStorage liegt.</summary>
    [Fact]
    public async Task Wiederherstellen_GespeicherteIdVorhanden_SetzAktiveUmgebung()
    {
        var env = TestMockFactory.CreateEnv(42, "Dev");
        var key = LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team);
        JSInterop.Setup<string?>("localStorage.getItem", inv => inv.Arguments.ElementAt(0) is string s && s == key).SetResult("42");
        _envRepoMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(env);

        var cut = Render<MainLayout>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));
        await cut.InvokeAsync(() => { });

        _activeEnvMock.Verify(s => s.SetActiveEnvironment(env), Times.Once);
    }

    /// <summary>SetActiveEnvironment(null) und localStorage.removeItem werden aufgerufen, wenn die gespeicherte ID nicht mehr in der DB existiert.</summary>
    [Fact]
    public async Task Wiederherstellen_UmgebungNichtMehrInDb_BereinigTLocalStorage()
    {
        var key = LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team);
        JSInterop.Setup<string?>("localStorage.getItem", inv => inv.Arguments.ElementAt(0) is string s && s == key).SetResult("99");
        _envRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SystemEnvironment?)null);

        var cut = Render<MainLayout>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));
        await cut.InvokeAsync(() => { });

        _activeEnvMock.Verify(s => s.SetActiveEnvironment(null), Times.AtLeastOnce);
        var removeItemCalls = JSInterop.Invocations
            .Where(i => i.Identifier == "localStorage.removeItem")
            .ToList();
        Assert.Single(removeItemCalls);
        Assert.Equal(key, removeItemCalls[0].Arguments.ElementAt(0));
    }

    /// <summary>SetActiveEnvironment wird nicht mit einem Wert aufgerufen, wenn localStorage.getItem null zurückgibt.</summary>
    [Fact]
    public async Task Wiederherstellen_KeinEintragImLocalStorage_SetzNichts()
    {
        JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);

        var cut = Render<MainLayout>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));
        await cut.InvokeAsync(() => { });

        _activeEnvMock.Verify(s => s.SetActiveEnvironment(It.Is<SystemEnvironment?>(e => e != null)), Times.Never);
    }

    /// <summary>Nach Moduswechsel wird der Schlüssel des neuen Modus für localStorage.getItem verwendet.</summary>
    [Fact]
    public async Task Wiederherstellen_BeiModuswechsel_VerwendetNeuenSchlüssel()
    {
        var userKey = LocalStorageKeys.SelectedEnvironmentId(StorageMode.User);
        JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);
        _storageMock.Setup(s => s.CurrentMode).Returns(StorageMode.Team);

        var cut = Render<MainLayout>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        JSInterop.Setup<string?>("localStorage.getItem", inv => inv.Arguments.ElementAt(0) is string s && s == userKey).SetResult(null);

        await cut.InvokeAsync(() => cut.Find("select.form-select").Change(StorageMode.User.ToString()));

        var getItemInvocations = JSInterop.Invocations
            .Where(i => i.Identifier == "localStorage.getItem")
            .ToList();
        Assert.Contains(getItemInvocations, i => i.Arguments.ElementAt(0) is string s && s == userKey);
    }

    /// <summary>Nach Wiederherstellung aus localStorage zeigt der EnvironmentSelector die gespeicherte Umgebung als ausgewählt.</summary>
    [Fact]
    public async Task Wiederherstellen_GespeicherteIdVorhanden_SelectorZeigtAuswahl()
    {
        var env = TestMockFactory.CreateEnv(42, "Dev");
        var key = LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team);

        SystemEnvironment? activeEnv = null;
        _activeEnvMock.Setup(s => s.ActiveEnvironment).Returns(() => activeEnv);
        _activeEnvMock.Setup(s => s.SetActiveEnvironment(It.IsAny<SystemEnvironment?>()))
            .Callback<SystemEnvironment?>(e => activeEnv = e);

        JSInterop.Setup<string?>("localStorage.getItem", inv => inv.Arguments.ElementAt(0) is string s && s == key).SetResult("42");
        _envRepoMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(env);
        _envRepoMock.Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync([env]);

        var cut = Render<MainLayout>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));
        await cut.InvokeAsync(() => { });

        var selector = cut.FindComponent<EnvironmentSelector>();
        var select = selector.Find("select");
        Assert.Equal("42", select.GetAttribute("value"));
    }

    /// <summary>OnEnvironmentChanged: Aktive Umgebung existiert noch in der DB → SetActiveEnvironment mit aktualisierten Daten aufgerufen.</summary>
    [Fact]
    public async Task OnEnvironmentChanged_AktiveUmgebungNochVorhanden_AktualisiertUmgebung()
    {
        var env = TestMockFactory.CreateEnv(5, "Dev");
        _activeEnvMock.Setup(s => s.ActiveEnvironment).Returns(env);
        _envRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(env);

        var cut = Render<MainLayout>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));
        var overlay = cut.FindComponent<EnvironmentManagementOverlay>();

        await cut.InvokeAsync(() => overlay.Instance.OnEnvironmentsChanged.InvokeAsync());

        _activeEnvMock.Verify(s => s.SetActiveEnvironment(env), Times.AtLeastOnce);
    }

    /// <summary>OnEnvironmentChanged: Aktive Umgebung wurde gelöscht → SetActiveEnvironment(null) und localStorage.removeItem aufgerufen.</summary>
    [Fact]
    public async Task OnEnvironmentChanged_AktiveUmgebungGelöscht_BereinigLocalStorageUndSetztNull()
    {
        var env = TestMockFactory.CreateEnv(7, "Prod");
        _activeEnvMock.Setup(s => s.ActiveEnvironment).Returns(env);
        _envRepoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((SystemEnvironment?)null);

        var cut = Render<MainLayout>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));
        var overlay = cut.FindComponent<EnvironmentManagementOverlay>();

        await cut.InvokeAsync(() => overlay.Instance.OnEnvironmentsChanged.InvokeAsync());

        var expectedKey = LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team);
        _activeEnvMock.Verify(s => s.SetActiveEnvironment(null), Times.AtLeastOnce);
        var removeItemCalls = JSInterop.Invocations
            .Where(i => i.Identifier == "localStorage.removeItem")
            .ToList();
        Assert.Contains(removeItemCalls, i => i.Arguments.ElementAt(0) is string key && key == expectedKey);
    }
}
