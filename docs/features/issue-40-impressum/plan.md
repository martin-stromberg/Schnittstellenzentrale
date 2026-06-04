# Umsetzungsplan: Impressum-Seite (Issue #40)

## Übersicht

Die Anwendung erhält eine neue Seite `/impressum`, die den Inhalt einer Markdown-Datei serverseitig in HTML rendert und via `MarkupString` anzeigt. Das Feature ist dateigesteuert optional: Existiert die Datei nicht, wird der Footer-Link in der `WorkspacesSidebar` ausgeblendet und die Seite zeigt einen lokalisierten Hinweistext. Betroffen sind das Core-Projekt (neues Interface), das Infrastructure-Projekt (neue Settings-Klasse und Service-Implementierung), die Hauptanwendung (neue Seite, Sidebar-Anpassung, Registrierung, Konfiguration, Lokalisierung) sowie das Test-Projekt.

---

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| `ImpressumService` | Service Layer (kein Repository, kein API-Client) | Das Feature greift direkt auf das Dateisystem zu; kein Datenentitäten-Objekt ist betroffen. Der API-First-Grundsatz gilt hier explizit nicht (laut Anforderung). |
| `ImpressumService` – Projektzugehörigkeit | `Schnittstellenzentrale.Infrastructure.Services` | Alle übrigen Services mit Infrastrukturzugriff liegen dort (`HistoryService`, `ThemeService` usw.). `Markdig` wird damit im Infrastructure-Projekt referenziert; das Core-Projekt bleibt frei von externen Abhängigkeiten. |
| `ImpressumSettings` – Projektzugehörigkeit | `Schnittstellenzentrale.Infrastructure.Services` | Analog zu `HistorySettings` und `UploadSettings`, die im selben Verzeichnis liegen. |
| Markdown-Rendering | Serverseitig in `ImpressumService` via `Markdig` | Rendering-Logik gehört in den Service, nicht in die Razor-Komponente. Die Komponente erhält fertiges HTML als `MarkupString`. |
| HTML-Sanitisierung | Keine — `MarkupString` ohne Sanitizer | Akzeptiertes Sicherheitsmodell, da die Datei ausschließlich vom Betreiber abgelegt wird. |
| Lebensdauer der Service-Registrierung | `Singleton` | Der Service hält keinen benutzersitzungsabhängigen Zustand. Der aufgelöste Dateipfad wird einmalig im Konstruktor berechnet und ist danach unveränderlich. Entspricht dem Muster von `ICurrentUserService` und `IHealthCheckService`. |
| Aktualisierungsverhalten | Datei wird bei jedem Aufruf neu geprüft und gelesen — kein Startup-Cache | Ermöglicht Aktivierung/Deaktivierung ohne Neustart. I/O-Overhead ist bei einer kleinen statischen Textdatei vernachlässigbar. |
| Verhalten bei fehlender Datei und direktem URL-Aufruf | `ImpressumPage` zeigt lokalisierten Hinweistext — keine Weiterleitung, kein HTTP-404 | Einfachste Implementierung ohne `NavigationManager`-Logik; konsistent mit dem Muster der `HelpPage`. Kein neuer Schlüssel für den Hinweistext nötig, sofern ein generischer Fallback-Text ausreicht. |
| Dateiname konfigurierbar | `ImpressumSettings.FilePath` nimmt den vollständigen Pfad auf — kein separates `FileName`-Feld | Der Dateiname ist implizit konfigurierbar über den `FilePath`. Ein separates Feld wäre redundant. |
| Navigationsposition des Impressum-Links | Ausschließlich im Footer der `WorkspacesSidebar` — `NavMenu` bleibt unverändert | Der Link ist dort bereits statisch vorhanden. Der Sidebar-Footer ist der konventionelle Ort für Impressum-Links. Kein weiterer Link wird hinzugefügt. |

---

## Programmabläufe

### Ablauf 1: Seitenaufruf `/impressum` — Datei vorhanden

1. Nutzer navigiert zu `/impressum`.
2. `ImpressumPage` wird gerendert; `OnInitializedAsync` wird aufgerufen.
3. `ImpressumPage` ruft `IImpressumService.IsAvailable()` auf.
4. `ImpressumService.IsAvailable()` prüft via `File.Exists(resolvedPath)`. Ergebnis: `true`.
5. `ImpressumPage` ruft `IImpressumService.GetContentAsHtmlAsync()` auf.
6. `ImpressumService.GetContentAsHtmlAsync()` liest den Dateiinhalt via `File.ReadAllTextAsync(resolvedPath)` und konvertiert ihn mit `Markdig.Markdown.ToHtml(...)` in HTML.
7. `ImpressumPage` rendert `<PageTitle>`, `<h1>` (lokalisiert) und den HTML-Inhalt als `MarkupString`.

Beteiligte Klassen/Komponenten: `ImpressumPage`, `IImpressumService`, `ImpressumService`

---

### Ablauf 2: Seitenaufruf `/impressum` — Datei fehlt

1. Nutzer navigiert zu `/impressum`.
2. `ImpressumPage` wird gerendert; `OnInitializedAsync` wird aufgerufen.
3. `ImpressumPage` ruft `IImpressumService.IsAvailable()` auf.
4. `ImpressumService.IsAvailable()` gibt `false` zurück.
5. `ImpressumPage` rendert einen lokalisierten Hinweistext statt des Markdown-Inhalts.

Beteiligte Klassen/Komponenten: `ImpressumPage`, `IImpressumService`, `ImpressumService`

---

### Ablauf 3: Bedingtes Rendering des Footer-Links in der `WorkspacesSidebar`

1. `WorkspacesSidebar` wird gerendert; `OnInitialized` wird aufgerufen.
2. `WorkspacesSidebar` ruft `IImpressumService.IsAvailable()` auf und speichert das Ergebnis in `_impressumAvailable`.
3. Der `<a href="/impressum">`-Link im Footer wird nur gerendert, wenn `_impressumAvailable` den Wert `true` hat.

Beteiligte Klassen/Komponenten: `WorkspacesSidebar`, `IImpressumService`, `ImpressumService`

---

### Ablauf 4: Pfadauflösung im `ImpressumService`-Konstruktor

1. `ImpressumService` erhält beim Konstruktoraufruf `IOptions<ImpressumSettings>` per DI.
2. Ist `ImpressumSettings.FilePath` leer oder null: Pfad wird als `Path.Combine(AppContext.BaseDirectory, "impressum.md")` aufgelöst.
3. Ist `FilePath` relativ: Pfad wird per `Path.GetFullPath(filePath, AppContext.BaseDirectory)` aufgelöst.
4. Ist `FilePath` absolut: wird direkt verwendet.
5. Der aufgelöste Pfad wird als private readonly-Variable gespeichert und bei jedem Aufruf von `IsAvailable()` und `GetContentAsHtmlAsync()` verwendet.

Beteiligte Klassen/Komponenten: `ImpressumService`, `ImpressumSettings`, `IOptions<ImpressumSettings>`

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `IImpressumService` | Interface | Abstraktion für Dateiverfügbarkeitsprüfung und Markdown-zu-HTML-Konvertierung |
| `ImpressumSettings` | Konfigurationsklasse (POCO) | Hält den konfigurierbaren Dateipfad (`FilePath`); Standardwert ist leerer String |
| `ImpressumService` | Klasse (Service Layer) | Implementierung: Pfadauflösung im Konstruktor, `File.Exists`-Prüfung, Dateieinlesen, Markdig-Rendering |
| `ImpressumPage` | Razor-Komponente (Page) | Seite unter `/impressum`; zeigt gerendertes HTML oder lokalisierten Hinweistext |
| `ImpressumServiceTests` | xUnit-Testklasse | Unit-Tests für `ImpressumService` |
| `ImpressumPageTests` | Playwright-Testklasse | Smoke- und Sichtbarkeitstests für `/impressum` und den Footer-Link |

---

## Änderungen an bestehenden Klassen

### `WorkspacesSidebar` (Razor-Komponente)

- **Neue Eigenschaften:** `_impressumAvailable` (`bool`, privat) — Zwischenspeicher für das Ergebnis von `IsAvailable()` für das bedingte Rendering des Footer-Links.
- **Neue Methoden:** keine
- **Geänderte Methoden:** `OnInitialized()` — ruft zusätzlich `IImpressumService.IsAvailable()` auf und setzt `_impressumAvailable`.
- **Neue Event-Handler:** keine
- **Anpassung Markup:** Der statisch verdrahtete `<a href="/impressum">`-Link im Footer wird mit `@if (_impressumAvailable)` umschlossen.

---

### `Program.cs` (Anwendungs-Startklasse)

- **Neue Registrierungen:**
  - `builder.Services.Configure<ImpressumSettings>(builder.Configuration.GetSection("Impressum"))` — Konfigurationsabschnitt binden.
  - `builder.Services.AddSingleton<IImpressumService, ImpressumService>()` — Service registrieren.

---

### `SharedResources.resx` (EN-Fallback)

- **Neue Einträge:** `ImpressumPage_PageTitle` (`"Imprint"`), `ImpressumPage_Heading` (`"Imprint"`) und `ImpressumPage_NotAvailable` (`"No imprint available."`) — jeweils mit ausgefülltem `Comment`-Feld.

---

### `SharedResources.de.resx` (Deutsch)

- **Neue Einträge:** `ImpressumPage_PageTitle` (`"Impressum"`), `ImpressumPage_Heading` (`"Impressum"`) und `ImpressumPage_NotAvailable` (`"Kein Impressum verfügbar."`) — jeweils mit ausgefülltem `Comment`-Feld.

---

### `appsettings.json`

- **Neuer Abschnitt:**
  ```json
  "Impressum": {
    "FilePath": ""
  }
  ```

---

### `Schnittstellenzentrale.Infrastructure.csproj`

- **Neue Paket-Referenz:** `Markdig` (aktuellste stabile Version).

---

## Datenbankmigrationen

Keine.

---

## Validierungsregeln

Keine. Die Impressum-Seite hat keine Benutzereingaben.

---

## Konfigurationsänderungen

| Eintrag | Typ | Standardwert | Zweck |
|---------|-----|--------------|-------|
| `Impressum:FilePath` | `string` | `""` (leer) | Pfad zur Impressum-Markdown-Datei; leer = `AppContext.BaseDirectory/impressum.md`. Dateiname ist über diesen Pfad implizit konfigurierbar. |

---

## Seiteneffekte und Risiken

- **`WorkspacesSidebar`:** Durch die neue DI-Injektion von `IImpressumService` und den Aufruf in `OnInitialized()` ändert sich das Initialisierungsverhalten. Da `IsAvailable()` ein synchroner `File.Exists()`-Aufruf ist, entsteht kein Async-Overhead. Bestehende Tests, die `WorkspacesSidebar` rendern, müssen einen Mock für `IImpressumService` bereitstellen.
- **`Markdig` als neue NuGet-Abhängigkeit im Infrastructure-Projekt:** Geringes Risiko; `Markdig` ist eine etablierte, wartungsaktive Bibliothek ohne transitive Abhängigkeiten auf EF Core oder andere kritische Infrastruktur.

---

## Umsetzungsreihenfolge

1. **`ImpressumSettings`** in `src/Schnittstellenzentrale.Infrastructure/Services/ImpressumSettings.cs` anlegen.
2. **`IImpressumService`** in `src/Schnittstellenzentrale.Core/Interfaces/IImpressumService.cs` anlegen.
3. **`Markdig` NuGet-Paket** in `Schnittstellenzentrale.Infrastructure.csproj` ergänzen.
4. **`ImpressumService`** in `src/Schnittstellenzentrale.Infrastructure/Services/ImpressumService.cs` anlegen — setzt Schritte 1–3 voraus.
5. **`appsettings.json`** um den `Impressum`-Abschnitt erweitern.
6. **`Program.cs`** um `ImpressumSettings`-Konfiguration und `IImpressumService`-Registrierung erweitern — setzt Schritte 1, 2 und 4 voraus.
7. **`SharedResources.resx`** und **`SharedResources.de.resx`** um die drei Lokalisierungsschlüssel ergänzen.
8. **`ImpressumPage.razor`** anlegen — setzt Schritte 2, 6 und 7 voraus.
9. **`WorkspacesSidebar.razor`** anpassen (bedingtes Rendering des Footer-Links) — setzt Schritt 6 voraus.
10. **`ImpressumServiceTests`** anlegen — setzt Schritte 2 und 4 voraus.
11. **`ImpressumPageTests`** anlegen — setzt Schritte 8 und 9 voraus.

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `IsAvailable_DateiVorhanden_GibtTrueZurueck` | `ImpressumServiceTests` | `IsAvailable()` gibt `true` zurück, wenn die konfigurierte Datei existiert |
| `IsAvailable_DateiFehlt_GibtFalseZurueck` | `ImpressumServiceTests` | `IsAvailable()` gibt `false` zurück, wenn die Datei nicht existiert |
| `GetContentAsHtmlAsync_MarkdownWirdKorrektGerendert` | `ImpressumServiceTests` | Markdown-Eingabe erzeugt erwartetes HTML-Fragment (z. B. `# Titel` → `<h1>Titel</h1>`) |
| `GetContentAsHtmlAsync_DateiFehlt_WirftException` | `ImpressumServiceTests` | Wenn Datei fehlt und `GetContentAsHtmlAsync()` aufgerufen wird, wird eine passende Exception geworfen |
| `Pfadaufloesung_LeerFilePath_VerwendetBaseDirectory` | `ImpressumServiceTests` | Leerer `FilePath` löst auf `AppContext.BaseDirectory/impressum.md` auf |
| `Pfadaufloesung_RelativerFilePath_WirdRelativZuBaseDirectoryAufgeloest` | `ImpressumServiceTests` | Relativer `FilePath` wird relativ zu `AppContext.BaseDirectory` aufgelöst |
| `Pfadaufloesung_AbsoluterFilePath_WirdDirektVerwendet` | `ImpressumServiceTests` | Absoluter `FilePath` wird unverändert verwendet |
| `ImpressumSeite_ZeigtInhalt_WennDateiVorhanden` | `ImpressumPageTests` | Playwright: `/impressum` zeigt Überschrift und Inhalt, wenn Impressum-Datei vorhanden ist |
| `ImpressumSeite_ZeigtHinweis_WennDateiFehlt` | `ImpressumPageTests` | Playwright: `/impressum` zeigt lokalisierten Hinweistext, wenn Datei fehlt |
| `SidebarFooter_LinkSichtbar_WennDateiVorhanden` | `ImpressumPageTests` | Playwright: Impressum-Link im Sidebar-Footer ist sichtbar, wenn Datei vorhanden ist |
| `SidebarFooter_LinkFehlt_WennDateiNichtVorhanden` | `ImpressumPageTests` | Playwright: Impressum-Link im Sidebar-Footer fehlt, wenn Datei nicht vorhanden ist |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| Alle bUnit- oder Playwright-Tests, die `WorkspacesSidebar` direkt rendern | `IImpressumService` muss als Mock (Ergebnis `IsAvailable() → false`) in den DI-Container eingetragen werden, da die Komponente den Service jetzt injiziert |

---

## Offene Punkte

Keine.
