using Schnittstellenzentrale.Tests.Infrastructure.Testing;

namespace Schnittstellenzentrale.Tests.Helpers;

/// <summary>Deterministische Datei-I/O-Tests des best-effort Temp-Verzeichnis-Cleanups.
/// OsInterface, weil die Tests das echte Temp-Dateisystem anrühren (gleiche Heuristik
/// wie <c>Services/ImpressumServiceTests</c>).</summary>
[OsInterface]
public sealed class TempDirectoryCleanupTests
{
    /// <summary>Ein Verzeichnis mit Read-only-Datei wird mit Attribut-Normalisierung vollständig entfernt.</summary>
    [Fact]
    public void DeleteBestEffort_ExistingDirectory_DeletesIncludingReadOnlyFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), "tempdir-cleanup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var readOnlyFile = Path.Combine(root, "readonly.txt");
        File.WriteAllText(readOnlyFile, "x");
        File.SetAttributes(readOnlyFile, FileAttributes.ReadOnly);

        TempDirectoryCleanup.DeleteBestEffort(root, normalizeAttributes: true);

        Assert.False(Directory.Exists(root));
    }

    /// <summary>Eine exklusiv geöffnete Datei lässt alle Löschversuche scheitern — die Methode
    /// kehrt trotzdem ohne Exception zurück, damit Cleanup nie einen Assert überdeckt.</summary>
    [Fact]
    public void DeleteBestEffort_LockedDirectory_DoesNotThrow()
    {
        var root = Path.Combine(Path.GetTempPath(), "tempdir-cleanup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var lockedFile = Path.Combine(root, "locked.txt");
        File.WriteAllText(lockedFile, "x");
        var stream = new FileStream(lockedFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        try
        {
            // Ein Cleanup-Fehler darf nie den eigentlichen Assert überdecken.
            var exception = Record.Exception(() => TempDirectoryCleanup.DeleteBestEffort(root));

            Assert.Null(exception);
        }
        finally
        {
            stream.Dispose();
            // Best-effort statt rohem Directory.Delete — ein Cleanup-Restfehler
            // (z. B. Lock-Verzögerung) darf den Assert nicht überdecken.
            TempDirectoryCleanup.DeleteBestEffort(root);
        }
    }

    /// <summary>Ein nicht vorhandener Pfad wird toleriert.</summary>
    [Fact]
    public void DeleteBestEffort_MissingDirectory_DoesNotThrow()
    {
        var root = Path.Combine(Path.GetTempPath(), "tempdir-cleanup-missing-" + Guid.NewGuid().ToString("N"));

        var exception = Record.Exception(() => TempDirectoryCleanup.DeleteBestEffort(root));

        Assert.Null(exception);
    }

    /// <summary>Die Datei-Variante löscht eine vorhandene Datei.</summary>
    [Fact]
    public void DeleteFileBestEffort_ExistingFile_DeletesFile()
    {
        var path = Path.Combine(Path.GetTempPath(), "tempfile-cleanup-" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(path, "x");

        TempDirectoryCleanup.DeleteFileBestEffort(path);

        Assert.False(File.Exists(path));
    }

    /// <summary>Die Datei-Variante toleriert einen nicht vorhandenen Pfad.</summary>
    [Fact]
    public void DeleteFileBestEffort_MissingFile_DoesNotThrow()
    {
        var path = Path.Combine(Path.GetTempPath(), "tempfile-cleanup-missing-" + Guid.NewGuid().ToString("N") + ".txt");

        var exception = Record.Exception(() => TempDirectoryCleanup.DeleteFileBestEffort(path));

        Assert.Null(exception);
    }

    /// <summary>Eine exklusiv geöffnete Datei lässt die Löschversuche scheitern —
    /// ohne Exception nach außen.</summary>
    [Fact]
    public void DeleteFileBestEffort_LockedFile_DoesNotThrow()
    {
        var path = Path.Combine(Path.GetTempPath(), "tempfile-cleanup-" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(path, "x");
        var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        try
        {
            var exception = Record.Exception(() => TempDirectoryCleanup.DeleteFileBestEffort(path));

            Assert.Null(exception);
        }
        finally
        {
            stream.Dispose();
            // Best-effort statt rohem File.Delete — ein Cleanup-Restfehler
            // (z. B. Lock-Verzögerung) darf den Assert nicht überdecken.
            TempDirectoryCleanup.DeleteFileBestEffort(path);
        }
    }
}
