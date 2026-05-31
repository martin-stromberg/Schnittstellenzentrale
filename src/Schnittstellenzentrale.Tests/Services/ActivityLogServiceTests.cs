using Microsoft.Extensions.Logging.Abstractions;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>Unit-Tests für <see cref="ActivityLogService"/>.</summary>
public class ActivityLogServiceTests
{
    private static ActivityLogService CreateService() => new(NullLogger<ActivityLogService>.Instance);

    /// <summary>Log_ErstelltEintragMitKorrektenFeldern</summary>
    [Fact]
    public void Log_ErstelltEintragMitKorrektenFeldern()
    {
        var service = CreateService();
        var before = DateTime.UtcNow;

        service.Log(ActivityLogCategory.EndpointExecuted, "Test-Nachricht", "Test-Details");

        var after = DateTime.UtcNow;
        Assert.Single(service.Entries);
        var entry = service.Entries[0];
        Assert.Equal(ActivityLogCategory.EndpointExecuted, entry.Category);
        Assert.Equal("Test-Nachricht", entry.Message);
        Assert.Equal("Test-Details", entry.Details);
        Assert.True(entry.Timestamp >= before && entry.Timestamp <= after);
    }

    /// <summary>Log_FeuertOnEntryAdded</summary>
    [Fact]
    public void Log_FeuertOnEntryAdded()
    {
        var service = CreateService();
        var fired = false;
        service.OnEntryAdded += () => fired = true;

        service.Log(ActivityLogCategory.EntityCreated, "Erstellt");

        Assert.True(fired);
    }

    /// <summary>Log_EventFehler_WirdIgnoriert</summary>
    [Fact]
    public void Log_EventFehler_WirdIgnoriert()
    {
        var service = CreateService();
        service.OnEntryAdded += () => throw new InvalidOperationException("Event-Fehler");

        var exception = Record.Exception(() => service.Log(ActivityLogCategory.InternalError, "Fehler"));

        Assert.Null(exception);
        Assert.Single(service.Entries);
    }

    /// <summary>Clear_LeertEintraege</summary>
    [Fact]
    public void Clear_LeertEintraege()
    {
        var service = CreateService();
        service.Log(ActivityLogCategory.EntityCreated, "Eintrag 1");
        service.Log(ActivityLogCategory.EntityModified, "Eintrag 2");

        service.Clear();

        Assert.Empty(service.Entries);
    }

    /// <summary>Log_MaxEntries_AeltesteEintraegeWerdenEntfernt</summary>
    [Fact]
    public void Log_MaxEntries_AeltesteEintraegeWerdenEntfernt()
    {
        var service = CreateService();

        for (var i = 0; i < 502; i++)
            service.Log(ActivityLogCategory.EntityCreated, $"Eintrag {i}");

        Assert.Equal(500, service.Entries.Count);
        Assert.Equal("Eintrag 2", service.Entries[0].Message);
        Assert.Equal("Eintrag 501", service.Entries[499].Message);
    }
}
