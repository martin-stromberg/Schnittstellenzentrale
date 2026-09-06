# Dokumentation

Übersicht über alle dokumentierten Funktionsbereiche.

## Kernfunktionen

- [Anwendungen](anwendungen/index.md) — Anwendungen und Anwendungsgruppen bilden die zentrale Struktur der Schnittstellenzentrale und werden im Navigationsbaum verwaltet.
- [Endpunkte](endpunkte/index.md) — Endpunkte bündeln HTTP-Anfragen, Platzhalter, Query-Parameter und Skripte für Ausführung und Analyse.
- [Schnittstellenzentrale](schnittstellenzentrale/index.md) — Die zentrale Blazor-Server-Anwendung verwaltet Workspaces, Environments und Historie in einer gemeinsamen Oberfläche.
- [Systemumgebungen](systemumgebungen/index.md) — Systemumgebungen definieren benannte Variablensätze für unterschiedliche Deployment-Ziele.

## Oberfläche und Benutzerführung

- [Aktivitätsprotokoll](aktivitaetsprotokoll/index.md) — Das Aktivitätsprotokoll zeichnet schreibende und ausführende Aktionen des angemeldeten Benutzers chronologisch auf.
- [Dark Mode](dark-mode/index.md) — Die Anwendung unterstützt ein helles und dunkles Farbschema mit persistenter Auswahl im Browser.
- [Impressum](impressum/index.md) — Das Impressum kann als Markdown-Datei unter `/impressum` ausgeliefert werden.
- [Mehrsprachigkeit DE/EN (Lokalisierung)](lokalisierung/index.md) — Die Oberfläche unterstützt Deutsch und Englisch mit automatischer Spracherkennung.
- [Speichermodus](speichermodus/index.md) — Team- und Benutzer-Modus unterscheiden geteilte und benutzerspezifische Daten.

## APIs und Integration

- [REST-API und OData v4-API](api/index.md) — Zwei API-Oberflächen ermöglichen internen und externen Zugriff auf Anwendungen, Endpunkte und Metadaten.

## Qualität und Tests

- [Playwright-Tests](playwright-tests/index.md) — Die Testinfrastruktur stellt browserbasierte E2E-Prüfungen und die Absicherung des Coverage-Gates für das UI-Verhalten bereit.
- [Git-Hooks](git-hooks/index.md) — Versionierte Git-Hooks unter `.githooks/` prüfen lokale Qualitätsregeln beim Commit und Push.
