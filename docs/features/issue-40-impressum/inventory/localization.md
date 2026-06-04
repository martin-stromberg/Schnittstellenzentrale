# Lokalisierung – Bestandsaufnahme

## `SharedResources` (EN-Fallback)
Datei: `src/Schnittstellenzentrale/Resources/SharedResources.resx`

### Bereits vorhandene impressum-relevante Schlüssel

| Schlüssel | Wert (EN) | Kommentar |
|-----------|-----------|-----------|
| `WorkspacesSidebar_ImpressumLink` | `Imprint` | Footer-Link zur Impressum-Seite in der `WorkspacesSidebar` |

### Noch fehlende Schlüssel (laut Anforderung)

| Schlüssel | Vorgesehener Zweck |
|-----------|-------------------|
| `ImpressumPage_PageTitle` | Browser-Tab-Titel der Impressum-Seite (`<PageTitle>`) |
| `ImpressumPage_Heading` | `<h1>`-Überschrift auf der Impressum-Seite |

---

## `SharedResources.de` (Deutsch)
Datei: `src/Schnittstellenzentrale/Resources/SharedResources.de.resx`

### Bereits vorhandene impressum-relevante Schlüssel

| Schlüssel | Wert (DE) | Kommentar |
|-----------|-----------|-----------|
| `WorkspacesSidebar_ImpressumLink` | `Impressum` | Footer-Link zur Impressum-Seite in der `WorkspacesSidebar` |

### Noch fehlende Schlüssel

Dieselben wie in der EN-Fallback-Datei: `ImpressumPage_PageTitle`, `ImpressumPage_Heading`.

---

## `SharedResources` Marker-Klasse
Datei: `src/Schnittstellenzentrale/Resources/SharedResources.cs`

```csharp
namespace Schnittstellenzentrale.Resources;
public class SharedResources { }
```

Typanker für `IStringLocalizer<SharedResources>`. In Razor-Komponenten wird der Localizer über `@inject IStringLocalizer<SharedResources> L` eingebunden; der `@using Microsoft.Extensions.Localization`-Eintrag ist global in `_Imports.razor` registriert.
