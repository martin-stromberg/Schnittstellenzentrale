namespace Schnittstellenzentrale.Core.Enums;

/// <summary>
/// Kategorie eines Eintrags im Aktivitätsprotokoll.
/// </summary>
public enum ActivityLogCategory
{
    /// <summary>Eine Entität wurde erstellt.</summary>
    EntityCreated,

    /// <summary>Eine Entität wurde geändert.</summary>
    EntityModified,

    /// <summary>Eine Entität wurde verschoben.</summary>
    EntityMoved,

    /// <summary>Der Kontext wurde gewechselt.</summary>
    ContextSwitched,

    /// <summary>Ein Endpunkt wurde ausgeführt.</summary>
    EndpointExecuted,

    /// <summary>Ein Skript wurde ausgeführt.</summary>
    ScriptExecuted,

    /// <summary>Konsolenausgabe eines Skripts.</summary>
    ScriptConsoleOutput,

    /// <summary>Ein HTTP-Fehler ist aufgetreten.</summary>
    HttpError,

    /// <summary>Ein interner Fehler ist aufgetreten.</summary>
    InternalError
}
