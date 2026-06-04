# UI-Komponenten – Bestandsaufnahme

## `HelpPage`
Datei: `src/Schnittstellenzentrale/Components/Pages/HelpPage.razor`

Referenz-Implementierung für eine einfache Inhaltsseite. Zeigt das Muster für `@page`-Direktive, `IStringLocalizer<SharedResources>`-Injection, `<PageTitle>` und `<h1>` mit lokalisierten Schlüsseln.

```razor
@page "/help"
@inject IStringLocalizer<SharedResources> L

<PageTitle>@L["HelpPage_PageTitle"]</PageTitle>
<div class="sz-help-page">
    <h1>@L["HelpPage_Heading"]</h1>
</div>
```

Die CSS-Klasse `sz-help-page` zeigt das Namensschema für seitenspezifische Stile.

---

## `NavMenu`
Datei: `src/Schnittstellenzentrale/Components/Layout/NavMenu.razor`

Enthält derzeit nur den Home-Link. Kein Impressum-Link vorhanden. Die `@inject IStringLocalizer<SharedResources> L`-Injection ist bereits eingebunden.

---

## `WorkspacesSidebar`
Datei: `src/Schnittstellenzentrale/Components/Shared/WorkspacesSidebar.razor`

Der Footer rendert bereits einen statisch verdrahteten Impressum-Link:

```razor
<div class="sz-sidebar-footer">
    <p class="sz-sidebar-footer-copyright">@_copyrightInfo</p>
    <a href="/impressum" class="sz-sidebar-footer-link">@L["WorkspacesSidebar_ImpressumLink"]</a>
</div>
```

Der Link wird bedingungslos gerendert. Eine Steuerung über `IImpressumService` ist noch nicht vorhanden.

---

## Noch nicht vorhanden

- `ImpressumPage` (`Pages/ImpressumPage.razor`) — die eigentliche Impressum-Seite unter `/impressum`
