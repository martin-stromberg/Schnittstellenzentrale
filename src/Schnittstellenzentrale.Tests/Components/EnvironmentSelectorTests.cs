using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für die <see cref="EnvironmentSelector"/>-Komponente.</summary>
public class EnvironmentSelectorTests : BunitContext
{
    private readonly Mock<ISystemEnvironmentRepository> _envRepoMock = new();
    private readonly Mock<IActiveEnvironmentService> _activeEnvMock = new();
    private readonly Mock<IStorageModeService> _storageMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    /// <summary>Initialisiert die Test-Services.</summary>
    public EnvironmentSelectorTests()
    {
        _storageMock.Setup(s => s.CurrentMode).Returns(StorageMode.Team);
        _activeEnvMock.Setup(s => s.ActiveEnvironment).Returns((SystemEnvironment?)null);
        _envRepoMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync([]);

        Services.AddSingleton(_envRepoMock.Object);
        Services.AddSingleton(_activeEnvMock.Object);
        Services.AddSingleton(_storageMock.Object);
        Services.AddSingleton(_currentUserMock.Object);

        JSInterop.SetupVoid("localStorage.removeItem", _ => true).SetVoidResult();
        JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();
    }

    /// <summary>Umgebungen aus dem Repository erscheinen als Optionen im Dropdown.</summary>
    [Fact]
    public void RendertUmgebungenAusRepository()
    {
        var envs = new List<SystemEnvironment> { TestMockFactory.CreateEnv(1, "Dev"), TestMockFactory.CreateEnv(2, "Prod") };
        _envRepoMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync(envs);

        var cut = Render<EnvironmentSelector>();

        var options = cut.FindAll("option");
        Assert.Contains(options, o => o.TextContent == "Dev");
        Assert.Contains(options, o => o.TextContent == "Prod");
    }

    /// <summary>Die aktive Umgebung wird im Dropdown vorausgewählt.</summary>
    [Fact]
    public void AktiveUmgebungWirdVorausgewählt()
    {
        var env = TestMockFactory.CreateEnv(3, "Staging");
        _envRepoMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync([env]);
        _activeEnvMock.Setup(s => s.ActiveEnvironment).Returns(env);

        var cut = Render<EnvironmentSelector>();

        var select = cut.Find("select");
        Assert.Equal("3", select.GetAttribute("value"));
    }

    /// <summary>Ohne aktive Umgebung zeigt das Dropdown keinen vorgewählten Wert.</summary>
    [Fact]
    public void OhneAktiveUmgebung_ZeigtKeineVorauswahl()
    {
        _activeEnvMock.Setup(s => s.ActiveEnvironment).Returns((SystemEnvironment?)null);

        var cut = Render<EnvironmentSelector>();

        var select = cut.Find("select");
        Assert.Equal(string.Empty, select.GetAttribute("value"));
    }

    /// <summary>RefreshAsync aktualisiert die Liste und wirft keine Dispatcher-Exception.</summary>
    [Fact]
    public async Task RefreshAsync_AktualistertListeOhneFehler()
    {
        var cut = Render<EnvironmentSelector>();
        Assert.Single(cut.FindAll("option")); // nur "— Keine Umgebung —"

        var env = TestMockFactory.CreateEnv(5, "Neu");
        _envRepoMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync([env]);

        await cut.InvokeAsync(() => cut.Instance.RefreshAsync());

        Assert.Contains(cut.FindAll("option"), o => o.TextContent == "Neu");
    }

    /// <summary>Bei Auswahl einer Umgebung wird localStorage.setItem mit korrektem Schlüssel und ID aufgerufen.</summary>
    [Fact]
    public async Task AuswählenEinerUmgebung_SchreibtLocalStorage()
    {
        var env = TestMockFactory.CreateEnv(7, "Prod");
        _envRepoMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync([env]);

        var cut = Render<EnvironmentSelector>();
        var expectedKey = LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team);

        await cut.InvokeAsync(() => cut.Find("select").Change("7"));

        var setItemCalls = JSInterop.Invocations
            .Where(i => i.Identifier == "localStorage.setItem")
            .ToList();
        Assert.Single(setItemCalls);
        Assert.Equal(expectedKey, setItemCalls[0].Arguments.ElementAt(0));
        Assert.Equal("7", setItemCalls[0].Arguments.ElementAt(1));
    }

    /// <summary>Bei Abwahl (leere Auswahl) wird localStorage.removeItem mit korrektem Schlüssel aufgerufen.</summary>
    [Fact]
    public async Task AbwählenEinerUmgebung_EntferntLocalStorage()
    {
        var env = TestMockFactory.CreateEnv(7, "Prod");
        _envRepoMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync([env]);

        var cut = Render<EnvironmentSelector>();
        var expectedKey = LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team);

        await cut.InvokeAsync(() => cut.Find("select").Change(string.Empty));

        var removeItemCalls = JSInterop.Invocations
            .Where(i => i.Identifier == "localStorage.removeItem")
            .ToList();
        Assert.Single(removeItemCalls);
        Assert.Equal(expectedKey, removeItemCalls[0].Arguments.ElementAt(0));
    }

    /// <summary>Bei Auswahl einer ID, die nicht in _environments vorhanden ist, wird localStorage.removeItem aufgerufen und SetActiveEnvironment(null) gesetzt.</summary>
    [Fact]
    public async Task AuswählenNichtExistierenderId_EntferntLocalStorageUndSetztNull()
    {
        _envRepoMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync([]);

        var cut = Render<EnvironmentSelector>();
        var expectedKey = LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team);

        await cut.InvokeAsync(() => cut.Find("select").Change("99"));

        var removeItemCalls = JSInterop.Invocations
            .Where(i => i.Identifier == "localStorage.removeItem")
            .ToList();
        Assert.Single(removeItemCalls);
        Assert.Equal(expectedKey, removeItemCalls[0].Arguments.ElementAt(0));
        _activeEnvMock.Verify(s => s.SetActiveEnvironment(null), Times.Once);
    }
}
