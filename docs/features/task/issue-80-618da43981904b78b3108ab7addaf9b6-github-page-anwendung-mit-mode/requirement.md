# Übersetzte Anforderungsbeschreibung

Quelle: `issue.md` (GitHub-Issue #80 — „GitHub Page: Anwendung mit modernem Stil präsentieren")

## Fachliche Zusammenfassung

Es wird eine statische Präsentations-Website (GitHub Page) für die Schnittstellenzentrale eingeführt, die öffentlich unter `https://martin-stromberg.github.io/Schnittstellenzentrale/` erreichbar ist. Die Seite stellt Zweck und zentrale Features der Blazor-Server-Anwendung prägnant dar, bindet Screenshots ein und verweist auf Downloads/Releases, das Repository und die Dokumentation. Auslieferung und Pflege erfolgen repositorybasiert — Quelldateien und Screenshots liegen im Repo, das Deployment läuft über einen GitHub-Actions-Workflow passend zur bestehenden CI/CD-Struktur (Staging-Workflow: Feature-Branch → `staging` → `main`). Design: responsiv, klare Typografie, helle und dunkle Darstellung, angelehnt an das Erscheinungsbild der Anwendung (`sz-*`-Design-System, shadcn-Tokens, `.dark`-Theme).

## Fachlicher Scope

- **Verarbeitungseinheit:** Ein Deployment-Lauf der Präsentationsseite — also das Bündel aus Seitenquellen (HTML/CSS/JS/Assets) plus generierte Screenshots, das als Ganzes gebaut und auf GitHub Pages veröffentlicht wird. Inhaltlich gehören dazu: Vorstellung der Anwendung, Feature-Übersicht, Screenshot-Galerie, Download-/Release-Verweise sowie Links auf Repository und Dokumentation.
- **Auslösende Ereignisse:**
  - Push auf den Release-Branch `main` bei geänderten Seitenquellen (Features erreichen `main` über den bestehenden Staging-Promotion-Pfad `staging` → `main`)
  - Manueller Trigger `workflow_dispatch` zum Testen/Aktualisieren ohne Codeänderung (Konvention aus den bestehenden Review-Workflows)
  - Optional: neue Releases, damit Versions-/Download-Verweise aktuell bleiben (alternativ stabile `releases/latest`-URL ohne erneutes Deployment — siehe Offene Fragen)
  - Einmalig vorgelagert: manuelle Aktivierung von GitHub Pages in den Repository-Einstellungen (Source: GitHub Actions) — nicht aus dem Repo heraus automatisierbar
- **Erwartete Wirkung:** Öffentlich abrufbare, moderne und responsive Webseite unter der Pages-URL, die Besuchern Zweck und zentrale Features der Anwendung vermittelt, Screenshots zeigt und zu Release-Download, Repository und Dokumentation weiterleitet. Hell-/Dunkel-Darstellung vorhanden. Pflege- und Aktualisierungsweg der Seite ist im Repository dokumentiert (`docs/help/`-Konvention + README-Verweis).
- **Umgang mit fehlenden Objekten:**
  - GitHub-Pages-Aktivierung / `github-pages`-Environment: existiert noch nicht; wird einmalig manuell in den Repository-Einstellungen **erzeugt** (Voraussetzung, im Doc-Abschnitt zu beschreiben); das Environment wird beim ersten `actions/deploy-pages`-Lauf automatisch angelegt
  - Seiten-Quellordner: existiert nicht; wird im Repository **neu erzeugt**
  - Screenshots: existieren nicht; werden aus dem Repository heraus **erzeugt** (Playwright nutzbar — Infrastruktur mit `PlaywrightServer`/`TestDatabaseSeeder` vorhanden) und als PNG-Assets **eingecheckt/wiederverwendet**
  - Deployment-Workflow: existiert nicht; wird **neu erzeugt** und **wiederverwendet** bestehender Actions-Konventionen (`configure-pages`/`upload-pages-artifact`/`deploy-pages`, `permissions`, `concurrency`, `timeout-minutes`)
- **Scope-Ausschlüsse:**
  - Keine Änderungen an der Blazor-Anwendung selbst (keine neuen C#-Domänenklassen, APIs oder UI-Komponenten der App)
  - Kein Veröffentlichen der bestehenden Hilfedokumentation als Website — `docs/help/` bleibt Repo-interne Markdown-Doku und wird von der Page nur verlinkt (kein Docs-Portal, kein Jekyll-Umbau von `docs/`)
  - Keine mehrsprachige Page gefordert — Sprachwahl ist eine offene Frage (Annahme: einstufig, siehe unten)
  - Kein automatischer Screenshot-Refresh zwingend pro Release — „kann Playwright genutzt werden" erlaubt manuelle/lokale Erzeugung mit eingecheckten PNGs
  - Kein Blog, keine Versionshistorie der Seite, keine Custom Domain, keine Such-/Kontaktfunktionen
  - Kein Verkauf/keine Analytics — statische Inhalte ohne Tracking

## Betroffene Klassen und Komponenten

Primär GitHub-Actions- und statische-Web-Artefakte — keine Änderung am C#-Domänenmodell. Voraussichtlich betroffene bzw. neu zu erstellende Artefakte:

- Datenmodellklassen: Keine
- Logikklassen / Services: Keine (optional kleine Erweiterung der Playwright-Infrastruktur für Screenshot-Erzeugung, siehe unten)
- Interfaces: Keine
- Enums: Keine
- Seitenquellen (neu): statische `index.html` plus eigene CSS-/JS-Dateien und Assets (Screenshots als PNG, ggf. `favicon.png` aus `src/Schnittstellenzentrale/wwwroot/` wiederverwendet). Ablageort Vorschlag: dedizierter Ordner wie `site/` oder `docs/page/` — **nicht** pauschal `docs/` als Pages-Source, da `docs/` bereits `help/` und `features/` enthält und die GitHub-„docs-folder"-Option alles darunter veröffentlichen würde
- Workflow-Datei (neu): `.github/workflows/pages.yml` (Name Vorschlag) — Deployment via `actions/configure-pages`, `actions/upload-pages-artifact`, `actions/deploy-pages`; `permissions: pages: write, id-token: write`; `concurrency`-Gruppe `pages`; Trigger `push` auf `main` mit `paths`-Filter auf den Seitenordner sowie `workflow_dispatch`
- Screenshot-Erzeugung (optional/neu): eine dedizierte Playwright-Testklasse oder ein Tool auf Basis von `Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightServer.cs` und `PlaywrightTestBase.cs` (Kestrel auf `http://127.0.0.1:5099`, `TestDatabaseSeeder` für reproduzierbare Daten, `Page.ScreenshotAsync()`); alternativ Skript unter `scripts/` (Konvention: Node/PowerShell-Helfer liegen dort, z. B. `scripts/resolve-release-version.mjs`)
- Dokumentation: neues Hilfethema `docs/help/github-page/` (`index.md`, `beschreibung.md`, `ablauf-technisch.md`) mit Beschreibung des Pflege- und Aktualisierungswegs inkl. Pages-Aktivierung; Eintrag in `docs/help/index.md` (Abschnitt „Qualität und Tests" oder eigener Abschnitt); kurzer Abschnitt in `README.md`
- Tests: kein klassischer Unit-/Integrationstest-Bedarf (kein App-Code); Absicherung über YAML-Validierung, `workflow_dispatch`-Testlauf und ggf. statische Prüfung der generierten Seite (Annahme)

## Implementierungsansatz

- **Deployment-Weg:** GitHub-Actions-Deployment (`actions/deploy-pages`) statt Branch-/Ordner-basiertem Pages-Serving — passt zu den bestehenden Workflows (`release.yml`, `staging-ci.yml`, Review-Workflows), hält die Seitenquelle frei wählbar und vermeidet die Publikation des gesamten `docs/`-Baums. Standard-Vier-Schritt-Sequenz: `configure-pages` → `upload-pages-artifact` (Pfad = Seitenordner) → `deploy-pages` mit `environment: github-pages` und `url`-Output.
- **Trigger-Strategie:** Push auf `main` mit `paths`-Filter (Seitenquellen, Screenshots, Workflow-Datei) plus `workflow_dispatch`. Da Features über `staging` nach `main` fließen, erscheint die Page erst nach Promotion — konsistent zum Release-Prozess. Download-Verweis vorzugsweise über die stabile URL `https://github.com/martin-stromberg/Schnittstellenzentrale/releases/latest` (ZIP-Asset `Schnittstellenzentrale-vX.Y.Z.zip`), damit kein Deployment pro Release nötig ist.
- **Design-Anlehnung an die App:** Farb-/Token-System aus `src/Schnittstellenzentrale/wwwroot/app.css` bzw. `shadcn/default-theme.css` nachbilden — Sidebar-Gradient (`rgb(5,39,103)` → `#3a0647`, dark: `#0d1b2a` → `#1a0a1e`), shadcn-Variablen (`--background`, `--foreground`, `--muted-foreground`, `--border`, `--radius`, `--card`), Font-Stack `'Helvetica Neue', Helvetica, Arial, sans-serif`. Theme-Umschaltung analog zur App: `.dark`-Klasse auf dem Root-Element (Konvention aus `theme.js`), zusätzlich `prefers-color-scheme` als Initialwert; Umschalter persistiert in `localStorage` (App-Parität). Responsiv über eigenes CSS ohne Bootstrap (Bootstrap ist in der App nicht geladen — Seite soll schlank bleiben, kein Build-Tool nötig; Annahme: plain HTML/CSS/JS).
- **Screenshots:** Playwright-Infrastruktur wiederverwenden — `PlaywrightServer` startet die echte App mit `TestAuthHandler` und In-Memory-SQLite, `TestDatabaseSeeder` liefert reproduzierbaren Datenstand; dedizierter Screenshot-Lauf (z. B. eigene Testklasse mit `Page.ScreenshotAsync()` nach definiertem Seeding oder Skript-Aufruf) schreibt PNGs direkt in den Seiten-Asset-Ordner; Light- und Dark-Varianten über die `.dark`-Theme-Klasse der App erzeugbar.
- **Inhalte:** Aus `README.md` ableitbar — prägnante Vorstellung (zentrale Verwaltung lokaler Webservice-Endpunkte), Feature-Kacheln (Workspaces, Environments, History, Swagger-/OData-Import, OData-v4-API, Team-/Benutzermodus, SignalR-Echtzeit, Health-Check, Mehrsprachigkeit, Dark Mode), Screenshots, Release-/Download-Badges, Links auf Repository, `docs/help/` und `LEGAL.md`. Inhalte leben als Dateien im Repo und wachsen mit der Anwendung mit.
- **Abhängigkeiten:** Keine C#-Abhängigkeiten; Workflow-Konventionen übernehmen (englischer Kommentarblock, `permissions`, `concurrency`, `timeout-minutes`, `pwsh`/`ubuntu-latest`). Kein Konflikt mit `release.yml`/`staging-ci.yml` — eigenständiger Workflow.

## Konfiguration

- Kein Konfigurationsbedarf auf Anwendungsebene (`appsettings.json` unberührt).
- GitHub Pages: einmalige manuelle Aktivierung in den Repository-Einstellungen (Settings → Pages → Source „GitHub Actions") — kann nicht per `GITHUB_TOKEN` aus dem Workflow heraus eingerichtet werden; als Voraussetzung in der Hilfe-Dokumentation zu beschreiben.
- Seiteninhalte (Texte, Feature-Liste, Screenshots) werden direkt in den Seitenquelldateien im Repo gepflegt — „Konfiguration" erfolgt über den Quellordner.
- Optional (Annahme): Repository-Variable für den Seiten-Quellpfad im Workflow; nicht gefordert.

## Offene Fragen

1. **Deployment-Branch/Öffentlichkeitszeitpunkt:** Soll die Page ausschließlich bei Push auf `main` deployen (Vorschlag, konsistent zum Staging→main-Promotion-Workflow) oder auch aus `staging` (Vorschau)? Deployt der Workflow auf den `main`-Merge des Feature-Branches — d. h. die Page ist erst nach Erreichen von `main` live?
2. **Ablageort der Seitenquellen:** Dedizierter Ordner (Vorschlag `site/` oder `docs/page/`) oder bewusst die GitHub-„docs folder"-Option — Letztere würde den kompletten vorhandenen `docs/`-Baum (`help/`, `features/`, `RELEASE_NOTES.md`) mitveröffentlichen. Annahme: dedizierter Ordner + Actions-Deployment.
3. **Sprache der Page:** Deutsch, Englisch oder Sprachumschalter? Die App ist DE/EN lokalisiert; das Issue ist deutsch formuliert. Annahme: einstufig starten (Sprache zu entscheiden), Umschaltung nicht gefordert.
4. **Screenshot-Automatisierungsgrad:** Screenshots manuell/lokal per Playwright-Lauf erzeugen und einchecken (Vorschlag, einfacher Pfad) oder im Pages-Workflow automatisch generieren (erfordert .NET-Build + Playwright-Browser im Deploy-Job — deutlich schwerer, Playwright-Schritt läuft in CI derzeit `continue-on-error`)? Zusätzlich: welche Ansichten (Startseite/Workspaces, Endpunkt-Ausführung, History, Import-Diff, Dark Mode) und welche Seed-Daten für repräsentative Screenshots?
5. **Pages-Aktivierung:** Reicht die dokumentierte manuelle Einmal-Aktivierung durch den Repository-Admin, oder soll ein `gh api`-Skript (erfordert PAT mit Admin-Rechten) unter `scripts/` bereitgestellt werden?
6. **Release-Verweis-Mechanik:** Genügt die stabile `releases/latest`-URL samt Hinweis auf das ZIP-Asset `Schnittstellenzentrale-vX.Y.Z.zip` (Vorschlag), oder soll die Page die jeweils aktuelle Versionsnummer anzeigen — d. h. Trigger auf `release`-Events bzw. dynamische Auflösung über die GitHub-API?
7. **Rechtliches:** Benötigt die öffentliche Page einen Impressums-/Lizenz-Hinweis (Verweis auf `LEGAL.md` bzw. die App-Impressums-Funktion), obwohl nicht gefordert? Annahme: Footer-Link auf `LEGAL.md`.
