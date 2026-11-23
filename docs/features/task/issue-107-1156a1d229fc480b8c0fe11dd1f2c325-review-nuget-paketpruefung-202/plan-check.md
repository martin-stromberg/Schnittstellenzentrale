# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

Abgeleitet aus `requirement.md` (Erwartete Wirkung + Implementierungsansatz; die Anforderung enthält keine nummerierte Akzeptanzkriterien-Liste):

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| Vollständige Prüfung aller `PackageReference` (4 Projekte, inkl. transitiv) | Verarbeitungseinheit = Top-Level-Referenzen; alle 35 outdated Top-Level-Einträge aus `inventory/outdated.md` sind zugeordnet (11 umsetzen, Rest dokumentieren/keine Aktion); Transitive fließen über Parents + Vulnerability-Scan | `dotnet list package --vulnerable --include-transitive` nach jedem Batch | Abgedeckt |
| Empfehlungsliste im Issue-Format (Paket, aktuell→Ziel, Update-Art, Risiko + Begründung) | Schritt 4: `recommendation-report.md` im Feature-Verzeichnis + Spiegelung in PR-Body; Gruppen umgesetzt/dokumentiert/keine-Aktion | Artefakt + PR-Body | Abgedeckt |
| Updates Risiko niedrig/mittel direkt umsetzen | Batch A: 3 Patches (ShadcnBlazor→1.0.22, Http.Abstractions→2.3.14, SignalR.Core→1.2.14); Batch B: 8 Minor-Pakete (Markdig→1.6.0, Jint→4.18.1 ×2, bunit→2.15.2, AngleSharp→1.10.5, coverlet.collector→10.1.1, NET.Test.Sdk→18.12.0, Playwright→1.65.1, Moq→4.21.0), vorbehaltlich Release-Notes-Recherche | `dotnet test --filter "Category!=OsInterface"` (Ziel 641/641) + Build nach jedem Batch | Abgedeckt |
| Hoch-Risiko/aufgeschoben dokumentieren bzw. Folge-Issue | Schritt 5: gebündeltes Folge-Issue ohne Label `automated-nuget-review` (xunit.v3, OpenApi 2→3 + Swashbuckle 10→11, Serilog.AspNetCore 10→11, xunit.runner.visualstudio 3→4, OData-Kohärenz, FrameworkReference-Ablösung, Floating-vs-Lock, npm/Actions-Hinweise) | Folge-Issue verlinkt im PR-Body | Abgedeckt |
| PR gegen `staging`, reguläre Gates bestehen | Schritt 6: PR gegen `staging`, Checks per `--watch --fail-fast`/Fail-Polling, Diagnose via `gh run view --job <id> --log-failed` | CI-Gates (Format, Security-Scan, TWAE-Build, Tests, Coverage ≥ 70 %) als Akzeptanznachweis in Szenario 1 | Abgedeckt |
| Security-Overrides nicht verwässern (AngleSharp, S.S.C.Pkcs) | AngleSharp: Override beibehalten + auf 1.10.5 angehoben (direkte Nutzung in `PlaywrightServer.cs` begründet Erhalt), Kommentar nach bunit-Update gegen tatsächliche transitive Auflösung verifiziert; Pkcs `10.0.*`: unverändert (floating, bereits ≥ 10.0.13 gepatcht) | Vulnerability-Scan 0 Befunde nach jedem Batch | Abgedeckt |
| Legacy-Pakete prüfen (`Http.Abstractions` 2.3.x, `SignalR.Core` 1.2.x) | Patch innerhalb der Linie umgesetzt (niedrig); Ablösung via `FrameworkReference Microsoft.AspNetCore.App` als hoch/Architekturänderung dokumentiert (Infrastructure hat keinen Framework-Bezug — Faktum aus code-usage.md korrekt übernommen) | `ApplicationApiClientTests` (`Mock<IHttpContextAccessor>`), `ODataNotificationIntegrationTests`/`ImportApplyNotificationTests` | Abgedeckt |
| OData-Versionssplit behandeln | `AspNetCore.OData` preview.2 + `OData.Edm` 8.4.3 unverändert (je hoch per Schema); Recherche stabiler 10.x-Release + Edm-8.x-Patch eingeplant; kohärente Empfehlung (Edm→9.0.0 stabil, SemVer 9.0.0 > 9.0.0-preview.4) ins Folge-Issue | `ODataControllerIntegrationTests` (78 Tests) als Regression | Abgedeckt |
| Floating-`10.0.*` gesondert behandeln | Keine `.csproj`-Änderung; als „keine Aktion" ausgewiesen; 11.0.0-„Neueste" TFM-gebunden (Ausschluss 5); implizite Patch-Auflösung dokumentiert | — | Abgedeckt |
| Lokale Verifikation vor PR | Pro Batch: `dotnet restore` → `dotnet build` → `dotnet test --filter "Category!=OsInterface"` → `--vulnerable`-Scan; Testbruch → Bisektion pro Paket + Umstufung auf hoch | Akzeptanzszenarien 1–4 | Abgedeckt |
| Scope-Ausschlüsse (npm, Actions-Pins, kein Workflow-/TFM-/SDK-Upgrade, kein Lock-File) | Scope-Ausschlüsse 1–7, alle mit Begründung + Status `bestätigt (requirement.md)` | — | Abgedeckt |

## Fehlende oder unvollständige Testanforderungen

Nicht ausgefüllt — Status ist `Plan vollständig`.

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| Kein Benutzerfluss betroffen — reine Paketversionsänderungen auf Build-Ebene (explizite Festlegung in `requirement.md`: „Keine Datenmodell-, Service- oder UI-Änderungen", „Keine neuen Tests") | Keine neuen E2E-Tests geplant; begründet im Plan-Abschnitt „E2E-Tests". Einzig UI-nah berührtes Paket (`ShadcnBlazor`-Patch) ist durch bestehende `LayoutSmokeTests` (BoundingBox-Assertions, reguläre Lane) abgesichert; `Microsoft.Playwright`-Update-Restrisiko über best-effort-OsInterface-Lane in der CI beobachtet | Nicht erforderlich mit Begründung |

## Fehlende oder unvollständige Planbestandteile

Nicht ausgefüllt — Status ist `Plan vollständig`.

## Hinweise

1. **Inkonsistenz in der Recherche-Aufzählung (nachplanen/korrigieren):** Der Abschnitt „Recherche und Risiko-Finalisierung" listet pro Kandidat 10 Einträge auf — `AngleSharp` 1.5.0→1.10.5 fehlt dort, obwohl es als mittel-Kandidat in Batch B umgesetzt wird und Umsetzungsreihenfolge-Schritt 1 von „aller 11 Kandidaten" spricht. Bei der Umsetzung sollte die Release-Notes-Prüfung auf AngleSharp (Sprung über mehrere Minor-Versionen) mit angewendet werden.
2. **Lokale Verifikation schwächer als CI-Gate 3:** Der Plan verifiziert lokal mit `dotnet build` (Debug, ohne `TreatWarningsAsErrors`). Die CI baut `-c Release -p:TreatWarningsAsErrors=true` — neue Obsolete-/NuGet-Warnungen nach Paket-Updates würden erst in der CI sichtbar. Fail-fast-Monitoring fängt das ab; ein optionaler lokaler TWAE-Lauf (z. B. vor dem PR) würde späte rote Gates vermeiden.
3. **Transitive Highlights in der Tabelle sichtbar machen:** Die ~120 outdated transitiven Pakete werden bewusst nicht direkt angefasst (Scope-Festlegung konsistent). Nennenswerte Fakten wie die transitiven OData-Previews (9.0.0-preview.4, stabile 9.0.0 existiert), `AngleSharp.Css` beta→1.5.0 und die IdentityModel/SqlClient-Major-Linien sollten im Report/Folge-Issue zumindest referenziert werden — für OData ist das über das Kohärenz-Item bereits implizit abgedeckt.
4. **Playwright-Browserstand:** Das `Microsoft.Playwright`-Update zieht neue Browser-Builds nach; das `InstallPlaywright`-MSBuild-Target (AfterTargets=Build) regelt dies implizit — bei Beobachtung der OsInterface-CI-Lane relevant, kein eigener Planpunkt nötig.
5. **Verifikation der Plan-Fakten:** Alle 13 genannten Zielversionen (1.0.22, 2.3.14, 1.2.14, 1.6.0, 4.18.1, 2.15.2, 1.10.5, 10.1.1, 18.12.0, 1.65.1, 4.21.0 sowie Folge-Issue-Ziele 9.0.0/3.11.1/11.0.1/11.0.0/4.0.1) stimmen mit `inventory/outdated.md` überein; alle im Plan genannten `.csproj`-Zeilenreferenzen wurden gegen die Projektdateien verifiziert und sind korrekt.
