using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Tests.Helpers;

/// <summary>Gemeinsame Mock-Fabrikmethoden für Unit-Tests.</summary>
public static class TestMockFactory
{
    /// <summary>Erstellt einen leeren <see cref="IActivityLogService"/>-Mock.</summary>
    public static Mock<IActivityLogService> CreateActivityLogServiceMock()
    {
        return new Mock<IActivityLogService>();
    }

    /// <summary>Erstellt eine <see cref="SystemEnvironment"/>-Testinstanz mit den angegebenen Werten.</summary>
    public static SystemEnvironment CreateEnv(int id, string name) => new()
    {
        Id = id,
        Name = name,
        Mode = StorageMode.Team,
        Variables = []
    };
}
