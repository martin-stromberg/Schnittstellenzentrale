using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Schnittstellenzentrale.Components.Layout;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für die Initialisierungsreihenfolge in <see cref="AppShell"/>.</summary>
public class AppShellTests : BunitContext
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
    public AppShellTests()
    {
        _storageMock.Setup(s => s.CurrentMode).Returns(StorageMode.Team);
        _storageMock.Setup(s => s.InitializeAsync()).Returns(Task.CompletedTask);
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

    /// <summary>StorageModeService.InitializeAsync() wird beim ersten Render vor RestoreEnvironmentFromLocalStorageAsync aufgerufen.</summary>
    [Fact]
    public async Task OnAfterRender_CallsStorageModeInitializeAsync_BeforeRestoreEnvironment()
    {
        var callOrder = new List<string>();

        // Snapshot der localStorage.getItem-Aufrufe zum Zeitpunkt, zu dem InitializeAsync ausgeführt wird.
        // Sind null: InitializeAsync noch nicht aufgerufen.
        int? localStorageCallCountBeforeInit = null;

        _storageMock
            .Setup(s => s.InitializeAsync())
            .Callback(() =>
            {
                localStorageCallCountBeforeInit = JSInterop.Invocations
                    .Count(i => i.Identifier == "localStorage.getItem");
                callOrder.Add("InitializeAsync");
            })
            .Returns(Task.CompletedTask);

        var cut = Render<AppShell>(p => p.Add(x => x.Body, (RenderFragment)(_ => { })));
        await cut.InvokeAsync(() => { });

        var localStorageCallCountAfterRender = JSInterop.Invocations
            .Count(i => i.Identifier == "localStorage.getItem");

        _storageMock.Verify(s => s.InitializeAsync(), Times.Once);
        Assert.NotNull(localStorageCallCountBeforeInit);
        Assert.True(localStorageCallCountAfterRender > 0,
            "localStorage.getItem muss nach InitializeAsync aufgerufen worden sein.");
        Assert.True(localStorageCallCountBeforeInit.Value == 0,
            "localStorage.getItem darf zum Zeitpunkt von InitializeAsync noch nicht aufgerufen worden sein.");
    }
}
