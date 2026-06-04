# Bestandsaufnahme: Mehrsprachigkeit DE/EN

Analysiert wurden die Middleware-Konfiguration, alle Razor-Komponenten unter `Components/Shared/` und `Components/Layout/`, die Contract-Klassen in `Schnittstellenzentrale.Core/Contracts/` sowie die bestehenden Test-Klassen — bezogen auf die Anforderung zur Einführung von DE/EN-Lokalisierung via `IStringLocalizer` und `AcceptLanguageHeaderRequestCultureProvider`.

## Zusammenfassung

- **Keine Lokalisierungsinfrastruktur vorhanden:** Weder `AddLocalization()`, `AddDataAnnotationsLocalization()` noch `UseRequestLocalization()` sind in `Program.cs` registriert. Es existieren keine `*.resx`-Dateien im gesamten Projekt, keine `SharedResources`- oder `CoreResources`-Klassen, keine `Resources/`-Verzeichnisse.
- **Alle UI-Texte sind hartcodiert auf Deutsch:** Sämtliche Razor-Komponenten enthalten deutsche Strings direkt im Markup. Kein `@inject IStringLocalizer<...>` ist in irgendeiner Komponente vorhanden. Die `_Imports.razor` enthält keinen `@using Microsoft.Extensions.Localization`.
- **Contract-Klassen verwenden `[Required]` und `[MaxLength]` ohne `ErrorMessage`:** Alle neun betroffenen Contract-Klassen tragen Validierungsattribute ohne expliziten `ErrorMessage`-Parameter, d. h. ASP.NET Core gibt derzeit die eingebauten englischen Standardmeldungen aus.
- **Tests prüfen deutsche Texte per hartcodierten Strings:** Die bUnit-Tests `ApplicationContextMenuTests`, `EndpointContextMenuTests` und `EndpointGroupContextMenuTests` suchen Buttons per `TextContent.Contains(...)` mit deutschen Bezeichnungen. Diese Tests werden nach der Lokalisierungsumstellung brechen, sofern kein `IStringLocalizer`-Mock registriert wird.
- **Keine `LocalizationTests`-Klasse vorhanden:** Eine Integrationstest-Klasse, die das Accept-Language-Verhalten über `WebApplicationFactory` prüft, existiert nicht.
- **`CLAUDE.md` enthält keine resx-Konvention:** Die Entwicklungskonvention für Ressourcen-Pakete ist noch nicht dokumentiert.

## Details

- [Datenmodell](inventory/models.md) — Contract-Klassen mit `[Required]`/`[MaxLength]` ohne `ErrorMessage`
- [Logik](inventory/logic.md) — `Program.cs` ohne Lokalisierungs-Middleware; alle Razor-Komponenten mit hartcodierten deutschen Texten
- [Tests](inventory/tests.md) — bUnit-Tests mit hartcodierten deutschen Texten; fehlende `LocalizationTests`-Klasse
