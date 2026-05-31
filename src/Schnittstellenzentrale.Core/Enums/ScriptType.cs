namespace Schnittstellenzentrale.Core.Enums;

/// <summary>Gibt an, ob ein Skript vor oder nach dem HTTP-Request ausgeführt wird.</summary>
public enum ScriptType
{
    /// <summary>Pre-Request-Skript, das vor dem HTTP-Request ausgeführt wird.</summary>
    PreRequest,

    /// <summary>Post-Request-Skript, das nach dem HTTP-Request ausgeführt wird.</summary>
    PostRequest
}
