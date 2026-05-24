using Microsoft.JSInterop;
using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>ThemeServiceTests</summary>
public class ThemeServiceTests
{
    private static (ThemeService service, Mock<IJSRuntime> jsRuntimeMock, Mock<IJSObjectReference> moduleMock)
        CreateService(string? storedValue = null)
    {
        var moduleMock = new Mock<IJSObjectReference>();
        moduleMock
            .Setup(m => m.InvokeAsync<string?>("getStoredTheme", It.IsAny<object?[]?>()))
            .ReturnsAsync(storedValue);
        moduleMock
            .Setup(m => m.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>("setStoredTheme", It.IsAny<object?[]?>()))
            .ReturnsAsync(Mock.Of<Microsoft.JSInterop.Infrastructure.IJSVoidResult>());
        moduleMock
            .Setup(m => m.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>("applyTheme", It.IsAny<object?[]?>()))
            .ReturnsAsync(Mock.Of<Microsoft.JSInterop.Infrastructure.IJSVoidResult>());

        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(r => r.InvokeAsync<IJSObjectReference>("import", It.IsAny<object?[]?>()))
            .ReturnsAsync(moduleMock.Object);

        var service = new ThemeService(jsRuntimeMock.Object);
        return (service, jsRuntimeMock, moduleMock);
    }

    /// <summary>InitialTheme_IsLight_WhenNoStoredPreference</summary>
    [Fact]
    public async Task InitialTheme_IsLight_WhenNoStoredPreference()
    {
        var (service, _, _) = CreateService(storedValue: null);

        await service.InitializeAsync();

        Assert.Equal(ColorScheme.Light, service.CurrentScheme);
    }

    /// <summary>InitialTheme_IsStoredValue_WhenPreferenceExists</summary>
    [Fact]
    public async Task InitialTheme_IsStoredValue_WhenPreferenceExists()
    {
        var (service, _, _) = CreateService(storedValue: "Dark");

        await service.InitializeAsync();

        Assert.Equal(ColorScheme.Dark, service.CurrentScheme);
    }

    /// <summary>SetTheme_FiresOnThemeChanged</summary>
    [Fact]
    public async Task SetTheme_FiresOnThemeChanged()
    {
        var (service, _, _) = CreateService();
        var fired = false;
        service.OnThemeChanged += () => fired = true;

        await service.SetTheme(ColorScheme.Dark);

        Assert.True(fired);
    }

    /// <summary>SetTheme_DoesNotFire_WhenValueUnchanged</summary>
    [Fact]
    public async Task SetTheme_DoesNotFire_WhenValueUnchanged()
    {
        var (service, _, _) = CreateService();
        var fired = false;
        service.OnThemeChanged += () => fired = true;

        await service.SetTheme(ColorScheme.Light);

        Assert.False(fired);
    }

    /// <summary>InitialTheme_IsLight_WhenStoredValueIsInvalid</summary>
    [Fact]
    public async Task InitialTheme_IsLight_WhenStoredValueIsInvalid()
    {
        var (service, _, _) = CreateService(storedValue: "invalid_value");

        await service.InitializeAsync();

        Assert.Equal(ColorScheme.Light, service.CurrentScheme);
    }

    /// <summary>InitializeAsync_ImportsModuleOnlyOnce_WhenCalledTwice</summary>
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

    /// <summary>SetTheme_PersistsValueToLocalStorage</summary>
    [Fact]
    public async Task SetTheme_PersistsValueToLocalStorage()
    {
        var (service, _, moduleMock) = CreateService();

        await service.SetTheme(ColorScheme.Dark);

        moduleMock.Verify(
            m => m.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                "setStoredTheme",
                It.Is<object?[]?>(args => IsDarkArg(args))),
            Times.Once);
    }

    /// <summary>SetTheme_AppliesThemeToDocument</summary>
    [Fact]
    public async Task SetTheme_AppliesThemeToDocument()
    {
        var (service, _, moduleMock) = CreateService();

        await service.SetTheme(ColorScheme.Dark);

        moduleMock.Verify(
            m => m.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                "applyTheme",
                It.Is<object?[]?>(args => IsDarkArg(args))),
            Times.Once);
    }

    /// <summary>SetTheme_ThrowsArgumentOutOfRangeException_WhenSchemeIsUndefined</summary>
    [Fact]
    public async Task SetTheme_ThrowsArgumentOutOfRangeException_WhenSchemeIsUndefined()
    {
        var (service, _, _) = CreateService();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.SetTheme((ColorScheme)999));
    }

    private static bool IsDarkArg(object?[]? args) =>
        args is [var a, ..] && a?.ToString() == "Dark";
}
