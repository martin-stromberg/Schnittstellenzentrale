using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Schnittstellenzentrale.Components.Layout;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

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

        Services.AddSingleton(_storageMock.Object);
        Services.AddSingleton(_themeMock.Object);
        Services.AddSingleton(_activeEnvMock.Object);
        Services.AddSingleton(_envRepoMock.Object);
        Services.AddSingleton(_currentUserMock.Object);
        Services.AddSingleton(_signalRMock.Object);
        Services.AddSingleton<ILogger<MainLayout>>(NullLogger<MainLayout>.Instance);

        JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);
        JSInterop.SetupVoid("localStorage.removeItem", _ => true);
        JSInterop.SetupVoid("localStorage.setItem", _ => true);
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
}
