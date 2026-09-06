namespace Schnittstellenzentrale.Core.Models;

/// <summary>Ergebnis der Ausführung eines Endpunkts.</summary>
public class EndpointExecutionResult
{
    /// <summary>Gibt an, ob die Ausführung technisch erfolgreich war.</summary>
    public bool Success { get; set; }

    /// <summary>Gibt an, ob der HTTP-Aufruf erfolgreich war.</summary>
    public bool HttpSuccess { get; set; }

    /// <summary>Der HTTP-Statuscode (optional).</summary>
    public int? StatusCode { get; set; }

    /// <summary>Details zum gesendeten Request (optional).</summary>
    public string? RequestDetails { get; set; }

    /// <summary>Der empfangene Response-Body (optional).</summary>
    public string? ResponseBody { get; set; }

    /// <summary>Fehlermeldung bei Misserfolg (optional).</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Die empfangenen Response-Header (optional).</summary>
    public IDictionary<string, string>? ResponseHeaders { get; set; }

    /// <summary>Die Dauer in Millisekunden (optional).</summary>
    public long? DurationMs { get; set; }

    /// <summary>Die Größe der Antwort in Bytes (optional).</summary>
    public long? ResponseSizeBytes { get; set; }
}
