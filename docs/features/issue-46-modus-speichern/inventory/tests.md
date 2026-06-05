# Tests

## Testklassen

### `ThemeServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ThemeServiceTests.cs`

Dient als direktes Referenzmuster für die zu erstellenden `StorageModeServiceTests`.

- `InitialTheme_IsLight_WhenNoStoredPreference` — `InitializeAsync` behält Standardwert, wenn kein gespeicherter Wert vorhanden
- `InitialTheme_IsStoredValue_WhenPreferenceExists` — `InitializeAsync` setzt `CurrentScheme` auf gespeicherten Wert
- `SetTheme_FiresOnThemeChanged` — `SetTheme` löst `OnThemeChanged` aus
- `SetTheme_DoesNotFire_WhenValueUnchanged` — `SetTheme` feuert Event nicht bei gleichem Wert
- `InitialTheme_IsLight_WhenStoredValueIsInvalid` — `InitializeAsync` behält Standardwert bei ungültigem gespeicherten Wert
- `InitializeAsync_ImportsModuleOnlyOnce_WhenCalledTwice` — JS-Modul wird beim zweiten `InitializeAsync`-Aufruf nicht erneut importiert
- `SetTheme_PersistsValueToLocalStorage` — `SetTheme` schreibt Wert per `setStoredTheme` in `localStorage`
- `SetTheme_AppliesThemeToDocument` — `SetTheme` wendet Theme per `applyTheme` auf das Dokument an
- `SetTheme_ThrowsArgumentOutOfRangeException_WhenSchemeIsUndefined` — `SetTheme` wirft bei undefiniertem Enum-Wert

### `LocalStorageKeysTests`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/LocalStorageKeysTests.cs`

- `SelectedEnvironmentId_TeamModus_GibtKorrektesFormat` — Schlüssel für `StorageMode.Team` lautet `"selectedEnvironmentId_Team"`
- `SelectedEnvironmentId_UserModus_GibtKorrektesFormat` — Schlüssel für `StorageMode.User` lautet `"selectedEnvironmentId_User"`

Ein Test für einen neuen `StorageMode`-Konstanteneintrag existiert noch **nicht**.

### `StorageModeTests` (Playwright)
Datei: `src/Schnittstellenzentrale.Tests/Playwright/StorageModeTests.cs`

- `SwitchToTeamMode_ShowsTeamData` — Nach Wechsel auf Team-Modus ist `.sz-tree-body` sichtbar
- `SwitchBackToUserMode_ShowsUserData` — Nach Rückwechsel auf User-Modus ist `.sz-tree-body` sichtbar

Diese Tests prüfen den Moduswechsel im laufenden Browser, aber **nicht** die Persistierung über Seitenneuladungen.

### `EnvironmentSelectorTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/EnvironmentSelectorTests.cs`

Bunit-Tests für `EnvironmentSelector`; mocken `IStorageModeService` (wird als Abhängigkeit injiziert).

- `RendertUmgebungenAusRepository` — Umgebungen aus Repository erscheinen als `<option>`-Elemente
- `AktiveUmgebungWirdVorausgewählt` — Aktive Umgebung wird im `<select>` vorausgewählt
- `OhneAktiveUmgebung_ZeigtKeineVorauswahl` — Ohne aktive Umgebung kein vorgewählter Wert
- `RefreshAsync_AktualistertListeOhneFehler` — `RefreshAsync` aktualisiert die Liste ohne Dispatcher-Fehler
- `AuswählenEinerUmgebung_SchreibtLocalStorage` — Auswahl schreibt Schlüssel und ID per `localStorage.setItem`
- `AbwählenEinerUmgebung_EntferntLocalStorage` — Abwahl ruft `localStorage.removeItem` mit korrektem Schlüssel auf
- `AuswählenNichtExistierenderId_EntferntLocalStorageUndSetztNull` — Ungültige ID: `removeItem` + `SetActiveEnvironment(null)`

## Hilfsmethoden

### `TestMockFactory`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestMockFactory.cs`

- `CreateActivityLogServiceMock()` — Erstellt leeren `IActivityLogService`-Mock
- `CreateEnv(int id, string name)` — Erstellt `SystemEnvironment`-Testinstanz (Mode `StorageMode.Team`, leere Variables)
- `CreateFakeLocalizer()` — Erstellt `IStringLocalizer<SharedResources>`-Mock, der Schlüssel unverändert zurückgibt

Ein `StorageModeServiceTests` mit gemocktem `IJSRuntime` existiert noch **nicht**.
