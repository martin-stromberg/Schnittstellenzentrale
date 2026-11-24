# Umsetzungsplan – Schritt 2: F6 – SignalR.Client ins Web-Projekt + FrameworkReference

Eingaben: `requirement.md`, `inventory.md` (+ `inventory/csproj-diff-plan.md`).

## Scope-Klärung

- **Verarbeitungseinheit:** Zwei csproj-Dateien + eine neue Entscheidungsdatei `decisions.md`. Kein Quellcode.
- **Auslösende Ereignisse:** Build, Testlauf, realer App-Start — keine Laufzeit-/UI-Funktion.
- **Erwartete Wirkung:** `Infrastructure` bezieht ASP.NET-Core-Typen aus dem Shared Framework; `SignalR.Client` liegt beim einzigen Nutzer (Web-Projekt); zwei Legacy-Paketlinien entfallen.
- **Umgang mit noch nicht vorhandenen Objekten:** `decisions.md` wird neu angelegt (im Projektplan bereits als Artefakt vorgesehen).

### Scope-Ausschlüsse

Keine — der Auftrag ist von der Projektleitung mit gewählter Variante (volle FrameworkReference) und verbindlichem Teil (SignalR.Client-Verschiebung) bereits vollständig festgelegt. Nicht-Ziele siehe `requirement.md`.

## Annahmen (dokumentiert, keine Rückfragen möglich)

1. **Variante:** Volle `FrameworkReference`-Variante — durch den Auftrag verbindlich vorgegeben (dokumentierte Annahme der Projektleitung).
2. **Versionslinie SignalR.Client:** bleibt `11.0.*` floating, konsistent zur Baseline aus Schritt 1.
3. **Platzierung FrameworkReference:** eigene `ItemGroup` in `Infrastructure.csproj`, analog zur getrennten `ProjectReference`-ItemGroup — Konvention „Referenz-Arten getrennt".
4. **E2E-Nachweis:** `GET /` → HTTP 200 unter echtem Kestrel-Start gilt laut Auftrag als ausreichend (SignalR-Client-Nutzung ist browserseitig).
5. **docs/help-Impact:** Der Umbau ändert keine Bedienung/Architektur-Sicht für Anwender; wo Hilfedokumente die Paket-/Referenzstruktur konkret benennen, werden sie aktualisiert, sonst entfällt eine Hilfe-Änderung begründet.

## Arbeitsschritte

1. **`src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`:**
   - Neue `ItemGroup` mit `<FrameworkReference Include="Microsoft.AspNetCore.App" />`.
   - PackageReferences `Microsoft.AspNetCore.Http.Abstractions` (2.3.0), `Microsoft.AspNetCore.SignalR.Client` (11.0.*) und `Microsoft.AspNetCore.SignalR.Core` (1.2.10) entfernen.
2. **`src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`:**
   - `<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="11.0.*" />` alphabetisch einsortiert aufnehmen.
3. **`docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md`** neu anlegen: Entscheidung F6 (volle FrameworkReference-Variante) mit Begründung, Alternativen (Minimalvariante) und Konsequenzen.
4. **Verifikation:** `dotnet build` (0 Fehler), `dotnet list package` auf beiden Projekten (Referenzlage belegen), `dotnet test --filter "Category!=OsInterface"` (vollständig grün).
5. **Reale E2E-Abnahme:** `dotnet run` (Kestrel) oder publizierte DLL starten, `GET /` → 200 dokumentieren.
6. **Doku:** `docs/RELEASE_NOTES.md` (Paketstruktur-Bereinigung), README/Hilfedocs nur bei konkretem Bezug.

## Akzeptanzszenarien

| # | Szenario | Nachweis |
|---|----------|----------|
| A1 | `dotnet build` der gesamten Lösung fehlerfrei (inkl. Testprojekt gegen transitive FrameworkReference) | Build-Log: 0 Fehler, 0 neue Warnungen |
| A2 | `dotnet test --filter "Category!=OsInterface"` vollständig grün, inkl. `TopBarTests` und `AppShellTests` | Testzusammenfassung |
| A3 | `dotnet list src/Schnittstellenzentrale.Infrastructure package` enthält weder `Http.Abstractions` noch `SignalR.Core` noch `SignalR.Client` als direkte Referenz | Tool-Ausgabe |
| A4 | `dotnet list src/Schnittstellenzentrale package` enthält `Microsoft.AspNetCore.SignalR.Client` | Tool-Ausgabe |
| A5 | Reale E2E-Abnahme: Kestrel-Start, `GET /` → HTTP 200 | `test-results.md`, Abschnitt „Reale End-to-End-Abnahme" |
| A6 | `decisions.md` existiert und begründet die F6-Entscheidung | Datei |

## Testbedarf

- **Keine neuen Tests** — rein strukturelle Änderung; vorhandene Suite (bunit `TopBarTests`/`AppShellTests`, Integrations- und Service-Tests) ist der Regressionsschirm.
- OsInterface-Lane explizit nicht erforderlich (Auftrag).

## Offene Punkte

Keine.
