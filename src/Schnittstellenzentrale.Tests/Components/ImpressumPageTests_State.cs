using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Components.Pages;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für Initialisierungs- und Fallback-Pfade von <see cref="ImpressumPage"/>.</summary>
public class ImpressumPageTests_State : BunitContext
{
    private readonly Mock<IImpressumService> _impressumServiceMock = new();
    private readonly Mock<INavigationStateService> _navigationStateServiceMock = new();

    /// <summary>Registriert Mocks und Lokalisierung für die Impressum-Komponente.</summary>
    public ImpressumPageTests_State()
    {
        _navigationStateServiceMock
            .Setup(n => n.SetAreaAsync(It.IsAny<NavigationArea>()))
            .Returns(Task.CompletedTask);

        Services.AddSingleton(_impressumServiceMock.Object);
        Services.AddSingleton(_navigationStateServiceMock.Object);
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    /// <summary>Wenn kein Impressum vorhanden ist, wird der Hinweistext gerendert.</summary>
    [Fact]
    public void OnInitialized_WhenImpressumNotAvailable_ShowsNotAvailableHint()
    {
        _impressumServiceMock.Setup(s => s.IsAvailable()).Returns(false);

        var cut = Render<ImpressumPage>();

        Assert.Contains("ImpressumPage_NotAvailable", cut.Markup);
        _navigationStateServiceMock.Verify(n => n.SetAreaAsync(NavigationArea.Impressum), Times.Once);
    }

    /// <summary>Wenn ein Impressum vorhanden ist, wird der HTML-Inhalt ausgegeben.</summary>
    [Fact]
    public void OnInitialized_WhenImpressumAvailable_RendersHtmlContent()
    {
        _impressumServiceMock.Setup(s => s.IsAvailable()).Returns(true);
        _impressumServiceMock.Setup(s => s.GetContentAsHtmlAsync()).ReturnsAsync("<p>Impressum Test</p>");

        var cut = Render<ImpressumPage>();

        Assert.Contains("<p>Impressum Test</p>", cut.Markup);
        _navigationStateServiceMock.Verify(n => n.SetAreaAsync(NavigationArea.Impressum), Times.Once);
    }

    /// <summary>Wenn das Lesen fehlschlägt, fällt die Seite auf den Nicht-verfügbar-Hinweis zurück.</summary>
    [Fact]
    public void OnInitialized_WhenReadingImpressumFails_ShowsNotAvailableHint()
    {
        _impressumServiceMock.Setup(s => s.IsAvailable()).Returns(true);
        _impressumServiceMock.Setup(s => s.GetContentAsHtmlAsync()).ThrowsAsync(new IOException("no access"));

        var cut = Render<ImpressumPage>();

        Assert.Contains("ImpressumPage_NotAvailable", cut.Markup);
    }
}
