using System.Reflection;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für Inline-Editing und Persistenzpfade in <see cref="EnvironmentContentView"/>.</summary>
public class EnvironmentContentViewTests_State : BunitContext
{
    private readonly Mock<ISystemEnvironmentRepository> _repositoryMock = new();
    private readonly Mock<IActiveEnvironmentService> _activeEnvironmentServiceMock = new();

    /// <summary>Registriert Mocks, Lokalisierung und stubbt den Kind-Editor für fokussierte Parent-Tests.</summary>
    public EnvironmentContentViewTests_State()
    {
        ComponentFactories.AddStub<EnvironmentEditor>();
        Services.AddSingleton(_repositoryMock.Object);
        Services.AddSingleton(_activeEnvironmentServiceMock.Object);
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    /// <summary>Beim Setzen der Parameter wird die Umgebung geladen und angezeigt.</summary>
    [Fact]
    public void OnParametersSetAsync_LoadsEnvironment_AndRendersName()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(41))
            .ReturnsAsync(new SystemEnvironment
            {
                Id = 41,
                Name = "Dev",
                Mode = StorageMode.Team,
                Variables = []
            });

        var cut = Render<EnvironmentContentView>(parameters => parameters.Add(x => x.EnvironmentId, 41));

        Assert.Contains("Dev", cut.Markup);
        _ = cut.FindComponent<Stub<EnvironmentEditor>>();
    }

    /// <summary>Enter im Namensfeld speichert den neuen Namen via Repository.</summary>
    [Fact]
    public async Task OnNameKeyDown_Enter_SavesName()
    {
        var environment = new SystemEnvironment { Id = 51, Name = "Before", Mode = StorageMode.Team, Variables = [] };
        _repositoryMock.Setup(r => r.GetByIdAsync(51)).ReturnsAsync(environment);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<SystemEnvironment>())).ReturnsAsync((SystemEnvironment env) => env);

        var cut = Render<EnvironmentContentView>(parameters => parameters.Add(x => x.EnvironmentId, 51));

        await cut.InvokeAsync(() => cut.Find("h1.sz-env-title").Click());
        await cut.InvokeAsync(() => cut.Find("input.sz-env-name-input").Change("After"));
        await cut.InvokeAsync(() => cut.Find("input.sz-env-name-input").KeyDown("Enter"));

        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<SystemEnvironment>(env => env.Id == 51 && env.Name == "After")), Times.Once);
    }

    /// <summary>Escape im Namensfeld bricht das Editing ab ohne Persistenzaufruf.</summary>
    [Fact]
    public async Task OnNameKeyDown_Escape_CancelsEditWithoutSaving()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(52))
            .ReturnsAsync(new SystemEnvironment { Id = 52, Name = "Name", Mode = StorageMode.Team, Variables = [] });

        var cut = Render<EnvironmentContentView>(parameters => parameters.Add(x => x.EnvironmentId, 52));

        await cut.InvokeAsync(() => cut.Find("h1.sz-env-title").Click());
        await cut.InvokeAsync(() => cut.Find("input.sz-env-name-input").KeyDown("Escape"));

        Assert.Empty(cut.FindAll("input.sz-env-name-input"));
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SystemEnvironment>()), Times.Never);
    }

    /// <summary>Beschreibung speichern aktualisiert die Umgebung über das Repository.</summary>
    [Fact]
    public async Task SaveDescriptionAsync_UpdatesEnvironmentDescription()
    {
        var environment = new SystemEnvironment
        {
            Id = 53,
            Name = "Env",
            Description = "old",
            Mode = StorageMode.Team,
            Variables = []
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(53)).ReturnsAsync(environment);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<SystemEnvironment>())).ReturnsAsync((SystemEnvironment env) => env);

        var cut = Render<EnvironmentContentView>(parameters => parameters.Add(x => x.EnvironmentId, 53));

        await cut.InvokeAsync(() => cut.Find("p.sz-env-subtitle").Click());
        await cut.InvokeAsync(() => cut.Find("textarea.sz-env-desc-input").Change("new description"));
        await cut.InvokeAsync(() => cut.Find("textarea.sz-env-desc-input").Blur());

        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<SystemEnvironment>(env => env.Description == "new description")), Times.Once);
    }

    /// <summary>Der interne OnSaved-Pfad lädt neu und informiert den ActiveEnvironmentService.</summary>
    [Fact]
    public async Task OnEnvironmentSaved_ReloadsEnvironment_AndNotifiesListChange()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(54)).ReturnsAsync(new SystemEnvironment
        {
            Id = 54,
            Name = "Initial",
            Mode = StorageMode.Team,
            Variables = []
        });

        var cut = Render<EnvironmentContentView>(parameters => parameters.Add(x => x.EnvironmentId, 54));

        _repositoryMock.Setup(r => r.GetByIdAsync(55)).ReturnsAsync(new SystemEnvironment
        {
            Id = 55,
            Name = "Reloaded",
            Mode = StorageMode.Team,
            Variables = []
        });

        var method = cut.Instance.GetType().GetMethod("OnEnvironmentSaved", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        var task = (Task?)method!.Invoke(cut.Instance, [new SystemEnvironment { Id = 55, Name = "Saved", Mode = StorageMode.Team, Variables = [] }]);
        Assert.NotNull(task);
        await task!;

        _repositoryMock.Verify(r => r.GetByIdAsync(55), Times.Once);
        _activeEnvironmentServiceMock.Verify(s => s.NotifyEnvironmentListChanged(), Times.Once);
    }
}
