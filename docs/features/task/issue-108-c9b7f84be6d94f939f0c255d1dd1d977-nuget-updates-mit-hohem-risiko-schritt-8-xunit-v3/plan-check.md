# Plan-Check – xunit.v3-Migration

**Status: Plan vollständig**

## Prüfung gegen Anforderung

| Anforderung | Abgedeckt in |
|-------------|--------------|
| OutputType Exe + MTP/Test.Sdk-Konfiguration | P1 + Architekturentscheidung |
| IAsyncLifetime → ValueTask (4 Dateien) | P3 |
| Xunit.Abstractions/ITestOutputHelper (22 Dateien) | P4 |
| Trait-Infra-Neubau mit identischer Semantik | P2 + P6 (empirische Verifikation) |
| Coverage-Gate unter neuem Runner | P7 (cobertura-Nachweis) |
| Mechanische Fact/Theory/Collection-Migration | P1+P5 (v3 behält Attributnamen) |
| CLAUDE.md + os-interface-tests.md | P8 |
| Akzeptanz: regulär grün / OsInterface-Selektion / vulnerable clean | P6+P7 |

## Prüfung auf Lücken

- Retry-FlakyTests.ps1-Kommandos: in P6/P8 konditional abgedeckt. ✓
- `dotnet list package --vulnerable`: in P7. ✓
- Lokales OsInterface-Gate (94 grün): in P7. ✓
- Fehlerfall „Fact-Attribut liefert keine Traits": Fallback in P2 benannt. ✓

Testbedarf: Meta-Tests (TraitParityTests, FlakyConventionTests) bleiben bestehen
und sichern die Konvention; kein neuer UI-/E2E-Fluss → keine E2E-Szenarien nötig.
Reale End-to-End-Abnahme = die gefilterten Testläufe selbst (P6/P7).
