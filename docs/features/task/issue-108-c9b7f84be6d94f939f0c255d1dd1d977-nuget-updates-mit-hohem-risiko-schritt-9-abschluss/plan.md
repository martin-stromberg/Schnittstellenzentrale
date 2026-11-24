# Umsetzungsplan – Schritt 9: F7-Prozessentscheidung + F8-Auslagerung

## Scope-Klärung

- **Verarbeitungseinheit:** `decisions.md` (Projektartefakte) + GitHub Issue #108
  (Kommentar) + ein neues Folge-Issue. Reiner Doku-/Prozessschritt.
- **Auslösendes Ereignis:** Projekt-Schritt 9 (letzter Schritt vor Abschluss-PR).
- **Erwartete Wirkung:** F5/F6/F7/F8 vollständig und begründet in `decisions.md`;
  F8-Folge-Issue existiert auf GitHub; F7-Entscheidung ist auf #108 kommentiert;
  reguläre Testlane bleibt grün (641).
- **Scope-Ausschlüsse:** Keine — die Anforderung definiert den Scope abschließend
  (kein Produktcode, kein Lock/CPM-Artefakt, kein Merge/Push/PR, kein Closing-Keyword,
  kein `steps.md`-Statuswechsel, kein `RELEASE_NOTES.md`-Eintrag).

## Umsetzungsschritte

### 1. F8-Folge-Issue anlegen (vor decisions.md, damit Nummer/URL eingetragen werden kann)

`gh issue create --repo martin-stromberg/Schnittstellenzentrale` mit deutschem Titel
und Body:

- **Titel (Entwurf):** „Nicht-NuGet-Abhängigkeiten: npm-devDependencies + GitHub-Actions-Pins — eigener Review-Rhythmus etablieren (aus #108, Befund 8)"
- **Body-Inhalte:**
  - Kontext: Issue #108, Befund 8 — npm-`devDependencies` der Release-Automatisierung
    (`package.json`, 5 Pakete: semantic-release-Toolchain, Caret-Ranges) und
    GitHub-Actions-Pins (11 Workflows + 3 Composite Actions, Major-Tag-Pins `@v5`–`@v7`,
    keine SHA-Pins, keine Dependabot-/Renovate-Konfiguration) wurden im NuGet-Review
    nicht geprüft/geändert.
  - Scope: periodische Prüfung beider Bereiche (Updates, Security, Deprecated,
    Breaking Changes).
  - Empfehlung: eigener Review-Rhythmus (z. B. monatlich oder quartalsweise — analog
    zum bestehenden `monthly-architecture-security-review`-Muster bzw. wöchentlichen
    NuGet-Review), ggf. später via Workflow automatisiert; erstmal manueller
    Prüf-Anlass.
  - Querverweis: „Ausgelagert aus #108 (Befund 8)."
- Issue-Nummer und -URL aus der Ausgabe ermitteln.
- **Fallback bei fehlender Berechtigung:** Titel + Body-Skizze stattdessen vollständig
  in `decisions.md` dokumentieren (F8-Eintrag mit Vermerk „Issue-Anlage nicht möglich").

### 2. `decisions.md` ergänzen

Datei: `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md`

Zwei neue Einträge am Dokumentende (Konvention: jeder Schritt hängt an):

**F7 – Update-Prozessstrategie: Floating `11.0.*` beibehalten + regelmäßige Paketprüfung (Schritt 9)**

- Status: Entschieden (Schritt-Branch `…-schritt-9-abschluss`) — reine
  Prozessentscheidung, keine Code-/Manifeständerung.
- Entscheidung: Variante (a) — Floating `11.0.*` beibehalten + etablierte regelmäßige
  Paketprüfung. **Keine** `packages.lock.json`, **kein** `Directory.Packages.props`.
- Begründung (aus der Anforderung + Inventory-Fakten):
  - Floating ist bewusst gewählt: Security-Patches sollen implizit einfließen —
    ursprünglich im `System.Security.Cryptography.Pkcs`-Kommentar begründet
    (GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7); seit F6 kommt Pkcs aus der Runtime,
    die Strategie gilt unverändert für die 11 verbliebenen `11.0.*`-Referenzen.
  - „Floating + Lock" ist kein sinnvoller Mittelweg: Bei Floating-Versionen löst die
    Lock-Datei bei jedem Restore neu auf — keine Reproduzierbarkeit, nur Overhead.
  - Die Paketprüfungs-Praxis ist bereits etabliert und automatisiert:
    `weekly-nuget-review.yml` (wöchentliches Review-Issue, Duplikat-Schutz) — die
    Issue-#107/#108-Kette ist deren konkrete Ausprägung.
- Betrachtete Alternativen (verworfen):
  - (b) Pinnen + `packages.lock.json` Locked-Mode + Bot-Updates: Reproduzierbarkeit
    erkauft mit neuem Automatisierungs-/Review-Aufwand (Dependabot/Renovate-Setup);
    konterkariert die bewusste Security-Patch-Implizitheit; außerhalb Issue-Scope.
  - (c) CPM via `Directory.Packages.props`: zentrale Versionswahrheit ohne
    Reproduzierbarkeits-Gewinn bei nur 4 Projekten und überschaubarer Referenzmenge;
    als „Zwischenschritt" ohne Zielentscheidung Mehraufwand ohne Gegenwert.
  - „Floating + Lock": kein sinnvoller Mittelweg (s. o.).
- Konsequenzen:
  - 11 Floating-Referenzen `11.0.*` bleiben; Restore löst auf neueste 11.0.x-Patches auf.
  - Update-Disziplin läuft weiter über die wöchentliche Paketprüfung; Major-/Risiko-
    Updates werden wie in #108 gebündelt und schrittweise entschieden.
  - Re-Evaluation der Entscheidung bei verändertem Risikobild (z. B. Reproduzierbarkeits-
    anforderung durch Audit/Compliance) ist zulässig und billig.

**F8 – Auslagerung: npm-devDependencies + GitHub-Actions-Pins (Schritt 9)**

- Status: Entschieden und umgesetzt — Folge-Issue **#<N>**
  (`https://github.com/martin-stromberg/Schnittstellenzentrale/issues/<N>`) angelegt.
- Entscheidung: Beide Bereiche werden **nicht** im NuGet-Scope von #108 bearbeitet,
  sondern in einem separaten Issue mit eigener Review-Kadenz.
- Begründung: außerhalb NuGet-Scope (Issue definiert sich als NuGet-Paketprüfung);
  eigener Rhythmus verhindert Scope-Creep; anderer Technologie-/Werkzeugkreis
  (npm, Actions-Marketplace).
- Konsequenzen: #108 ist mit diesem Schritt entscheidungsseitig abgeschlossen;
  Umsetzung des Folge-Issues ist eigenständig.

### 3. F7-Kommentar auf Issue #108 posten

`gh issue comment 108 --repo martin-stromberg/Schnittstellenzentrale --body-file <datei>`
— deutscher Kommentar, knapp:

- Entscheidung Variante (a) Floating beibehalten + regelmäßige Paketprüfung.
- Bezug auf die im Issue genannten Optionen (a) Floating, (b) Pinnen+Lock+Bot,
  (c) CPM — mit Kurzbegründung je verworfener Option.
- Verweis auf `decisions.md` (F7-Eintrag) und auf das F8-Folge-Issue #<N>.
- Body-Datei temporär außerhalb des Repos oder unter dem Feature-Verzeichnis ablegen
  (wird vor dem Feature-Verzeichnis-Löschen mit entfernt; ggf. in `%TEMP%`).

### 4. Konsolidierungsprüfung decisions.md

Checkliste (erfüllt = Häkchen in `test-results.md`/Review dokumentieren):

- F5: OData-Strategie (preview.2 verharren) **+ Exit-Kriterium** vorhanden → bereits
  vorhanden (Zeilen 35–61), nur verifizieren.
- F6: Variante (volle FrameworkReference + SignalR.Client-Verschiebung) → vorhanden.
- F7: neu eingetragen (Schritt 2).
- F8: neu eingetragen inkl. Issue-Nummer/URL (Schritt 2).

### 5. Verifikation

- `git status`/Diff-Kontrolle: nur `decisions.md` + Feature-Artefakte geändert —
  kein csproj/Workflow/Produktcode-Diff, keine `packages.lock.json`/
  `Directory.Packages.props` entstanden.
- `dotnet test --filter "Category!=OsInterface"` → 641 grün erwartet
  (MTP-Modus via `global.json`; ggf. `--`-Form wie in Schritt 8 dokumentiert).

## Offene Punkte

Keine — alle Defaults sind durch die Anforderung verbindlich vorgegeben (Variante (a)
für F7, Issue-Anlage für F8).

## Akzeptanzszenarien

1. `decisions.md` enthält F5/F6/F7/F8 vollständig mit Begründungen (F7/F8 neu).
2. F8-Folge-Issue existiert auf GitHub; Nummer/URL stehen im F8-Eintrag.
3. Auf Issue #108 existiert ein neuer Kommentar, der die F7-Entscheidung mit
   Optionsbezug dokumentiert.
4. `dotnet test --filter "Category!=OsInterface"` → 641 grün.
5. `git diff` zeigt außer `decisions.md` + Lifecycle-Artefakten keine Änderung.
