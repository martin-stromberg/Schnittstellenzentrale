using Microsoft.JSInterop;
using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>Unit-Tests für <see cref="StorageModeService"/> mit gemocktem <see cref="IJSRuntime"/>.</summary>
public class StorageModeServiceTests
{
    private static (StorageModeService service, Mock<IJSRuntime> jsRuntimeMock, Mock<IJSObjectReference> moduleMock)
        CreateService(string? storedValue = null)
    {
        var moduleMock = new Mock<IJSObjectReference>();
        moduleMock
            .Setup(m => m.InvokeAsync<string?>("getStoredMode", It.IsAny<object?[]?>()))
            .ReturnsAsync(storedValue);
        moduleMock
            .Setup(m => m.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>("setStoredMode", It.IsAny<object?[]?>()))
            .ReturnsAsync(Mock.Of<Microsoft.JSInterop.Infrastructure.IJSVoidResult>());

        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(r => r.InvokeAsync<IJSObjectReference>("import", It.IsAny<object?[]?>()))
            .ReturnsAsync(moduleMock.Object);

        var service = new StorageModeService(jsRuntimeMock.Object);
        return (service, jsRuntimeMock, moduleMock);
    }

    /// <summary>InitializeAsync setzt CurrentMode auf den gespeicherten gültigen Wert.</summary>
    [Fact]
    public async Task InitializeAsync_SetsCurrentMode_WhenStoredValueIsValid()
    {
        var (service, _, _) = CreateService(storedValue: "User");

        await service.InitializeAsync();

        Assert.Equal(StorageMode.User, service.CurrentMode);
    }

    /// <summary>InitializeAsync behält StorageMode.Team, wenn kein Wert gespeichert ist.</summary>
    [Fact]
    public async Task InitializeAsync_KeepsDefaultMode_WhenNoStoredValue()
    {
        var (service, _, _) = CreateService(storedValue: null);

        await service.InitializeAsync();

        Assert.Equal(StorageMode.Team, service.CurrentMode);
    }

    /// <summary>InitializeAsync behält StorageMode.Team bei ungültigem gespeicherten Wert.</summary>
    [Fact]
    public async Task InitializeAsync_KeepsDefaultMode_WhenStoredValueIsInvalid()
    {
        var (service, _, _) = CreateService(storedValue: "invalid_value");

        await service.InitializeAsync();

        Assert.Equal(StorageMode.Team, service.CurrentMode);
    }

    /// <summary>SetMode schreibt den Wert per setStoredMode in localStorage.</summary>
    [Fact]
    public async Task SetMode_PersistsValueToLocalStorage()
    {
        var (service, _, moduleMock) = CreateService();

        service.SetMode(StorageMode.User);
        await Task.Yield();

        moduleMock.Verify(
            m => m.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                "setStoredMode",
                It.Is<object?[]?>(args => args != null && args.Length > 0 && args[0]!.ToString() == "User")),
            Times.Once);
    }

    /// <summary>SetMode löst OnModeChanged aus.</summary>
    [Fact]
    public void SetMode_FiresOnModeChanged()
    {
        var (service, _, _) = CreateService();
        var fired = false;
        service.OnModeChanged += () => fired = true;

        service.SetMode(StorageMode.User);

        Assert.True(fired);
    }

    /// <summary>SetMode feuert Event nicht, wenn Modus bereits gesetzt ist.</summary>
    [Fact]
    public void SetMode_DoesNotFire_WhenValueUnchanged()
    {
        var (service, _, _) = CreateService();
        var fired = false;
        service.OnModeChanged += () => fired = true;

        service.SetMode(StorageMode.Team);

        Assert.False(fired);
    }

    /// <summary>JS-Modul wird beim zweiten InitializeAsync-Aufruf nicht erneut importiert.</summary>
    [Fact]
    public async Task InitializeAsync_ImportsModuleOnlyOnce_WhenCalledTwice()
    {
        var (service, jsRuntimeMock, _) = CreateService();

        await service.InitializeAsync();
        await service.InitializeAsync();

        jsRuntimeMock.Verify(
            r => r.InvokeAsync<IJSObjectReference>("import", It.IsAny<object?[]?>()),
            Times.Once);
    }
}
