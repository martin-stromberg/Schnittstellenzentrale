# Mehrsprachigkeit DE/EN — Beschreibung

## Zweck

Die Anwendung stellt alle sichtbaren UI-Texte — Labels, Buttons, Überschriften, Validierungsmeldungen
und nutzerseitige Fehlermeldungen — in Deutsch und Englisch bereit. Die Sprachauswahl erfolgt
automatisch, ohne dass der Anwender etwas konfigurieren muss.

## Funktionsweise

Die Sprache wird aus dem HTTP-Header `Accept-Language` ermittelt, den der Browser bei jedem Request
mitsendet. ASP.NET Core wertet diesen Header über die `RequestLocalizationMiddleware` aus
(`AcceptLanguageHeaderRequestCultureProvider`) und setzt `CultureInfo.CurrentUICulture` für den
jeweiligen Request-Thread.

Alle Razor-Komponenten binden Texte über `@L["Schlüssel"]` ein (wobei `L` eine Instanz von
`IStringLocalizer<SharedResources>` ist). Der Localizer sucht zuerst in der Ressourcen-Datei der
erkannten Sprache (`SharedResources.de.resx` bei `de`); fehlt ein Schlüssel dort, fällt er
automatisch auf den englischen Fallback (`SharedResources.resx`) zurück.

DataAnnotations-Validierungsmeldungen (`[Required]`, `[MaxLength]`, `[Range]`) werden ebenfalls
lokalisiert; sie stehen in denselben Ressourcen-Dateien.

Technische Meldungen (`throw new Exception(...)`) und Logging-Ausgaben bleiben immer auf Englisch.

## Beispiele

| Szenario | Ergebnis |
|---|---|
| Browser sendet `Accept-Language: de` | Alle UI-Texte auf Deutsch, Validierungsmeldungen auf Deutsch |
| Browser sendet `Accept-Language: en` | Alle UI-Texte auf Englisch |
| Browser sendet `Accept-Language: fr` | Fallback auf Englisch (Französisch nicht unterstützt) |
| Kein `Accept-Language`-Header | Fallback auf Englisch (Default-Kultur) |
| Schlüssel in `SharedResources.de.resx` fehlt | Englischer Fallback aus `SharedResources.resx` |

## Betroffene Komponenten

Alle Razor-Komponenten unter `Components/Layout/` und `Components/Shared/` nutzen Lokalisierung:

- Layout: `AppShell`, `TopBar`, `WorkspacesLayout`
- Kontext-Menüs: `ApplicationContextMenu`, `ApplicationGroupContextMenu`, `EndpointContextMenu`, `EndpointGroupContextMenu`
- Editoren: `ApplicationEditor`, `ApplicationGroupEditor`, `EnvironmentEditor`, `EnvironmentManagementOverlay`, `EnvironmentsSidebar`
- Dialoge: `ConfirmDeleteApplicationDialog`, `ConfirmDeleteGroupDialog`, `ConfirmDeleteEndpointGroupDialog`, `RenameGroupDialog`, `RenameEndpointGroupDialog`, `CreateEndpointGroupDialog`, `ConcurrencyWarningDialog`, `HealthCheckDialog`, `ImportDialog`
- Seiten/Views: `EndpointPage`, `EmptyContentView`, `EnvironmentContentView`, `ApplicationGroupTree`, `ApplicationCard`, `ContentHeader`, `LinksManager`, `RequestAuthPanel`, `EnvironmentSelector`, `WorkspacesSidebar`

## Einschränkungen

- Nur Deutsch und Englisch werden unterstützt.
- Ein manueller Sprachumschalter in der UI ist nicht vorhanden; ein Sprachwechsel erfordert eine
  Browser-Einstellungsanpassung und anschließenden Seiten-Reload.
- Bei Blazor Server gilt die Kultur des initialen HTTP-Requests für den gesamten SignalR-Circuit;
  eine Kulturänderung während einer laufenden Sitzung wird erst nach einem vollständigen Seiten-Reload
  wirksam.
- `throw`-Meldungen (Exception-Texte) und Serilog-Log-Ausgaben sind immer Englisch und werden nicht
  lokalisiert.
