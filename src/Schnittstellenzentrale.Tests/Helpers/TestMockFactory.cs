using Microsoft.Extensions.Localization;
using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Resources;

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

    /// <summary>Erstellt einen <see cref="IStringLocalizer{SharedResources}"/>, der jeden Schlüssel unverändert als Wert zurückgibt.</summary>
    public static IStringLocalizer<SharedResources> CreateFakeLocalizer()
    {
        var mock = new Mock<IStringLocalizer<SharedResources>>();
        mock.Setup(l => l[It.IsAny<string>()])
            .Returns<string>(key => new LocalizedString(key, key));
        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns<string, object[]>((key, args) => new LocalizedString(key, string.Format(key, args)));
        return mock.Object;
    }
}
