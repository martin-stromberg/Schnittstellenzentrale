# Offene Aufgaben

Erstellt am: 2026-05-14
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen (Iteration 2: 3 Befunde, Iteration 3: 4 Befunde)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan-Review ergab Status "Vollständig umgesetzt".

## Code-Review-Befunde

### theme.js

- **Doppelter Code** — Die Funktion `getAndApplyStoredTheme` dupliziert die Logik von `getStoredTheme` und `applyTheme`. Empfehlung: Umschreiben auf `const stored = getStoredTheme(); applyTheme(stored ?? 'light'); return stored;`

### ThemeService.cs

- **Fehlende Validierung** — `SetTheme` prüft nicht, ob der übergebene `ColorScheme`-Wert gültig ist. Empfehlung: Guard ergänzen: `if (!Enum.IsDefined(typeof(ColorScheme), scheme)) throw new ArgumentOutOfRangeException(nameof(scheme));`

### ThemeServiceTests.cs

- **Fehlende Testabdeckung: ungültiger gespeicherter Wert** — Der False-Pfad von `Enum.TryParse` in `InitializeAsync` ist nicht getestet. Test `InitialTheme_IsLight_WhenStoredValueIsInvalid` ergänzen.

- **Fehlende Testabdeckung: Lazy-Init** — Das einmalige Importieren des JS-Moduls via `GetModuleAsync` ist nicht getestet. Test ergänzen, der `InitializeAsync` zweimal aufruft und sicherstellt, dass `import` nur einmal ausgeführt wird.

### MainLayout.razor

- **Toter Code** — Feld `_themeInitialized` ist redundant (Blazor garantiert `firstRender == true` nur einmal). Feld und Bedingung entfernen, vereinfachen auf `if (firstRender)`.
