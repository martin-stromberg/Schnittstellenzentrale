# Strukturierte Anforderung: Bereitstellen der fertigen Anwendung

## Ausgangslage

Nach Fertigstellung eines Features soll die Anwendung automatisch als ZIP-Datei ueber GitHub bereitgestellt werden. Die Bereitstellung soll an den Merge-Prozess in `main` gekoppelt sein und eine reproduzierbare Versionierung, Build-Erstellung und Release-Verteilung sicherstellen.

## Ziel

Bei jedem Merge eines Entwicklungs-Branches in `main` wird automatisch ein Release-Prozess ausgefuehrt, der:

1. anhand der Commit-Historie eine Semantic-Release-Version bestimmt,
2. die Anwendung mit .NET 10 veroeffentlicht,
3. das komplette Publish-Verzeichnis als ZIP-Datei paketiert,
4. ein GitHub Release fuer die ermittelte Version erstellt,
5. die ZIP-Datei als Release-Asset am GitHub Release bereitstellt.

Optional muss ein manuell gesetzter Git-Tag im Format `vX.Y.Z` die automatisch berechnete Version ueberschreiben koennen.

## Funktionale Anforderungen

### 1. GitHub-Actions-Workflow

- Es wird ein GitHub-Actions-Workflow fuer den Release-Build angelegt.
- Der Workflow startet automatisch bei Pushes auf `main`.
- Der Workflow unterstuetzt zusaetzlich manuelle Versionsvorgaben ueber Git-Tags im Format `vX.Y.Z`.
- Merges in `main` erfolgen gemaess Team-Regel ausschliesslich ueber Pull Requests; der Workflow muss daher auf den resultierenden Push nach `main` reagieren.

### 2. Semantic Release

- Der Workflow verwendet Semantic Release zur automatischen Versionsermittlung.
- Semantic Release analysiert die Commits seit dem letzten Release-Tag.
- Die Versionserhoehung folgt den Conventional-Commits-Regeln:
  - `feat:` erzeugt eine Minor-Version.
  - `fix:` erzeugt eine Patch-Version.
  - `feat!:` oder `BREAKING CHANGE:` erzeugt eine Major-Version.
  - Andere Typen wie `docs:`, `refactor:` oder `chore:` erzeugen keine neue Version.
- Es wird eine Semantic-Release-Konfiguration bereitgestellt, z. B. `.releaserc` oder `release.config.js`.
- Die Konfiguration enthaelt die fuer das Repository erforderlichen Plugins, mindestens fuer GitHub-Releases und Release Notes; Changelog- und Git-Integration werden beruecksichtigt, wenn sie zum gewaehlten Release-Flow passen.
- Wird keine releaserelevante Aenderung erkannt, soll kein neues Release mit ZIP-Asset erzeugt werden.

### 3. Manueller Versions-Fallback per Tag

- Ein manuell gesetzter und gepushter Tag im Format `vX.Y.Z` gilt als harte Versionsvorgabe.
- Beispiel:

```bash
git tag v3.2.0
git push origin v3.2.0
```

- Bei einem solchen Tag-Lauf wird fuer genau diese Version ein Release erstellt oder aktualisiert.
- Die ZIP-Datei wird dem Release dieser Version als Asset hinzugefuegt.
- Der Tag-Fallback funktioniert unabhaengig davon, ob die Commit-Typen seit dem letzten Release eine neue Version ergeben wuerden.

### 4. .NET-10-Publish

- Der Workflow richtet eine .NET-10-Buildumgebung ein.
- Die Anwendung wird mit `dotnet publish` in Release-Konfiguration gebaut.
- Das Publish-Ergebnis wird in ein eindeutig definiertes Publish-Verzeichnis geschrieben.
- Der zu publizierende Projektpfad wird im Rahmen der Umsetzung anhand der vorhandenen Repository-Struktur festgelegt.
- Build- oder Publish-Fehler brechen den Workflow ab; in diesem Fall darf kein Release-Asset veroeffentlicht werden.

### 5. ZIP-Erstellung

- Das komplette Publish-Verzeichnis wird nach erfolgreichem Publish als ZIP-Datei gepackt.
- Die ZIP-Datei enthaelt die deploybaren Dateien direkt aus dem Publish-Verzeichnis.
- Der Dateiname der ZIP-Datei enthaelt mindestens den Anwendungsnamen und die Release-Version.
- Die ZIP-Datei wird als Workflow-Artefakt erzeugt und anschliessend als GitHub-Release-Asset veroeffentlicht.

### 6. GitHub Release und Asset-Upload

- Fuer jede ermittelte oder manuell vorgegebene Release-Version wird ein GitHub Release bereitgestellt.
- Das Release enthaelt automatisch erzeugte Release Notes.
- Die erzeugte ZIP-Datei wird als Asset an das GitHub Release angehaengt.
- Der Workflow nutzt die in GitHub Actions verfuegbaren Berechtigungen bzw. Tokens, ohne projektspezifische Secrets unnoetig einzufuehren.

## Nicht-Ziele

- Es wird kein automatisches Deployment auf Server, IIS oder Cloud-Umgebungen gefordert.
- Es wird kein Installer erstellt.
- Es wird keine Aenderung der fachlichen Anwendungsfunktionalitaet gefordert.
- Es wird keine Verpflichtung eingefuehrt, bei nicht releaserelevanten Commits ein Release zu erzwingen.

## Team-Regeln und Konventionen

- Commit-Typen muessen strikt nach Conventional Commits verwendet werden.
- Breaking Changes muessen eindeutig mit `!` im Commit-Typ oder mit `BREAKING CHANGE:` markiert werden.
- Tags duerfen nur gesetzt werden, wenn eine Version bewusst manuell vorgegeben oder ueberschrieben werden soll.
- Merges in `main` erfolgen ausschliesslich ueber Pull Requests.
- Die Release-Pipeline muss zur bestehenden Repository-Struktur passen und darf bestehende Entwicklungs- und Testablaeufe nicht unnoetig veraendern.

## Akzeptanzkriterien

1. Nach einem Pull-Request-Merge in `main` startet automatisch ein GitHub-Actions-Workflow.
2. Bei einem Commit mit `feat:` seit dem letzten Release wird eine neue Minor-Version veroeffentlicht.
3. Bei einem Commit mit `fix:` seit dem letzten Release wird eine neue Patch-Version veroeffentlicht.
4. Bei einem Commit mit `feat!:` oder `BREAKING CHANGE:` seit dem letzten Release wird eine neue Major-Version veroeffentlicht.
5. Bei ausschliesslich nicht releaserelevanten Commit-Typen wird kein neues Release erzeugt.
6. Der Workflow fuehrt einen erfolgreichen .NET-10-Publish der Anwendung aus.
7. Das Publish-Verzeichnis wird vollstaendig als ZIP-Datei paketiert.
8. Das GitHub Release enthaelt Release Notes und die ZIP-Datei als herunterladbares Asset.
9. Ein gepushter Tag im Format `vX.Y.Z` fuehrt zu einem Release genau fuer diese Version und laedt das ZIP-Asset dort hoch.
10. Schlaegt Build, Publish, ZIP-Erstellung oder Release-Upload fehl, endet der Workflow mit Fehlerstatus.

## Technische Klaerpunkte fuer die Umsetzung

- Das Repository dokumentiert aktuell ASP.NET Core/.NET 9, waehrend die Anforderung explizit einen .NET-10-Build fordert. In der Bestandsaufnahme ist zu pruefen, ob das Projekt bereits auf .NET 10 umgestellt werden muss oder ob nur der CI-Build vorbereitet werden soll.
- Der konkrete zu publishende Projektpfad und der gewuenschte Artefaktname sind aus der Repository-Struktur abzuleiten.
- Der genaue Semantic-Release-Flow fuer manuelle Tags ist so zu gestalten, dass Tags im Format `vX.Y.Z` reproduzierbar als Versionsvorgabe wirken und nicht mit automatisch erzeugten Tags kollidieren.
