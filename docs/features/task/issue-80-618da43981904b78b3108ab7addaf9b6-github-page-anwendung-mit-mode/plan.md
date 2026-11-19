# Umsetzungsplan: GitHub-Page-Präsentationsseite (Issue #80)

## Übersicht

Es wird eine statische Präsentations-Website für die Schnittstellenzentrale eingeführt: ein neuer Quellordner `site/` (HTML/CSS/JS, eingecheckte Screenshots), ein neuer GitHub-Actions-Workflow `.github/workflows/pages.yml` (Deployment via `actions/configure-pages` → `actions/upload-pages-artifact` → `actions/deploy-pages`, Environment `github-pages`) sowie eine dedizierte Playwright-Klasse `SiteScreenshotTests`, die repräsentative App-Ansichten (hell und dunkel) als PNG in `site/screenshots/` schreibt. Ergänzt wird die Dokumentation (`docs/help/github-page/`, Eintrag in `docs/help/index.md`, kurzer README-Abschnitt, `docs/RELEASE_NOTES.md`-Eintrag). Der Anwendungscode der Blazor-App bleibt unverändert; einzige C#-Änderungen sind additive Testinfrastruktur im Testprojekt.

## Scope-Klärung

| Feld | Festlegung |
|------|------------|
| Verarbeitungseinheit | Ein Deployment-Lauf der Präsentationsseite: das Bündel aus `site/`-Quellen (HTML/CSS/JS, Favicon, eingecheckte PNG-Screenshots), das als Ganzes als Pages-Artefakt gebaut und veröffentlicht wird |
| Auslösende Ereignisse | `push` auf `staging` und `main` mit `paths`-Filter auf `site/**`, `.github/workflows/pages.yml` und `scripts/verify-site.mjs`; zusätzlich `workflow_dispatch`. Einmalig vorgelagert: Aktivierung von GitHub Pages im Repository (in diesem Lauf bereits per `gh api` erledigt — Source „GitHub Actions") |
| Erwartete Wirkung | Öffentlich abrufbare deutsche Webseite unter `https://martin-stromberg.github.io/Schnittstellenzentrale/` mit Vorstellung der Anwendung, Feature-Kacheln, Screenshot-Galerie, Download-Verweis (`releases/latest` + Asset-Hinweis `Schnittstellenzentrale-vX.Y.Z.zip`, optional Release-Badge via shields.io) und Footer-Links auf Repository, `docs/help/` und `LEGAL.md`; helle und dunkle Darstellung mit Toggle; responsiv. Der Aktualisierungsweg ist repo-seitig dokumentiert (`docs/help/github-page/`, `docs/help/index.md`-Eintrag, README-Abschnitt, `docs/RELEASE_NOTES.md`-Eintrag) und die Dokumentations-Artefakte werden durch `scripts/verify-site.mjs` auf Existenz und Mindestinhalt geprüft |
| Umgang mit fehlenden Objekten | Pages-Aktivierung: wird per `gh api` **erzeugt** (in diesem Lauf, nicht als Workflow-Schritt); `github-pages`-Environment: wird beim ersten `actions/deploy-pages`-Lauf automatisch **erzeugt**; `site/`-Ordner: wird **neu erzeugt**; Screenshots: werden per Playwright-Testklasse **erzeugt**, eingecheckt und danach **wiederverwendet**; `pages.yml`: wird **neu erzeugt**; Demo-Daten für Screenshots: werden je Testlauf in der In-Memory-Testdatenbank **erzeugt** und nach dem Lauf verworfen |

### Scope-Ausschlüsse

| # | Einschränkung / Ausschluss | Begründung | Status |
|---|----------------------------|------------|--------|
| 1 | Keine Änderungen am Anwendungscode der Blazor-App (keine neuen Domänenklassen, APIs, UI-Komponenten) | Anforderung betrifft ausschließlich Präsentationsfläche; Ausnahme: additive Testinfrastruktur (`SiteScreenshotTests`, `SitePageTests`, Seeder-Erweiterung, Accessor in `PlaywrightTestBase`) | bestätigt (requirement.md + decisions.md) |
| 2 | `docs/` wird nicht als Pages-Source veröffentlicht; kein Docs-Portal, kein Jekyll-Umbau | `docs/` enthält `help/`, `features/` und `RELEASE_NOTES.md` — die GitHub-„docs-folder"-Option würde alles darunter veröffentlichen; Entscheidung: dedizierter `site/`-Ordner + Actions-Deployment | bestätigt (decisions.md Nr. 2) |
| 3 | Keine Mehrsprachigkeit der Page, kein Sprachumschalter — Seite nur auf Deutsch | Issue und Repository-Dokumentation sind deutsch; Umschalter nicht gefordert und später nachrüstbar | bestätigt (decisions.md Nr. 3) |
| 4 | Keine Screenshot-Generierung im Deploy-Workflow | Erforderte .NET-Build + Playwright-Browser im Deploy-Job (schwer, CI-Playwright läuft derzeit `continue-on-error`); eingecheckte PNGs sind die Quelle | bestätigt (decisions.md Nr. 4) |
| 5 | Kein `release`-Event-Trigger, keine dynamische Versionsnummer-Auflösung | Stabile URL `releases/latest` bleibt ohne Redeploy aktuell; Versionsnummer-Anzeige nicht gefordert | bestätigt (decisions.md Nr. 6) |
| 6 | Kein Blog, keine Versionshistorie der Seite, keine Custom Domain, keine Such-/Kontakt-/Analytics-Funktionen | Nicht gefordert; Seite bleibt statisch ohne Tracking | bestätigt (requirement.md Scope-Ausschlüsse) |

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| Seitentechnologie | Plain HTML/CSS/JS ohne Build-Tool, ohne Bootstrap, ohne Framework; Dateien: `site/index.html`, `site/site.css`, `site/theme.js`, `site/favicon.png` (Kopie aus `wwwroot/favicon.png`), `site/screenshots/*.png` | Kleinster Pfad, kein Build-Schritt nötig — `actions/upload-pages-artifact` liefert den Ordner direkt aus; Bootstrap ist bewusst auch in der App nicht geladen. Jekyll läuft beim Actions-Deployment ohnehin nicht — weder `_config.yml` noch `.nojekyll` erforderlich |
| Farb-/Token-System | Eigene kompakte Token-Datei in `site.css`: `:root`/`html.dark`-Variablen mit den Werten aus `shadcn/default-theme.css` (`--background`, `--foreground`, `--card`, `--primary`, `--muted-foreground`, `--border`, `--radius`, `--accent`) plus Gradient-Variablen nach Vorbild `--sz-sidebar-bg-start/-end` (`rgb(5,39,103)` → `#3a0647`, dark `#0d1b2a` → `#1a0a1e`) für Hero/Header | `app.css` ist 2639 Zeilen app-spezifisches Shell-Layout und nicht wiederverwendbar; nur die Token-**Werte** werden übernommen, damit die Seite optisch zur App passt. Gradient-Referenz existiert nur noch als Farbwert (die `--sz-*`-Variablen sind in der App unreferenzierte Relikte) — die Seite nutzt sie bewusst als Gestaltungselement |
| Theme-Mechanik der Seite | `.dark`-Klasse auf `document.documentElement` (Konvention aus `theme.js`/`.claude/css-conventions.md`), `localStorage`-Schlüssel `colorScheme` mit Werten `Light`/`Dark` (App-Parität), `prefers-color-scheme` als Initial-Fallback wenn kein gespeicherter Wert; Inline-Init-Script im `<head>` gegen Flash-of-Theme | Die App selbst wertet `prefers-color-scheme` nicht aus (Default Light); für die öffentliche Seite ist der Media-Query-Fallback als Initialwert sinnvoll, weil Besucher keine App-Voreinstellung haben — gespeicherte Wahl schlägt Media-Query |
| Typografie | Font-Stack `'Helvetica Neue', Helvetica, Arial, sans-serif` (wie `app.css`), Code-/URL-Elemente in `'JetBrains Mono', 'Cascadia Code', 'Consolas', monospace`; keine externen Font-Imports | App-Parität, keine externe Abhängigkeit |
| Workflow-Job-Struktur `pages.yml` | **Ein** Job `deploy` (`ubuntu-latest`, `timeout-minutes: 10`), der alle Steps von checkout bis deploy enthält; `environment:` und `url:` werden als **Job-Level-Keys** gesetzt (`environment: { name: github-pages, url: ${{ steps.deployment.outputs.page_url }} }`), `deploy-pages` trägt lediglich `id: deployment` | GitHubs Referenzmuster nutzt zwei Jobs (`build` + `deploy` mit `needs:`), weil dort der Deploy-Job separat das Environment bindet — nötig ist das nicht: `environment`/`url` sind gültige Job-Level-Keys und funktionieren in einem einzigen Job ebenso. Da kein Build-Schritt existiert (verify → configure → upload → deploy), ist ein Job das einfachste korrekte Modell und deckt sich mit den Ein-Job-Konventionen der Review-Workflows im Repo |
| Screenshot-Erzeugung | Dedizierte Playwright-Klasse `SiteScreenshotTests` in der bestehenden `"Playwright"`-Collection; PNGs über `Page.ScreenshotAsync()` mit festem Viewport (1440×900) in `site/screenshots/`; Repo-Root per `FindRepoRoot()`-Helfer (Hochwandern von `AppContext.BaseDirectory` bis `Schnittstellenzentrale.slnx` gefunden — Muster aus `PlaywrightServer.FindWebProjectRoot()`) | Infrastruktur (`PlaywrightServer`, `PlaywrightTestBase`, `TestAuthHandler`, In-Memory-SQLite) ist fertig; eingecheckte PNGs sind die Quelle der Galerie (Entscheidung 4). Fester Viewport + `WaitForLoadStateAsync(NetworkIdle)` + Circuit-Retry-Muster aus `LayoutSmokeTests` sorgen für reproduzierbare Bilder |
| Demo-Daten für Screenshots | Neue Methode `SeedShowcaseDataAsync()` in `TestDatabaseSeeder`, die nach `ResetAsync()` zusätzlich über `IApplicationRepository`/`IEndpointRepository`/`ISystemEnvironmentRepository` repräsentative Daten anlegt (1–2 `ApplicationGroup`s, 2–3 `Application`s mit `Description`/`Owner`/`BaseUrl`, `EndpointGroup`s, mehrere `Endpoint`s mit gemischten `HttpMethod`-Werten für die Method-Badges, 1–2 `SystemEnvironment`s). `SystemEnvironment.Mode`/`StorageMode` bleibt auf dem Default `Team` (`StorageModeService.CurrentMode` defaultet auf `Team`) — im Team-Modus greift der Owner-Filter in `ApplicationRepository.ApplyOwnerFilter` nicht, alle geseedeten Daten sind daher unabhängig vom `Owner`-Wert sichtbar. Erst wenn später gezielt User-Modus-Ansichten gezeigt werden sollen, muss `Application.Owner = "TEST\testuser"` gesetzt und der Modus passend geschaltet werden | `ResetAsync()` bleibt unverändert — der minimale Standard-Seed (nur Systemgruppe + `POST /authenticate`) ist Baseline aller 64 bestehenden Playwright-Tests; eine Erweiterung dort würde deren Ausgangszustand verändern. Der Showcase-Seed wird ausschließlich von `SiteScreenshotTests` aufgerufen |
| Zugriff der Testklasse auf Services | `PlaywrightTestBase` erhält eine `protected`-Eigenschaft `Server` (Rückgabe des bisher `private` gehaltenen `_server`), über die `Server.Services` erreichbar ist | Additiv, bricht keinen bestehenden Test; nötig, damit `SiteScreenshotTests` den `TestDatabaseSeeder` bauen und `SeedShowcaseDataAsync()` aufrufen kann |
| Statische Seitenprüfung | Eigene Playwright-Klasse `SitePageTests` (Namespace `…Tests.Playwright`, **ohne** `PlaywrightTestBase`-Server-Abhängigkeit und **ohne** `[Collection("Playwright")]`-Attribut): eigener Browser-Lebenszyklus, `site/` wird über `Context.RouteAsync`-Interception unter einer Pseudo-Origin (`http://site.test/`) ausgeliefert (`RouteFulfillOptions.Path`, Content-Type aus Extension) | Bewusst **nicht** in der `"Playwright"`-Collection: Deren Collection-Fixture würde den Kestrel-App-Server (`PlaywrightServer`) ungenutzt mitstarten und die Klasse mit allen Server-Tests serialisieren. Ohne Collection-Attribut bildet `SitePageTests` eine eigene implizite Collection, läuft unabhängig und braucht keinen Server. Ein echter `http`-Origin (statt `file://`) macht die `localStorage`-Prüfung des Toggles zuverlässig. Läuft weiter unter dem Suite-Filter `FullyQualifiedName~Playwright` |
| Vorab-Validierung vor Deploy | Neues Node-Skript `scripts/verify-site.mjs` (Konvention: Node-Tooling in `scripts/` mit begleitender `verify-site.test.mjs`) prüft statisch die `site/`-Dateien **und** die Doku-Artefakte des Aktualisierungswegs (siehe „Programmabläufe" → „Statische Prüfung"); Aufruf als Job-Step in `pages.yml` **vor** `configure-pages` | Bricht den Deploy früh bei fehlenden Screenshot-Dateien, kaputten Asset-Referenzen oder fehlender/gestutzter Pflege-Doku, ohne neue Dependencies (`node` ist auf `ubuntu-latest` vorhanden; `actions/setup-node@v7` wird konventionsgemäß zur Version-Pinnung genutzt). Die Doku-Prüfung ist der konkrete Testnachweis für das Akzeptanzkriterium „Aktualisierungsweg dokumentiert". YAML-Lint des Workflows: nicht als eigener Check — GitHub validiert die Workflow-Datei beim ersten Lauf; das Skript deckt stattdessen die deploy-relevanten Artefakte ab |
| Release-Verweis | Stabiler Link `https://github.com/martin-stromberg/Schnittstellenzentrale/releases/latest` + Text-Hinweis auf Asset `Schnittstellenzentrale-vX.Y.Z.zip` + optionales shields.io-Release-Badge (`github/v/release`) | Entscheidung 6 — kein Redeploy pro Release nötig |
| Impressum/Rechtliches | Footer-Link auf `LEGAL.md` im Repository | Entscheidung 7 — kein eigenes Impressum auf der Page nötig |

## Programmabläufe

### Pages-Deployment (Workflow `pages.yml`)

Job-Struktur (verbindlich): Workflow-Header `name:` + englischer Kommentarkopf; `on: push` (`branches: [staging, main]`, `paths:` = `site/**`, `.github/workflows/pages.yml`, `scripts/verify-site.mjs`) und `workflow_dispatch`; `permissions: { contents: read, pages: write, id-token: write }`; `concurrency: { group: pages, cancel-in-progress: false }`. **Ein einziger Job** `deploy` mit `runs-on: ubuntu-latest`, `timeout-minutes: 10` und den **Job-Level-Keys**:

```yaml
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
```

`environment:` und `url:` gehören auf Job-Ebene — sie sind **keine** Step-Attribute von `actions/deploy-pages`. Innerhalb des Jobs laufen die Steps:

1. `actions/checkout@v7` auf den auslösenden Ref.
2. `actions/setup-node@v7` (`node-version: 22`), dann `node scripts/verify-site.mjs` (`shell: pwsh`) — statische Prüfung der Seitenartefakte und Doku-Artefakte; bei Fehler bricht der Job vor jeglichem Deploy ab.
3. `actions/configure-pages` — Pages-Kontext für den Deploy.
4. `actions/upload-pages-artifact` mit `path: site/` — packt den Ordner als Pages-Artefakt.
5. `actions/deploy-pages` mit `id: deployment` — publiziert unter `https://martin-stromberg.github.io/Schnittstellenzentrale/`; die Action füllt `steps.deployment.outputs.page_url`, das der Job-Level-Key `url` auswertet. Das `github-pages`-Environment legt GitHub beim ersten Lauf automatisch an.

Hinweise für die Implementierung: Die Action-Versionen (`configure-pages`, `upload-pages-artifact`, `deploy-pages` — Stand des Plans `@v5`/`@v4`/`@v4`) sind bei Umsetzung gegen die im Repo üblichen Pin-Stände (`@v6`/`@v7`-Konvention laut Inventory) und die tatsächlich verfügbaren Versionen zu verifizieren. `workflow_dispatch` ist erst nutzbar, sobald die Workflow-Datei auf dem Default-Branch (`main`) liegt — der erste Deploy erfolgt ohnehin über den `push`-Trigger auf `staging`; für die Abnahme-Reihenfolge relevant (manueller Trigger verfügbar erst nach Promotion).

Beteiligte Artefakte: `.github/workflows/pages.yml`, `scripts/verify-site.mjs`, `site/**`.

### Statische Prüfung (`scripts/verify-site.mjs`)

Das Skript läuft lokal (Entwickler) und als Job-Step in `pages.yml` vor `configure-pages`. Zwei Prüfblöcke:

1. **Seiten-Artefakte** (`site/`): Pflicht-Sektionen in `index.html` (Hero/Vorstellung, Feature-Kacheln, Galerie, Download, Footer), `lang="de"`, Viewport-Meta, alle lokal referenzierten Assets/Screenshots existieren und sind nicht leer, kein Bootstrap-/CDN-`link`.
2. **Doku-Artefakte des Aktualisierungswegs** (Existenz + Mindestinhalt):
   - `docs/help/github-page/index.md`, `docs/help/github-page/beschreibung.md`, `docs/help/github-page/ablauf-technisch.md` existieren und sind nicht leer; `index.md` enthält eine `## Inhalt`-Liste mit Links auf die beiden Teilseiten.
   - `ablauf-technisch.md` enthält die Stichworte des Aktualisierungswegs: `pages.yml`, `gh api` (Pages-Aktivierung), `SiteScreenshotTests` (Screenshot-Regenerierung), `site/` (Pflegeort).
   - `docs/help/index.md` enthält einen Link `github-page/index.md`.
   - `README.md` enthält einen Abschnitt `## GitHub Page`.
   - `docs/RELEASE_NOTES.md` enthält einen Eintrag, der die GitHub Page nennt (Stichwort `GitHub Page` bzw. `github.io`) — in beiden Sprachblöcken.

Jeder Verstoß erzeugt eine benannte Fehlermeldung und einen Exit-Code ≠ 0. `scripts/verify-site.test.mjs` deckt Positivfall und typische Fehlerfälle (fehlendes PNG, fehlendes `lang="de"`, fehlende Doku-Datei, fehlender Doku-Stichwort-Treffer) per `node --test` ab.

### Screenshot-Erzeugung (lokaler Lauf)

1. Entwickler führt aus: `dotnet test --filter "FullyQualifiedName~SiteScreenshotTests"` (lokal sichtbarer Browser, headless unter `CI=true`).
2. `PlaywrightTestBase.InitializeAsync()` startet via `PlaywrightServer` die echte App auf `http://127.0.0.1:5099` und setzt die In-Memory-DB per `TestDatabaseSeeder.ResetAsync()` zurück.
3. `SiteScreenshotTests` ruft `SeedShowcaseDataAsync()` auf (Demo-Daten; `SystemEnvironment.Mode` bleibt `Team` → Owner-Filter inaktiv, alle Daten sichtbar) und setzt `Page.SetViewportSizeAsync(1440, 900)`.
4. Je Screenshot-Fact: Navigation `Page.GotoAsync(BaseUrl)` → `WaitForLoadStateAsync(NetworkIdle)` → Baum aufklappen über `.sz-tree-chevron-btn` / Klick auf `.sz-tree-item-btn` (Applikation/Endpunkt auswählen) → Warten auf Zielselektor → `Page.ScreenshotAsync()` → PNG nach `site/screenshots/<name>.png`.
5. Dark-Varianten: `Context.AddInitScriptAsync` mit `localStorage.setItem('colorScheme','Dark')` vor Navigation bzw. `Page.EvaluateAsync` zum Setzen der `.dark`-Klasse auf `<html>`.
6. PNGs werden eingecheckt; `site/index.html` referenziert die festen Dateinamen in der Galerie.

Beteiligte Klassen: `SiteScreenshotTests`, `PlaywrightTestBase`, `PlaywrightServer`, `TestDatabaseSeeder`, `IApplicationRepository`, `IEndpointRepository`, `ISystemEnvironmentRepository`.

### Seitenpflege (Aktualisierungsweg)

1. Texte/Features in `site/index.html`, Styles in `site/site.css` ändern; Screenshots bei Bedarf per `SiteScreenshotTests`-Lauf neu erzeugen.
2. Lokal prüfen: statischer Server auf `site/` (z. B. `npx serve site` oder `python -m http.server -d site`) + Browser-Sichtprüfung; zusätzlich `node scripts/verify-site.mjs` und `dotnet test --filter "FullyQualifiedName~SitePageTests"`.
3. PR → Merge auf `staging` → `pages.yml` deployt automatisch (paths-Filter); Promotion nach `main` deployt erneut denselben Stand.
4. Dokumentationspflicht: Inhalts-/Strukturänderungen der Pflege (z. B. neue Doku-Datei, anderer Workflow-Name) müssen die Doku-Artefakte mitanpassen — `verify-site.mjs` bricht sonst den Deploy ab.

Beteiligte Artefakte: `site/**`, `docs/help/github-page/ablauf-technisch.md` (Dokumentation dieses Pfads).

### Dark-Mode-Toggle der Seite

1. `site/theme.js` (kleines Inline-Init im `<head>`): `localStorage.getItem('colorScheme')` lesen; wenn leer → `matchMedia('(prefers-color-scheme: dark)')` als Initialwert.
2. `applyTheme`: setzt/entfernt `.dark` auf `document.documentElement`.
3. Toggle-Button im Header: invertiert Schema, schreibt `colorScheme` (`Light`/`Dark`) nach `localStorage`, wendet Klasse an; Reload stellt Zustand wieder her.

Beteiligte Artefakte: `site/theme.js`, `site/site.css` (`.dark`-Variablen), `site/index.html` (Toggle-Button, Inline-Init).

## Neue Klassen und Dateien

| Klasse / Datei | Typ | Zweck |
|----------------|-----|-------|
| `site/index.html` | Statische Seite | Deutsche Präsentationsseite: Hero mit Vorstellung + CTA (Download, Repository), Feature-Kacheln, Screenshot-Galerie, Download-Abschnitt (`releases/latest` + Asset-Hinweis + optional Badge), Footer (Repository, `docs/help/`, `LEGAL.md`) |
| `site/site.css` | Statische Datei | Eigenes Token-/Klassensystem (`:root` + `html.dark`-Variablen, Gradient-Hero, Karten, Galerie, Footer, Media-Queries für Responsivität) |
| `site/theme.js` | Statische Datei | Theme-Init + Toggle-Logik (`colorScheme`-Schlüssel, `prefers-color-scheme`-Fallback) |
| `site/favicon.png` | Asset | Kopie von `src/Schnittstellenzentrale/wwwroot/favicon.png` (32×32) |
| `site/screenshots/*.png` | Assets | Von `SiteScreenshotTests` erzeugte, eingecheckte Ansichten (mind. `home-light.png`, `application-light.png`, `endpoint-light.png`, `home-dark.png`) |
| `.github/workflows/pages.yml` | Workflow | Pages-Deploy wie unter „Programmabläufe" beschrieben (ein `deploy`-Job, `environment`/`url` als Job-Level-Keys) |
| `SiteScreenshotTests` | Playwright-Testklasse (`src/Schnittstellenzentrale.Tests/Playwright/`, Collection `"Playwright"`) | Erzeugt die Galerie-PNGs; Facts je Ansicht (Workspaces-Light, Anwendungsdetail-Light, Endpunkt-Light, Home-Dark) |
| `SitePageTests` | Playwright-Testklasse (`src/Schnittstellenzentrale.Tests/Playwright/`, **keine** Collection, eigenständiger Browser ohne `PlaywrightTestBase`-Server) | Smoke-Test der statischen Seite über Route-Interception (Inhalte, Bild-Ladestatus, Dark-Toggle + Persistenz, Responsivität) |
| `scripts/verify-site.mjs` | Node-Skript | Statische Prüfung der `site/`-Artefakte (Pflicht-Sektionen, `lang="de"`, Viewport-Meta, alle referenzierten lokalen Assets/Screenshots existieren und sind nicht leer, kein Bootstrap-/CDN-`link`) **und** der Doku-Artefakte des Aktualisierungswegs (Existenz + Stichwort-Mindestinhalt, siehe „Statische Prüfung") |
| `scripts/verify-site.test.mjs` | Node-Test | `node --test`-Tests für `verify-site.mjs` (Konvention: `*.test.mjs` in `scripts/`), inkl. Fälle für die Doku-Artefakt-Prüfung |
| `docs/help/github-page/index.md` | Hilfe-Doku | Themenseite nach `docs/help/`-Konvention (H1 + 1–2 Sätze + `## Inhalt`) |
| `docs/help/github-page/beschreibung.md` | Hilfe-Doku | Zweck, Inhalte, Design-Anlehnung, Sprache |
| `docs/help/github-page/ablauf-technisch.md` | Hilfe-Doku | Aufbau `site/`, Workflow-Ablauf, Pages-Aktivierung (inkl. `gh api`-Befehl für Wiederholungsfälle), Screenshot-Regenerierung, Pflegeprozess |

## Änderungen an bestehenden Klassen

### `PlaywrightTestBase` (Testinfrastruktur)

- **Neue Eigenschaften:** `Server` (`protected PlaywrightServer`) — gibt die Collection-Fixture-Instanz `_server` frei, damit abgeleitete Klassen `Server.Services` und `Server.BaseAddress` erreichen; `_server` selbst bleibt `private`.
- **Neue Methoden:** keine
- **Geänderte Methoden:** keine — rein additive Erweiterung, kein Verhaltensunterschied für die 64 bestehenden Tests.

### `TestDatabaseSeeder` (Testinfrastruktur)

- **Neue Methoden:** `SeedShowcaseDataAsync()` — legt repräsentative Demo-Daten an (siehe Designentscheidung „Demo-Daten"); wird ausschließlich von `SiteScreenshotTests` nach `ResetAsync()` aufgerufen; `ResetAsync()` selbst bleibt unverändert.
- **Geänderte Methoden:** keine.

### `docs/help/index.md` (Dokumentation)

- Neuer Eintrag `- [GitHub Page](github-page/index.md) — …` im Abschnitt „Qualität und Tests" (dort liegen bereits die Workflow-/Test-Themen `review-automatisierung`, `playwright-tests`, `git-hooks`).

### `README.md` (Dokumentation)

- Neuer kurzer Abschnitt `## GitHub Page` (vor „Rechtliches"): URL, Quellordner `site/`, Workflow-Name, Verweis auf `docs/help/github-page/`.

### `docs/RELEASE_NOTES.md` (Dokumentation)

- Neuer Bullet im EN-Block unter `## What's New` und Pendant im DE-Block unter `## Neuerungen` (fortlaufendes bilinguales Dokument ohne Versionskapitel): öffentliche GitHub-Page-Präsentationsseite unter `https://martin-stromberg.github.io/Schnittstellenzentrale/` — muss die Stichworte enthalten, die `verify-site.mjs` prüft (`GitHub Page`/`github.io`).

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine anwendungsseitigen Validierungen (kein App-Input). Die statischen Prüfregeln von `scripts/verify-site.mjs` (Seiten-Artefakte + Doku-Artefakte) und `SitePageTests` sind unter „Tests" geführt.

## Konfigurationsänderungen

Keine (`appsettings.json` unberührt). Die Workflow-Parameter (`permissions`, `concurrency`, `environment`, `paths`-Filter) sind Inhalt der neuen `pages.yml` und unter „Programmabläufe" beschrieben.

## Seiteneffekte und Risiken

- **Bestehende Playwright-Tests:** `PlaywrightTestBase`/`TestDatabaseSeeder` werden nur additiv erweitert; `ResetAsync()`-Baseline bleibt identisch — die 64 bestehenden Tests sind nicht betroffen.
- **CI-Laufzeit:** `SiteScreenshotTests` + `SitePageTests` laufen mit im Playwright-Step der PR-/Staging-CI (`continue-on-error: true`); sie erhöhen die Laufzeit moderat (wenige zusätzliche Facts auf einem bereits laufenden Server). `SitePageTests` startet keinen Server (keine `"Playwright"`-Collection) und ist entsprechend billig.
- **Neue Testklassen im Repo-Root schreiben:** `SiteScreenshotTests` schreibt PNGs nach `site/screenshots/` — in CI landen sie im Wegwerf-Workspace (kein Commit); nur lokale Läufe erzeugen committbare Änderungen. Gewollt, aber in der Hilfe-Doku zu erklären.
- **`staging`-Deploys:** Jeder `staging`-Push mit `site/`-Änderung aktualisiert die öffentliche Seite — bewusste Entscheidung (Präsentationsfläche, kein Release-Artefakt); PR-Review muss Seitenänderungen entsprechend prüfen.
- **Erster Deploy ohne aktivierte Pages-Funktion:** Schlägt `deploy-pages` fehl, wenn Pages nicht aktiviert ist — wird in diesem Lauf per `gh api` vorbereitet (expliziter Schritt in der Umsetzungsreihenfolge); `docs/help/github-page/ablauf-technisch.md` dokumentiert den Schritt.
- **Doku-Drift:** Die `verify-site.mjs`-Dokuprüfung verhindert, dass die Pflege-Doku still gelöscht oder entkernt wird — umgekehrt muss jede bewusste Doku-Umstrukturierung die Stichwort-Prüfung des Skripts mitpflegen.
- **Commit-Konvention:** Abschließende Commits enthalten `Closes #80` (Issue schließt bei Promotion nach `main`, Staging-Workflow).

## Umsetzungsreihenfolge

1. **`site/`-Grundgerüst anlegen**
   - Voraussetzungen: Keine (Designentscheidungen oben).
   - Beschreibung: `site/index.html` (deutsche Inhalte, Sektionen Hero/Vorstellung, Feature-Kacheln aus README-Featureliste, Screenshot-Galerie mit festen PNG-Dateinamen, Download-Abschnitt `releases/latest` + Asset-Hinweis `Schnittstellenzentrale-vX.Y.Z.zip` + optionales shields.io-Release-Badge, Footer mit Links auf Repository/`docs/help/`/`LEGAL.md`, `lang="de"`, Viewport-Meta), `site/site.css` (Token + Layout + `.dark`-Set + Media-Queries), `site/theme.js` + Inline-Init im `<head>`, `site/favicon.png` kopieren, `site/screenshots/` mit `.gitkeep` anlegen (PNGs folgen in Schritt 3).

2. **Testinfrastruktur für Screenshots erweitern**
   - Voraussetzungen: `PlaywrightServer`, `PlaywrightTestBase`, `TestDatabaseSeeder` (vorhanden); XML-Dokumentationspflicht des Testprojekts beachten (pre-commit-Hook: CS1591 als Fehler, `<param>`/`<returns>` vollständig).
   - Beschreibung: `protected`-Eigenschaft `Server` in `PlaywrightTestBase` ergänzen; `SeedShowcaseDataAsync()` in `TestDatabaseSeeder` implementieren (Demo-Daten über `IApplicationRepository`, `IEndpointRepository`, `ISystemEnvironmentRepository`; `SystemEnvironment.Mode`/`StorageMode` auf Default `Team` belassen, damit alle Daten ohne Owner-Filter sichtbar sind); `FindRepoRoot()`-Helfer in `SiteScreenshotTests`.

3. **`SiteScreenshotTests` schreiben, Screenshots erzeugen und einchecken**
   - Voraussetzungen: Schritte 1–2 (Galerie-Dateinamen stehen fest; Seed-Methode vorhanden).
   - Beschreibung: Facts je Ansicht (Viewport 1440×900, `NetworkIdle`, Baum-Selektoren `.sz-tree-chevron-btn`/`.sz-tree-item-btn`, Dark via `colorScheme`-Init-Script); Lauf `dotnet test --filter "FullyQualifiedName~SiteScreenshotTests"`; PNGs prüfen und committen.

4. **`SitePageTests` + `scripts/verify-site.mjs` (inkl. `.test.mjs`) erstellen**
   - Voraussetzungen: Schritt 3 (echte PNGs vorhanden, damit Bild-Ladestatus prüfbar ist).
   - Beschreibung: `SitePageTests` (keine Collection) serviert `site/` via `Context.RouteAsync` unter Pseudo-Origin und prüft Inhalte/Bilder/Dark-Toggle/Responsivität; `verify-site.mjs` prüft Pflicht-Struktur und Asset-Referenzen der Seite **sowie** Existenz und Stichwort-Mindestinhalt der Doku-Artefakte (`docs/help/github-page/*`, `docs/help/index.md`-Eintrag, README-Abschnitt, `docs/RELEASE_NOTES.md`-Eintrag — siehe „Statische Prüfung"); `verify-site.test.mjs` deckt beide Prüfblöcke ab. Hinweis: Die Doku-Prüfung schlägt lokal fehl, bis Schritt 7 ausgeführt ist — das ist gewollt (Der Check erzwingt das Dokumentations-Artefakt); für den lokalen Seiten-Check kann der Doku-Block per Flag (z. B. `--site-only`) übersprungen werden, im Workflow läuft der volle Check.

5. **GitHub Pages im Repository per `gh api` aktivieren (Orchestrator-Schritt)**
   - Voraussetzungen: `gh`-CLI authentifiziert mit Token des Repository-Inhabers (Admin-Rechte auf `martin-stromberg/Schnittstellenzentrale`). Keine Repo-Dateien nötig — kann jederzeit vor dem ersten Deploy laufen.
   - Beschreibung: Vom Orchestrator-Lauf manuell ausgeführt (kein Workflow-Schritt): `gh api repos/martin-stromberg/Schnittstellenzentrale/pages -X POST -f build_type=workflow` (Source „GitHub Actions"); Erfolg per `gh api repos/martin-stromberg/Schnittstellenzentrale/pages` verifizieren (`build_type: "workflow"`, `status` gesetzt). Bei „Already exists" ggf. `PUT` mit gleichem Body. Muss abgeschlossen sein, bevor der erste `pages.yml`-Lauf deployt (sonst schlägt `deploy-pages` fehl). Der Schritt wird zusätzlich in `docs/help/github-page/ablauf-technisch.md` dokumentiert.

6. **`.github/workflows/pages.yml` erstellen**
   - Voraussetzungen: Schritt 4 (Verify-Skript existiert und ist lokal grün — im `--site-only`-Modus bis Schritt 7 fertig ist); Schritt 5 ist keine Build-Voraussetzung, muss aber vor dem ersten Deploy-Lauf erledigt sein.
   - Beschreibung: Workflow exakt gemäß „Programmabläufe" → „Pages-Deployment": Kommentarkopf, Trigger (`push` auf `staging`/`main` mit `paths`-Filter + `workflow_dispatch`), `permissions` (`contents: read`, `pages: write`, `id-token: write`), `concurrency: pages` ohne cancel, **ein** Job `deploy` (`ubuntu-latest`, `timeout-minutes: 10`) mit Job-Level-Keys `environment: { name: github-pages, url: ${{ steps.deployment.outputs.page_url }} }`, Steps checkout → setup-node → `node scripts/verify-site.mjs` → configure-pages → upload-pages-artifact (`path: site/`) → deploy-pages mit `id: deployment`. Action-Versionen dabei gegen Repo-Konventionen (`@v6`/`@v7`) und tatsächliche Verfügbarkeit verifizieren.

7. **Dokumentation schreiben**
   - Voraussetzungen: Schritte 1–6 (fertige Pfade/Befehle dokumentierbar).
   - Beschreibung: `docs/help/github-page/{index,beschreibung,ablauf-technisch}.md` (inkl. Pages-Aktivierung per `gh api`, Screenshot-Regenerierung, Pflegeprozess), Eintrag in `docs/help/index.md` („Qualität und Tests"), README-Abschnitt `## GitHub Page`, Eintrag in `docs/RELEASE_NOTES.md` (EN- + DE-Bullet mit Stichwort `GitHub Page`/`github.io`). Danach muss `node scripts/verify-site.mjs` vollständig grün sein.

8. **Lokale Abnahme**
   - Voraussetzungen: Schritte 1–7.
   - Beschreibung: statischer Server auf `site/` + Browsercheck aller Akzeptanzszenarien; `node scripts/verify-site.mjs` (voller Lauf inkl. Doku-Block), `node --test scripts/verify-site.test.mjs`, `dotnet test --filter "FullyQualifiedName~SitePageTests|FullyQualifiedName~SiteScreenshotTests"`; Remote-Deploy-Verifikation erfolgt nach Merge (siehe Akzeptanzszenarien).

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `SeedShowcaseDataAsync()` | `TestDatabaseSeeder` (Hilfsmethode) | Stellt repräsentative Demo-Daten bereit (Gruppen, Anwendungen, Endpunktgruppen, Endpunkte, Environments) |
| `Workspaces_Light_Screenshot` | `SiteScreenshotTests` | Schreibt `site/screenshots/home-light.png` (aufgeklappter Baum mit Demo-Daten, hell) |
| `ApplicationDetail_Light_Screenshot` | `SiteScreenshotTests` | Schreibt `site/screenshots/application-light.png` (Anwendungsdetailansicht) |
| `EndpointPage_Light_Screenshot` | `SiteScreenshotTests` | Schreibt `site/screenshots/endpoint-light.png` (Endpunktansicht mit Method-Badge) |
| `Workspaces_Dark_Screenshot` | `SiteScreenshotTests` | Schreibt `site/screenshots/home-dark.png` (`.dark`-Variante) |
| `Index_ZeigtAllePflichtsektionen` | `SitePageTests` | Hero/Titel „Schnittstellenzentrale", Feature-Kacheln (≥ 6), Galerie, Download- und Footer-Bereiche sichtbar |
| `Screenshots_SindGeladen` | `SitePageTests` | Alle `img` in der Galerie haben `naturalWidth > 0` |
| `DownloadLink_ZeigtAufLatestRelease` | `SitePageTests` | Download-Link enthält `releases/latest`; Footer verlinkt Repository, `docs/help/`, `LEGAL.md` |
| `DarkToggle_SchaltetUm_UndPersistiert` | `SitePageTests` | Klick auf Toggle setzt `.dark` auf `<html>`, `localStorage.colorScheme === "Dark"`; nach Reload bleibt Dark aktiv |
| `MobileViewport_KeinHorizontalScroll` | `SitePageTests` | Viewport 390×844: `scrollWidth <= clientWidth`, Kerninhalte sichtbar |
| `verify-site.mjs` — Seiten-Artefakt-Block | Node-Skript (kein Test, Deploy-Gate) | Prüft `site/`: Pflicht-Sektionen, `lang="de"`, Viewport-Meta, Asset-/Screenshot-Referenzen, kein CDN-`link` |
| `verify-site.mjs` — Doku-Artefakt-Block | Node-Skript (kein Test, Deploy-Gate) | Prüft Existenz + Mindestinhalt: `docs/help/github-page/{index,beschreibung,ablauf-technisch}.md` (inkl. Stichworte `pages.yml`, `gh api`, `SiteScreenshotTests`, `site/` in `ablauf-technisch.md`), Link `github-page/index.md` in `docs/help/index.md`, Abschnitt `## GitHub Page` in `README.md`, Eintrag `GitHub Page`/`github.io` in `docs/RELEASE_NOTES.md` (beide Sprachblöcke) |
| `verify-site.test.mjs` | `node --test` | Prüflogik von `verify-site.mjs` (positiver Fall + typische Fehlerfälle: fehlendes PNG, fehlendes `lang="de"`, fehlende Doku-Datei, fehlendes Doku-Stichwort) |

### Betroffene bestehende Tests

Keine.

### E2E-Tests (primärer Funktionsnachweis)

Der „Benutzerfluss" dieser Anforderung ist der Besuch der öffentlichen Seite; der Deploy-Ablauf ist ein CI/CD-Vorgang. Primärer lokaler Funktionsnachweis ist `SitePageTests` (echter Browser, echte statische Dateien):

| Priorität | Szenario | Testdatei / Testklasse | Abgedecktes Akzeptanzkriterium | Warum E2E nötig ist |
|-----------|----------|------------------------|-------------------------------|-------------------|
| Pflicht | Seite rendern, alle Inhalte sichtbar | `SitePageTests.Index_ZeigtAllePflichtsektionen` | Zweck, zentrale Features, Download-/Release-Verweis vorhanden | Nur im Browser nachweisbar (HTML+CSS+JS-Zusammenspiel) |
| Pflicht | Screenshot-Galerie lädt echte PNGs | `SitePageTests.Screenshots_SindGeladen` | Screenshots auf der Page vorhanden | `naturalWidth` ist nur im Browser messbar |
| Pflicht | Dark Mode umschalten + persistieren | `SitePageTests.DarkToggle_SchaltetUm_UndPersistiert` | Helle und dunkle Darstellung | localStorage + Klassenmechanik nur e2e prüfbar |
| Pflicht | Responsives Layout | `SitePageTests.MobileViewport_KeinHorizontalScroll` | Design ist responsiv | Viewport-Verhalten nur im Browser prüfbar |
| Pflicht | Screenshots aus der echten App | `SiteScreenshotTests` (alle Facts) | Screenshots zeigen die Anwendung | Erzeugt die Galerie-Artefakte aus der laufenden App |
| Pflicht | Dokumentierter Aktualisierungsweg | `verify-site.mjs` Doku-Artefakt-Block (+ `verify-site.test.mjs`) | Aktualisierung der Seite im Repository dokumentiert | Kein Browserfluss — Existenz-/Inhaltsnachweis der Doku-Artefakte (`docs/help/github-page/*`, `docs/help/index.md`-Eintrag, README-Abschnitt, `RELEASE_NOTES.md`-Eintrag) als statischer Check, der zudem als Deploy-Gate dient |
| Nachgelagert | Remote-Deploy nach Merge | `pages.yml`-Lauf + Abruf der Pages-URL | Seite öffentlich erreichbar | Erst nach Merge auf `staging`/`main` verifizierbar — siehe Akzeptanzszenarien |

Welche bestehenden E2E-Tests müssen angepasst werden?

Keine.

## Akzeptanzszenarien

| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
|---|----------|----------|------------------|----------|-------------------|----------|
| 1 | Happy Path lokal: Seite ansehen | Entwickler/Rezensent startet statischen Server auf `site/` | `site/` vollständig inkl. eingecheckter PNGs; `github-pages`-Environment und Live-Seite noch nicht vorhanden | `npx serve site` (o. ä.) → `http://localhost:<port>` im Browser öffnen → alle Sektionen sichtbar → Dark-Toggle klicken → Reload → schmales Fenster prüfen | Deutsche Seite mit Vorstellung, Feature-Kacheln, Galerie mit echten App-Screenshots, Download-Verweis (`releases/latest` + `Schnittstellenzentrale-vX.Y.Z.zip`-Hinweis), Footer-Links (Repository, `docs/help/`, `LEGAL.md`); `.dark` aktiv und nach Reload persistent; kein Horizontal-Scroll auf Mobile-Breite | `SitePageTests` (alle Facts) + `node scripts/verify-site.mjs` + Sichtprüfung |
| 2 | Deployment-Pfad nach Merge | PR-Merge auf `staging` (oder Push auf `main`) mit Änderungen an `site/**` | GitHub Pages aktiviert (per `gh api`, Source „GitHub Actions" — Umsetzungsschritt 5); `github-pages`-Environment existiert ggf. noch nicht | `pages.yml` startet → Job `deploy`: checkout → setup-node → `verify-site.mjs` → `configure-pages` → `upload-pages-artifact` → `deploy-pages` (`id: deployment`; legt Environment beim ersten Mal an; Job-Level-`url` übernimmt `page_url`) → `gh run watch`/`gh run view` → URL abrufen | Lauf endet `success`; `https://martin-stromberg.github.io/Schnittstellenzentrale/` liefert HTTP 200 mit allen Inhalten aus Szenario 1; der Lauf ist im Repo unter Environments `github-pages` mit URL verlinkt | `gh run view` + Browser-/`curl`-Abruf der Pages-URL. **Ehrliche Grenze:** Erst nach Merge auf `staging` verifizierbar; der `staging`-Trigger macht die Abnahme direkt nach dem PR-Merge möglich, ohne auf die `main`-Promotion zu warten. `workflow_dispatch` steht erst nach Promotion zur Verfügung (Workflow-Datei muss auf `main` liegen) und ist für die Erstabnahme nicht erforderlich |
| 3 | Deploy ohne auslösende Seitenänderung | Push auf `staging`/`main`, der nur App-Code betrifft | `pages.yml` liegt auf dem Branch | Push ändert keine der `paths`-Filter-Pfade | Workflow startet nicht; Seite bleibt im letzten Stand | `gh run list --workflow=pages.yml` zeigt keinen neuen Lauf (indirekte Prüfung; Pfade-Filter in der YAML kontrollieren) |
| 4 | Fehlerfall Deploy bei kaputtem `site/` | Push/Workflow-Dispatch mit fehlendem Screenshot-Asset oder unvollständigem HTML | Seitenquellen defekt | Job bricht im `verify-site.mjs`-Step ab (vor `configure-pages`) | Workflow-Status `fail`, kein Deploy, Live-Seite unverändert | `gh run view --log-failed` zeigt die Verify-Meldung; lokal zuvor an einem absichtlich kaputten Stand testbar |
| 5 | Screenshot-Regenerierung | UI der App hat sich geändert; Galerie soll neue Bilder zeigen | `SiteScreenshotTests` + `SeedShowcaseDataAsync` vorhanden | `dotnet test --filter "FullyQualifiedName~SiteScreenshotTests"` lokal → PNGs in `site/screenshots/` aktualisiert → `git diff` auf binäre Änderung prüfen → committen → Push | Neue PNG-Versionen im Repo; Seite zeigt nach Deploy aktualisierte Ansichten | Testlauf lokal grün; geänderte PNGs im `git status` sichtbar |
| 6 | Wartefall: parallele Deploys | Zwei aufeinanderfolgende Pushes mit `site/`-Änderungen | `pages.yml` aktiv | Erster Lauf läuft, zweiter Push triggert | `concurrency: pages` mit `cancel-in-progress: false` serialisiert: zweiter Lauf wartet bzw. ersetzt den noch wartenden; kein halbfertiges Deploy | `gh run list` zeigt wartenden/serialisierten Lauf; abschließend nur letzter Stand live |
| 7 | Dokumentierter Aktualisierungsweg nachvollziehbar | Rezensent will wissen, wie die Seite aktualisiert wird | `docs/help/github-page/`, `docs/help/index.md`-Eintrag, README-Abschnitt und `RELEASE_NOTES.md`-Eintrag existieren (Schritt 7 abgeschlossen) | `docs/help/index.md` → Link `github-page/index.md` öffnen → `ablauf-technisch.md` lesen (Workflow-Ablauf, `gh api`-Aktivierung, Screenshot-Regenerierung, Pflegeprozess) → `node scripts/verify-site.mjs` lokal ausführen | Doku beschreibt den vollständigen Pflege-Pfad von `site/`-Änderung über PR/Merge bis zum automatischen Deploy; Verify-Lauf bestätigt Existenz und Mindestinhalt aller Doku-Artefakte | `verify-site.mjs` Doku-Artefakt-Block grün (+ `verify-site.test.mjs`-Abdeckung); manuelle Lektüre-Probe des Hilfethemas |

## Offene Punkte

Keine.
