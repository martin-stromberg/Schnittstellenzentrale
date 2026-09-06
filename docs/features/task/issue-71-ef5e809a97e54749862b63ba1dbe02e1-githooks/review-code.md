# Code-Review – GitHooks

Review der uncommitted Änderungen (Subagent + Nachprüfung).

## Befunde und Bewertung

1. ~~`TopBarTests` vs. `CreateFakeLocalizer` (Rückgabe der Schlüssel statt Werte)~~
   — **Kein Befund**: `Contains("Workspaces")` matcht auf den Schlüssel
   `TopBar_TabWorkspaces` (Teilstring). Testlauf bestätigt: 565/565 grün.
   Der Selektor `button[title="TopBar_Tooltip_ActivityLog"]` ist bewusst
   auf den Fake-Localizer ausgelegt.

2. `<param name="Endpoints">` in `SwaggerOperationHelper.cs` (Tupel-Element)
   — **Bewusst belassen**: Der `csproj-xmldoc-check` verlangt das Tag für
   benannte Tupel-Elemente; Entfernen ließe den Hook wieder fehlschlagen.
   Erzeugt keine Warnung (Build geprüft). Gleiches gilt für `Factory` in
   `TestHelpers.cs` (dort 2× CS1572-Warnung — `NoWarn` ist durch den Hook
   verboten, siehe review.md).

3. ~~Veralteter Kommentar `MainLayout._activityLogPanelHeight` in
   `ActivityLogPanel.razor`~~ — **Behoben** (→ `AppShell`).

4. ~~Tippfehler `Endpunkt schlüssel`~~ — bereits als `Endpunktschlüssel`
   korrigiert vorhanden (Nachprüfung zeigt korrekte Schreibweise).

5. `RequestBodyPanel.razor`: einzelne hartkodierte einwortige UI-Strings
   (`Body-Format:`, `Formatieren`) — **nicht geändert**: pre-existing,
   wird vom `razor-l10n-check` (bewusst) nicht bemängelt; außerhalb des
   Anforderungsumfangs.

6. Leerer `OnEnvironmentSelectedByUser` in `TopBar.razor` — pre-existing
   Erweiterungspunkt, unverändert.

## Geprüft ohne Befund

- `.githooks/`: keine Abschwächung; Skripte entsprechen dem Original.
- Event-Abmeldungen in `AppShell`/`TopBar`/`ActivityLogPanel` vollständig.
- resx-Schlüssel `TopBar_Tooltip_ActivityLog` in EN+DE mit Kommentar;
  `NavMenu_ToggleTitle` entfernt; Pakete konsistent.
- Keine Restreferenzen auf `MainLayout`/`NavMenu` (resx, CSS, Tests).
- `ThrowingApplicationRepository`: nur `GetSystemGroupAsync` wirft
  (faulted Task statt sync throw — beim `await` semantisch identisch);
  alle anderen Member echt implementiert.
- Neue Tests folgen bUnit-/xUnit-Konventionen inkl. XML-Doku.
- Keine `RaiseUiActionRequested`-Aktionen im Projekt vorhanden
  (Regel aus SKILL.md nicht anwendbar).

## Status: Keine Befunde
