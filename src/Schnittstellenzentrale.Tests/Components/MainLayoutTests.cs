using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Schnittstellenzentrale.Components.Layout;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für die <see cref="AppShell"/>- und <see cref="TopBar"/>-Komponenten.</summary>
public class MainLayoutTests : BunitContext
{
    private readonly Mock<IStorageModeService> _storageMock = new();
    private readonly Mock<IThemeService> _themeMock = new();
    private readonly Mock<IActiveEnvironmentService> _activeEnvMock = new();
    private readonly Mock<ISystemEnvironmentRepository> _envRepoMock = new();
    private readonly Mock<IApplicationApiClient> _apiClientMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ISignalRNotificationService> _signalRMock = new();
    private readonly Mock<IActivityLogService> _activityLogMock = new();
    private readonly Mock<INavigationStateService> _navigationStateMock = new();

    /// <summary>Initialisiert die Test-Services und JS-Interop-Mocks.</summary>
    public MainLayoutTests()
    {
        _storageMock.Setup(s => s.CurrentMode).Returns(StorageMode.Team);
        _activeEnvMock.Setup(s => s.ActiveEnvironment).Returns((SystemEnvironment?)null);
        _activeEnvMock.Setup(s => s.ActiveVariables).Returns(new Dictionary<string, string>());
        _envRepoMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync([]);
        _apiClientMock
            .Setup(c => c.GetEnvironmentByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((SystemEnvironment?)null);
        _currentUserMock.Setup(s => s.GetCurrentUserName()).Returns("DOMAIN\\testuser");
        _activityLogMock.Setup(s => s.Entries).Returns([]);
        _navigationStateMock.Setup(s => s.CurrentArea).Returns(NavigationArea.Workspaces);
        _navigationStateMock.Setup(s => s.CurrentSelection).Returns((WorkspaceSelection?)null);
        _navigationStateMock.Setup(s => s.CurrentSelectionPath).Returns([]);

        Services.AddSingleton(_storageMock.Object);
        Services.AddSingleton(_themeMock.Object);
        Services.AddSingleton(_activeEnvMock.Object);
        Services.AddSingleton(_envRepoMock.Object);
        Services.AddSingleton(_apiClientMock.Object);
        Services.AddSingleton(_currentUserMock.Object);
        Services.AddSingleton(_signalRMock.Object);
        Services.AddSingleton(_activityLogMock.Object);
        Services.AddSingleton(_navigationStateMock.Object);
        Services.AddSingleton<ILogger<AppShell>>(NullLogger<AppShell>.Instance);
        Services.AddSingleton(Options.Create(new UploadSettings()));
        Services.AddSingleton(Options.Create(new HistorySettings()));
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());

        JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);
        JSInterop.SetupVoid("localStorage.removeItem", _ => true).SetVoidResult();
        JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();

        ComponentFactories.AddStub<WorkspacesLayout>();
        ComponentFactories.AddStub<EnvironmentsLayout>();
        ComponentFactories.AddStub<HistoryLayout>();
    }

    /// <summary>AppShell rendert den Bereichs-Tab Workspaces in TopBar.</summary>
    [Fact]
    public void AppShell_RendertWorkspacesTab()
    {
        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        var buttons = cut.FindAll("button.sz-topbar-tab");
        Assert.Contains(buttons, b => b.TextContent.Contains("Workspaces"));
    }

    /// <summary>AppShell rendert den Bereichs-Tab Environments in TopBar.</summary>
    [Fact]
    public void AppShell_RendertEnvironmentsTab()
    {
        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        var buttons = cut.FindAll("button.sz-topbar-tab");
        Assert.Contains(buttons, b => b.TextContent.Contains("Environments"));
    }

    /// <summary>AppShell rendert den Bereichs-Tab History in TopBar.</summary>
    [Fact]
    public void AppShell_RendertHistoryTab()
    {
        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        var buttons = cut.FindAll("button.sz-topbar-tab");
        Assert.Contains(buttons, b => b.TextContent.Contains("History"));
    }

    /// <summary>AppShell rendert den StorageMode-Selektor in TopBar.</summary>
    [Fact]
    public void AppShell_RendertModusSelektor()
    {
        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        Assert.NotEmpty(cut.FindAll("select.sz-topbar-select"));
    }

    /// <summary>AppShell rendert das Profil-Icon mit Initiale in TopBar.</summary>
    [Fact]
    public void AppShell_RendertProfilIcon()
    {
        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        var profileIcon = cut.Find("span.sz-topbar-profile");
        Assert.Equal("T", profileIcon.TextContent);
    }

    /// <summary>DisposeAsync kann ohne Fehler aufgerufen werden, wenn keine HubConnection aufgebaut wurde.</summary>
    [Fact]
    public async Task DisposeAsync_OhneHubConnection_WirftKeinenFehler()
    {
        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        var exception = await Record.ExceptionAsync(() => cut.Instance.DisposeAsync().AsTask());

        Assert.Null(exception);
    }

    /// <summary>SetActiveEnvironment wird mit der aus dem API geladenen Umgebung aufgerufen, wenn eine ID im localStorage liegt.</summary>
    [Fact]
    public async Task Wiederherstellen_GespeicherteIdVorhanden_SetzAktiveUmgebung()
    {
        var env = TestMockFactory.CreateEnv(42, "Dev");
        var key = LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team);
        JSInterop.Setup<string?>("localStorage.getItem", inv => inv.Arguments.ElementAt(0) is string s && s == key).SetResult("42");
        _apiClientMock.Setup(c => c.GetEnvironmentByIdAsync(42)).ReturnsAsync(env);

        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));
        await cut.InvokeAsync(() => { });

        _activeEnvMock.Verify(s => s.SetActiveEnvironment(env), Times.Once);
    }

    /// <summary>SetActiveEnvironment(null) und localStorage.removeItem werden aufgerufen, wenn die gespeicherte ID nicht mehr via API zurückkommt.</summary>
    [Fact]
    public async Task Wiederherstellen_UmgebungNichtMehrInDb_BereinigTLocalStorage()
    {
        var key = LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team);
        JSInterop.Setup<string?>("localStorage.getItem", inv => inv.Arguments.ElementAt(0) is string s && s == key).SetResult("99");
        _apiClientMock.Setup(c => c.GetEnvironmentByIdAsync(99)).ReturnsAsync((SystemEnvironment?)null);

        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));
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

        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));
        await cut.InvokeAsync(() => { });

        _activeEnvMock.Verify(s => s.SetActiveEnvironment(It.Is<SystemEnvironment?>(e => e != null)), Times.Never);
    }

    /// <summary>OnAreaChanged wird nach SetAreaAsync gefeuert.</summary>
    [Fact]
    public async Task AppShell_SetAreaAsync_AktualisiertBereich()
    {
        _navigationStateMock.Setup(s => s.SetAreaAsync(NavigationArea.History))
            .Callback(() => _navigationStateMock.Setup(s => s.CurrentArea).Returns(NavigationArea.History))
            .Returns(Task.CompletedTask);

        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        await cut.InvokeAsync(() => _navigationStateMock.Object.SetAreaAsync(NavigationArea.History));

        _navigationStateMock.Verify(s => s.SetAreaAsync(NavigationArea.History), Times.Once);
    }

    /// <summary>Nach Moduswechsel wird der Schlüssel des neuen Modus für localStorage.getItem verwendet.</summary>
    [Fact]
    public async Task Wiederherstellen_BeiModuswechsel_VerwendetNeuenSchlüssel()
    {
        var userKey = LocalStorageKeys.SelectedEnvironmentId(StorageMode.User);
        JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);

        // Nach SetMode(User) muss CurrentMode User zurückliefern, damit AppShell den richtigen Key verwendet
        var currentMode = StorageMode.Team;
        _storageMock.Setup(s => s.CurrentMode).Returns(() => currentMode);
        _storageMock.Setup(s => s.SetMode(It.IsAny<StorageMode>()))
            .Callback<StorageMode>(m =>
            {
                currentMode = m;
                _storageMock.Raise(s => s.OnModeChanged += null);
            });

        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));

        JSInterop.Setup<string?>("localStorage.getItem", inv => inv.Arguments.ElementAt(0) is string s && s == userKey).SetResult(null);

        await cut.InvokeAsync(() => cut.Find("select.sz-topbar-select").Change(StorageMode.User.ToString()));

        var getItemInvocations = JSInterop.Invocations
            .Where(i => i.Identifier == "localStorage.getItem")
            .ToList();
        Assert.Contains(getItemInvocations, i => i.Arguments.ElementAt(0) is string s && s == userKey);
    }
}
