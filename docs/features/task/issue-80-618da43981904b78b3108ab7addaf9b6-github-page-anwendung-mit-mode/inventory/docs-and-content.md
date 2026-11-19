# Dokumentation, Inhaltsquellen und Assets

## docs/-Struktur

```
docs/
├── RELEASE_NOTES.md        # Bilinguale Release Notes (EN-Abschnitt zuerst, dann DE)
├── features/               # Lifecycle-Arbeitsverzeichnisse je Aufgabe
│   ├── task/…issue-71…, …issue-79…, …issue-80…
│   └── task-issue-78-….md
└── help/                   # Produkt-Dokumentation (15 Themengebiete + index.md)
```

### `docs/help/`-Konvention

- `docs/help/index.md`: deutschsprachige Übersicht mit Überschrift `# Dokumentation`, Kurzsatz und thematischen Abschnitten (`## Kernfunktionen`, `## Oberfläche und Benutzerführung`, `## APIs und Integration`, `## Qualität und Tests`), Einträge als `- [Titel](thema/index.md) — Ein-Satz-Beschreibung.`
- 15 Themengebiete: `aktivitaetsprotokoll`, `anwendungen`, `api`, `dark-mode`, `endpunkte`, `git-hooks`, `impressum`, `lokalisierung`, `playwright-tests`, `review-automatisierung`, `schnittstellenzentrale`, `speichermodus`, `systemumgebungen` (u. a.).
- Pro Thema ein Verzeichnis mit `index.md` (H1-Titel + 1–2 Sätze + `## Inhalt`-Liste) plus Teilmenge von: `beschreibung.md`, `ablauf-technisch.md`, `ablauf-anwender.md`, `api.md`, `business-rules.md`, `installation.md`, `datenmodell.md`. Kleinste Ausprägung (`review-automatisierung/`): nur `index.md`, `beschreibung.md`, `ablauf-technisch.md`.

### `docs/RELEASE_NOTES.md`-Format

H1 `# Release Notes`; zwei Sprachblöcke nacheinander — EN: `## Important Notes Before Update` + `## What's New`, DE: `## Wichtige Hinweise vor dem Update` + `## Neuerungen`; jeweils Bullet-Listen, keine Versionierung/Kapitel je Release (ein fortlaufendes Dokument).

## Inhaltsquellen für die Page

### `README.md` (228 Zeilen, deutsch)

- Badges-Zeile: Release-Workflow-Status (`actions/workflow/status/.../release.yml`), .NET 10, Blazor Server, EF Core 10, xUnit+Playwright, Platform Windows/IIS (shields.io).
- Einleitungssatz (Z. 10): „Blazor Server-Anwendung zur zentralen Verwaltung lokaler Webservice-Endpunkte …"
- `## Features` (Z. 12–29): Workspaces, Environments, History, Import (Swagger/OpenAPI + OData-$metadata), OData v4-API, Authentifizierungstypen (None/Basic/Negotiate/BearerToken/Impersonation), Team-/Benutzermodus, SignalR-Echtzeit, Icons/Links, Health-Check, Mehrsprachigkeit DE/EN, Impressum.
- `## Releases` (Z. 99–101): ZIP-Asset-Namenskonvention `Schnittstellenzentrale-vX.Y.Z.zip`, automatisch bei Push nach `main` oder manuell per Tag `vX.Y.Z`.
- API-Beschreibung, Projektstruktur, Tests, Git-Hooks, Review-Workflows, Rechtliches (`LEGAL.md`-Verweis; explizit: „Im Repository-Stamm ist keine separate `LICENSE`-Datei hinterlegt." — **Fakt:** `find` bestätigt, kein LICENSE-File vorhanden; `LEGAL.md` verweist dennoch auf eine `LICENSE`-Datei).

### `issue.md` (Repo-Root)

Original-Issue-Text (#80) mit Akzeptanzkriterien; Ziel-URL `https://martin-stromberg.github.io/Schnittstellenzentrale/`; Repo `martin-stromberg/Schnittstellenzentrale`.

### `LEGAL.md`

MIT-License-Hinweis, „No Warranty", Operator-Disclaimer (kein öffentlich betriebener Dienst). Als Footer-/Rechts-Verweis nutzbar.

## Wiederverwendbare Assets

| Asset | Pfad | Details |
|-------|------|---------|
| Favicon | `src/Schnittstellenzentrale/wwwroot/favicon.png` | PNG, **32×32 px**, RGBA, 1148 Bytes — einziges Bild-Asset im Repo (Icons von Anwendungen liegen in der DB, nicht als Dateien) |
| CSS/JS | `wwwroot/app.css`, `shadcn/*.css`, `theme.js`, `theme-init.js` | siehe [design-system.md](design-system.md) |
| Bootstrap-Distribution | `wwwroot/lib/bootstrap/` | eingecheckt, aber nicht geladen — irrelevant für die Page |

## Nicht vorhanden (bestätigt)

- Kein `site/`-, `docs/page/`- oder vergleichbarer Seiten-Quellordner.
- Keine GitHub-Pages-Konfiguration: keine `_config.yml`, `CNAME`, `.nojekyll`; keine `pages:`-Permission/`github-pages`-Environment/`deploy-pages`-Action in `.github/` (grep über `.github/` → keine Treffer).
- Kein Screenshot-Asset-Verzeichnis; `Page.ScreenshotAsync()` nirgends im Code.
- Kein Impressum als Repo-Datei (Impressum-Feature liest `impressum.md` zur Laufzeit aus dem App-Verzeichnis; `docs/help/impressum/` dokumentiert es).

## Weitere relevante Konventionen

- `scripts/` enthält Node-Tooling (`.mjs`/`.cjs`) inkl. begleitender `*.test.mjs`-Dateien.
- `docs/features/task/issue-80-…/decisions.md` existiert bereits und enthält beschlossene Antworten auf die offenen Fragen (u. a. Ablageort `site/`, Trigger `staging`+`main`+`workflow_dispatch`, Sprache Deutsch, eingecheckte Playwright-PNGs, `gh api`-Aktivierung von Pages).
