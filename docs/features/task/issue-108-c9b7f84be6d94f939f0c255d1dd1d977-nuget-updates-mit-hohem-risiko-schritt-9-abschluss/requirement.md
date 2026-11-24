# Übersetzte Anforderung – Schritt 9: F7-Prozessentscheidung + F8-Auslagerung

## Ausgangstext (Kundenanforderung, sinngemäß)

Abschließender Dokumentationsschritt des Projekts Issue #108 („NuGet-Updates mit hohem
Risiko"), im Default **ohne Produktcode-Änderung**:

1. **F7 – Prozessentscheidung (Floating `11.0.*` vs. `packages.lock.json`/CPM):**
   Entscheidung mit Begründung in `decisions.md` dokumentieren.
   **Default = Variante (a): Floating beibehalten + regelmäßige Paketprüfung.**
   Begründung: Floating ist bewusst gewählt (Security-Patches fließen implizit,
   dokumentiert im `System.Security.Cryptography.Pkcs`-Kommentar / dessen F6-Nachfolge);
   „Floating + Lock" ist kein sinnvoller Mittelweg; die etablierte Paketprüfungs-Praxis
   (Issue-#107/#108-Kette, wöchentlicher `weekly-nuget-review`-Workflow) trägt das schon.
   Im Default: reine Dokumentation — **keine** `packages.lock.json`, kein
   `Directory.Packages.props` einführen.

2. **F7-Issue-Kommentar:** Kommentar auf Issue #108
   (`martin-stromberg/Schnittstellenzentrale`) per
   `gh issue comment 108 --repo martin-stromberg/Schnittstellenzentrale --body-file …`
   posten, der die F7-Entscheidung knapp dokumentiert (deutsch, mit Bezug auf die
   Entscheidungsoptionen a/b/c).

3. **F8 – Auslagerung:** Separates Issue für npm-`devDependencies` und
   GitHub-Actions-Pins mit eigener Review-Kadenz **anlegen** via
   `gh issue create --repo martin-stromberg/Schnittstellenzentrale` (deutsch; Inhalt:
   Kontext aus Issue #108 Befund 8, Scope npm-devDependencies + Actions-Pins,
   empfohlener eigener Review-Rhythmus, Querverweis #108). Issue-Nummer und -URL in
   `decisions.md` vermerken. Falls Berechtigung fehlt: Titel + Skizze in `decisions.md`
   dokumentieren.

4. **decisions.md-Konsolidierung:** Prüfen, dass F5 (OData-Strategie + Exit-Kriterium),
   F6 (Variante), F7 (Prozess) und F8 (Auslagerung) vollständig mit Begründungen
   dokumentiert sind — fehlende Einträge ergänzen.

5. **Verifikation:** `dotnet test --filter "Category!=OsInterface"` grün
   (unverändert 641 erwartet — Doku-Schritt sollte nichts ändern; bei Abweichung
   Befund dokumentieren).

## Strukturierte Anforderung

### Ziel

Die beiden verbleibenden offenen Entscheidungen des Issue #108 (F7: Update-Prozessstrategie,
F8: Auslagerung der Nicht-NuGet-Punkte) werden getroffen, begründet und nach außen
sichtbar dokumentiert (Projekt-`decisions.md` + GitHub-Issue). Danach ist die
Entscheidungslage F5/F6/F7/F8 vollständig und das Projekt ist abschlussreif.

### Scope

- **Enthalten:**
  - `decisions.md`: neuer Eintrag **F7** (Entscheidung Variante (a) Floating beibehalten
    + regelmäßige Paketprüfung, mit Begründung und betrachteten Alternativen (b)
    Pinnen+Lock+Bot, (c) CPM) und neuer Eintrag **F8** (Auslagerung in separates Issue,
    mit Issue-Nummer/-URL).
  - Konsolidierungsprüfung: F5/F6 bereits vorhanden und vollständig → nur verifizieren,
    nicht erneut schreiben.
  - GitHub: F8-Folge-Issue anlegen; F7-Kommentar auf #108 posten.
  - Verifikationslauf der regulären Testlane.
- **Nicht enthalten (explizit):**
  - Kein Produktcode, keine csproj-/Workflow-Änderung, keine `packages.lock.json`,
    kein `Directory.Packages.props`.
  - Kein Merge, kein Push, kein PR, kein Closing-Keyword (`Closes #108` kommt erst im
    Abschluss-PR des Orchestrators nach dem Merge).
  - `steps.md`-Status „Fertig" wird auf dem Projekt-Basisbranch nach dem Merge gesetzt
    (Konvention aus den Schritten 1–8), nicht auf diesem Schritt-Branch.
  - `RELEASE_NOTES.md`: kein Eintrag (Prozessentscheidung ohne Produktwirkung).

### Betroffene Bereiche

- `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md`
- GitHub Issue #108 (Kommentar) und ein neu anzulegendes Folge-Issue
  (`martin-stromberg/Schnittstellenzentrale`).

### Akzeptanzkriterien

1. `decisions.md` enthält F5/F6/F7/F8 vollständig mit Begründungen.
2. F8-Issue angelegt (Nummer/URL in `decisions.md`) oder begründeter Verzicht dokumentiert.
3. `dotnet test --filter "Category!=OsInterface"` grün (641 erwartet).
4. F7-Kommentar auf #108 gepostet.
5. Kein Produktcode-/Paketmanifest-/Workflow-Diff.

### Randbedingungen

- Deutsch für alle Artefakte und GitHub-Inhalte (Repo-Konvention).
- Commit ohne Closing-Keyword; Format `docs:` (Doku-Schritt) bzw. `plan:` für den
  Planungscommit.
- `gh` ist authentifiziert (Account `softwarewerkstatt`, Repo `martin-stromberg/Schnittstellenzentrale`).
