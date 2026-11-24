# Plan-Review – Schritt 9: F7-Prozessentscheidung + F8-Auslagerung

**Status: Vollständig umgesetzt**

Geprüft: Umsetzung gegen `plan.md` und `inventory.md` (Eigenregie, frischer Durchgang —
keine Unteragenten startbar, Abweichung in `todo.md` dokumentiert).

## Prüfung der Umsetzungsschritte

| Plan-Schritt | Umgesetzt? | Nachweis |
|---|---|---|
| 1. F8-Folge-Issue anlegen | ✓ | `gh issue create` erfolgreich → **Issue #110** (`https://github.com/martin-stromberg/Schnittstellenzentrale/issues/110`), deutsch, enthält Kontext (Befund 8/#108), Scope (5 npm-devDependencies mit Versionen, Actions-Pins `@v5`–`@v7`, keine SHA-Pins, keine Bot-Konfig), empfohlene eigene Kadenz (monatlich), Querverweis #108 |
| 2. decisions.md: F7-Eintrag | ✓ | `decisions.md` Zeilen 159–181: Status, Entscheidung Variante (a), 3-teilige Begründung (bewusstes Floating inkl. Pkcs-Nachfolge via F6, Floating+Lock-Mittelweg-Widerlegung, etablierte Prüfpraxis `weekly-nuget-review`), verworfene Alternativen (b)/(c)/Kombination, Konsequenzen |
| 2. decisions.md: F8-Eintrag | ✓ | Zeilen 183–203: Status, Entscheidung mit Issue-Nummer/URL (#110), Scope des Folge-Issues, Begründung, Konsequenzen (inkl. sauberer Zuständigkeitsabgrenzung Dependabot-Frage → #110 statt F7) |
| 3. F7-Kommentar auf #108 | ✓ | `gh issue comment 108` erfolgreich → `…/issues/108#issuecomment-7123076000`; deutsch, nennt Variante (a), alle drei Optionen mit Kurzbegründung, verweist auf decisions.md und #110 |
| 4. Konsolidierung F5–F8 | ✓ | F5 (Zeilen 35–61: preview.2-Strategie + Exit-Kriterium „stabiler 10.x-/11.x-Release") vorhanden; F6 (Zeilen 63–83: volle FrameworkReference-Variante + SignalR.Client-Verschiebung) vorhanden; F7/F8 neu vollständig — alle vier Entscheidungen begründet dokumentiert |
| 5. Diff-Kontrolle | ✓ | `git status`/`git diff --stat`: ausschließlich `decisions.md` (+46 Zeilen); keine csproj-/Workflow-/Produktcode-Änderung; keine `packages.lock.json`/`Directory.Packages.props` entstanden |

## Prüfung der Akzeptanzszenarien

1. decisions.md F5/F6/F7/F8 vollständig mit Begründungen — ✓
2. F8-Folge-Issue existiert (#110), Nummer/URL im F8-Eintrag — ✓
3. F7-Kommentar auf #108 existiert mit Optionsbezug — ✓
4. Testverifikation — ausstehend (Schritt 10, siehe `test-results.md`)
5. Kein Diff außer decisions.md + Lifecycle-Artefakte — ✓

## Offene Aufgaben

Keine.
