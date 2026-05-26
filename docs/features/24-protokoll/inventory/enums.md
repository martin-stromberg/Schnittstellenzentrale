# Enum-Bestandsaufnahme

## `StorageMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/StorageMode.cs`

| Wert | Bedeutung |
|------|-----------|
| `Team` | Team-Modus (geteilte Daten) |
| `User` | Benutzermodus (persönliche Daten) |

Hinweis: Wird in `MainLayout` für den Moduswechsel verwendet — Auslöser für `ContextSwitched`-Protokolleinträge.

---

Hinweis: `ActivityLogCategory` existiert noch nicht. Der Enum ist vollständig neu zu erstellen mit den Werten: `EntityCreated`, `EntityModified`, `EntityMoved`, `ContextSwitched`, `EndpointExecuted`, `ScriptExecuted`, `ScriptConsoleOutput`, `HttpError`, `InternalError`.
