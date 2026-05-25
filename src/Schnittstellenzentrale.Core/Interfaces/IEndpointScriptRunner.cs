using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Contract für die JavaScript-Skriptausführung im Rahmen von Endpunkt-Requests.</summary>
public interface IEndpointScriptRunner
{
    /// <summary>Führt das angegebene JavaScript-Skript im Kontext des übergebenen <paramref name="context"/> aus.</summary>
    Task<ScriptExecutionResult> ExecuteAsync(string script, ScriptContext context);
}
