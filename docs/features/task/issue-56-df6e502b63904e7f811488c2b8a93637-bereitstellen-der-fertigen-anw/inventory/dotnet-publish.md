# Detailinventar: .NET-Projekte und Publish-Bestand

## Solution

`Schnittstellenzentrale.slnx` enthaelt vier Projekte:

| Projekt | Rolle |
|---|---|
| `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` | ASP.NET-Core/Blazor-Server-Webprojekt, Einstiegspunkt und Publish-Ziel |
| `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj` | Core-/Domain-Bibliothek |
| `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj` | Infrastruktur, EF Core, Services, Migrationen |
| `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` | xUnit-, Integrations- und Playwright-Tests |

## TargetFrameworks

Alle Projekte verwenden aktuell `TargetFramework` = `net9.0`:

| Projekt | SDK | TargetFramework |
|---|---|---|
| `Schnittstellenzentrale` | `Microsoft.NET.Sdk.Web` | `net9.0` |
| `Schnittstellenzentrale.Core` | `Microsoft.NET.Sdk` | `net9.0` |
| `Schnittstellenzentrale.Infrastructure` | `Microsoft.NET.Sdk` | `net9.0` |
| `Schnittstellenzentrale.Tests` | `Microsoft.NET.Sdk` | `net9.0` |

Es gibt kein `global.json`. Lokal ist `dotnet` SDK `10.0.301` installiert.

## Paketlage mit Versionsbezug

Viele Microsoft-Pakete sind explizit auf 9.x gesetzt, unter anderem:

- `Microsoft.AspNetCore.Authentication.Negotiate` `9.0.16`
- `Microsoft.EntityFrameworkCore*` `9.0.16` bzw. `9.*`
- `Microsoft.AspNetCore.Mvc.Testing` `9.0.*`
- `Microsoft.Extensions.*` `9.0.16`
- `Microsoft.AspNetCore.OData` `9.4.1`

Eine echte Umstellung auf `net10.0` kann daher Paketupdates auf kompatible 10.x-Versionen erforderlich machen. Nicht-Microsoft-Pakete wie `Serilog`, `ShadcnBlazor`, `Jint`, `Markdig`, `Swashbuckle`, `bunit`, `xunit` und `Microsoft.Playwright` muessen im Rahmen der Umsetzung auf Kompatibilitaet geprueft werden.

## Publish-Bestand

Das README dokumentiert:

```powershell
dotnet publish src/Schnittstellenzentrale/Schnittstellenzentrale.csproj -c Release -o publish/
```

Dieser Projektpfad ist fuer die Release-Anforderung der passende Publish-Einstieg. Das Webprojekt referenziert Core und Infrastructure, sodass deren Build automatisch im Publish enthalten ist.

`.gitignore` ignoriert `publish/` als ClickOnce-/Publish-Ausgabeverzeichnis. Fuer CI ist das unkritisch, weil Workflow-Artefakte nicht ins Repository eingecheckt werden. Fuer klare Trennung und weniger Kollisionsrisiko ist ein Workflow-interner Pfad wie `artifacts/publish` oder `${{ runner.temp }}/publish` besser.

## Runtime-/Installationsdokumentation

`README.md` und `docs/help/schnittstellenzentrale/installation.md` dokumentieren derzeit:

- Windows Server oder Windows-Arbeitsstation mit IIS
- .NET 9.0 Runtime (ASP.NET Core)
- IIS Windows-Authentifizierung aktiviert
- Anonyme Authentifizierung deaktiviert
- SQLite oder SQL Server
- Automatische EF-Core-Migrationen beim Start

Bei einer Umstellung auf .NET 10 muessen README und Installationsdokumentation nachgezogen werden.

## Test-/CI-Relevanz

`src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` enthaelt:

```xml
<Target Name="InstallPlaywright" AfterTargets="Build" Condition="'$(SkipPlaywrightInstall)' != 'true'">
  <Exec Command="pwsh -NoProfile -File &quot;$(MSBuildProjectDirectory)\bin\$(Configuration)\$(TargetFramework)\playwright.ps1&quot; install chromium" />
</Target>
```

Fuer Release-Publish allein ist das Testprojekt nicht erforderlich. Falls der Release-Workflow Tests ausfuehren soll, sollte `-p:SkipPlaywrightInstall=true` genutzt werden oder ein explizites Playwright-Setup/Caching ergaenzt werden.

Dokumentierte Testbefehle:

```powershell
dotnet test --filter "FullyQualifiedName!~Playwright"
dotnet test --filter "FullyQualifiedName~Playwright"
dotnet test -p:SkipPlaywrightInstall=true
```

## Implikation fuer .NET 10

Ein Workflow mit `actions/setup-dotnet` und SDK 10 kann ein `net9.0`-Projekt bauen, wenn die passende Targeting-Unterstuetzung vorhanden ist. Das erfuellt aber semantisch nicht die Anforderung "mit .NET 10 veroeffentlicht", solange `TargetFramework` `net9.0` bleibt. Fuer eine harte Erfuellung muss mindestens das Webprojekt, praktisch aber alle referenzierten Projekte, auf `net10.0` umgestellt werden.
