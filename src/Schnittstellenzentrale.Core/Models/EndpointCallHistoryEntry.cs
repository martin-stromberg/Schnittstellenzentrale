namespace Schnittstellenzentrale.Core.Models;

/// <summary>Protokolleintrag eines früheren Endpunktaufrufs.</summary>
public class EndpointCallHistoryEntry
{
    /// <summary>Der eindeutige Bezeichner.</summary>
    public long Id { get; set; }

    /// <summary>ID der aufgerufenen Anwendung (optional).</summary>
    public int? ApplicationId { get; set; }

    /// <summary>Die aufgerufene Anwendung (optional).</summary>
    public Application? Application { get; set; }

    /// <summary>ID des aufgerufenen Endpunkts (optional).</summary>
    public int? EndpointId { get; set; }

    /// <summary>Der aufgerufene Endpunkt (optional).</summary>
    public Endpoint? Endpoint { get; set; }

    /// <summary>Der Ausführungszeitpunkt (optional).</summary>
    public DateTime? ExecutedAt { get; set; }

    /// <summary>Die HTTP-Methode (optional).</summary>
    public string? HttpMethod { get; set; }

    /// <summary>Der relative Pfad (optional).</summary>
    public string? RelativePath { get; set; }

    /// <summary>Der HTTP-Statuscode (optional).</summary>
    public int? StatusCode { get; set; }

    /// <summary>Die Dauer in Millisekunden (optional).</summary>
    public int? DurationMs { get; set; }
}
