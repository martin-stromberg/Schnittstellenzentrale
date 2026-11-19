# GitHub Page — Technischer Ablauf

## Übersicht

Die Präsentationsseite liegt als statische Quellen im Ordner `site/` des Repositorys und wird über den Workflow `.github/workflows/pages.yml` auf GitHub Pages deployt. Es gibt keinen Build-Schritt: Der Workflow verpackt den `site/`-Ordner direkt als Pages-Artefakt.

## Aufbau von `site/`

| Pfad | Inhalt |
|------|--------|
| `site/index.html` | Deutsche Präsentationsseite (Hero, Feature-Kacheln, Screenshot-Galerie, Download, Footer) |
| `site/site.css` | Eigenes Token-/Klassensystem mit `:root`- und `html.dark`-Variablen und Media-Queries |
| `site/theme.js` | Theme-Init und Toggle-Logik (`localStorage`-Schlüssel `colorScheme`, `prefers-color-scheme`-Fallback) |
| `site/favicon.png` | Kopie von `src/Schnittstellenzentrale/wwwroot/favicon.png` |
| `site/screenshots/*.png` | Von `SiteScreenshotTests` erzeugte, eingecheckte Ansichten der Anwendung |

## Deployment-Workflow `pages.yml`

Der Workflow besteht aus einem einzigen Job `deploy` auf `ubuntu-latest` mit `timeout-minutes: 10`. Die Keys `environment` (`github-pages`) und `url` (`${{ steps.deployment.outputs.page_url }}`) sind Job-Level-Keys; `actions/deploy-pages` trägt lediglich `id: deployment`.

Trigger:

- `push` auf `staging` und `main` mit `paths`-Filter auf `site/**`, `.github/workflows/pages.yml`, `scripts/verify-site*.mjs`, `docs/help/github-page/**`, `docs/help/index.md`, `README.md` und `docs/RELEASE_NOTES.md` — also auf die Seitenquellen und alle von `verify-site.mjs` geprüften Artefakte, damit ein Doku-Bruch den Deploy sofort abfängt
- `workflow_dispatch` (erst verfügbar, sobald die Workflow-Datei auf dem Default-Branch `main` liegt)

Schritte des Jobs:

1. `actions/checkout@v7` auf den auslösenden Ref.
2. `actions/setup-node@v7` (Node 22), dann `node scripts/verify-site.mjs` — statische Prüfung der Seiten- und Doku-Artefakte; bricht den Deploy vor jeglichem Publish ab, wenn Pflichtsektionen, Assets oder die Pflege-Doku fehlen.
3. `actions/configure-pages@v6` — Pages-Kontext für den Deploy.
4. `actions/upload-pages-artifact@v5` mit `path: site/` — packt den Ordner als Pages-Artefakt.
5. `actions/deploy-pages@v5` mit `id: deployment` — publiziert unter `https://martin-stromberg.github.io/Schnittstellenzentrale/`; füllt den `page_url`-Output, den der Job-Level-Key `url` auswertet.

Weitere Eigenschaften:

- `permissions`: `contents: read`, `pages: write`, `id-token: write` sowie `actions: read` (letztere benötigt `deploy-pages` ab v4 zur Artefakt-Auflösung).
- `concurrency`: Gruppe `pages` mit `cancel-in-progress: false` — serialisiert Deploys, damit zwei aufeinanderfolgende Pushes keine Race auf die Live-Seite erzeugen.
- Das `github-pages`-Environment wird beim ersten `deploy-pages`-Lauf automatisch angelegt.

## Pages-Aktivierung (einmalig, manuell)

GitHub Pages kann nicht aus dem Workflow heraus aktiviert werden; die Aktivierung erfolgt einmalig mit einem Token mit Admin-Rechten auf dem Repository:

```
gh api repos/martin-stromberg/Schnittstellenzentrale/pages -X POST -f build_type=workflow
```

Das setzt die Source auf „GitHub Actions". Erfolgskontrolle:

```
gh api repos/martin-stromberg/Schnittstellenzentrale/pages
```

(`build_type: "workflow"` erwartet.) Antwortet die API mit „Already exists", ist ein `PUT` mit demselben Body möglich. Alternativ: Repository → Settings → Pages → Source „GitHub Actions". Ohne diesen Schritt schlägt der erste `deploy-pages`-Lauf fehl.

## Screenshot-Regenerierung

Die Galerie-PNGs werden aus der echten Anwendung erzeugt:

```
dotnet test --filter "FullyQualifiedName~SiteScreenshotTests"
```

Die Klasse `SiteScreenshotTests` (Playwright, Collection `"Playwright"`) startet die App auf `http://127.0.0.1:5099`, seedet über `TestDatabaseSeeder.SeedShowcaseDataAsync()` repräsentative Demo-Daten (Gruppen, Anwendungen, Endpunktgruppen, Endpunkte und Umgebungen im Team-Speichermodus) und schreibt die PNGs mit festem Viewport 1440×900 nach `site/screenshots/` — lokal sichtbar, headless unter `CI=true`. Die zugrunde liegende Testinfrastruktur ist unter [Playwright-Tests](../playwright-tests/index.md) dokumentiert. In CI landen die Dateien im Wegwerf-Workspace; nur lokale Läufe erzeugen committbare Änderungen. Geänderte PNGs werden eingecheckt und fließen beim nächsten Deploy mit aus.

Zusätzlich prüft `SitePageTests` (Playwright ohne Server, serviert `site/` per Route-Interception) Inhalte, Bild-Ladestatus, Dark-Toggle-Persistenz und Responsivität der statischen Seite.

## Pflegeprozess

1. Texte und Features in `site/index.html`, Styles in `site/site.css` ändern; Screenshots bei Bedarf per `SiteScreenshotTests`-Lauf neu erzeugen.
2. Lokal prüfen: statischer Server auf `site/` (z. B. `npx serve site` oder `python -m http.server -d site`), `node scripts/verify-site.mjs` sowie `dotnet test --filter "FullyQualifiedName~SitePageTests"`.
3. PR → Merge auf `staging` → `pages.yml` deployt automatisch (Paths-Filter); die Promotion nach `main` deployt denselben Stand erneut.
4. **Dokumentationspflicht:** `scripts/verify-site.mjs` prüft neben den Seiten-Artefakten auch die Doku-Artefakte des Aktualisierungswegs (dieses Hilfethema, den `docs/help/index.md`-Eintrag, den README-Abschnitt und den `docs/RELEASE_NOTES.md`-Eintrag). Werden Doku-Dateien umbenannt, entfernt oder entkernt, bricht der Verify-Schritt den Deploy ab — Strukturänderungen müssen die Stichwort-Prüfung des Skripts mitpflegen.

## Hinweise und Einschränkungen

- **`staging`-Deploys sind bewusst so gewollt:** Jeder `staging`-Push mit `site/`-Änderung aktualisiert die öffentliche Seite — die Page ist Präsentationsfläche, kein Release-Artefakt. Seitenänderungen gehören deshalb in die PR-Review.
- **Keine Screenshot-Generierung im Deploy:** Die PNGs sind eingecheckte Artefakte; der Workflow baut die Anwendung nicht.
- **Kein `release`-Trigger:** Der Download-Verweis nutzt die stabile `releases/latest`-URL und bleibt ohne Redeploy aktuell.
- **`docs/` wird nicht als Pages-Source verwendet:** Die GitHub-„docs-folder"-Option würde den gesamten `docs/`-Baum (`help/`, `features/`, `RELEASE_NOTES.md`) veröffentlichen; deshalb existiert der dedizierte `site/`-Ordner mit Actions-Deployment.
