# Logikklassen

## `ImpressumService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ImpressumService.cs`

Implementiert `IImpressumService`.

| Methode / Member | Sichtbarkeit | Kurzbeschreibung |
|------------------|-------------|------------------|
| `_resolvedPath` | `private readonly string` | Einmalig im Konstruktor aufgelöster absoluter Dateipfad zur Impressum-Datei |
| `ImpressumService(IOptions<ImpressumSettings>)` | `public` | Konstruktor: löst `FilePath` aus `ImpressumSettings` auf — leer → `AppContext.BaseDirectory/impressum.md`, relativ → `Path.GetFullPath` relativ zu `AppContext.BaseDirectory`, absolut → direkte Verwendung |
| `IsAvailable()` | `public` | Gibt `File.Exists(_resolvedPath)` zurück — **kein Sprachbezug**, keine Laufzeitauflösung |
| `GetContentAsHtmlAsync()` | `public async` | Liest `_resolvedPath` per `File.ReadAllTextAsync` und wandelt Markdown in HTML um (`Markdig.Markdown.ToHtml`) |

**Service-Lebenszyklus:** In `Program.cs` als **`Singleton`** registriert (`builder.Services.AddSingleton<IImpressumService, ImpressumService>()`). `_resolvedPath` wird dadurch einmalig pro App-Start gesetzt und ist für alle Requests identisch. Eine request-spezifische Laufzeit-Kulturauflösung über `CultureInfo.CurrentUICulture` ist im Singleton-Lebenszyklus nicht zuverlässig möglich.

Abonnierte Events: keine
Publizierte Events: keine

---

## `ImpressumSettings`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ImpressumSettings.cs`

Konfigurationsklasse, die über `builder.Configuration.GetSection("Impressum")` befüllt wird.

| Eigenschaft | Typ | Beschreibung |
|-------------|-----|--------------|
| `FilePath` | `string` | Pfad zur Impressum-Markdown-Datei. Leerstring bedeutet `AppContext.BaseDirectory/impressum.md`. |

Wird aufgerufen von: `ImpressumService` (Konstruktor via `IOptions<ImpressumSettings>`).
