# Mehrsprachigkeit DE/EN — Installation und Konfiguration

## Voraussetzungen

- ASP.NET Core 9
- Keine zusatzlichen NuGet-Pakete erforderlich; `Microsoft.Extensions.Localization` ist im
  ASP.NET Core 9 SDK enthalten.

## Konfiguration in `Program.cs`

Die Lokalisierung ist in `Program.BuildWebApplicationAsync` fest konfiguriert:

```csharp
// Service-Registrierung
builder.Services.AddControllers()
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResources)));

// Middleware-Konfiguration (nach UseAuthorization)
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures("en", "de")
    .AddSupportedUICultures("en", "de");
app.UseRequestLocalization(localizationOptions);
```

`AddLocalization()` wird nicht separat aufgerufen, da `AddDataAnnotationsLocalization()` dies
implizit einschließt.

## Konfigurationsparameter

| Parameter | Wert | Beschreibung |
|---|---|---|
| Default-Kultur | `"en"` | Sprache bei fehlendem oder unbekanntem `Accept-Language`-Header |
| Unterstutzte Kulturen | `["en", "de"]` | Vollstandige Liste der unterstutzten Sprachen |
| DataAnnotations-Provider | `SharedResources` | Zentraler Localizer fur alle Validierungsmeldungen |

Keine Einträge in `appsettings.json`. Alle Lokalisierungsparameter sind in `Program.cs` hart kodiert.

## Ressourcen-Dateien

| Datei | Zweck |
|---|---|
| `src/Schnittstellenzentrale/Resources/SharedResources.cs` | Leere Marker-Klasse als Typisierungsanker |
| `src/Schnittstellenzentrale/Resources/SharedResources.resx` | Englische Texte (Fallback) |
| `src/Schnittstellenzentrale/Resources/SharedResources.de.resx` | Deutsche Ubersetzungen |

## Neue Schlüssel hinzufugen

1. Eintrag in `SharedResources.resx` anlegen (englischer Text + `Comment`-Feld mit UI-Kontext).
2. Aquivalenten Eintrag in `SharedResources.de.resx` anlegen (deutscher Text).
3. In der Razor-Komponente `@inject IStringLocalizer<SharedResources> L` hinzufugen (sofern noch
   nicht vorhanden; `@using Microsoft.Extensions.Localization` ist global in `_Imports.razor`).
4. Text durch `@L["SchluesselName"]` ersetzen.

Schlüsselschema: `{KomponentenName}_{Rolle}` — Beispiele: `ApplicationEditor_SaveButton`,
`ConfirmDeleteGroupDialog_Title`, `EndpointPage_Tab_Auth`.

Jeder Schlüssel erfordert einen ausgefullten `Comment`-Eintrag in der `.resx`, der den UI-Kontext
beschreibt (Pflicht gemas CLAUDE.md-Konvention).

## Uberprufung

Nach einem Deployment kann gepruft werden, ob die Lokalisierung korrekt funktioniert, indem der
Browser temporar auf Deutsch umgestellt wird (Spracheinstellungen des Browsers) und eine Seite neu
geladen wird. Alternativ kann ein HTTP-Request mit gesetztem `Accept-Language: de`-Header an
`/` gesendet werden — die Antwort muss den Text `Neu laden` (statt `Reload`) enthalten.

Die vier Integrationstests in `LocalizationTests` (Klasse in
`src/Schnittstellenzentrale.Tests/Integration/LocalizationTests.cs`) prüfen dieses Verhalten
automatisiert:

| Test | Was wird gepruft |
|---|---|
| `DeRequestMitAcceptLanguageDe_ZeigtDeutscheTexte` | Response enthalt `"Neu laden"` bei `Accept-Language: de` |
| `DeRequestMitAcceptLanguageEn_ZeigtEnglischeTexte` | Response enthalt `"Reload"` bei `Accept-Language: en` |
| `DeRequestOhneAcceptLanguage_ZeigtEnglischeTexte` | Response enthalt `"Reload"` ohne Header |
| `DeRequestMitUnbekannterSprache_ZeigtEnglischeTexte` | Response enthalt `"Reload"` bei `Accept-Language: fr` |
