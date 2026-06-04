# Anforderung 38: Mehrsprachigkeit DE/EN

## Fachliche Zusammenfassung

Die Anwendung wird von rein deutschsprachig auf zweisprachig (Englisch/Deutsch) umgestellt. Die Sprachauswahl erfolgt vollautomatisch anhand des HTTP-Headers `Accept-Language`, den ASP.NET Core Localization per `AcceptLanguageHeaderRequestCultureProvider` auswertet; es ist kein manueller Sprachumschalter vorgesehen. Englisch ist Standardsprache und Fallback. Alle sichtbaren UI-Texte — Labels, Buttons, Überschriften, Validierungsmeldungen und Fehlermeldungen, die dem Nutzer angezeigt werden — werden übersetzt; `throw`-Meldungen bleiben immer auf Englisch.

## Betroffene Klassen und Komponenten

### Neu zu erstellende Artefakte

| Artefakt | Pfad | Beschreibung |
|---|---|---|
| `SharedResources` (Marker-Klasse) | `src/Schnittstellenzentrale/Resources/SharedResources.cs` | Leere Klasse als Typisierungsanker für `IStringLocalizer<SharedResources>` |
| `SharedResources.resx` | `src/Schnittstellenzentrale/Resources/SharedResources.resx` | Englische Fallback-Texte (alle UI-Komponenten) |
| `SharedResources.de.resx` | `src/Schnittstellenzentrale/Resources/SharedResources.de.resx` | Deutsche Übersetzungen |
| `CoreResources` (Marker-Klasse) | `src/Schnittstellenzentrale.Core/Resources/CoreResources.cs` | Typisierungsanker, nur falls Core eigene Texte benötigt (siehe Offene Fragen) |
| `CoreResources.resx` | `src/Schnittstellenzentrale.Core/Resources/CoreResources.resx` | Englische Texte für DataAnnotation-Attribute auf Contracts (bedingt) |
| `CoreResources.de.resx` | `src/Schnittstellenzentrale.Core/Resources/CoreResources.de.resx` | Deutsche Übersetzungen für Core-Texte (bedingt) |

### Zu ändernde Artefakte

| Artefakt | Art der Änderung |
|---|---|
| `Program.cs` | `AddLocalization()`, `AddDataAnnotationsLocalization()` und `UseRequestLocalization()` mit `SupportedCultures: ["en", "de"]`, `DefaultRequestCulture: "en"` hinzufügen |
| Alle Razor-Komponenten unter `Components/Shared/` und `Components/Pages/` | Hartcodierte Zeichenketten durch `@L["Schluessel"]`-Aufrufe ersetzen; `IStringLocalizer<SharedResources>` per `@inject` einbinden |
| Contract-Klassen in `Schnittstellenzentrale.Core/Contracts/` (`UpdateApplicationRequest`, `CreateApplicationGroupRequest`, `CreateApplicationRequest`, `CreateEndpointGroupRequest`, `UpdateApplicationGroupRequest`, `UpdateEndpointGroupRequest`, `UpdateEndpointRequest`, `CreateEndpointRequest`, `AddEndpointKeyValueRequest`) | `[Required]`- und `[MaxLength]`-Attribute mit lokalisierten Fehlermeldungen über `AddDataAnnotationsLocalization()` versehen |
| `CLAUDE.md` | Konvention für resx-Pakete (ein Paket pro Projekt, keine komponentenindividuellen Dateien) dokumentieren |

### Tests

| Artefakt | Art |
|---|---|
| Neue Testklasse, z. B. `LocalizationTests` | Integrationstest mit `WebApplicationFactory`: prüft deutsche Darstellung (Accept-Language: de), englische Darstellung (Accept-Language: en), Fallback bei fehlendem oder unbekanntem Header |
| Bestehende Komponenten-Tests (`Components/`) | Sicherstellen, dass Texte aus Ressourcen kommen (kein Hardcode-Assert auf deutschen Text ohne Lokalisierungskontext) |

## Implementierungsansatz

### Middleware-Konfiguration (`Program.cs`)

```csharp
builder.Services.AddLocalization();
builder.Services.AddDataAnnotationsLocalization(options =>
    options.DataAnnotationLocalizerProvider = (type, factory) =>
        factory.Create(typeof(SharedResources)));

// Im Middleware-Stack nach UseRouting:
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures("en", "de")
    .AddSupportedUICultures("en", "de");
app.UseRequestLocalization(localizationOptions);
```

Der `AcceptLanguageHeaderRequestCultureProvider` ist standardmäßig aktiv und muss nicht explizit registriert werden.

### Ressourcen-Struktur

Pro Projekt wird genau ein resx-Paket angelegt. Keine komponentenindividuellen resx-Dateien. Schlüssel folgen dem Schema `{KomponentenName}_{Rolle}` (Beispiele: `ApplicationEditor_TitleNew`, `ConfirmDeleteGroupDialog_Message`). Jeder Schlüssel erhält eine Beschreibung im `Comment`-Feld der resx, die den UI-Kontext erklärt.

Gültige Rollen-Suffixe (nicht abschließend):

- Buttons: `SaveButton`, `CancelButton`, `DeleteButton`, `ConfirmButton`, `CloseButton`, `AddButton`
- Titel: `Title`, `TitleNew`, `TitleEdit`
- Labels/Hints: `Label_{Feld}`, `Placeholder_{Feld}`, `Tooltip_{Aktion}`
- Zustände: `EmptyState`
- Meldungen: `Message`, `Error_{Typ}`, `Warning_{Typ}`, `Info_{Typ}`

### Einbindung in Razor-Komponenten

```razor
@inject IStringLocalizer<SharedResources> L

<button class="sz-btn sz-btn-primary">@L["ApplicationEditor_SaveButton"]</button>
```

### Umgang mit DataAnnotations auf Core-Contracts

Die Contract-Klassen (z. B. `UpdateApplicationRequest`, `CreateApplicationGroupRequest`) in `Schnittstellenzentrale.Core/Contracts/` verwenden bereits `[Required]` und `[MaxLength]`. Diese Validierungsmeldungen werden über `AddDataAnnotationsLocalization()` automatisch lokalisiert, sofern die Ressourcen-Datei (`CoreResources.resx` / `CoreResources.de.resx`) bereitgestellt wird.

### Scope der Lokalisierung

| Bereich | Zu lokalisieren | Anmerkung |
|---|---|---|
| Razor-Komponenten (Labels, Buttons, Überschriften) | Ja | Alle Dateien unter `Components/` |
| DataAnnotation-Validierungsmeldungen | Ja | Via `AddDataAnnotationsLocalization()` |
| Fehlermeldungen aus Services/Komponenten, die dem Nutzer angezeigt werden | Ja | Z. B. `_errorMessage = $"Speichern fehlgeschlagen: ..."` |
| `throw`-Meldungen (Exception-Texte) | Nein | Immer Englisch |

## Konfiguration

Die Sprachauswahl ist nicht benutzerspezifisch konfigurierbar. Die unterstützten Kulturen (`"en"`, `"de"`) und die Standardkultur (`"en"`) sind in `Program.cs` fest kodiert. Keine Cookie- oder Session-basierte Persistenz. Keine UI-Option zum Wechseln der Sprache.

## Akzeptanzkriterien (technisch formuliert)

1. `Accept-Language: de` am HTTP-Request -> alle UI-Texte in Deutsch (inkl. Validierungsmeldungen)
2. `Accept-Language: en` oder unbekannte Sprache -> alle UI-Texte in Englisch
3. Kein `Accept-Language`-Header -> Englisch (Default-Kultur)
4. Fehlender Schlüssel in `SharedResources.de.resx` -> englischer Fallback aus `SharedResources.resx`
5. `throw new Exception(...)` und Logging-Meldungen sind sprachunabhängig immer Englisch
6. Pro Projekt maximal ein resx-Paket; keine komponentenindividuellen resx-Dateien
7. Alle Ressourcen-Schlüssel folgen dem Schema `{KomponentenName}_{Rolle}`
8. Jeder Schlüssel hat einen ausgefüllten `Comment`-Eintrag in der resx
9. `CLAUDE.md` enthält die resx-Konvention (ein Paket pro Projekt)
10. Tests decken ab: deutsche Darstellung, englische Darstellung, Fallback-Verhalten

## Offene Fragen

1. **Core-resx-Paket erforderlich?** Die Contract-Klassen in `Schnittstellenzentrale.Core/Contracts/` verwenden `[Required]` und `[MaxLength]` ohne `ErrorMessage`-Parameter. Diese Meldungen werden von ASP.NET Core standardmäßig auf Englisch ausgegeben. Zu klären: Werden diese Validierungsmeldungen dem Endnutzer in der UI direkt angezeigt (via `<ValidationMessage>`)? Falls ja, ist `CoreResources.resx` / `CoreResources.de.resx` erforderlich. Falls die Contracts ausschließlich für API-Validierung (Controller-Ebene) genutzt werden und Validierungsfehler nicht in der Blazor-UI erscheinen, kann das Core-resx-Paket entfallen.

2. **Migrationsaufwand bestehender Tests:** Mehrere Komponenten-Tests (z. B. `MainLayoutTests`, `ApplicationContextMenuTests`) prüfen möglicherweise Texte, die hartcodiert auf Deutsch sind. Nach der Umstellung müssen diese Tests angepasst werden, um mit dem Lokalisierungs-Mock oder einer explizit gesetzten Kultur zu arbeiten.

3. **Blazor Server und Circuit-Kultur:** Bei Blazor Server-Rendering wird die Kultur pro SignalR-Circuit gesetzt. Es ist sicherzustellen, dass `CultureInfo.CurrentCulture` und `CultureInfo.CurrentUICulture` korrekt pro Request/Circuit aus dem `Accept-Language`-Header übernommen werden (ggf. über `CultureSetter`-Middleware oder `OnCircuitOpened`-Hook).
