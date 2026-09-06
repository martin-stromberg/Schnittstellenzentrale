# Plan-Check – GitHooks

Geprüft: `plan.md` gegen `requirement.md` und `inventory.md`
(inkl. `inventory/xmldoc-findings.txt`).

## Abdeckung der Anforderung

| Anforderung | Geplant in |
|-------------|-----------|
| Hooks aus Pattern-Collection übernehmen, alte ersetzen | WP1 |
| Aktivierung via `core.hooksPath` | WP1 |
| Fehler in der Anwendung beheben, Checks nicht entschärfen | WP2–WP6, explizit im Plan verankert |
| Thematische Aufteilung bei großer Fehlerliste | WP-Aufteilung nach Projekt/Thema |

## Abdeckung aller gemessenen Befunde

- translation-check: bereits grün — kein Handlungsbedarf.
- razor-l10n-check: bereits grün — kein Handlungsbedarf.
- csproj-xmldoc: Core-csproj-Konfiguration (WP2), ~45 Dateien mit
  unvollständiger XML-Doku (WP3), 5 pragma-Verstöße (WP3) — abgedeckt.
- razor-usage: alle 5 gemeldeten Komponenten adressiert (WP4).
- no-notimplemented: `ThrowingApplicationRepository` (WP5) — abgedeckt.
- enum-coverage: alle 3 gemeldeten Lücken (WP6) — abgedeckt.

## Risiken / Hinweise

- Entfernen der `#pragma warning disable CS1591` macht CS1591 in den
  betroffenen Dateien zum Build-Fehler → WP3 verlangt vollständige
  Dokumentation aller öffentlichen Member dieser Dateien; `dotnet build`
  als Zwischencheck ist vorgesehen.
- WP2 kann neue CS1591-Build-Fehler in Core erzeugen → im Plan enthalten.
- UI-Änderung (ActivityLogPanel-Wiedereinbindung): bUnit- **und**
  Playwright-Test sind im Plan verankert (E2E-Nachweis).
- `MainLayout`/`NavMenu`-Löschung: Verweise in Tests prüfen
  (`MainLayoutTests.cs` testet AppShell/TopBar, muss ggf. umbenannt
  werden — Implementierungsdetail).

## Status: Plan vollständig
