# Tasks: GitHub-Page-Präsentationsseite (Issue #80)

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Seitenquellen | `site/index.html` anlegen: `lang="de"`, Viewport-Meta, Hero mit Vorstellung, Feature-Kacheln, Screenshot-Galerie (feste PNG-Namen), Download-Abschnitt (`releases/latest` + Asset-Hinweis + optionales shields.io-Badge), Footer (Repository, `docs/help/`, `LEGAL.md`) | Offen | — |
| 2 | Seitenquellen | `site/site.css` anlegen: Token-Set `:root`/`html.dark` (shadcn-Werte + sz-Gradient), Layout, Karten, Galerie, Footer, Media-Queries für Responsivität | Offen | — |
| 3 | Seitenquellen | `site/theme.js` + Inline-Init im `<head>` anlegen: `colorScheme`-localStorage, `prefers-color-scheme`-Fallback, `.dark`-Toggle auf `document.documentElement` | Offen | — |
| 4 | Seitenquellen | `site/favicon.png` aus `src/Schnittstellenzentrale/wwwroot/favicon.png` kopieren | Offen | — |
| 5 | Seitenquellen | `site/screenshots/` mit `.gitkeep` anlegen | Offen | — |
| 6 | Testinfrastruktur | `protected`-Eigenschaft `Server` in `PlaywrightTestBase` ergänzen (Zugriff auf `Server.Services`/`Server.BaseAddress`), inkl. XML-Doku | Offen | — |
| 7 | Testinfrastruktur | `SeedShowcaseDataAsync()` in `TestDatabaseSeeder` implementieren (ApplicationGroups, Applications, EndpointGroups, Endpoints mit gemischten Methoden, SystemEnvironments; `Mode`/`StorageMode` auf Default `Team` belassen, damit kein Owner-Filter greift), inkl. XML-Doku | Offen | — |
| 8 | Tests | `SiteScreenshotTests` anlegen: `FindRepoRoot()`, Viewport 1440×900, Fact `Workspaces_Light_Screenshot` → `home-light.png` | Offen | — |
| 9 | Tests | Facts `ApplicationDetail_Light_Screenshot` und `EndpointPage_Light_Screenshot` → `application-light.png`, `endpoint-light.png` | Offen | — |
| 10 | Tests | Fact `Workspaces_Dark_Screenshot` → `home-dark.png` (Dark via `colorScheme`-Init/localStorage) | Offen | — |
| 11 | Tests | `dotnet test --filter "FullyQualifiedName~SiteScreenshotTests"` lokal ausführen und PNGs in `site/screenshots/` einchecken | Offen | — |
| 12 | Tests | `SitePageTests` anlegen (eigener Browser-Lebenszyklus, **keine** `"Playwright"`-Collection, `site/` via `Context.RouteAsync` unter Pseudo-Origin): Facts `Index_ZeigtAllePflichtsektionen`, `Screenshots_SindGeladen`, `DownloadLink_ZeigtAufLatestRelease` | Offen | — |
| 13 | Tests | `SitePageTests`: Facts `DarkToggle_SchaltetUm_UndPersistiert` und `MobileViewport_KeinHorizontalScroll` | Offen | — |
| 14 | Tooling | `scripts/verify-site.mjs` erstellen — Block 1 Seiten-Artefakte (Pflichtsektionen, `lang="de"`, Viewport-Meta, Asset-/Screenshot-Referenzen auf Existenz, kein CDN-`link`) | Offen | — |
| 15 | Tooling | `scripts/verify-site.mjs` — Block 2 Doku-Artefakte: Existenz + Stichwort-Mindestinhalt von `docs/help/github-page/{index,beschreibung,ablauf-technisch}.md` (`pages.yml`, `gh api`, `SiteScreenshotTests`, `site/`), Link `github-page/index.md` in `docs/help/index.md`, Abschnitt `## GitHub Page` in `README.md`, Eintrag `GitHub Page`/`github.io` in `docs/RELEASE_NOTES.md` (beide Sprachblöcke); optional `--site-only`-Flag für lokale Läufe vor Fertigstellung der Doku | Offen | — |
| 16 | Tooling | `scripts/verify-site.test.mjs` erstellen (`node --test`, Positivfall + Fehlerfälle: fehlendes PNG, fehlendes `lang="de"`, fehlende Doku-Datei, fehlendes Doku-Stichwort) | Offen | — |
| 17 | Deployment | `.github/workflows/pages.yml` erstellen: englischer Kommentarkopf, Trigger `push` auf `staging`+`main` mit `paths`-Filter (`site/**`, Workflow-Datei, `scripts/verify-site.mjs`) + `workflow_dispatch`, `permissions` (`contents: read`, `pages: write`, `id-token: write`), `concurrency: pages` ohne cancel; Action-Versionen gegen Repo-Konventionen (`@v6`/`@v7`) und Verfügbarkeit verifizieren | Offen | — |
| 18 | Deployment | Job-Struktur in `pages.yml`: **ein** Job `deploy` (`ubuntu-latest`, `timeout-minutes: 10`) mit **Job-Level**-Keys `environment: { name: github-pages, url: ${{ steps.deployment.outputs.page_url }} }` (keine Step-Attribute); Steps: checkout → setup-node → `node scripts/verify-site.mjs` → configure-pages → upload-pages-artifact (`path: site/`) → deploy-pages mit `id: deployment` | Offen | — |
| 19 | Deployment | GitHub Pages per `gh api` aktivieren (Orchestrator-Schritt, kein Workflow-Schritt): `gh api repos/martin-stromberg/Schnittstellenzentrale/pages -X POST -f build_type=workflow`, Erfolg per `gh api .../pages` verifizieren; muss vor dem ersten Deploy-Lauf abgeschlossen sein | Offen | — |
| 20 | Dokumentation | `docs/help/github-page/index.md` erstellen (H1 + Kurzbeschreibung + `## Inhalt`) | Offen | — |
| 21 | Dokumentation | `docs/help/github-page/beschreibung.md` erstellen (Zweck, Inhalte, Design, Sprache Deutsch) | Offen | — |
| 22 | Dokumentation | `docs/help/github-page/ablauf-technisch.md` erstellen (site/-Aufbau, Workflow-Ablauf, Pages-Aktivierung per `gh api`, Screenshot-Regenerierung, Pflegeprozess) | Offen | — |
| 23 | Dokumentation | Eintrag `github-page` in `docs/help/index.md` (Abschnitt „Qualität und Tests") ergänzen | Offen | — |
| 24 | Dokumentation | README-Abschnitt `## GitHub Page` (vor „Rechtliches") ergänzen: URL, `site/`, Workflow, Verweis auf Hilfethema | Offen | — |
| 25 | Dokumentation | `docs/RELEASE_NOTES.md`-Eintrag ergänzen: EN-Bullet unter „What's New" + DE-Bullet unter „Neuerungen" mit Stichwort `GitHub Page`/`github.io` | Offen | — |
| 26 | Abnahme | Lokale Abnahme: statischer Server auf `site/` + Browsercheck (Inhalte, Dark-Toggle, Responsive), `node scripts/verify-site.mjs` (voller Lauf inkl. Doku-Block), `node --test scripts/verify-site.test.mjs`, `dotnet test --filter "FullyQualifiedName~SitePageTests"` | Offen | — |
| 27 | Abnahme | Nach Merge: `pages.yml`-Lauf auf `staging` via `gh run watch`/`gh run view` verifizieren und Pages-URL `https://martin-stromberg.github.io/Schnittstellenzentrale/` abrufen (HTTP 200 + Inhalte) | Offen | — |
