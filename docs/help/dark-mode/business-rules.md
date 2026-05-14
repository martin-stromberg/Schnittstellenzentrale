# Dark Mode — Business Rules

## Kein Event bei unverändertem Wert

**Beschreibung:** `OnThemeChanged` wird nicht ausgelöst, wenn `SetTheme()` mit demselben Wert aufgerufen wird, der bereits aktiv ist. Dies verhindert unnötige Neu-Renders aller abonnierenden Komponenten.

**Bedingungen:**
- `SetTheme(scheme)` wird aufgerufen.
- `scheme` ist gleich `CurrentScheme`.

**Verhalten:**
- Wenn `scheme == CurrentScheme`: Methode kehrt sofort zurück; kein `localStorage`-Schreibvorgang, kein `OnThemeChanged`.
- Sonst: `CurrentScheme` wird aktualisiert, Persistierung und Event-Auslösung folgen.

**Umsetzung:** `ThemeService.SetTheme()` — Eingangsvergleich `if (CurrentScheme == scheme) return;`

---

## Standardfallback auf Light

**Beschreibung:** Wenn `localStorage` keinen gespeicherten Theme-Wert enthält oder der gespeicherte Wert keinem gültigen `ColorScheme`-Enum-Wert entspricht, bleibt `ColorScheme.Light` aktiv. Es gibt keinen serverseitigen Konfigurationswert für das Standardschema.

**Bedingungen:**
- `InitializeAsync()` wird aufgerufen.
- `localStorage` enthält keinen Wert für den Schlüssel `colorScheme` oder der Wert ist kein gültiger `ColorScheme`-Name.

**Verhalten:**
- Wenn `stored == null` oder `Enum.TryParse` schlägt fehl: `CurrentScheme` bleibt `ColorScheme.Light`.
- Wenn `stored` ein gültiger Wert ist (z. B. `"Dark"`): `CurrentScheme` wird entsprechend gesetzt.

**Umsetzung:** `ThemeService.InitializeAsync()` — `Enum.TryParse<ColorScheme>(stored, ignoreCase: true, out var parsed)` mit `if (stored != null && ...)` als Schutz.

---

## Frühzeitige Anwendung vor Blazor-Render

**Beschreibung:** Das `data-bs-theme`-Attribut muss am `<html>`-Element gesetzt sein, bevor der Browser die Seite erstmalig rendert. Geschieht dies nicht, erscheint die Seite kurz im falschen Farbschema (Flash of Unstyled Content). Deshalb setzt `theme-init.js` das Attribut synchron im `<head>`, noch bevor Blazor gestartet ist.

**Bedingungen:**
- `App.razor` bindet `theme-init.js` als `<script type="module">` im `<head>` ein.
- Der Browser führt das Modul aus, bevor er das `<body>`-Element rendert.

**Verhalten:**
- `theme-init.js` importiert `theme.js` und ruft sofort `getAndApplyStoredTheme()` auf.
- Das Attribut ist gesetzt, bevor Blazor die erste Komponente rendert.
- `ThemeService.InitializeAsync()` liest `localStorage` erneut, um `CurrentScheme` serverseitig zu synchronisieren; ein erneutes Setzen von `data-bs-theme` durch `applyTheme` erfolgt erst beim nächsten `SetTheme()`-Aufruf.

**Umsetzung:** `theme-init.js` — `import('./theme.js').then(m => m.getAndApplyStoredTheme())`; `App.razor` — `<script type="module" src="theme-init.js">` im `<head>`.

---

## Lazy JS-Modul-Import

**Beschreibung:** Das `theme.js`-ES-Modul wird erst beim ersten tatsächlichen Zugriff importiert und dann gecacht. Dies folgt dem Muster des Blazor JS-Interop mit `IJSObjectReference` und vermeidet unnötige Importe in Szenarien, in denen `ThemeService` zwar instanziiert, aber nicht verwendet wird.

**Umsetzung:** `ThemeService.GetModuleAsync()` — `_module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./theme.js")`
