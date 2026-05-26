using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>Unit-Tests für <see cref="ActivityLogService"/>.</summary>
public class ActivityLogServiceTests
{
    /// <summary>Log_ErstelltEintragMitKorrektenFeldern</summary>
    [Fact]
    public void Log_ErstelltEintragMitKorrektenFeldern()
    {
        var service = new ActivityLogService();
        var before = DateTime.Now;

        service.Log(ActivityLogCategory.EndpointExecuted, "Test-Nachricht", "Test-Details");

        var after = DateTime.Now;
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
        var service = new ActivityLogService();
        var fired = false;
        service.OnEntryAdded += () => fired = true;

        service.Log(ActivityLogCategory.EntityCreated, "Erstellt");

        Assert.True(fired);
    }

    /// <summary>Log_EventFehler_WirdIgnoriert</summary>
    [Fact]
    public void Log_EventFehler_WirdIgnoriert()
    {
        var service = new ActivityLogService();
        service.OnEntryAdded += () => throw new InvalidOperationException("Event-Fehler");

        var exception = Record.Exception(() => service.Log(ActivityLogCategory.InternalError, "Fehler"));

        Assert.Null(exception);
        Assert.Single(service.Entries);
    }

    /// <summary>Clear_LeertEintraege</summary>
    [Fact]
    public void Clear_LeertEintraege()
    {
        var service = new ActivityLogService();
        service.Log(ActivityLogCategory.EntityCreated, "Eintrag 1");
        service.Log(ActivityLogCategory.EntityModified, "Eintrag 2");

        service.Clear();

        Assert.Empty(service.Entries);
    }
}
