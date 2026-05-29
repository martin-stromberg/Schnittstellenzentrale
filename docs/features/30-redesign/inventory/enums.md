# Enums

## `ColorScheme`
Datei: `src/Schnittstellenzentrale.Core/Enums/ColorScheme.cs`

| Wert | Bedeutung |
|---|---|
| `Light` | Helles Theme |
| `Dark` | Dunkles Theme |

Wird von `ThemeService` und `ThemeToggle` verwendet.

---

## `StorageMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/StorageMode.cs`

| Wert | Bedeutung |
|---|---|
| `Team` | Gemeinsamer Datenspeicher (alle Benutzer) |
| `User` | Benutzerspezifischer Datenspeicher |

Wird von `StorageModeService`, `ApplicationRepository`, `SystemEnvironmentRepository` und mehreren Komponenten verwendet.

---

## `InterfaceType`
Datei: `src/Schnittstellenzentrale.Core/Enums/InterfaceType.cs`

| Wert | Bedeutung |
|---|---|
| `Unknown` (0) | Schnittstellentyp unbekannt |
| `Rest` (1) | REST-API (Swagger/OpenAPI) |
| `OData` (2) | OData-Schnittstelle |

---

## `ActivityLogCategory`
Datei: `src/Schnittstellenzentrale.Core/Enums/ActivityLogCategory.cs`

| Wert | Bedeutung |
|---|---|
| `EntityCreated` | Entität angelegt |
| `EntityModified` | Entität geändert |
| `EntityMoved` | Entität verschoben |
| `ContextSwitched` | Modus oder Umgebung gewechselt |
| `EndpointExecuted` | Endpunkt ausgeführt |
| `ScriptExecuted` | Skript ausgeführt |
| `ScriptConsoleOutput` | Skript-Konsolenausgabe |
| `HttpError` | HTTP-Fehler |
| `InternalError` | Interner Fehler |

---

## `HttpMethod`
Datei: `src/Schnittstellenzentrale.Core/Enums/HttpMethod.cs`

| Wert | Bedeutung |
|---|---|
| `GET` | HTTP GET |
| `POST` | HTTP POST |
| `PUT` | HTTP PUT |
| `DELETE` | HTTP DELETE |
| `PATCH` | HTTP PATCH |
| `HEAD` | HTTP HEAD |
| `OPTIONS` | HTTP OPTIONS |
