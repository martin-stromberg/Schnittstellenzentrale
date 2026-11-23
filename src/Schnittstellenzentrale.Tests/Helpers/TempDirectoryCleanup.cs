namespace Schnittstellenzentrale.Tests.Helpers;

/// <summary>Best-effort-Löschung temporärer Testverzeichnisse und -dateien: das Verzeichnis
/// wird mit Retry/Backoff gelöscht, Restfehler werden nur diagnostisch ausgegeben statt
/// geworfen, damit ein Cleanup-Fehler nie den eigentlichen Test-Assert überdeckt
/// (kein AggregateException-Overlay aus Dispose).</summary>
internal static class TempDirectoryCleanup
{
    private const int MaxDeleteAttempts = 5;

    /// <summary>Löscht <paramref name="root"/> rekursiv best-effort. Wirft nie — ein letzter
    /// Restfehler geht nur nach <see cref="Console.Error"/>.</summary>
    /// <param name="root">Zu löschendes Verzeichnis.</param>
    /// <param name="normalizeAttributes">Dateiattribute vor dem Löschen auf Normal setzen
    /// (z. B. Read-only-Dateien).</param>
    internal static void DeleteBestEffort(string root, bool normalizeAttributes = false)
    {
        try
        {
            if (!Directory.Exists(root))
                return;

            if (normalizeAttributes)
            {
                try
                {
                    // EnumerateFiles wertet lazy aus: Ein unzugängliches Unterverzeichnis wirft
                    // erst mitten in der Schleife. Der Fehler darf den Lösch-Retry-Loop unten
                    // nicht überspringen — Directory.Delete kann trotzdem erfolgreich sein.
                    foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                    {
                        try { File.SetAttributes(file, FileAttributes.Normal); }
                        catch (Exception) { }
                    }
                }
                catch (Exception)
                {
                    // Best-effort: Attribut-Normalisierung ist nur Vorbereitung, kein Muss.
                }
            }

            for (var attempt = 1; attempt <= MaxDeleteAttempts; attempt++)
            {
                try
                {
                    Directory.Delete(root, recursive: true);
                    return;
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                {
                    if (attempt == MaxDeleteAttempts)
                    {
                        Console.Error.WriteLine(
                            $"TempDirectoryCleanup: '{root}' konnte nach {MaxDeleteAttempts} Versuchen nicht gelöscht werden: {error.Message}");
                        return;
                    }
                    Thread.Sleep(100 * attempt);
                }
            }
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"TempDirectoryCleanup: '{root}' konnte nicht gelöscht werden: {error.Message}");
        }
    }

    /// <summary>Löscht eine einzelne Datei best-effort (Retry/Backoff wie
    /// <see cref="DeleteBestEffort"/>). Wirft nie — ein letzter Restfehler geht nur nach
    /// <see cref="Console.Error"/>.</summary>
    /// <param name="path">Zu löschende Datei.</param>
    internal static void DeleteFileBestEffort(string path)
    {
        try
        {
            if (!File.Exists(path))
                return;

            for (var attempt = 1; attempt <= MaxDeleteAttempts; attempt++)
            {
                try
                {
                    File.Delete(path);
                    return;
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                {
                    if (attempt == MaxDeleteAttempts)
                    {
                        Console.Error.WriteLine(
                            $"TempDirectoryCleanup: Datei '{path}' konnte nach {MaxDeleteAttempts} Versuchen nicht gelöscht werden: {error.Message}");
                        return;
                    }
                    Thread.Sleep(100 * attempt);
                }
            }
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"TempDirectoryCleanup: Datei '{path}' konnte nicht gelöscht werden: {error.Message}");
        }
    }
}
