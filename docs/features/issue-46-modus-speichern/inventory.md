# Bestandsaufnahme: Modus und Umgebung im localStorage speichern

Analysiert wurden alle Artefakte rund um `StorageModeService`, `IStorageModeService`, `LocalStorageKeys`, `AppShell` und `TopBar`, bezogen auf die Anforderung, den `StorageMode` persistent im `localStorage` des Browsers zu speichern und beim Anwendungsstart wiederherzustellen.

## Zusammenfassung

- `StorageModeService` hält den Modus ausschließlich im Arbeitsspeicher; kein `IJSRuntime`-Zugriff, keine Persistierung, kein `InitializeAsync()`.
- `IStorageModeService` kennt nur `CurrentMode`, `OnModeChanged` und `SetMode()`; `InitializeAsync()` fehlt.
- `LocalStorageKeys` enthält keinen Eintrag für den `storageMode`-Schlüssel.
- `AppShell.OnAfterRenderAsync` ruft bereits `ThemeService.InitializeAsync()` vor `RestoreEnvironmentFromLocalStorageAsync` auf – das korrekte Sequenzierungsmuster ist also vorhanden, muss aber für `StorageModeService.InitializeAsync()` ergänzt werden.
- `TopBar.OnStorageModeChanged` ruft `StorageModeService.SetMode()` synchron auf; keine Persistierungslogik.
- Das vollständige Referenzmuster (lazy JS-Modul, `InitializeAsync`, `SetTheme` mit Persistierung) ist in `ThemeService` / `IThemeService` / `theme.js` implementiert.
- `EnvironmentSelector` schreibt die Umgebungsauswahl bereits direkt per `localStorage.setItem`/`removeItem` ohne eigenes JS-Modul.
- `StorageModeServiceTests` existiert noch nicht; `ThemeServiceTests` deckt alle relevanten Testszenarien des Referenzmusters ab und kann als Vorlage dienen.
- `AppShellTests` (bUnit) existiert noch nicht.
- `StorageModeTests` (Playwright) testet Moduswechsel, aber keine Persistierung über Seitenneuladungen.

## Details

- [Enums und Konstanten](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [Logik](inventory/logic.md)
- [Tests](inventory/tests.md)
