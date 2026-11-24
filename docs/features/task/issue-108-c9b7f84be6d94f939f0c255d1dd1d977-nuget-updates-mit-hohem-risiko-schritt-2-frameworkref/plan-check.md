# Plan-Check – Schritt 2: F6 – SignalR.Client + FrameworkReference

**Status: Plan vollständig**

## Prüfung Anforderung ↔ Plan

| Anforderung (Akzeptanzkriterium) | Im Plan abgedeckt? |
|----------------------------------|--------------------|
| `dotnet build` fehlerfrei | ✅ A1, Arbeitsschritt 4 |
| `dotnet test --filter "Category!=OsInterface"` grün inkl. `TopBarTests` | ✅ A2, Arbeitsschritt 4 |
| `SignalR.Client` im Web-csproj, nicht in Infrastructure | ✅ Arbeitsschritte 1+2, A3/A4 |
| Keine `Http.Abstractions`-/`SignalR.Core`-PackageReference in Infrastructure | ✅ Arbeitsschritt 1, A3 |
| Testprojekt baut/läuft gegen transitive FrameworkReference | ✅ A1/A2 (bunit-Tests als Verifikation) |
| Entscheidung mit Begründung in `decisions.md` | ✅ Arbeitsschritt 3, A6 |
| csproj-Konventionen nicht aushebeln | ✅ Plan nennt „unverändert"-Liste; Implementierung prüft Diff |
| Reale E2E-Abnahme (Kestrel, GET / → 200) | ✅ A5, Arbeitsschritt 5 |
| Keine OsInterface-Lane nötig | ✅ Testbedarf vermerkt |
| Kein Merge/Push/PR, Abschlusscommit `feat:` | ✅ Lifecycle-Endzustand |

## Konsistenzprüfung Plan ↔ Inventory

- Die drei zu entfernenden PackageReferences stimmen mit den verifizierten Zeilen 10–12 in `Infrastructure.csproj` überein (Volltextsuche bestätigt: nur dort referenziert).
- Code-Nutzung ist belegt: `Microsoft.AspNetCore.SignalR` (nur `SignalRNotificationService.cs`), `Microsoft.AspNetCore.Http` (nur `WindowsCurrentUserService.cs`) — beides vollständig im Shared Framework.
- `SignalR.Client`-Nutzung nur im Web-Projekt (`AppShell.razor`, `ApplicationGroupTree.razor`); Testprojekt nutzt das Paket nicht direkt, erhält es transitiv über die Web-`ProjectReference` → Verschiebung bricht keinen Test-Compile.
- FrameworkReference-Transitivität: `Microsoft.AspNetCore.App` fließt über `ProjectReference` an referenzierende Projekte weiter; das Testprojekt erhält zusätzlich bereits implizit die ASP.NET-Core-Referenz über das Web-SDK-Projekt — bunit-Verträglichkeit ist plausibel, wird aber dennoch per Build+Test verifiziert (A1/A2).

## Lücken-/Risikobewertung

- **Alphabetische Einordnung SignalR.Client im Web-csproj:** Plan nennt die Sortierung; exakte Position (nach `OData`, vor `EntityFrameworkCore.Design`) in `inventory/csproj-diff-plan.md` konkretisiert — konsistent.
- **NU1603-/Floating-Auflösung `11.0.*`:** Paket ist seit Schritt 1 bereits in dieser Linien aufgelöst — kein neues Risiko.
- **Keine E2E-Screenshots nötig:** keine UI-Änderung; Sichtprüfung entfällt begründet (reine Referenzstruktur). Im Testschritt zu vermerken.
- Minor: `decisions.md` soll die verworfene Alternative (Minimalvariante) und Konsequenzen (z. B. SignalR-Version an Laufzeit gebunden, kein Floating mehr nötig) benennen — im Arbeitsschritt 3 gefordert.

## Fazit

Der Plan deckt alle Akzeptanzkriterien, ist gegen die Bestandsaufnahme verifiziert und enthält überprüfbare Akzeptanzszenarien inkl. realer End-to-End-Abnahme. **Plan vollständig — Implementierung freigegeben.**
