namespace Schnittstellenzentrale.Core.Models;

/// <summary>Ergebnis einer Skriptausführung durch <see cref="Schnittstellenzentrale.Core.Interfaces.IEndpointScriptRunner"/>.</summary>
public class ScriptExecutionResult
{
    /// <summary>Gibt an, ob das Skript erfolgreich ausgeführt wurde.</summary>
    public bool Success { get; set; }

    /// <summary>Fehlermeldung bei Syntaxfehler oder Runtime-Exception; <see langword="null"/> bei Erfolg.</summary>
    public string? ErrorMessage { get; set; }
}
