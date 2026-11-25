# Enums und Kategoriekonstanten

## `TestCategories` (Konstanten-Klasse, kein Enum)

Datei: `src/Schnittstellenzentrale.Tests/Infrastructure/Testing/TestCategories.cs`

| Wert | Bedeutung |
|------|-----------|
| `OsInterface = "OsInterface"` | Trait-Wert `Category=OsInterface`; Lane-Zuordnung best-effort (`os-interface-tests`-CI-Job) |
| `Flaky = "Flaky"` | Trait-Wert `Category=Flaky`; berechtigt zum Einmal-Retry via `Retry-FlakyTests.ps1` — nur in Kombination mit `OsInterface` erlaubt (`FlakyConventionTests`) |

## `HttpMethod` (`Core.Enums`)

Datei: `src/Schnittstellenzentrale.Core/Enums/` (`HttpMethod.cs`)

Im Testfluss relevant: `GET` — Default beim „Endpunkt anlegen" (`WorkspacesLayout.razor` Zeile 274) und im Swagger-Mock der `PlaywrightServer`-Konfiguration. Die Methode wird im Test nicht geändert.

## `BodyMode` (`Core.Enums`)

Im Testfluss relevant: `None` — Default beim Anlegen (`WorkspacesLayout.razor` Zeile 276); steuert `SyncAutoContentType` (kein Auto-Content-Type-Header).

## `InterfaceType` (`Core.Enums`)

Nur indirekt relevant: `Application.DetectInterfaceType` klassifiziert `InterfaceUrl` (`$metadata` → OData, `swagger`/`openapi` → Rest, sonst `Unknown`); die Systemanwendung erhält `Rest` (Swagger-URL aus `Api:BaseUrl`).
