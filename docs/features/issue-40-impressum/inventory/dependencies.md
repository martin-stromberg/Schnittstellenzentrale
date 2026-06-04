# Abhängigkeiten / NuGet-Pakete – Bestandsaufnahme

## Hauptprojekt (`Schnittstellenzentrale.csproj`)

Datei: `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`

Relevante vorhandene Pakete:

| Paket | Version |
|-------|---------|
| `ShadcnBlazor` | 1.0.14 |
| `Serilog.AspNetCore` | 10.0.0 |
| `Swashbuckle.AspNetCore` | 10.1.7 |

**Markdig ist nicht vorhanden.** Für das Markdown-Rendering muss `Markdig` als NuGet-Paket ergänzt werden.

---

## Core-Projekt (`Schnittstellenzentrale.Core.csproj`)

Datei: `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`

Keine externen Paket-Referenzen vorhanden (nur implizites SDK).

---

## Infrastructure-Projekt (`Schnittstellenzentrale.Infrastructure.csproj`)

Datei: `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`

Enthält EF Core, OData, OpenAPI, Jint und weitere Pakete. Kein Markdig.

---

## Tests-Projekt (`Schnittstellenzentrale.Tests.csproj`)

Datei: `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`

| Paket | Version |
|-------|---------|
| `bunit` | 2.7.2 |
| `Microsoft.Playwright` | 1.52.0 |
| `Moq` | 4.20.72 |
| `xunit` | 2.9.3 |
| `Microsoft.AspNetCore.Mvc.Testing` | 9.0.* |

Das Test-Framework für Unit-Tests (`xunit` + `Moq`), Integrationstests (`WebApplicationFactory`) und Playwright-Tests ist vollständig vorhanden.
