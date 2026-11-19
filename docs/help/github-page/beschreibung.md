# GitHub Page — Beschreibung

## Zweck

Die GitHub Page ist die öffentliche Visitenkarte der Schnittstellenzentrale. Sie richtet sich an Besucher, die das Projekt auf GitHub finden, und vermittelt auf einer einzigen deutschsprachigen Seite, was die Anwendung leistet und wo sie heruntergeladen werden kann — ohne dass dafür ein Repository-Konto oder tieferes Projekt-Know-how nötig ist.

## Inhalte

- **Vorstellung** — Hero-Bereich mit der Kernbotschaft der Anwendung und direkten Verweisen auf Release-Download und Repository.
- **Feature-Kacheln** — die zentralen Funktionen (Workspaces, Environments, History, Swagger-/OData-Import, OData-v4-API, Authentifizierungstypen, Team-/Benutzermodus, Echtzeit-Synchronisation, Health-Check).
- **Screenshot-Galerie** — echte Ansichten der laufenden Anwendung in hellem und dunklem Theme.
- **Download** — stabiler Verweis auf `releases/latest` mit Hinweis auf das ZIP-Asset `Schnittstellenzentrale-vX.Y.Z.zip`.
- **Footer** — Links auf Repository, `docs/help/` und `LEGAL.md`.

## Design-Anlehnung

Die Seite übernimmt die Token-Werte des Anwendungs-Designsystems (shadcn-Variablen, Sidebar-Gradient, Helvetica-Font-Stack), ist aber technisch eigenständig: reine HTML-/CSS-/JavaScript-Dateien ohne Build-Tool, ohne Framework und ohne Bootstrap — die Anwendung lädt Bootstrap ebenfalls nicht. Wie die App unterstützt die Seite ein helles und dunkles Theme (siehe [Dark Mode](../dark-mode/index.md)): Die `.dark`-Klasse liegt auf dem `<html>`-Element, die Auswahl wird unter dem `localStorage`-Schlüssel `colorScheme` persistiert und fällt ohne gespeicherte Wahl auf `prefers-color-scheme` zurück. Das Layout ist responsiv.

## Sprache

Die Seite ist bewusst nur auf Deutsch verfügbar — Issue, Repository-Dokumentation und primäre Zielgruppe sind deutschsprachig. Ein Sprachumschalter ist nicht vorgesehen, aber später nachrüstbar.

## Datenquellen der Screenshots

Die Galerie-PNGs werden nicht manuell erstellt, sondern aus der echten Anwendung generiert: Die Playwright-Testklasse `SiteScreenshotTests` startet die App mit reproduzierbaren Showcase-Daten (`TestDatabaseSeeder.SeedShowcaseDataAsync`) und schreibt die PNGs in `site/screenshots/`. Sie sind Teil des Repositorys und werden bei Bedarf lokal neu erzeugt und eingecheckt. Die zugrunde liegende Testinfrastruktur ist unter [Playwright-Tests](../playwright-tests/index.md) dokumentiert.

## Einschränkungen

- Die Seite ist rein statisch: keine Suche, keine Formulare, kein Tracking, keine Blog- oder Versionshistorie der Seite selbst.
- Die Screenshots aktualisieren sich nicht automatisch — Änderungen an der Anwendung werden erst sichtbar, wenn die PNGs neu erzeugt und eingecheckt werden.
- Bereits Pushes auf `staging` aktualisieren die öffentliche Seite; sie ist Präsentationsfläche und kein an Releases gekoppeltes Artefakt (Details siehe [Technischer Ablauf](ablauf-technisch.md)).
