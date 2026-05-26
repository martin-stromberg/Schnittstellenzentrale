using Moq;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Tests.Helpers;

/// <summary>Gemeinsame Mock-Fabrikmethoden für Unit-Tests.</summary>
public static class TestMockFactory
{
    /// <summary>Erstellt einen leeren <see cref="IActivityLogService"/>-Mock.</summary>
    public static Mock<IActivityLogService> CreateActivityLogServiceMock()
    {
        return new Mock<IActivityLogService>();
    }
}
