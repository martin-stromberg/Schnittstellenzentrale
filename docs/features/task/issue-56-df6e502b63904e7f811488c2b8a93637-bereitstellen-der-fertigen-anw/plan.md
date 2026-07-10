# Umsetzungsplan: Bereitstellen der fertigen Anwendung

Hinweis zur Ausfuehrung: Lifecycle-Schritt 5 wurde in dieser Umgebung direkt durch Codex erstellt, da kein Unteragenten-Mechanismus verfuegbar ist.

## Zielbild

Nach einem Push auf `main` erzeugt GitHub Actions nur dann ein Release, wenn Semantic Release anhand der Commit-Historie eine neue Version bestimmt. In diesem Fall wird die ASP.NET-Core-Anwendung mit .NET 10 publiziert, das Publish-Verzeichnis als ZIP paketiert und die ZIP-Datei als Asset an das GitHub Release angehaengt.

Ein gepushter Tag im Format `vX.Y.Z` nutzt denselben Publish-/ZIP-Pfad, ueberspringt aber die automatische Versionsberechnung. Die Version wird hart aus dem Tag abgeleitet und das Release fuer genau diesen Tag erstellt oder aktualisiert.

## Leitentscheidungen

### .NET-10-Build

Die Anforderung wird als echter .NET-10-Publish umgesetzt. Ein Workflow mit .NET-10-SDK, der weiterhin `net9.0`-Projekte publiziert, waere technisch nur ein .NET-9-Target auf einem neueren SDK und erfuellt Akzeptanzkriterium 6 nicht sauber.

Umsetzung:

- Alle vier Projekte werden von `net9.0` auf `net10.0` angehoben:
  - `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`
  - `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`
  - `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`
  - `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`
- Microsoft-/ASP.NET-/EF-Core-/Extensions-Pakete mit explizitem `9.x`-Bezug werden auf kompatible `10.x`-Versionen angehoben.
- Nicht-Microsoft-Pakete werden nur angepasst, wenn Restore, Build oder Tests eine Inkompatibilitaet zeigen.
- Ein `global.json` wird ergaenzt, um lokal und in CI .NET 10 zu bevorzugen. Konservativer Vorschlag:

```json
{
  "sdk": {
    "version": "10.0.301",
    "rollForward": "latestFeature"
  }
}
```

Falls GitHub Actions mit `10.0.301` nicht verfuegbar ist, wird im Workflow `actions/setup-dotnet` mit `dotnet-version: 10.0.x` verwendet; `global.json` bleibt mit `rollForward` kompatibel.

### Release-Flow

Der Workflow wird in einem Job umgesetzt und unterscheidet frueh anhand von `github.ref_type`:

- Branch-Run auf `main`: Semantic Release berechnet die naechste Version. Gibt es keine releaserelevante Aenderung, werden Publish, ZIP und Release-Upload uebersprungen.
- Tag-Run auf `refs/tags/v*.*.*`: Version wird direkt aus dem Tag gelesen. Publish, ZIP und Release werden immer fuer diese Version ausgefuehrt.

Der Publish-/ZIP-Teil ist gemeinsam, damit beide Pfade dieselben Build-Artefakte erzeugen.

## Geplante Dateiaenderungen

### Neue CI-/Release-Dateien

1. `.github/workflows/release.yml`
   - Trigger:
     - `push` auf `main`
     - `push` auf Tags nach Muster `v*.*.*`
   - Berechtigungen:
     - `contents: write`
   - Schritte:
     - Checkout mit vollstaendiger Historie und Tags: `fetch-depth: 0`
     - .NET 10 einrichten: `actions/setup-dotnet@v4`, `dotnet-version: 10.0.x`
     - Node einrichten: `actions/setup-node@v4`, `node-version: 22`, `cache: npm`
     - `npm ci`
     - Release-Version bestimmen:
       - Bei `main`: Node-Skript fuer Semantic-Release-Dry-Run ausfuehren und Outputs `should_release`, `version`, `tag` schreiben.
       - Bei Tag: `version=${GITHUB_REF_NAME#v}`, `tag=$GITHUB_REF_NAME`, `should_release=true`.
     - Bei `should_release=false`: Job sauber beenden, ohne Publish/ZIP/Release.
     - `dotnet restore Schnittstellenzentrale.slnx`
     - `dotnet test Schnittstellenzentrale.slnx -c Release -p:SkipPlaywrightInstall=true`
     - `dotnet publish src/Schnittstellenzentrale/Schnittstellenzentrale.csproj -c Release -o artifacts/publish --no-restore`
     - ZIP aus Inhalt von `artifacts/publish` erstellen:
       - Ziel: `artifacts/release/Schnittstellenzentrale-v<version>.zip`
       - ZIP-Wurzel enthaelt direkt die Publish-Dateien, nicht den Ordner `publish`.
     - Workflow-Artefakt hochladen: `actions/upload-artifact@v4`, Name `Schnittstellenzentrale-v<version>`.
     - Branch-Run: Semantic Release final ausfuehren und ZIP als GitHub-Asset veroeffentlichen.
     - Tag-Run: GitHub Release per `gh release create` erstellen oder per `gh release upload --clobber` aktualisieren.

2. `release.config.cjs`
   - `branches: ['main']`
   - `tagFormat: 'v${version}'`
   - Plugins:
     - `@semantic-release/commit-analyzer`
     - `@semantic-release/release-notes-generator`
     - `@semantic-release/github`
   - Commit-Regeln gemaess Anforderung:
     - `feat` -> minor
     - `fix` -> patch
     - Breaking Change -> major
     - `docs`, `refactor`, `chore`, `plan` und andere Typen -> kein Release
   - GitHub-Asset-Pfad wird aus `process.env.RELEASE_ASSET_PATH` gelesen, damit der Workflow die bereits erzeugte versionierte ZIP-Datei eindeutig uebergibt.

3. `package.json`
   - Private Node-Konfiguration nur fuer Release-Automatisierung.
   - Scripts:
     - `release:probe`: fuehrt das Versionsermittlungs-Skript aus.
     - `release`: fuehrt `semantic-release` final aus.
   - Dev-Abhaengigkeiten:
     - `semantic-release`
     - `@semantic-release/commit-analyzer`
     - `@semantic-release/release-notes-generator`
     - `@semantic-release/github`
     - `conventional-changelog-conventionalcommits`

4. `package-lock.json`
   - Wird mit `npm install --package-lock-only` bzw. `npm install` erzeugt und eingecheckt, damit GitHub Actions reproduzierbar `npm ci` nutzen kann.

5. `scripts/resolve-release-version.mjs`
   - Ruft Semantic Release programmatisch im Dry-Run auf.
   - Schreibt GitHub-Action-Outputs:
     - `should_release=true|false`
     - `version=<semver>`
     - `tag=v<semver>`
   - Bei `nextRelease` leer: `should_release=false`, Exit-Code 0.
   - Bei echten Fehlern: Exit-Code != 0.

### .NET-Projektdateien

1. `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`
   - `TargetFramework` auf `net10.0`.
   - `Microsoft.AspNetCore.Authentication.Negotiate` von `9.0.16` auf kompatible `10.x`.
   - `Microsoft.EntityFrameworkCore.Design` von `9.*` auf kompatible `10.x`.
   - `Microsoft.AspNetCore.OData` nur anheben, wenn fuer `net10.0` erforderlich oder eine kompatiblere Version verfuegbar ist.

2. `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`
   - `TargetFramework` auf `net10.0`.

3. `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`
   - `TargetFramework` auf `net10.0`.
   - Alle expliziten `Microsoft.EntityFrameworkCore*`, `Microsoft.Extensions.*`, `Microsoft.JSInterop` und `Microsoft.AspNetCore.SignalR.Client` 9.x-Pakete auf kompatible `10.x`.
   - Alte Pakete `Microsoft.AspNetCore.Http.Abstractions` 2.3.0 und `Microsoft.AspNetCore.SignalR.Core` 1.2.10 nicht vorsorglich ersetzen; nur aendern, wenn Build/Restore unter `net10.0` scheitert oder Obsolet-/Kompatibilitaetsfehler auftreten.

4. `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`
   - `TargetFramework` auf `net10.0`.
   - `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.*`, `Microsoft.JSInterop` von 9.x auf kompatible `10.x`.
   - Playwright-Target bleibt erhalten; CI-Tests setzen `SkipPlaywrightInstall=true`, damit der Release-Workflow nicht unnoetig Chromium installiert.

### Dokumentation

1. `README.md`
   - .NET-Badge und Technologiehinweise von .NET 9 auf .NET 10 aktualisieren.
   - Runtime-Voraussetzung auf ASP.NET Core Runtime 10.0 aktualisieren.
   - Release-Abschnitt ergaenzen:
     - Releases entstehen bei Push nach `main`, wenn Conventional Commits eine Version ergeben.
     - Manuelle Version per Tag `vX.Y.Z`.
     - Download-Asset: `Schnittstellenzentrale-vX.Y.Z.zip`.

2. `docs/help/schnittstellenzentrale/installation.md`
   - Runtime-Voraussetzung auf .NET/ASP.NET Core 10 aktualisieren.
   - Hinweis auf GitHub-Release-ZIP als bevorzugtes Installationsartefakt ergaenzen.

3. `docs/help/playwright-tests/installation.md`
   - Pfade mit `net9.0` auf `net10.0` aktualisieren.
   - Bestehende CI-Hinweise nicht zu einem vollstaendigen Workflow ausbauen, ausser es ist fuer die Release-Dokumentation noetig.

## Workflow-Details

### Versionsermittlung fuer `main`

Das Skript `scripts/resolve-release-version.mjs` nutzt die gleiche `release.config.cjs` wie der finale Release-Schritt. Dadurch unterscheiden sich Probe und echter Release nicht in den Commit-Regeln.

Erwartetes Verhalten:

- Relevanter Commit vorhanden: Output `should_release=true`, `version=...`.
- Nur irrelevante Commits vorhanden: Output `should_release=false`; alle Build-/Release-Schritte mit `if: steps.version.outputs.should_release == 'true'` ueberspringen.
- Fehler in Semantic Release, GitHub-Token oder Git-Historie: Workflow bricht ab.

### Finaler Release fuer `main`

Nach erfolgreichem Publish und ZIP setzt der Workflow:

- `RELEASE_ASSET_PATH=artifacts/release/Schnittstellenzentrale-v<version>.zip`
- `GITHUB_TOKEN=${{ secrets.GITHUB_TOKEN }}`

Danach laeuft `npm run release`. Semantic Release erzeugt Tag `v<version>`, GitHub Release Notes und laedt die ZIP-Datei als Release-Asset hoch.

### Tag-Fallback

Bei Tag-Runs wird kein Semantic Release ausgefuehrt. Der Workflow:

1. validiert, dass der Tag exakt `v<major>.<minor>.<patch>` entspricht,
2. setzt `version` aus dem Tag,
3. publiziert und zippt die Anwendung,
4. prueft `gh release view "$tag"`,
5. falls Release fehlt: `gh release create "$tag" "$zip" --title "$tag" --generate-notes`,
6. falls Release existiert: `gh release upload "$tag" "$zip" --clobber`.

Damit kann ein bewusst gesetzter Tag eine automatisch berechnete Version ueberschreiben, ohne dass Semantic Release versucht, einen bereits existierenden Tag erneut zu erstellen.

## Validierung

Vor Abschluss der Implementierung sind mindestens diese Befehle lokal auszufuehren:

```powershell
dotnet --list-sdks
dotnet restore Schnittstellenzentrale.slnx
dotnet build Schnittstellenzentrale.slnx -c Release -p:SkipPlaywrightInstall=true
dotnet test Schnittstellenzentrale.slnx -c Release -p:SkipPlaywrightInstall=true
dotnet publish src/Schnittstellenzentrale/Schnittstellenzentrale.csproj -c Release -o artifacts/publish
```

Zusaetzlich fuer Node/Semantic Release:

```powershell
npm install
npm run release:probe
```

Der echte `npm run release` soll lokal nicht gegen GitHub ausgefuehrt werden, sofern kein bewusstes Release erstellt werden soll. Die finale Release-Erstellung wird ueber GitHub Actions validiert.

## Risiken und Gegenmassnahmen

| Risiko | Gegenmassnahme |
|---|---|
| Einzelne NuGet-Pakete haben noch keine kompatible 10.x-Version oder brechen unter `net10.0`. | Erst Microsoft-/ASP.NET-/EF-Core-Pakete anheben, dann Restore/Build ausfuehren. Nicht-Microsoft-Pakete nur gezielt aktualisieren. Falls ein Paket keine kompatible Version hat, Implementierung abbrechen und den konkreten Paketkonflikt als Nutzerentscheidung melden. |
| Semantic-Release-Probe und finaler Release ermitteln unterschiedliche Versionen. | Beide Schritte nutzen dieselbe `release.config.cjs`; zwischen Probe und finalem Release erfolgen keine Git-Aenderungen. |
| Tag-Run kollidiert mit bestehendem Asset. | Tag-Pfad nutzt `gh release upload --clobber`, sodass ein manuell erneut gestarteter Tag-Run das ZIP ersetzt. |
| Release-Workflow baut Tests mit Playwright-Installationsaufwand. | Release-CI nutzt `-p:SkipPlaywrightInstall=true`; Playwright-End-to-End-Tests bleiben ausserhalb dieses Release-Pfads. |
| Dokumentation bleibt auf .NET 9. | README und relevante Help-Dokumente werden im selben Feature auf .NET 10 und Release-ZIP aktualisiert. |

## Umsetzungsreihenfolge

1. `.NET`-Migration vorbereiten:
   - `global.json` anlegen.
   - Projekt-TargetFrameworks auf `net10.0` setzen.
   - 9.x Microsoft-Pakete auf kompatible 10.x-Versionen aktualisieren.
2. Lokal `dotnet restore` und `dotnet build` ausfuehren; Paketkonflikte gezielt beheben.
3. `dotnet test` mit `SkipPlaywrightInstall=true` ausfuehren.
4. `dotnet publish` gegen `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` validieren.
5. Node-/Semantic-Release-Dateien anlegen: `package.json`, Lockdatei, `release.config.cjs`, `scripts/resolve-release-version.mjs`.
6. `.github/workflows/release.yml` mit Branch-/Tag-Pfad und gemeinsamem Publish-/ZIP-Teil anlegen.
7. Dokumentation in README und Help-Dateien aktualisieren.
8. Finale lokale Validierung ausfuehren:
   - Restore, Build, Test, Publish.
   - `npm ci` bzw. `npm install` und `npm run release:probe`.
9. Geaenderte Dateien pruefen, keine fremden Aenderungen zuruecksetzen.

## Offene Punkte

Keine zwingende Nutzerentscheidung vor der Implementierung.

Die .NET-10-Anforderung wird konservativ als echte Migration auf `net10.0` geplant. Sollte waehrend der Umsetzung ein konkretes Paket oder Framework-Bestandteil nicht kompatibel sein, ist das kein Planungsentscheid mehr, sondern ein Implementierungsblocker mit konkreter Fehlermeldung.
