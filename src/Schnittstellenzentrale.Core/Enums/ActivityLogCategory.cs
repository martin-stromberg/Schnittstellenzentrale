namespace Schnittstellenzentrale.Core.Enums;

public enum ActivityLogCategory
{
    EntityCreated,
    EntityModified,
    EntityMoved,
    ContextSwitched,
    EndpointExecuted,
    ScriptExecuted,
    ScriptConsoleOutput,
    HttpError,
    InternalError
}
