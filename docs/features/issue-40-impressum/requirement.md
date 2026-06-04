# Anforderungsübersetzung – Impressum-Seite (Issue #40)

## Fachliche Zusammenfassung

Die Anwendung erhält eine neue Seite `/impressum`, die den Inhalt einer Markdown-Datei (`impressum.md`) aus dem Programmverzeichnis (`AppContext.BaseDirectory`) rendert und als HTML anzeigt. Die Seite ist optional: Existiert die Datei nicht, wird sie ausgeblendet – weder im Navigationsmenü (`NavMenu`) noch beim direkten Aufruf der Route wird sie angezeigt. Damit ist das Feature für den internen Betrieb (ohne Impressumspflicht) unsichtbar, kann aber durch einfaches Ablegen der Datei im Programmverzeichnis aktiviert werden, ohne dass eine Konfigurationsänderung oder ein Neustart der Anwendung erforderlich ist.

## Betroffene Klassen und Komponenten

### Neu zu erstellen

| Artefakt | Typ | Beschreibung |
|----------|-----|--------------|
| `ImpressumPage` | Razor-Komponente (`Pages/ImpressumPage.razor`) | Seite unter `/impressum`; liest Dateiinhalt, rendert Markdown als HTML |
| `IImpressumService` | Interface (`Core/Interfaces/`) | Abstraktion für Dateizugriff und Markdown-Rendering |
| `ImpressumService` | Service-Klasse (`Infrastructure/Services/` oder `Services/`) | Implementierung: prüft Existenz der Datei, liest Inhalt, wandelt Markdown in HTML um |
| `ImpressumSettings` | Konfigurationsklasse (Annahme) | Optionale Einstellung für den Dateipfad (Standard: `AppContext.BaseDirectory`) |

### Zu erweitern

| Artefakt | Änderung |
|----------|----------|
| `NavMenu` | Navigationslink für `/impressum` nur rendern, wenn `IImpressumService.IsAvailableAsync()` true ergibt |
| `Program.cs` | Registrierung von `IImpressumService` (Singleton oder Transient) |
| `SharedResources.resx` / `SharedResources.de.resx` | Lokalisierungsschlüssel: `ImpressumPage_PageTitle`, `ImpressumPage_Heading` |

### Tests

| Artefakt | Typ |
|----------|-----|
| `ImpressumServiceTests` | Unit-Test: Datei vorhanden / nicht vorhanden, Markdown-Ausgabe korrekt |
| `ImpressumPageTests` (Playwright) | Smoke-Test: Seite sichtbar wenn Datei vorhanden, Menüeintrag fehlt wenn nicht vorhanden |

## Implementierungsansatz

1. **Dateiprüfung und -zugriff:** `ImpressumService` prüft beim Aufruf (kein Caching oder einmaliges Caching beim Start), ob `impressum.md` im Programmverzeichnis (`AppContext.BaseDirectory`) existiert, und liest den Inhalt bei Bedarf ein.

2. **Markdown-Rendering:** Der Markdown-Text wird serverseitig in HTML konvertiert. Hierfür ist eine Bibliothek wie `Markdig` einzubinden (Annahme: noch nicht im Projekt vorhanden; muss als NuGet-Paket ergänzt werden). Das erzeugte HTML wird in der Razor-Komponente via `MarkupString` gerendert.

3. **Optionale Sichtbarkeit:** `IImpressumService` stellt eine Methode `IsAvailableAsync()` (oder synchron `IsAvailable()`) bereit. `NavMenu` und `ImpressumPage` injizieren den Service und steuern Darstellung und Routing-Verhalten davon abhängig. Wird die Route direkt aufgerufen, aber die Datei fehlt, gibt die Seite einen 404-ähnlichen Hinweis aus oder leitet zur Startseite weiter.

4. **Kein Datenbankbezug, kein API-Client:** Das Feature greift direkt auf das Dateisystem zu. Der API-First-Grundsatz gilt hier nicht, da kein Datenentitäten-Objekt (Application, Endpoint usw.) betroffen ist.

5. **Lokalisierung:** Seitenüberschrift und `<PageTitle>` werden über `IStringLocalizer<SharedResources>` lokalisiert (Schlüssel `ImpressumPage_PageTitle`, `ImpressumPage_Heading`).

## Konfiguration

**Ebene:** Anwendungseinstellungen (`appsettings.json`)

Vorschlag für einen optionalen Konfigurationsabschnitt:

```json
"Impressum": {
  "FilePath": ""
}
```

- `FilePath` leer oder nicht vorhanden: Standard ist `Path.Combine(AppContext.BaseDirectory, "impressum.md")`.
- `FilePath` absolut: wird direkt verwendet.
- `FilePath` relativ: wird relativ zu `AppContext.BaseDirectory` aufgelöst.

Eine zugehörige Konfigurationsklasse `ImpressumSettings` wird über `builder.Services.Configure<ImpressumSettings>(...)` registriert.

> **Annahme:** Ein konfigurierbarer Pfad ist sinnvoll, da der Betreiber die Datei ggf. außerhalb des Programmverzeichnisses ablegen möchte (z. B. bei containerisierten Deployments). Falls der Kunde keine Konfigurierbarkeit wünscht, kann auf `ImpressumSettings` verzichtet und der Pfad hartcodiert werden.

## Offene Fragen

1. **Dateiname:** Soll die Datei zwingend `impressum.md` heißen, oder soll der Name konfigurierbar sein?
2. **Verhalten bei fehlender Datei und direktem URL-Aufruf:** Stille Weiterleitung zur Startseite, Anzeige einer Fehlermeldung oder HTTP-404-Antwort?
3. **Aktualisierungsverhalten:** Soll die Datei bei jedem Seitenaufruf neu eingelesen werden (immer aktuell, aber I/O-Overhead), oder soll ein Startup-Cache verwendet werden (performanter, aber Änderungen erst nach Neustart sichtbar)?
4. **Markdig bereits im Projekt?** Falls eine Markdown-Bibliothek schon vorhanden ist, ist diese zu verwenden.
5. **Sicherheit des gerenderten HTML:** Soll das aus Markdown erzeugte HTML sanitisiert werden, um XSS zu verhindern? Relevant, wenn die Datei von Dritten bearbeitet werden kann.
6. **Navigationsposition:** An welcher Stelle im Navigationsmenü soll der Impressum-Link erscheinen (z. B. am Ende, in einem separaten Footer-Bereich)?
