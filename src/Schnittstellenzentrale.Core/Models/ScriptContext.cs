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

    /// <summary>Rekursionsschutz: Aufrufzähler pro Endpunkt-ID.</summary>
    public Dictionary<int, int> CallDepth { get; set; } = [];

    /// <summary>Ermöglicht <c>EndpointScriptRunner</c> den Zugriff auf das Repository für die gezielte Persistierung einer einzelnen Variable. Nullable, weil <c>ScriptContext</c> auch außerhalb von <c>EndpointExecutionService</c> erzeugt werden kann.</summary>
    public ISystemEnvironmentRepository? EnvironmentRepository { get; set; }

    /// <summary>Ermöglicht <c>EndpointScriptRunner</c> den Aufruf von <c>NotifyEnvironmentChangedAsync()</c> nach der Persistierung. Nullable aus demselben Grund wie <c>EnvironmentRepository</c>.</summary>
    public ISignalRNotificationService? SignalRNotificationService { get; set; }
}
