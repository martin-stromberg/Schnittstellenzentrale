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

/// <summary>bUnit-Tests für Laden, Auswahl, Erstellen und Löschen in <see cref="EnvironmentsSidebar"/>.</summary>
public class EnvironmentsSidebarTests_State : BunitContext
{
    private readonly Mock<ISystemEnvironmentRepository> _repositoryMock = new();
    private readonly Mock<IStorageModeService> _storageModeServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IActiveEnvironmentService> _activeEnvironmentServiceMock = new();

    /// <summary>Registriert Standard-Mocks und Lokalisierung für die Sidebar.</summary>
    public EnvironmentsSidebarTests_State()
    {
        _storageModeServiceMock.SetupGet(s => s.CurrentMode).Returns(StorageMode.Team);
        _currentUserServiceMock.Setup(s => s.GetCurrentUserName()).Returns("tester");
        _repositoryMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync(new List<SystemEnvironment>());

        Services.AddSingleton(_repositoryMock.Object);
        Services.AddSingleton(_storageModeServiceMock.Object);
        Services.AddSingleton(_currentUserServiceMock.Object);
        Services.AddSingleton(_activeEnvironmentServiceMock.Object);
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    /// <summary>Die Initialisierung lädt Umgebungen und rendert deren Namen.</summary>
    [Fact]
    public void OnInitialized_LoadsAndRendersEnvironmentList()
    {
        _repositoryMock
            .Setup(r => r.GetEnvironmentsAsync(StorageMode.Team, "tester"))
            .ReturnsAsync(new List<SystemEnvironment>
            {
                new() { Id = 1, Name = "Team A", Mode = StorageMode.Team, Variables = [] },
                new() { Id = 2, Name = "Team B", Mode = StorageMode.Team, Variables = [] }
            });

        var cut = Render<EnvironmentsSidebar>();

        Assert.Contains("Team A", cut.Markup);
        Assert.Contains("Team B", cut.Markup);
    }

    /// <summary>Die Auswahl einer Umgebung löst den Selection-Callback mit der ID aus.</summary>
    [Fact]
    public async Task SelectEnvironment_InvokesSelectionCallbackWithId()
    {
        _repositoryMock
            .Setup(r => r.GetEnvironmentsAsync(StorageMode.Team, "tester"))
            .ReturnsAsync(new List<SystemEnvironment>
            {
                new() { Id = 11, Name = "Select Me", Mode = StorageMode.Team, Variables = [] }
            });

        var selected = 0;
        var cut = Render<EnvironmentsSidebar>(parameters => parameters
            .Add(x => x.OnEnvironmentSelected, EventCallback.Factory.Create<int>(this, id => selected = id)));

        var selectButton = cut.Find("button.sz-env-list-btn");
        await cut.InvokeAsync(() => selectButton.Click());

        Assert.Equal(11, selected);
        Assert.Contains("active", cut.Find("li.sz-env-list-item").ClassList);
    }

    /// <summary>Leerer Name beim Erstellen zeigt Validierungsfehler und führt kein Add aus.</summary>
    [Fact]
    public async Task ConfirmCreateAsync_WhenNameIsEmpty_ShowsValidationError()
    {
        var cut = Render<EnvironmentsSidebar>();

        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());
        await cut.InvokeAsync(() => cut.FindAll("button.sz-btn-primary").Last().Click());

        Assert.Contains("EnvironmentsSidebar_Error_NameEmpty", cut.Markup);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SystemEnvironment>()), Times.Never);
    }

    /// <summary>Gültiges Erstellen ruft Repository und Umgebungslisten-Benachrichtigung auf.</summary>
    [Fact]
    public async Task ConfirmCreateAsync_WhenValid_AddsEnvironment_AndNotifies()
    {
        _repositoryMock
            .SetupSequence(r => r.GetEnvironmentsAsync(StorageMode.Team, "tester"))
            .ReturnsAsync(new List<SystemEnvironment>())
            .ReturnsAsync(new List<SystemEnvironment>
            {
                new() { Id = 21, Name = "Created", Mode = StorageMode.Team, Variables = [] }
            });

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SystemEnvironment>()))
            .ReturnsAsync(new SystemEnvironment { Id = 21, Name = "Created", Mode = StorageMode.Team, Variables = [] });

        var cut = Render<EnvironmentsSidebar>();

        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());
        await cut.InvokeAsync(() => cut.Find("input.sz-input").Change("Created"));
        await cut.InvokeAsync(() => cut.FindAll("button.sz-btn-primary").Last().Click());

        _repositoryMock.Verify(r => r.AddAsync(It.Is<SystemEnvironment>(e => e.Name == "Created")), Times.Once);
        _activeEnvironmentServiceMock.Verify(s => s.NotifyEnvironmentListChanged(), Times.Once);
        Assert.Contains("Created", cut.Markup);
    }

    /// <summary>Fehler beim Löschen werden als Fehlermeldung angezeigt.</summary>
    [Fact]
    public async Task DeleteAsync_WhenRepositoryThrows_ShowsDeleteError()
    {
        _repositoryMock
            .Setup(r => r.GetEnvironmentsAsync(StorageMode.Team, "tester"))
            .ReturnsAsync(new List<SystemEnvironment>
            {
                new() { Id = 31, Name = "Cannot delete", Mode = StorageMode.Team, Variables = [] }
            });

        _repositoryMock
            .Setup(r => r.DeleteAsync(31))
            .ThrowsAsync(new InvalidOperationException("delete failed"));

        var cut = Render<EnvironmentsSidebar>();

        var deleteButton = cut.Find("button.sz-btn-icon");
        await cut.InvokeAsync(() => deleteButton.Click());

        Assert.Contains("EnvironmentsSidebar_Error_Delete", cut.Markup);
    }
}
