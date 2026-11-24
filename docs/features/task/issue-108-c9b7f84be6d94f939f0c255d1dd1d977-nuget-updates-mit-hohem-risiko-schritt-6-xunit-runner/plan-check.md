# Plan-Check

**Status: Plan vollständig**

(Hinweis: Selbstprüfung — keine Unteragenten startbar, Abweichung in todo.md
dokumentiert.)

## Prüfung gegen die Anforderung

| Anforderung | Plan-Abdeckung | Befund |
|-------------|----------------|--------|
| Nur `xunit.runner.visualstudio` 3.1.5 → 4.0.1 in Tests.csproj | Änderung exakt auf Zeile 33 begrenzt, `PrivateAssets`/`IncludeAssets` bleiben | OK |
| xunit 2.9.3 unverändert | Plan benennt es explizit als unverändert | OK |
| Filter `--filter "Category!=OsInterface"` grün (641-Baseline) | Verifikationsschritt 3 | OK |
| Filter `--filter "Category=OsInterface"` selektiert korrekt | Verifikationsschritt 1 + Plausibilisierung gegen 25 markierte Klassen | OK |
| Coverage-Collection fehlerfrei | Verifikationsschritt 4 inkl. Datei-Entstehung | OK |
| Kein Issue-Kommentar, kein Merge/Push/PR | Rahmenbedingung, kein Planpunkt nötig | OK |

## Prüfung auf Testbedarf

- Akzeptanzkriterien sind reine Ausführungskriterien (Selektion, grüner Lauf,
  Coverage) — kein neuer Testcode erforderlich. Bestehende
  `TraitParityTests`/`FlakyConventionTests` sichern die Konvention.
- Kein UI-/Benutzerfluss → keine E2E-Abnahme-Szenarien im UI-Sinne; die
  End-to-End-Verifikation ist der reale `dotnet test`-Lauf selbst.

## Konsistenz mit Bestandsaufnahme

- 25 OsInterface-markierte Klassen (24 Dateien, ImpressumPageTests.cs mit zwei
  Klassen) — konsistent.
- Baseline 641 ist als Annahme dokumentiert und wird gegen den Ist-Lauf
  verifiziert — vertretbar, da die Anforderung die Zahl selbst liefert.

## Ergebnis

Der Plan deckt die Anforderung vollständig ab; keine Lücken festgestellt.
