# Umsetzungsplan – xunit.runner.visualstudio 3.1.5 → 4.0.1

## Scope-Klärung

- **Verarbeitungseinheit:** `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`
- **Auslösendes Ereignis:** Issue #108, Schritt 6 — risikoarmer Zwischenschritt vor
  der xunit-v3-Migration.
- **Erwartete Wirkung:** Test-Runner 4.0.1 aktiv; Filter-Syntax und Coverage-Lane
  funktionieren unverändert.
- **Scope-Ausschlüsse:** Keine — die Anforderung ist explizit auf den
  Versions-Bump und seine Verifikation begrenzt (xunit bleibt 2.9.3, keine
  CI-YAML-Änderungen, keine Trait-Infrastruktur-Änderungen).

## Annahmen (autonomer Lauf, keine Rückfragen möglich)

1. Die in der Anforderung genannte Version `4.0.1` existiert auf NuGet.org und ist
   die gemeinte. Sollte der Restore scheitern, wird der Schritt als Blocker
   gemeldet statt eine andere Version zu wählen.
2. „641-Test-Baseline" bezieht sich auf die reguläre Lane in Debug-Standardkonfiguration
   (`dotnet test` ohne `-c Release`). Abweichungen um ±wenige Tests durch
   datengetriebene Theory-Aufzählung werden bei der Auswertung geprüft; ein
   vollständig grüner Lauf ist das harte Kriterium.
3. Keine `packages.lock.json` → Versionswechsel erfolgt direkt im csproj, kein
   Lockfile-Update nötig (verifiziert: kein Lockfile im Repo).

## Änderung

In `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` (Zeile 33):

```xml
<PackageReference Include="xunit.runner.visualstudio" Version="4.0.1">
```

Alle übrigen Zeilen (inkl. `PrivateAssets`/`IncludeAssets`-Kindelemente) bleiben
unverändert.

## Verifikation (Akzeptanzszenarien)

Reihenfolge nach Restore + Build:

1. **Selektion OsInterface:** `dotnet test --filter "Category=OsInterface" --list-tests`
   — gelistete Testzahl gegen die 25 OsInterface-markierten Klassen plausibilisieren
   (> 0, deckungsfähig mit den markierten Facts/Theories).
2. **Selektion reguläre Lane:** `dotnet test --filter "Category!=OsInterface" --list-tests`
   — Gesamtzahl = OsInterface-Zahl + reguläre Zahl muss die Gesamtmenge ergeben.
3. **Reguläre Lane ausführen:** `dotnet test --filter "Category!=OsInterface"`
   — muss vollständig grün sein (Ziel: 641 bestanden).
4. **Coverage-Lane:** `dotnet test --filter "Category!=OsInterface"
   --collect:"XPlat Code Coverage"` — muss ohne Fehler laufen, Coverage-Datei
   (coverage.cobertura.xml unter TestResults/) muss entstehen.

Hinweis: `SkipPlaywrightInstall=true` wird gesetzt, wo kein Browser-Download
nötig ist (Selektion/Coverage), um Laufzeit zu sparen — die reguläre Lane enthält
keine Playwright-Tests, der Install-Target ist nur Build-Noise.

## Testbedarf

Keine neuen Tests — es handelt sich um eine Infrastruktur-Versionsänderung; die
Akzeptanzkriterien sind selbst die Verifikation. `TraitParityTests` und
`FlakyConventionTests` (reguläre Lane) decken die Markierungskonvention
implizit ab.

## Dokumentation

- `docs/help/stabilitaet/os-interface-tests.md` auf Versionsverweise prüfen
  (keine Änderung erwartet — dokumentiert die Lane, nicht Versionen).
- `docs/RELEASE_NOTES.md`: Eintrag für den Schritt ergänzen, sofern vorherige
  Schritte dort verzeichnet sind.
- README: keine Änderung erwartet (keine Runner-Versionsbezüge).

## Offene Punkte

Keine.
