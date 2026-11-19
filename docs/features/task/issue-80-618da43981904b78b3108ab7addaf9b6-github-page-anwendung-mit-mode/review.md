# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### Neue Dateien / Objekte

- [x] `site/index.html` (Statische Seite) — angelegt: `lang="de"` (Zeile 2), Viewport-Meta (Zeile 5), Hero mit Vorstellung + CTAs (Zeilen 39–57), 9 Feature-Kacheln (Zeilen 64–101), Galerie mit den 4 geplanten PNG-Namen (Zeilen 110–125), Download-Abschnitt mit `releases/latest` + Asset-Hinweis `Schnittstellenzentrale-vX.Y.Z.zip` + shields.io-Release-Badge (Zeilen 51, 129–142), Footer mit Repository-/`docs/help/`-/`LEGAL.md`-Links (Zeilen 145–152), Inline-Theme-Init im `<head>` (Zeilen 9–21)
- [x] `site/site.css` (Statische Datei) — angelegt: shadcn-Token-Werte in `:root` und `html.dark` (Zeilen 3–40), Gradient-Variablen `rgb(5,39,103)`→`#3a0647` bzw. dark `#0d1b2a`→`#1a0a1e` (Zeilen 18–19, 37–38), Font-Stacks Helvetica/JetBrains Mono (Zeilen 52, 67), Header/Hero/Karten/Galerie/Footer-Styles, Media-Queries für Responsivität (Zeilen 345–389)
- [x] `site/theme.js` (Statische Datei) — angelegt: `colorScheme`-localStorage mit `Light`/`Dark`, `prefers-color-scheme`-Fallback, `.dark` auf `document.documentElement`, Toggle-Handler mit Persistenz
- [x] `site/favicon.png` (Asset) — angelegt, per `cmp` bytegleich mit `src/Schnittstellenzentrale/wwwroot/favicon.png` verifiziert
- [x] `site/screenshots/*.png` (Assets) — alle 4 geplanten PNGs vorhanden und eingecheckt: `home-light.png`, `application-light.png`, `endpoint-light.png`, `home-dark.png` (jeweils 1440×900, echte App-Screenshots); `.gitkeep` liegt ebenfalls im Ordner
- [x] `.github/workflows/pages.yml` (Workflow) — angelegt: englischer Kommentarkopf, `push` auf `staging`/`main` mit `paths`-Filter (`site/**`, Workflow-Datei, `scripts/verify-site.mjs`), `workflow_dispatch`, `permissions` (`contents: read`, `pages: write`, `id-token: write` — plus `actions: read`, siehe Hinweise), `concurrency: { group: pages, cancel-in-progress: false }`, **ein** Job `deploy` (`ubuntu-latest`, `timeout-minutes: 10`) mit Job-Level-Keys `environment.name: github-pages` + `environment.url: ${{ steps.deployment.outputs.page_url }}`, Steps: `checkout@v7` → `setup-node@v7` (Node 22) → `node scripts/verify-site.mjs` (`shell: pwsh`) → `configure-pages@v6` → `upload-pages-artifact@v5` (`path: site/`) → `deploy-pages@v5` mit `id: deployment`
- [x] `SiteScreenshotTests` (Playwright-Testklasse, `src/Schnittstellenzentrale.Tests/Playwright/SiteScreenshotTests.cs`) — angelegt in Collection `"Playwright"`, abgeleitet von `PlaywrightTestBase`; `FindRepoRoot()`-Helfer (Zeilen 160–171), Viewport 1440×900, `NetworkIdle`-Warten, Baum-Selektoren `.sz-tree-chevron-btn`/`.sz-tree-item-btn`, Dark via `Context.AddInitScriptAsync` mit `colorScheme=Dark` (Zeile 77), Circuit-Retry-Klickmuster (Zeilen 131–146)
- [x] `SitePageTests` (Playwright-Testklasse, `src/Schnittstellenzentrale.Tests/Playwright/SitePageTests.cs`) — angelegt **ohne** Collection-Attribut und ohne `PlaywrightTestBase`; eigener Browser-Lebenszyklus, `site/` via `Context.RouteAsync` unter Pseudo-Origin `http://site.test/` mit `RouteFulfillOptions.Path` und Content-Type aus Extension (Zeilen 140–177)
- [x] `scripts/verify-site.mjs` (Node-Skript) — angelegt: Block 1 Seiten-Artefakte (Pflicht-Sektionen `hero`/`features`/`screenshots`/`download`/`footer`, `lang="de"`, Viewport-Meta, lokale Asset-Referenzen auf Existenz/Nicht-Leere, kein externer `link`, kein Bootstrap-/CDN-Verweis) und Block 2 Doku-Artefakte (Existenz + `## Inhalt`-Links der drei `docs/help/github-page/`-Dateien, Stichworte `pages.yml`/`gh api`/`SiteScreenshotTests`/`site/` in `ablauf-technisch.md`, Link in `docs/help/index.md`, `## GitHub Page` in `README.md`, `GitHub Page`/`github.io` in beiden `RELEASE_NOTES.md`-Sprachblöcken); Exit-Code 1 bei Verstößen, `--site-only`-Flag vorhanden
- [x] `scripts/verify-site.test.mjs` (Node-Test) — angelegt: 10 `node --test`-Fälle (Positivfall, fehlendes PNG, fehlendes `lang="de"`, fehlende Pflicht-Sektion, Bootstrap/CDN, fehlende Doku-Datei, fehlende Doku-Stichworte, fehlender Sprachblock-Eintrag, `--site-only`) — alle bestanden
- [x] `docs/help/github-page/index.md` (Hilfe-Doku) — angelegt (H1 + Kurzbeschreibung + `## Inhalt` mit Links auf beide Teilseiten)
- [x] `docs/help/github-page/beschreibung.md` (Hilfe-Doku) — angelegt (Zweck, Inhalte, Design-Anlehnung, Sprache, Screenshot-Datenquelle)
- [x] `docs/help/github-page/ablauf-technisch.md` (Hilfe-Doku) — angelegt (`site/`-Aufbau, `pages.yml`-Ablauf inkl. Action-Versionen, Pages-Aktivierung per `gh api` mit Verifikationsbefehl, Screenshot-Regenerierung, Pflegeprozess, Doku-Pflicht)

### Änderungen an bestehenden Klassen / Dateien

- [x] Eigenschaft `Server` in `PlaywrightTestBase` — vorhanden (`protected PlaywrightServer Server => _server`, `src/.../Infrastructure/PlaywrightTestBase.cs` Zeile 29), `_server` bleibt `private`, rein additiv
- [x] Methode `SeedShowcaseDataAsync` in `TestDatabaseSeeder` — vorhanden (`src/.../Infrastructure/TestDatabaseSeeder.cs` Zeile 48): 2 `ApplicationGroup`s, 3 `Application`s mit `Description`/`Owner`/`BaseUrl`, `EndpointGroup` + 7 `Endpoint`s mit gemischten `CoreHttpMethod`-Werten, 2 `SystemEnvironment`s mit Variablen; über `IApplicationRepository`/`IEndpointRepository`/`ISystemEnvironmentRepository`; `ResetAsync` unverändert
- [x] Eintrag in `docs/help/index.md` — vorhanden im Abschnitt „Qualität und Tests" (`- [GitHub Page](github-page/index.md) — …`)
- [x] Abschnitt `## GitHub Page` in `README.md` — vorhanden vor „Rechtliches" (URL, `site/`, `pages.yml`, Verweis auf Hilfethema)
- [x] Eintrag in `docs/RELEASE_NOTES.md` — vorhanden: EN-Bullet unter „What's New" und DE-Bullet unter „Neuerungen", jeweils mit `github.io`-Stichwort

### Geplante Testmethoden

- [x] `SiteScreenshotTests.Workspaces_Light_Screenshot` — vorhanden (Zeile 26)
- [x] `SiteScreenshotTests.ApplicationDetail_Light_Screenshot` — vorhanden (Zeile 37)
- [x] `SiteScreenshotTests.EndpointPage_Light_Screenshot` — vorhanden (Zeile 51)
- [x] `SiteScreenshotTests.Workspaces_Dark_Screenshot` — vorhanden (Zeile 75)
- [x] `SitePageTests.Index_ZeigtAllePflichtsektionen` — vorhanden (Zeile 47); Feature-Kacheln-Anzahl 9 statt geforderter „≥ 6" (erfüllt)
- [x] `SitePageTests.Screenshots_SindGeladen` — vorhanden (Zeile 62, `naturalWidth > 0` für alle Galerie-Bilder)
- [x] `SitePageTests.DownloadLink_ZeigtAufLatestRelease` — vorhanden (Zeile 82, inkl. Footer-Links)
- [x] `SitePageTests.DarkToggle_SchaltetUm_UndPersistiert` — vorhanden (Zeile 99, `.dark` + `localStorage` + Reload-Persistenz)
- [x] `SitePageTests.MobileViewport_KeinHorizontalScroll` — vorhanden (Zeile 119, Viewport 390×844)

### Orchestrator-/Externe Schritte

- [x] GitHub-Pages-Aktivierung per `gh api` (Umsetzungsschritt 5) — verifiziert: `gh api repos/martin-stromberg/Schnittstellenzentrale/pages` liefert `build_type: "workflow"`; gleichzeitig in `ablauf-technisch.md` dokumentiert

## Hinweise

- `pages.yml` ergänzt über den Plan hinaus die Permission `actions: read`. Das ist eine begründete additive Abweichung: `actions/deploy-pages` ab v4 benötigt sie zur Artefakt-Auflösung (im Kommentarkopf des Workflows dokumentiert, `pages.yml` Zeile 15). Die drei geplanten Permissions sind alle gesetzt.
- Verifikation im Review-Lauf: `node scripts/verify-site.mjs` grün (Seiten- + Doku-Block), `node --test scripts/verify-site.test.mjs` 10/10, `dotnet build` des Testprojekts fehlerfrei ohne Warnungen, `CI=true dotnet test --filter "FullyQualifiedName~SitePageTests"` 5/5 bestanden.
- Die Tasks-Datei (`docs/features/task-issue-80-...-tasks.md`) wurde aktualisiert: Tasks 1–26 „Erledigt" mit Testnachweis. Einziger offener Punkt ist Task 27 (Remote-Verifikation des `pages.yml`-Laufs und Abruf der Pages-URL nach Merge auf `staging`) — planungsgemäß erst nach dem Merge durchführbar, keine Implementierungslücke.
- `workflow_dispatch` steht erst nach Promotion der Workflow-Datei auf `main` zur Verfügung (im Plan so vorgesehen).
- `site/screenshots/.gitkeep` ist nach dem Einchecken der PNGs funktional überflüssig, aber harmlos (kein Plan-Verstoß).
