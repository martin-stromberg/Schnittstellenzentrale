# Offene Aufgaben

Erstellt am: 2026-05-24
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

(keine — Plan-Review ergab Status "Vollständig umgesetzt")

## Code-Review-Befunde

- [ ] `SignalRSyncTests.cs` — Doppelter Code / fehlende Kapselung: `SignalRSyncTests` implementiert `IAsyncLifetime` direkt und dupliziert die gesamte Playwright-Initialisierungs- und Teardown-Logik aus `PlaywrightTestBase`. `PlaywrightTestBase` erweitern (z.B. `CreateAdditionalContextAsync()`-Methode) oder duplizierte Logik in Hilfsklasse auslagern.
- [ ] `ApplicationCrudTests.cs` / `EndpointExecutionTests.cs` — Instabiler Selektor: `GetByText("⚙️")` zum Finden des Kontext-Menü-Toggles ist fragil. Stattdessen `data-testid="context-menu-toggle"` oder `.context-menu-toggle` verwenden.
- [ ] `EndpointExecutionTests.cs` — `int.Parse(statusText ?? "0")` kann bei unerwarteten Inhalten eine kryptische `FormatException` werfen. Stattdessen `int.TryParse` mit expliziter Assertion verwenden: `Assert.True(int.TryParse(statusText?.Trim(), out var statusCode), $"Statuscode konnte nicht geparst werden: '{statusText}'"); Assert.InRange(statusCode, 200, 299);`
