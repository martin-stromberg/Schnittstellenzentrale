# Planprüfung – Schritt 9: F7-Prozessentscheidung + F8-Auslagerung

**Status: Plan vollständig**

Geprüft: `plan.md` gegen `requirement.md` und `inventory.md` (Eigenregie, frischer
Durchgang — keine Unteragenten startbar, Abweichung in `todo.md` dokumentiert).

## Prüfpunkte

| Kriterium | Ergebnis |
|---|---|
| Alle Anforderungselemente im Plan abgedeckt | ✓ F7-Eintrag in decisions.md (1), F7-Kommentar auf #108 (2), F8-Issue + F8-Eintrag (3), Konsolidierung F5–F8 (4), Verifikationslauf (5) — alle als eigene Umsetzungsschritte verankert |
| Defaults korrekt übernommen | ✓ F7 = Variante (a) Floating + Paketprüfung, keine Lock-/CPM-Artefakte; F8 = Issue anlegen mit Fallback „Titel+Skizze in decisions.md" |
| Reihenfolge der Umsetzung konsistent | ✓ F8-Issue wird vor dem decisions.md-Eintrag angelegt, damit Nummer/URL eingetragen werden kann — Abhängigkeit korrekt modelliert |
| Befund-Fakten aus Inventory stimmen | ✓ 11 Floating-Referenzen `11.0.*`, keine `packages.lock.json`/`Directory.Packages.props`, keine Dependabot-Konfiguration, Major-Tag-Pins `@v5`–`@v7`, npm-devDependencies = semantic-release-Toolchain — alle im Plan referenzierten Fakten verifiziert |
| Testbedarf realistisch | ✓ Reiner Doku-/Prozess-Schritt: reguläre Lane (`Category!=OsInterface`, 641 erwartet) als Unverändertheits-Nachweis ist das richtige Gate; keine E2E-/UI-Szenarien nötig (kein Benutzerfluss, keine UI-/CLI-/Persistenz-Änderung — reale E2E-Abnahme entfällt begründet) |
| Externe Wirkungen vollständig | ✓ Zwei GitHub-Wirkungen (F8-Issue, F7-Kommentar) als verifizierbare Akzeptanzszenarien definiert; `gh`-Auth vorhanden (Account softwarewerkstatt), Fallback-Pfad bei fehlender Berechtigung spezifiziert |
| Scope-Ausschlüsse | ✓ Keine offenen Ausschlüsse; Anforderung schließt Produktcode, Lock/CPM, Merge/Push/PR, Closing-Keyword, steps.md-Status und RELEASE_NOTES explizit aus — alle im Plan übernommen |
| Offene Punkte | ✓ Abschnitt leer — alle Entscheidungen per Anforderungs-Default vorgegeben |
| Akzeptanzszenarien überprüfbar | ✓ 5 Szenarien, jeweils objektiv prüfbar (Dateiinhalt, Issue-Existenz, Kommentar-Existenz, Testzahl, Diff-Umfang) |

## Hinweise (nicht lückenhaft, nur Klarstellungen)

1. **MTP-Kommandoform:** Unter `Microsoft.Testing.Platform` (global.json, Schritt 8)
   ist die dokumentierte Form `dotnet test … -- --filter "Category!=OsInterface"`;
   die Anforderung nennt `dotnet test --filter "Category!=OsInterface"`. Im
   Verifikationsschritt wird zuerst die angeforderte Form ausgeführt; bei
   Argument-Parsing-Problemen die dokumentierte `--`-Form — inhaltlich identisch.
2. **Kommentar-Body-Datei:** in `%TEMP%` ablegen, damit kein Fremdartefakt im Repo
   verbleibt (Feature-Verzeichnis wird ohnehin am Ende entfernt).
3. **decisions.md-Reihenfolge:** Einträge sind chronologisch-numerisch unsortiert
   (F3, F5, F6, F4, F1, F2); Anhängen von F7/F8 am Ende folgt der Bestandskonvention.

## Ergebnis

Plan ist vollständig, konsistent und ohne offene Punkte — Implementierung freigegeben.
