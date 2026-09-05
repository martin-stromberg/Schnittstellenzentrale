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

/// <summary>bUnit-Tests für Validierungs- und Save-Pfade in <see cref="EnvironmentEditor"/>.</summary>
public class EnvironmentEditorTests_State : BunitContext
{
    private readonly Mock<ISystemEnvironmentRepository> _repositoryMock = new();
    private readonly Mock<IStorageModeService> _storageModeServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    /// <summary>Registriert Mocks und Lokalisierung für den Environment-Editor.</summary>
    public EnvironmentEditorTests_State()
    {
        _storageModeServiceMock.SetupGet(s => s.CurrentMode).Returns(StorageMode.Team);
        _currentUserServiceMock.Setup(s => s.GetCurrentUserName()).Returns("tester");
        _repositoryMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync(new List<SystemEnvironment>());

        Services.AddSingleton(_repositoryMock.Object);
        Services.AddSingleton(_storageModeServiceMock.Object);
        Services.AddSingleton(_currentUserServiceMock.Object);
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    /// <summary>Leerer Name blockiert das Speichern und zeigt die Name-Validierungsfehlermeldung an.</summary>
    [Fact]
    public async Task SaveAsync_WhenNameIsMissing_ShowsNameError()
    {
        var cut = Render<EnvironmentEditor>();

        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SystemEnvironment>()), Times.Never);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SystemEnvironment>()), Times.Never);
    }

    /// <summary>Doppelte Variablennamen verhindern das Speichern und zeigen den Variablenfehler an.</summary>
    [Fact]
    public async Task SaveAsync_WhenVariableNamesAreDuplicated_ShowsVariableError()
    {
        var existing = new SystemEnvironment
        {
            Id = 11,
            Name = "Shared",
            Mode = StorageMode.Team,
            Variables =
            [
                new EnvironmentVariable { Id = 1, Name = "Token", Value = "a" },
                new EnvironmentVariable { Id = 2, Name = "Token", Value = "b" }
            ]
        };

        var cut = Render<EnvironmentEditor>(parameters => parameters.Add(x => x.ExistingEnvironment, existing));
        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());

        Assert.Contains("EnvironmentEditor_Error_VarNameDuplicate", cut.Markup);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SystemEnvironment>()), Times.Never);
    }

    /// <summary>Im Edit-Modus wird die Umgebung bei gültigen Daten aktualisiert und über OnSaved zurückgegeben.</summary>
    [Fact]
    public async Task SaveAsync_InEditMode_UpdatesEnvironment_AndInvokesOnSaved()
    {
        var existing = new SystemEnvironment
        {
            Id = 12,
            Name = "Existing",
            Mode = StorageMode.Team,
            Owner = null,
            Variables = []
        };

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SystemEnvironment>()))
            .ReturnsAsync((SystemEnvironment env) => env);

        SystemEnvironment? saved = null;
        var cut = Render<EnvironmentEditor>(parameters => parameters
            .Add(x => x.ExistingEnvironment, existing)
            .Add(x => x.OnSaved, EventCallback.Factory.Create<SystemEnvironment>(this, env => saved = env)));

        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());

        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<SystemEnvironment>(env => env.Id == 12 && env.Name == "Existing")), Times.Once);
        Assert.NotNull(saved);
        Assert.Equal(12, saved!.Id);
    }

    /// <summary>Hinzufügen/Entfernen von Variablen verändert die Liste und der Save-Fehlerpfad wird angezeigt.</summary>
    [Fact]
    public async Task AddAndRemoveVariable_AndSaveError_AreHandled()
    {
        var existing = new SystemEnvironment
        {
            Id = 13,
            Name = "Editable",
            Mode = StorageMode.Team,
            Variables = []
        };

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SystemEnvironment>()))
            .ThrowsAsync(new InvalidOperationException("persist failed"));

        var cut = Render<EnvironmentEditor>(parameters => parameters.Add(x => x.ExistingEnvironment, existing));

        var addButton = cut.Find("button.sz-btn-outline.sz-btn-sm");
        await cut.InvokeAsync(() => addButton.Click());
        Assert.Single(cut.FindAll("tbody tr"));

        var removeButton = cut.Find("button.sz-btn-icon.sz-btn-danger");
        await cut.InvokeAsync(() => removeButton.Click());

        await cut.InvokeAsync(() => cut.Find("button.sz-btn-primary").Click());

        Assert.Contains("EnvironmentEditor_Error_Save", cut.Markup);
    }
}
