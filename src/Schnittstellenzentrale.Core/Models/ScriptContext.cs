using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Core.Models;

/// <summary>Kapselt alle Eingaben für eine Skriptausführung.</summary>
public class ScriptContext
{
    /// <summary>Der aktive Umgebungsservice für <c>sz.environment</c>-Zugriff.</summary>
    public IActiveEnvironmentService EnvironmentService { get; set; } = null!;

    /// <summary>Snapshot der Request-Daten.</summary>
    public ScriptRequestData Request { get; set; } = null!;

    /// <summary>Snapshot der HTTP-Antwort; nur im Post-Request-Skript gesetzt.</summary>
    public ScriptResponseData? Response { get; set; }

    /// <summary>Callback für <c>sz.execute(name)</c>; führt einen anderen Endpunkt aus.</summary>
    public Func<string, Task<EndpointExecutionResult>> ExecuteEndpoint { get; set; } = null!;

    /// <summary>Gibt an, ob <c>sz.repeat()</c> für diesen Skriptkontext eine Wiederholung auslösen darf.</summary>
    public bool CanRepeat { get; set; }

    /// <summary>Gibt an, ob der letzte <c>sz.execute(name)</c>-Aufruf erfolgreich einen Authenticate-Endpunkt ausgeführt hat.</summary>
    public bool LastExecuteWasSuccessfulAuthenticate { get; private set; }

    /// <summary>Wird gesetzt, wenn das Skript über <c>sz.repeat()</c> eine Wiederholung angefordert hat.</summary>
    public bool RepeatRequested { get; private set; }

    /// <summary>Rekursionsschutz: Aufrufzähler pro Endpunkt-ID.</summary>
    public Dictionary<int, int> CallDepth { get; set; } = [];

    /// <summary>Optionaler Endpunktname für die Protokollierung in <c>EndpointScriptRunner</c>.</summary>
    public string? EndpointName { get; set; }

    /// <summary>Gibt an, ob das Skript ein Pre- oder Post-Request-Skript ist.</summary>
    public ScriptType ScriptType { get; set; }

    /// <summary>Fordert eine Wiederholung des aktuellen Endpunkts an, sofern der Kontext dies erlaubt.</summary>
    public void RequestRepeat()
    {
        if (CanRepeat && LastExecuteWasSuccessfulAuthenticate)
            RepeatRequested = true;
    }

    /// <summary>Merkt, ob der letzte <c>sz.execute(name)</c>-Aufruf erfolgreich einen Authenticate-Endpunkt ausgeführt hat.</summary>
    public void RecordExecuteEndpointResult(bool wasSuccessfulAuthenticateEndpoint)
    {
        LastExecuteWasSuccessfulAuthenticate = wasSuccessfulAuthenticateEndpoint;
    }
}
