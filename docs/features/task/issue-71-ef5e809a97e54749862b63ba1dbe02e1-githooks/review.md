# Plan-Review – GitHooks

Geprüft: Umsetzung gegen `plan.md` und `inventory.md`.

## Prüfung der Arbeitspakete

| WP | Geplant | Umgesetzt |
|----|---------|-----------|
| WP1 | `.githooks/` übernehmen, `core.hooksPath` setzen | ✅ 10 Dateien aus Pattern-Collection `9683e0c` unverändert kopiert; `git config core.hooksPath` = `.githooks`; keine Altversionen vorhanden |
| WP2 | Core.csproj: GenerateDocumentationFile + CS1591 | ✅ gesetzt; Build grün |
| WP3 | XML-Doku + pragma-Entfernung | ✅ `csproj-xmldoc-check.py --all` sauber (275 Dateien); 6 pragma-Zeilen in 5 Dateien entfernt, betroffene Dateien vollständig dokumentiert |
| WP4 | Verwaiste Razor-Komponenten | ✅ Error.razor BOM entfernt; Routes.razor `typeof(AppShell)` + `_Imports.razor`-Using; MainLayout/NavMenu samt CSS und resx-Key `NavMenu_ToggleTitle` entfernt; ActivityLogPanel über TopBar-Button + AppShell-State wieder eingebunden inkl. localStorage-Persistenz |
| WP5 | Throw-Stubs im Testprojekt | ✅ `ThrowingApplicationRepository` vollständig implementiert; `no-notimplemented-check --all --strict` sauber |
| WP6 | Enum-Testabdeckung | ✅ Alle fehlenden Werte über Verhaltenstests abgedeckt; `enum-coverage-check --all --strict` sauber |
| WP7 | Verifikation | ✅ Alle 6 Checks Exit 0; Build 0 Fehler; 629 Tests grün |

## Abweichungen vom Plan

- **Program.cs**: Zusätzliche Korrektur — im Playwright-Environment wurde
  `UseStaticFiles` ohne `UseStaticWebAssets` verwendet, sodass
  `_framework/blazor.web.js` und `Schnittstellenzentrale.styles.css` mit
  404 scheiterten und die Blazor-Circuit nie startete (alle klickbasierten
  E2E-Tests waren dadurch bereits vorher rot). Fix:
  `StaticWebAssetsLoader.UseStaticWebAssets` im Playwright-Environment
  laden. Dadurch sind nun auch die vorbestehenden, fehlschlagenden
  E2E-Tests grün.
- Tests für WP4: bUnit-Test `TopBar_ToggleActivityLog_ZeigtActivityLogPanel`
  und Playwright-Test `ActivityLog_KlickAufToggleButton_ZeigtPanel`
  (mit Retry-Klick, da Klicks vor Circuit-Aufbau verloren gehen) —
  beide grün.
- `MainLayoutTests.cs` wurde in `TopBarTests.cs` umbenannt (testete bereits
  AppShell/TopBar, nicht MainLayout).

## Bekannte Warnungen

- 2× CS1572 in `TestHelpers.cs`: Der Hook verlangt `<param name="Factory">`
  für das benannte Rückgabetupel, der Compiler meldet es als Warnung.
  Da `NoWarn`/`WarningsNotAsErrors` für XML-Doc-Codes durch den Hook
  verboten sind, bleiben die Warnungen bestehen (kein Build-Fehler).

## Offene Aufgaben

Keine.

## Status: Vollständig umgesetzt
