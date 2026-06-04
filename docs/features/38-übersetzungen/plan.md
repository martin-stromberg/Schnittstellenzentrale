# Umsetzungsplan: Mehrsprachigkeit DE/EN

## Übersicht

Die Anwendung wird von rein deutschsprachig auf zweisprachig (Englisch/Deutsch) umgestellt. Die Sprachauswahl erfolgt automatisch über den HTTP-Header `Accept-Language`; Englisch ist Standardsprache und Fallback. Betroffen sind die Middleware-Konfiguration in `Program.cs`, alle Razor-Komponenten mit sichtbaren UI-Texten, die Contract-Klassen im Core-Projekt sowie betroffene bUnit-Tests.

---

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|---|---|---|
| Ressourcen-Struktur | Ein resx-Paket pro Projekt (`SharedResources` in `Schnittstellenzentrale`, `CoreResources` in `Schnittstellenzentrale.Core`) | Anforderung schreibt diese Konvention explizit vor; verhindert Fragmentierung durch komponentenindividuelle Dateien |
| DataAnnotations-Lokalisierung | `AddDataAnnotationsLocalization()` mit zentralem Provider auf `SharedResources` | Einheitlicher Lokalisierungs-Einstiegspunkt; Contract-Klassen im Core-Projekt benötigen eigene `CoreResources`, da `AddDataAnnotationsLocalization()` den Assembly-Namespace des Typs für die Ressourcensuche heranzieht |
| Blazor-Server-Kulturpropagation | `UseRequestLocalization()` in der Middleware-Pipeline; kein expliziter `CultureSetter` oder `OnCircuitOpened`-Hook | ASP.NET Core 9 mit Blazor Server übernimmt beim SSR-Rendering die Kultur aus dem HTTP-Request automatisch in den Circuit; der `AcceptLanguageHeaderRequestCultureProvider` ist standardmäßig aktiv und muss nicht zusätzlich registriert werden |
| Test-Lokalisierungsmocking | `IStringLocalizer<SharedResources>` in bUnit-Tests über `AddSingleton` mit `FakeStringLocalizer` (gibt Schlüssel als Wert zurück) registrieren | Einfachste Strategie: Tests sind sprachunabhängig, da der zurückgegebene Wert dem Schlüssel entspricht; Suchausdrücke in Tests werden auf Schlüssel umgestellt |
| Lokalisierung der `blazor-error-ui`-Fehlermeldung | Statisches HTML-Attribut in `AppShell.razor` wird durch `@L["AppShell_ErrorMessage"]` und `@L["AppShell_ReloadLink"]` ersetzt | Konsistenz mit allen anderen sichtbaren UI-Texten |
| `CoreResources`-Entscheid (offene Frage) | `CoreResources.resx` / `CoreResources.de.resx` wird erstellt | Die Validierungsattribute der Contract-Klassen werden via `<ValidationMessage>` in der Blazor-UI angezeigt (erkennbar an den Inline-Validierungen in `RenameEndpointGroupDialog`, `CreateEndpointGroupDialog`, `EndpointPage`, `EnvironmentEditor`); eine Lokalisierung dieser Meldungen ist daher erforderlich |

---

## Programmabläufe

### Sprachauflösung bei HTTP-Request

1. Browser sendet HTTP-Request mit `Accept-Language: de` (oder `en`, oder ohne Header).
2. `UseRequestLocalization()` in der Middleware-Pipeline wertet den Header aus (`AcceptLanguageHeaderRequestCultureProvider`).
3. ASP.NET Core setzt `CultureInfo.CurrentCulture` und `CultureInfo.CurrentUICulture` für den Request-Thread auf die erkannte Kultur (oder auf `"en"` als Default).
4. Der Blazor-Server-Circuit übernimmt die Kultur aus dem initialen HTTP-Request.
5. Razor-Komponenten rufen über `@L["Schlüssel"]` den `IStringLocalizer<SharedResources>` auf.
6. Der Localizer sucht zuerst in `SharedResources.de.resx` (bei `de`), fällt bei fehlendem Schlüssel auf `SharedResources.resx` (englischer Fallback) zurück.

Beteiligte Klassen/Komponenten: `Program`, `RequestLocalizationOptions`, `AcceptLanguageHeaderRequestCultureProvider`, `IStringLocalizer<SharedResources>`, alle Razor-Komponenten

### DataAnnotations-Validierung (lokalisiert)

1. Nutzer sendet Formular mit ungültigen Eingaben (z. B. leerem Pflichtfeld).
2. Blazors `EditForm`-Validierung ruft die DataAnnotations-Attribute der Contract-Klasse auf.
3. `AddDataAnnotationsLocalization()` leitet die Fehlermeldungs-Auflösung an den konfigurierten Provider (auf Basis von `SharedResources` im Hauptprojekt, `CoreResources` im Core-Projekt) weiter.
4. `<ValidationMessage>` rendert den lokalisierten Fehlertext in der aktuellen Kultur.

Beteiligte Klassen/Komponenten: Contract-Klassen in `Schnittstellenzentrale.Core/Contracts/`, `IStringLocalizer<CoreResources>`, `AddDataAnnotationsLocalization()`, Razor-Komponenten mit `<EditForm>`

---

## Neue Klassen

| Klasse | Typ | Zweck |
|---|---|---|
| `SharedResources` | Marker-Klasse (leer) | Typisierungsanker für `IStringLocalizer<SharedResources>` im Hauptprojekt |
| `CoreResources` | Marker-Klasse (leer) | Typisierungsanker für `IStringLocalizer<CoreResources>` im Core-Projekt |
| `LocalizationTests` | xUnit-Testklasse (Integrationstest) | Prüft DE-Darstellung, EN-Darstellung und Fallback über `WebApplicationFactory` mit Accept-Language-Header |

---

## Änderungen an bestehenden Klassen

### `Program` (Middleware-Konfiguration)

- **Geänderte Methode:** `BuildWebApplicationAsync` — Hinzufügen von `AddLocalization()`, `AddDataAnnotationsLocalization()` mit Provider auf `SharedResources`, sowie `UseRequestLocalization()` mit `DefaultCulture: "en"`, `SupportedCultures: ["en", "de"]` im Middleware-Stack nach `UseAuthentication`/`UseAuthorization`.

### `_Imports.razor`

- **Neuer `@using`-Eintrag:** `@using Microsoft.Extensions.Localization` — damit `IStringLocalizer<SharedResources>` in allen Komponenten ohne expliziten Namespace verfügbar ist.

### `AppShell.razor`

- **Geänderte Texte:** Hartcodierter HTML-Inhalt des `#blazor-error-ui`-Div (`"Ein unbehandelter Fehler ist aufgetreten."` und `"Neu laden"`) wird durch `@L["AppShell_ErrorMessage"]` und `@L["AppShell_ReloadLink"]` ersetzt.
- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`

### `ApplicationEditor.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Alle hartcodierten deutschen Strings (Titel, Labels, Optionen, Hints, Buttons, nutzerseitige Fehlermeldungen) werden durch `@L["ApplicationEditor_..."]`-Aufrufe ersetzt.

### `ApplicationGroupEditor.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Titel, Labels, Buttons, nutzerseitige Fehlermeldungen durch `@L["ApplicationGroupEditor_..."]`.

### `ApplicationGroupContextMenu.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** `"Umbenennen"`, `"Löschen"` durch `@L["ApplicationGroupContextMenu_RenameButton"]`, `@L["ApplicationGroupContextMenu_DeleteButton"]`.

### `ApplicationContextMenu.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** `"Bearbeiten"`, `"Ordner anlegen"`, `"Endpunkt anlegen"`, `"Aus Sammlung entfernen"`, `"Löschen"` durch `@L["ApplicationContextMenu_..."]`.

### `EndpointContextMenu.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** `"Endpunkt löschen"` durch `@L["EndpointContextMenu_DeleteButton"]`.

### `EndpointGroupContextMenu.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** `"Endpunkt anlegen"`, `"Ordner umbenennen"`, `"Ordner löschen"` durch `@L["EndpointGroupContextMenu_..."]`.

### `ConfirmDeleteGroupDialog.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Dialog-Titel, Nachrichten, Buttons durch `@L["ConfirmDeleteGroupDialog_..."]`.

### `ConfirmDeleteApplicationDialog.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Dialog-Titel, Nachricht, Buttons durch `@L["ConfirmDeleteApplicationDialog_..."]`.

### `ConfirmDeleteEndpointGroupDialog.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Dialog-Titel, Nachrichten, Buttons durch `@L["ConfirmDeleteEndpointGroupDialog_..."]`.

### `RenameGroupDialog.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Dialog-Titel, Label, Buttons, nutzerseitige Fehlermeldung durch `@L["RenameGroupDialog_..."]`.

### `RenameEndpointGroupDialog.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Dialog-Titel, Label, Buttons, Inline-Validierungsmeldung, nutzerseitige Fehlermeldung durch `@L["RenameEndpointGroupDialog_..."]`.

### `CreateEndpointGroupDialog.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Dialog-Titel, Label, Buttons, Inline-Validierungsmeldung, nutzerseitige Fehlermeldung durch `@L["CreateEndpointGroupDialog_..."]`.

### `ConcurrencyWarningDialog.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Dialog-Titel, Nachricht, Buttons durch `@L["ConcurrencyWarningDialog_..."]`.

### `EndpointPage.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Badge, Buttons, Inline-Validierung, Tabs, Response-Labels, Placeholders, `confirm()`-Dialog, nutzerseitige Fehlermeldungen durch `@L["EndpointPage_..."]`.

### `ImportDialog.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Status-Texte, Buttons, nutzerseitige Fehlermeldungen durch `@L["ImportDialog_..."]`.

### `HealthCheckDialog.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Status-Meldungen, Buttons durch `@L["HealthCheckDialog_..."]`.

### `EnvironmentEditor.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Sektion, Tabellen-Header, Leerzustand, Tooltips, Buttons, Inline-Validierungen, nutzerseitige Fehlermeldungen durch `@L["EnvironmentEditor_..."]`.

### `EnvironmentManagementOverlay.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Titel, Bestätigungsnachricht, Buttons, Leerzustand, nutzerseitige Fehlermeldungen durch `@L["EnvironmentManagementOverlay_..."]`.

### `EnvironmentSelector.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Platzhalter-Option `"— Keine Umgebung —"` durch `@L["EnvironmentSelector_NoEnvironmentOption"]`.

### `EnvironmentsSidebar.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Button, Tooltip, Placeholder, Buttons durch `@L["EnvironmentsSidebar_..."]`.

### `WorkspacesSidebar.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Buttons, Footer-Link durch `@L["WorkspacesSidebar_..."]`.

### `TopBar.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** `"Modus:"`, Optionen `"Team"`/`"Benutzer"`, `title`-Attribute (`"Einstellungen"`, `"Hilfe"`) durch `@L["TopBar_..."]`. Die Tab-Beschriftungen `"Workspaces"`, `"Environments"`, `"History"` sind bereits englisch und werden ebenfalls als Ressourcen-Schlüssel abgelegt (für DE-Konsistenz).

### `EmptyContentView.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** `"Wählen Sie eine Sammlung oder Anwendung aus."` durch `@L["EmptyContentView_Hint"]`.

### `LinksManager.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Sektion, Button, Placeholders, Buttons durch `@L["LinksManager_..."]`.

### `RequestAuthPanel.razor`

- **Neue Injektion:** `@inject IStringLocalizer<SharedResources> L`
- **Geänderte Texte:** Labels, Hints, Placeholders durch `@L["RequestAuthPanel_..."]`.

### `CLAUDE.md`

- **Neue Konvention:** Abschnitt zur resx-Struktur hinzufügen: ein Paket pro Projekt (`SharedResources` / `CoreResources`), keine komponentenindividuellen resx-Dateien, Schlüsselschema `{KomponentenName}_{Rolle}`, verpflichtender `Comment`-Eintrag pro Schlüssel.

---

## Datenbankmigrationen

Keine.

---

## Validierungsregeln

Keine neuen Validierungsregeln. Die bestehenden `[Required]`, `[MaxLength]` und `[Range]`-Attribute auf den Contract-Klassen bleiben unverändert; ihre Fehlermeldungen werden über `AddDataAnnotationsLocalization()` und `CoreResources` lokalisiert — ohne Änderung an den Attributen selbst, da ASP.NET Core die Standard-Ressourcen-Keys aus Typ- und Property-Namen ableitet.

---

## Konfigurationsänderungen

Keine Einträge in `appsettings.json`. Die Lokalisierungskonfiguration (Kulturen, Default) wird ausschließlich in `Program.cs` hart kodiert.

---

## Seiteneffekte und Risiken

- **bUnit-Tests mit hartcodierten deutschen Texten:** `ApplicationContextMenuTests`, `EndpointContextMenuTests` und `EndpointGroupContextMenuTests` brechen, weil `IStringLocalizer<SharedResources>` in der bUnit-Umgebung nicht automatisch registriert ist. Die Tests müssen auf Schlüssel-basierte Suche umgestellt und ein `FakeStringLocalizer` registriert werden.
- **`EnvironmentSelectorTests.RefreshAsync_AktualistertListeOhneFehler`:** Impliziert genau eine Option (`"— Keine Umgebung —"`); nach Lokalisierung der Platzhalter-Option muss der Test einen `FakeStringLocalizer` erhalten, der einen definierten Rückgabewert liefert.
- **`MainLayoutTests`:** Kein Handlungsbedarf für die Tab-Texte, jedoch benötigt `AppShell` nach der Umstellung eine `IStringLocalizer<SharedResources>`-Registrierung im bUnit-Service-Container, da die Komponente injiziert.
- **Blazor-Server-Circuit-Kultur:** Bei Blazor Server gilt die Kultur des initialen HTTP-Requests für den gesamten Circuit. Eine Kulturänderung erfordert einen Seiten-Reload. Das ist konform mit der Anforderung (kein manueller Sprachumschalter).
- **`SwaggerImportDialog` / `ODataImportDialog`:** Beide Komponenten sind reine Weiterleitungen auf `ImportDialog` und erhalten keinen eigenen Localizer; die Übersetzung erfolgt ausschließlich in `ImportDialog`.

---

## Umsetzungsreihenfolge

1. **`CoreResources`-Marker-Klasse und resx-Dateien anlegen** (`src/Schnittstellenzentrale.Core/Resources/CoreResources.cs`, `CoreResources.resx`, `CoreResources.de.resx`) — muss vor der Middleware-Konfiguration existieren, damit der DataAnnotations-Provider korrekt auflösen kann.
2. **`SharedResources`-Marker-Klasse und resx-Dateien anlegen** (`src/Schnittstellenzentrale/Resources/SharedResources.cs`, `SharedResources.resx`, `SharedResources.de.resx`) — alle Schlüssel für sämtliche Komponenten vollständig mit EN-Text und DE-Übersetzung befüllen (inkl. `Comment`-Felder).
3. **`Program.cs` anpassen** — `AddLocalization()`, `AddDataAnnotationsLocalization()` (Provider auf `SharedResources`), `UseRequestLocalization()` mit Default `"en"` und Kulturen `["en", "de"]` hinzufügen.
4. **`_Imports.razor` anpassen** — `@using Microsoft.Extensions.Localization` hinzufügen.
5. **Alle Razor-Komponenten umstellen** — hartcodierte Strings durch `@L["Schlüssel"]`-Aufrufe ersetzen; `@inject IStringLocalizer<SharedResources> L` hinzufügen. Reihenfolge: Layout-Komponenten (`AppShell`, `TopBar`), dann Shared-Komponenten (alphabetisch oder nach Komplexität).
6. **`CLAUDE.md` ergänzen** — Konvention für resx-Pakete dokumentieren.
7. **Betroffene bUnit-Tests anpassen** — `FakeStringLocalizer` in `TestMockFactory` (oder als lokale Hilfsklasse) bereitstellen; Test-Suchausdrücke von deutschen Texten auf Ressourcen-Schlüssel umstellen; `IStringLocalizer<SharedResources>` in betroffenen Testklassen registrieren.
8. **Neue `LocalizationTests`-Klasse erstellen** — Integrationstests mit `ControllerTestFactory`/`WebApplicationFactory`, die Accept-Language-Header setzen und Antwort-Inhalte prüfen.

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|---|---|---|
| `DeRequestMitAcceptLanguageDe_ZeigtDeutscheTexte` | `LocalizationTests` | GET-Request mit `Accept-Language: de` liefert deutschen UI-Text in der Response |
| `DeRequestMitAcceptLanguageEn_ZeigtEnglischeTexte` | `LocalizationTests` | GET-Request mit `Accept-Language: en` liefert englischen UI-Text |
| `DeRequestOhneAcceptLanguage_ZeigtEnglischeTexte` | `LocalizationTests` | GET-Request ohne `Accept-Language`-Header fällt auf Englisch zurück |
| `DeRequestMitUnbekannterSprache_ZeigtEnglischeTexte` | `LocalizationTests` | GET-Request mit `Accept-Language: fr` fällt auf Englisch zurück |
| `CreateFakeLocalizer()` | `TestMockFactory` (Hilfsmethode) | Erstellt einen `IStringLocalizer<SharedResources>`, der jeden Schlüssel als Wert zurückgibt |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|---|---|
| `ApplicationContextMenuTests` (alle 5 Tests) | Suchen Buttons per hartcodiertem deutschen Text; nach Lokalisierung müssen `IStringLocalizer<SharedResources>` (FakeLocalizer) registriert und Suchausdrücke auf Schlüssel oder englische Fallback-Werte umgestellt werden |
| `EndpointContextMenuTests.LöschenEintrag_LöstCallbackAus` | Sucht Button per `"Endpunkt löschen"`; analoge Anpassung wie oben |
| `EndpointGroupContextMenuTests` (alle 3 Tests) | Suchen Buttons per hartcodierten deutschen Texten; analoge Anpassung |
| `EnvironmentSelectorTests.RefreshAsync_AktualistertListeOhneFehler` | Geht implizit von genau einer Option aus; nach Lokalisierung des Platzhalter-Texts muss `IStringLocalizer<SharedResources>` registriert werden |
| `MainLayoutTests` (alle Tests, die `AppShell` rendern) | `AppShell` injiziert nach Umstellung `IStringLocalizer<SharedResources>`; bUnit-Kontext muss den FakeLocalizer registrieren |

---

## Offene Punkte

| # | Offener Punkt | Empfohlener Vorschlag |
|---|---|---|
| 1 | **Schlüsselgenauigkeit für DataAnnotations in `CoreResources`:** ASP.NET Core sucht DataAnnotations-Ressourcen-Keys per Konvention `{PropertyName}` (oder `{TypeName}_{PropertyName}`, je nach Konfiguration). Der exakte Schlüsselname, den `AddDataAnnotationsLocalization()` erwartet, muss mit dem tatsächlich generierten Key übereinstimmen. | Bei `AddDataAnnotationsLocalization()` mit zentralem Provider (auf `SharedResources`) wird der Key aus dem Attribut-Typ und der Fehlermeldungsvorlage gebildet. Da kein `ErrorMessage`-Parameter gesetzt ist, verwendet ASP.NET Core die eingebauten englischen Standardtexte — es sei denn, die Ressourcen-Datei enthält Einträge mit den exakten Standard-Keys (z. B. `The field {0} is required.`). Empfehlung: Die Standard-Keys in `SharedResources.de.resx` eintragen, um die eingebauten englischen Meldungen auf Deutsch zu überschreiben. Bei Bedarf können die Keys auch explizit im resx-Kommentar dokumentiert werden. |
| 2 | **Migrationsaufwand Playwright-Tests:** Die Playwright-Tests (`ApplicationCrudTests`, `GroupCrudTests` etc.) prüfen primär Interaktionsflüsse. Es ist nicht ausgeschlossen, dass einzelne Tests per `HasText` oder ähnlichem auf deutschen UI-Texten aufbauen, die nach der Umstellung englisch sind (Browser-Default). | Playwright-Tests nach der Lokalisierungsumstellung einmalig ausführen; bei Fehlern Suchausdrücke auf englische Texte oder `data-testid`-Attribute umstellen. Kein präventiver Umbau erforderlich. |
