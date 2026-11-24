# Übersetzte Anforderung – [Review] NuGet-Paketprüfung 2026-KW48 (Issue #107)

## Fachliche Zusammenfassung

Es ist eine vollständige Abhängigkeitsprüfung aller `PackageReference`-Einträge der Solution `Schnittstellenzentrale.slnx` (Target Framework `net10.0`, SDK `10.0.301` per `global.json`) inklusive transitiver Abhängigkeiten durchzuführen. Ergebnis ist eine Liste konkreter, risikobewerteter Update-Empfehlungen (Paketname, aktuelle Version → Zielversion, Update-Art, Risiko niedrig/mittel/hoch). Empfehlungen mit niedrigem bis mittlerem Risiko werden direkt in einem Pull Request gegen `staging` umgesetzt; risikoreiche oder aufgeschobene Empfehlungen werden im PR-Body bzw. als eigenes Folge-Issue dokumentiert.

## Fachlicher Scope

- **Verarbeitungseinheit:** Ein einzelnes NuGet-Paket bzw. eine `PackageReference` in einem der vier Projektdateien — `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`, `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`, `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`, `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` — einschließlich der transitiv daraus aufgelösten Pakete (`dotnet list package --include-transitive`). Pro Paket wird je ein Prüf- und Bewertungsdurchlauf (verfügbare Updates nach Patch/Minor/Major, Security, Vulnerabilities via `--vulnerable --include-transitive`, Deprecation via `--deprecated`, Upstream-Aktivität, Nachfolgepaket, Breaking Changes aus Release Notes) ausgeführt.
- **Auslösende Ereignisse:** Wiederkehrender, kalendergetriebener Review-Zyklus — das Issue wird wöchentlich (montags 06:00 UTC) durch `.github/workflows/weekly-nuget-review.yml` mit Label `automated-nuget-review` neu angelegt (Duplikatsprüfung in der Composite Action `.github/actions/create-review-issue`); Auslöser dieser konkreten Ausführung ist Issue #107 „NuGet-Paketprüfung 2026-KW48".
- **Erwartete Wirkung:** (1) Ein Pull Request gegen `staging` mit allen als niedrig/mittel bewerteten Paket-Updates, der die regulären Gates besteht (Staging-CI `pr-staging-ci.yml` inkl. Security-Scan-Gate „vulnerable packages" über `.github/actions/security-scan` sowie Unit-Tests `dotnet test --filter "Category!=OsInterface"`); (2) eine nachvollziehbare, risikobewertete Empfehlungsliste im PR-Body; (3) für hochrisiko-/aufgeschobene Empfehlungen entweder eine begründete Dokumentation im PR-Body oder ein eigenes Folge-Issue mit Begründung.
- **Umgang mit fehlenden Objekten:**
  - Folge-Issue für aufgeschobene/hochriskante Empfehlungen: bei Bedarf **erzeugen** (neues GitHub-Issue mit Begründung und Vorschlag).
  - `packages.lock.json` / zentrales Paketmanagement (`Directory.Packages.props`): existiert nicht im Repo; nur **erzeugen**, wenn die Floating-Version-Politik-Frage zugunsten von Lock-Dateien entschieden wird — sonst nicht einführen (siehe Offene Fragen).
  - Label `automated-nuget-review`: wird vom Workflow sichergestellt; **wiederverwenden**, nicht neu anlegen.
- **Scope-Ausschlüsse:**
  - `npm`-`devDependencies` der Release-Automatisierung (`package.json`: `semantic-release` ^25.0.5, `@semantic-release/*`, `conventional-changelog-conventionalcommits`) werden **nicht** aktualisiert — nur als Hinweis dokumentiert, falls auffällig.
  - Versionspins der GitHub Actions in `.github/workflows/` (z. B. `actions/checkout@v7`) werden **nicht** geändert — ebenfalls nur Hinweis, falls auffällig.
  - Keine automatische Umsetzung von Updates mit Risiko „hoch" (Major-Sprünge, Preview→Stable-Wechsel mit Breaking Changes, Verhaltensänderungen) — diese werden nur dokumentiert.
  - Keine Änderung des wöchentlichen Review-Workflows selbst (`weekly-nuget-review.yml` bleibt unverändert).
  - Kein Upgrade des Target Frameworks oder des .NET SDK (`net10.0` / `10.0.301` bleiben fix).

## Betroffene Klassen und Komponenten

Keine Datenmodell-, Service- oder UI-Änderungen — es handelt sich um eine Wartungs-/Prüfaufgabe auf Build-Ebene. Betroffene Artefakte:

- **Projektdateien (primär):** die vier `.csproj`-Dateien oben; dort Versionsangaben der `PackageReference`-Einträge anpassen. Aktuell referenziert (Stand Bestandsaufnahme):
  - `Schnittstellenzentrale.csproj`: `Microsoft.AspNetCore.Authentication.Negotiate` 10.0.*, `Microsoft.AspNetCore.OData` 10.0.0-preview.2, `Microsoft.EntityFrameworkCore.Design` 10.0.*, `Serilog.AspNetCore` 10.0.0, `Serilog.Sinks.EventLog` 4.0.0, `Serilog.Sinks.File` 7.0.0, `ShadcnBlazor` 1.0.14, `Swashbuckle.AspNetCore` 10.1.7.
  - `Schnittstellenzentrale.Core.csproj`: `Microsoft.OpenApi` 2.10.0.
  - `Schnittstellenzentrale.Infrastructure.csproj`: `Jint` 4.9.2, `Markdig` 1.2.0, `Microsoft.AspNetCore.Http.Abstractions` 2.3.0, `Microsoft.AspNetCore.SignalR.Client` 10.0.*, `Microsoft.AspNetCore.SignalR.Core` 1.2.10, `Microsoft.EntityFrameworkCore`/`Design`/`Sqlite`/`SqlServer` 10.0.*, `Microsoft.Extensions.*` 10.0.*, `Microsoft.JSInterop` 10.0.*, `Microsoft.OData.Edm` 8.4.3, `Microsoft.OpenApi` 2.10.0, `SQLitePCLRaw.lib.e_sqlite3` 3.53.3, `System.Security.Cryptography.Pkcs` 10.0.*.
  - `Schnittstellenzentrale.Tests.csproj`: `AngleSharp` 1.5.0, `bunit` 2.7.2, `coverlet.collector` 10.0.0, `Jint` 4.9.2, `Microsoft.AspNetCore.Mvc.Testing` 10.0.*, `Microsoft.EntityFrameworkCore.Sqlite`/`SqlServer` 10.0.*, `Microsoft.JSInterop` 10.0.*, `Microsoft.NET.Test.Sdk` 18.5.1, `Microsoft.Playwright` 1.52.0, `Moq` 4.20.72, `SQLitePCLRaw.lib.e_sqlite3` 3.53.3, `xunit` 2.9.3, `xunit.runner.visualstudio` 3.1.5.
- **Dokumentation:** PR-Body mit Empfehlungsliste; ggf. neues Folge-Issue; ggf. Hinweis-Dokumentation zu npm/Actions-Pins.
- **Tests:** Keine neuen Tests; Nachweis über Bestehen der bestehenden Suite (`dotnet test --filter "Category!=OsInterface"`) und der CI-Gates.

## Implementierungsansatz

1. Bestandsaufnahme pro Projekt: `dotnet list package` sowie `dotnet list package --outdated --include-transitive`, `dotnet list package --vulnerable --include-transitive`, `dotnet list package --deprecated` auf `Schnittstellenzentrale.slnx`.
2. Floating-Versionen (`10.0.*`) gesondert behandeln: Patch-Updates dieser Pakete fließen bereits bei jedem `dotnet restore` ein — als Empfehlung nur echte Versionslinien-Wechsel (Minor/Major oder Pin-Änderung) ausweisen, nicht die implizit auflösenden Patches.
3. Pro Befund Release Notes prüfen (Breaking Changes, Verhaltensänderungen) und Risiko bewerten:
   - `Microsoft.AspNetCore.OData` 10.0.0-preview.2: prüfen, ob ein stabiler 10.x-Release existiert und welche Änderungen der Wechsel Preview→Stable mit sich bringt.
   - `Microsoft.AspNetCore.Http.Abstractions` 2.3.0 und `Microsoft.AspNetCore.SignalR.Core` 1.2.10 (Legacy-Linien): prüfen, ob die Referenzen unter `net10.0` noch benötigt werden oder durch Framework-Funktionalität/`Microsoft.AspNetCore.App`-Framework-Reference ersetzbar sind (Entfernen wäre die sauberste Lösung).
   - Direkt-Overrides nicht verwässern: `AngleSharp` 1.5.0 in Tests (Schutz vor transitivem AngleSharp 1.4.0 via bunit, GHSA-pgww-w46g-26qg) und `System.Security.Cryptography.Pkcs` 10.0.* in Infrastructure (GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7) — Begründungs-Kommentare in den `.csproj` stehen lassen; nur prüfen, ob der Override nach einem Update des Parent-Pakets (z. B. bunit) noch nötig ist und die Kommentare ggf. aktualisieren.
4. Umsetzung nur für niedriges/mittleres Risiko: Versionsangaben in den `.csproj` anpassen, `dotnet restore` + `dotnet build` + `dotnet test --filter "Category!=OsInterface"` lokal verifizieren, PR gegen `staging` öffnen (Repo-Workflow: Feature-Branch → staging → main).
5. Risikobewertungsschema (Vorschlag aus dem Issue, zur Anwendung sofern nicht anders entschieden): Patch ohne API-Änderung = niedrig; Minor ohne Breaking Changes = mittel; Major, Preview-Wechsel oder Verhaltensänderungen (z. B. EF Core) = hoch.
6. Ergebnisformat im PR-Body pro relevantem Paket: Paketname, aktuelle Version → Zielversion, Update-Art (Patch/Minor/Major/Security/Deprecation/Nachfolgepaket), Risiko niedrig/mittel/hoch mit ein- bis zweizeiliger Begründung; bei hoch/aufgeschoben zusätzlich Vorschlag für ein Folge-Issue.

## Konfiguration

Keine anwendungsseitige Konfiguration betroffen. Einzig relevante „Policy"-Frage ist die Floating-Version-Strategie (`10.0.*` beibehalten vs. pinnen via `packages.lock.json`) — aktuell existiert keine `packages.lock.json` und kein `Directory.Packages.props` im Repo; ohne explizite Entscheidung wird der Status quo (Floating Versions) beibehalten.

## Offene Fragen

1. **Risikokriterien final festlegen?** Das Issue liefert einen Vorschlag (Patch ohne API-Änderung = niedrig; Minor ohne Breaking Changes = mittel; Major/Preview-Wechsel/Verhaltensänderungen = hoch). Annahme: Dieser Vorschlag gilt als Arbeitsdefinition, sofern kein Widerspruch erfolgt.
2. **Floating-Version-Politik:** `10.0.*`-Floats beibehalten oder auf `packages.lock.json`-Pins umstellen? Eine Umstellung wäre eine größere Prozessänderung über dieses Issue hinaus — Annahme: beibehalten, höchstens als Folge-Issue vorschlagen.
3. **Security-Updates mit Breaking Changes:** sofort umsetzen oder als Folge-Issue eskalieren? Annahme: nach Schema „hoch" behandeln = dokumentieren/eskalieren, außer die CI-Security-Gates (`vulnerable-packages`-Scan) erzwingen ein sofortiges Update.
4. **Nachverfolgung aufgeschobener Empfehlungen über Wochen:** Das Issue wird wöchentlich neu angelegt; es ist unklar, wie aufgeschobene Empfehlungen zwischen den Zyklen getrackt werden. Annahme: Aufgeschobenes als eigenes, vom Label `automated-nuget-review` unabhängiges Folge-Issue anlegen, damit es nicht in der Duplikatsprüfung des Wochen-Workflows untergeht.
5. **Umgang mit Preview-Abhängigkeit OData:** Falls kein stabiler `Microsoft.AspNetCore.OData` 10.x-Release existiert — Preview weiter tracken (nächstes Preview) oder auf Preview.2 verharren? Abhängig vom Rechercheergebnis.
