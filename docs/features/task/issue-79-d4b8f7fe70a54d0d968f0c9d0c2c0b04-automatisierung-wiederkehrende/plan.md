# Umsetzungsplan: Automatisierung wiederkehrender Architektur-, Sicherheits- und Compliance-Reviews

## Übersicht

Es werden drei eigenständige, zeitgesteuerte GitHub-Actions-Workflows unter `.github/workflows/` eingeführt, die automatisiert GitHub-Issues mit `@softwarewerkstatt`-Erwähnung und Label-basierter Duplikat-Vermeidung erzeugen (monatliches Architektur-/Sicherheitsreview, wöchentliche NuGet-Prüfung, jährliches Compliance-/Copyright-Review). Die gemeinsame Logik (Label sicherstellen, Duplikat prüfen, Issue anlegen) wird in einer neuen Composite Action unter `.github/actions/` zentralisiert. Ergänzend wird ein neues Hilfethema unter `docs/help/` angelegt, in `docs/help/index.md` verlinkt und in `README.md` kurz referenziert. C#-Artefakte, Datenbank und bestehende Workflows bleiben unverändert.

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| Duplikat-Erkennung | Dediziertes Label pro Prüfungstyp (`automated-architecture-review`, `automated-nuget-review`, `automated-compliance-review`); Prüfung per `gh issue list --label <label> --state open --json number` | Entspricht exakt dem etablierten Muster aus `sync-staging-with-main.yml`/`staging-to-main-promotion.yml` (`gh label create --force` + `gh pr list --state open` vor dem Anlegen). Robuster als Titelvergleich, da die Titel ein Perioden-Suffix (Monat/KW/Jahr) tragen und sich pro Zyklus ändern. |
| Issue-Erzeugungs-Logik | Wiederverwendbare Composite Action `.github/actions/create-review-issue/action.yml` (Transaction-Script-artiger Ablauf: Label sicherstellen → Duplikat prüfen → Issue anlegen), die von allen drei Workflows aufgerufen wird | Repo-Konvention zentralisiert mehrfach genutzte CI-Logik in Composite Actions (`security-scan` wird an drei Stellen verwendet, Beschreibung: „so this logic only exists once"). Die Duplikat-/Erzeugungs-Logik ist bei allen drei Workflows identisch und soll genau einmal existieren und verifiziert werden. |
| Ablage der Issue-Texte | Inline im jeweiligen Workflow als YAML-Blockskalar (`with: issue-body: \|`), nicht als pwsh-Here-String und nicht als separate Markdown-Template-Dateien | Die Texte sind statisch, aus Issue #79 wörtlich vorgegeben und je Workflow einmalig. Als `with:`-Input bleiben sie direkt am Ort der Verwendung lesbar und versioniert; separate Templates würden zusätzlich `actions/checkout` erfordern, ohne Mehrwert zu bieten. Die Übergabe an `gh` erfolgt in der Composite Action über eine `env`-Variable (`ISSUE_BODY`), nicht über String-Interpolation — damit entfallen Quoting-/Encoding-Risiken der langen, mehrzeiligen deutschen Texte. |
| Workflow-Struktur | Drei separate Workflow-Dateien statt eines parametrisierten Workflows | Akzeptanzkriterium „Für jede Prüfung existiert eine separate GitHub Action"; außerdem unterschiedliche Cron-Pläne und Labels — eine Matrix/Parametrisierung würde die Zuordnung verwässern. |
| Issue-Titel | Deutsche Titel mit festem Präfix `[Review]` und kulturunabhängigem Perioden-Suffix: `[Review] Architektur- und Sicherheitsreview <yyyy-MM>`, `[Review] NuGet-Paketprüfung <yyyy-KWww>` (ISO-Woche), `[Review] Compliance- und Copyright-Review <yyyy>` | Erfüllt das Akzeptanzkriterium „aussagekräftige Titel" und macht den Zyklus sofort erkennbar. Numerische Formate (`Get-Date -Format` / `ISOWeek`) vermeiden die Kulturabhängigkeit von Monatsnamen auf `ubuntu-latest`-Runnern. Das Suffix beeinflusst die Duplikat-Erkennung nicht, da diese ausschließlich labelbasiert ist. |
| Titel-Suffix-Erzeugung | In der Composite Action über einen `cadence`-Input (`monthly`/`weekly`/`yearly`), der das Suffix-Format auswählt | `with:`-Inputs sind statisch — ein datumsabhängiger Titel kann nicht im Workflow vorberechnet werden, ohne einen zusätzlichen Output-Schritt einzuführen. Ein `switch` auf drei Formate in der Action ist die einfachste konsistente Lösung. |
| `concurrency` | Statische Gruppe pro Workflow (`<workflow-name>`) mit `cancel-in-progress: false` | Serialisierung ist für die Duplikat-Vermeidung erforderlich: Zwei parallele Läufe (z. B. `schedule` + `workflow_dispatch`) könnten beide „kein offenes Issue" sehen und doppelt anlegen. `cancel-in-progress: false` folgt dem Muster von `sync-staging-with-main.yml`. |
| Cron-Zeitpunkte | Wöchentlich `0 6 * * 1` (Montag 06:00 UTC, zwei Stunden versetzt zu `security-scan.yml` um 04:00); monatlich `0 5 1 * *` (1. des Monats 05:00 UTC); jährlich `0 5 1 1 *` (1. Januar 05:00 UTC); jeweils plus `workflow_dispatch` | Orientierung an `security-scan.yml` (frühe UTC-Morgenstunden, Montag für Wochenläufe). Der Zeitversatz vermeidet Runner-Kollisionen am selben Tag. |
| Runner / Shell / Token | `ubuntu-latest`, `timeout-minutes: 5`, `shell: pwsh`, `env: GH_TOKEN: ${{ github.token }}` auf Step-Ebene | Reine `gh`-Aufrufe brauchen keinen .NET-Build; `ubuntu-latest` ist das Repo-Muster für solche Jobs (`backmerge`, `promote`, `version`). `pwsh` und `GH_TOKEN` sind die etablierten Konventionen. |
| Permissions | `contents: read`, `issues: write` (expliziter `permissions:`-Block auf Workflow-Ebene) | `issues: write` deckt `gh label create` und `gh issue create`/`gh issue list` ab; `contents: read` folgt dem Minimal-Privileg-Muster von `sync-staging-with-main.yml`. Kein `pull-requests: write` nötig, da keine PRs erzeugt werden. |
| Assignee / Projekt / Milestone | Keiner — nur die `@softwarewerkstatt`-Erwähnung im Body | Die Erwähnung ist die geforderte Schnittstelle zum externen Bearbeitungsprozess; `softwarewerkstatt` ist Collaborator (verifiziert), daher triggert die Mention die Notification. Bei späterem Bedarf ist `gh issue create --assignee` eine einzeilige Erweiterung. |

## Programmabläufe

### Zeitgesteuerte Issue-Erzeugung (identisch in allen drei Workflows)

1. Der Workflow wird durch `schedule` (Cron, UTC) oder `workflow_dispatch` gestartet.
2. Ein Job (Name `create-review-issue`) startet auf `ubuntu-latest` und beginnt mit `actions/checkout@v7` — zwingend erforderlich, da `uses: ./.github/actions/...` die Action-Datei im Workspace benötigt (`contents: read` reicht).
3. Der Job ruft die Composite Action `./.github/actions/create-review-issue` mit den Inputs `label`, `label-color`, `label-description`, `issue-title-prefix`, `issue-body`, `cadence` auf.
4. Innerhalb der Composite Action:
   1. `GH_TOKEN` wird als `env` auf Step-Ebene aus `${{ github.token }}` gesetzt.
   2. Schritt „Ensure label exists": `gh label create <label> --color "<hex>" --description "<text>" --force` (idempotent, Muster aus `sync-staging-with-main.yml` Zeile 57).
   3. Schritt „Create issue if none open": `gh issue list --label "<label>" --state open --json number | ConvertFrom-Json`; bei `Count -eq 0` wird der Titel aus `issue-title-prefix` + Perioden-Suffix (per `cadence`: `yyyy-MM` / ISO-`yyyy-KWww` / `yyyy`) gebildet und `gh issue create --title <titel> --body $env:ISSUE_BODY --label "<label>"` ausgeführt; andernfalls `Write-Host`-Meldung mit der Nummer des bestehenden Issues (Skip-Pfad, kein Fehler).
5. Die erzeugten Issues enthalten als erste Zeile `@softwarewerkstatt`, gefolgt von der jeweiligen, fachlich vorgegebenen Prüfliste. **Wichtig:** `issue.md` enthält doppelt kodiertes UTF-8 (Mojibake, z. B. `EinfÃ¼hrung`, `dÃ¼rfen`, `WÃ¶chentlich`). Die Issue-Bodies dürfen diesen Text **nicht** wörtlich übernehmen, sondern müssen den korrekten deutschen Text enthalten (`Einführung`, `dürfen`, `Wöchentlich`, `Prüfe`, `füße`→`üße` usw. — Dekodierregel: `Ã¼`→`ü`, `Ã¶`→`ö`, `Ã¤`→`ä`, `Ã„`→`Ä`, `Ã–`→`Ö`, `Ãœ`→`Ü`, `ÃŸ`→`ß`).

Beteiligte Artefakte: `.github/workflows/monthly-architecture-security-review.yml`, `.github/workflows/weekly-nuget-review.yml`, `.github/workflows/yearly-compliance-review.yml`, `.github/actions/create-review-issue/action.yml`

### Duplikat-Vermeidung (Detail)

Die Erkennung „offenes Issue desselben Typs" erfolgt ausschließlich über das dedizierte Label: Ein Issue gilt als „desselben Typs", wenn es offen ist und das Label des Prüfungstyps trägt. Geschlossene Issues blockieren keine neuen Zyklen. Manuell angelegte Issues ohne das Label werden nicht als Duplikat erkannt — gewollt, da der Typ maschinell definiert ist.

### Manuelle Auslösung und Testlauf

Über `workflow_dispatch` kann jeder Workflow manuell gestartet werden; die Logik ist identisch (Duplikat-Prüfung greift auch hier, ein Testlauf erzeugt also bei bestehendem offenem Issue kein Duplikat). Da `schedule` und `workflow_dispatch` erst greifen, sobald die Workflow-Datei auf dem Default-Branch (`main`) liegt, erfolgt der reale Vorab-Test über einen temporären `push`-Trigger auf den Feature-Branch (siehe Abschnitt „Tests").

## Neue Klassen

Keine C#-Klassen — stattdessen neue CI-/Dokumentations-Artefakte:

| Artefakt | Typ | Zweck |
|----------|-----|-------|
| `.github/actions/create-review-issue/action.yml` | Composite Action | Kapselt Label-Sicherstellung, Duplikat-Prüfung und Issue-Erzeugung; Inputs: `label`, `label-color`, `label-description`, `issue-title-prefix`, `issue-body`, `cadence` |
| `.github/workflows/monthly-architecture-security-review.yml` | Workflow | Monatliches Architektur-/Sicherheitsreview; Cron `0 5 1 * *` + `workflow_dispatch`; Label `automated-architecture-review` (Vorschlag Farbe `5319E7`); Titel-Präfix `[Review] Architektur- und Sicherheitsreview` |
| `.github/workflows/weekly-nuget-review.yml` | Workflow | Wöchentliche NuGet-Prüfung; Cron `0 6 * * 1` + `workflow_dispatch`; Label `automated-nuget-review` (Vorschlag Farbe `D93F0B`); Titel-Präfix `[Review] NuGet-Paketprüfung` |
| `.github/workflows/yearly-compliance-review.yml` | Workflow | Jährliches Compliance-/Copyright-Review; Cron `0 5 1 1 *` + `workflow_dispatch`; Label `automated-compliance-review` (Vorschlag Farbe `B60205`); Titel-Präfix `[Review] Compliance- und Copyright-Review` |
| `docs/help/review-automatisierung/index.md` | Hilfedatei | Themeneinstieg nach `git-hooks`-Muster (`# Titel` + 1–2 Sätze + „## Inhalt") |
| `docs/help/review-automatisierung/beschreibung.md` | Hilfedatei | Zweck, die drei Workflows mit Intervallen, `@softwarewerkstatt`-Erwähnung, Duplikat-Vermeidung, Labels, manuelle Auslösung |
| `docs/help/review-automatisierung/ablauf-technisch.md` | Hilfedatei | Technischer Ablauf: Composite Action, `GH_TOKEN`, `gh issue list`/`create`, Cron-Zeiten, Permissions, Testlauf-Hinweis |

Strukturvorgaben für die Workflow-Dateien (Konvention aus `security-scan.yml`/`sync-staging-with-main.yml`): englischer Zweck-Kommentarblock direkt unter `name:`; `permissions:`-Block (`contents: read`, `issues: write`); `concurrency` mit statischer Gruppe und `cancel-in-progress: false`; ein Job `create-review-issue` mit `runs-on: ubuntu-latest`, `timeout-minutes: 5`; Schritte `actions/checkout@v7` → `uses: ./.github/actions/create-review-issue` mit den oben genannten Inputs.

## Änderungen an bestehenden Klassen

Keine C#-Klassen betroffen. Anzupassende Dokumentationsdateien:

### `docs/help/index.md` (Hilfe-Übersicht)

- **Neuer Eintrag:** Zeile `- [Automatisierte Review-Workflows](review-automatisierung/index.md) — <Ein-Satz-Beschreibung>` im Abschnitt „Qualität und Tests" (thematisch passend zu Playwright-Tests und Git-Hooks).

### `README.md` (Projektbeschreibung)

- **Neuer Abschnitt:** Kurzer Abschnitt „Automatisierte Review-Workflows" (Vorschlag: nach dem Abschnitt „Git-Hooks", vor „Rechtliches"), der die drei Workflows, ihre Intervalle und den Link auf `docs/help/review-automatisierung/index.md` nennt — analog zum bestehenden Kurzabschnitt „Git-Hooks".

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine Eingabevalidierung im Anwendungssinn. Die fachliche Schutzregel „kein Duplikat anlegen" ist im Programmablauf verankert: `gh issue create` darf nur ausgeführt werden, wenn `gh issue list --label <label> --state open` ein leeres Ergebnis liefert (Fehlerfall: bestehendes offenes Issue → Skip mit `Write-Host`, Exit-Code 0, kein Workflow-Fehlschlag).

## Konfigurationsänderungen

Keine `appsettings`- oder Anwendungskonfiguration. Die Intervalle sind fest in den Cron-Ausdrücken codiert; Label-Namen, Titel-Präfixe und Issue-Texte stehen als Literale in den Workflow-Dateien (keine Repository-Variablen — nicht gefordert).

## Seiteneffekte und Risiken

- **`security-scan.yml` (thematische Überschneidung):** Der bestehende wöchentliche Vulnerability-Scan (Montag 04:00 UTC, Fehlschlag + Artefakt bei Befund) bleibt unverändert und läuft parallel zum neuen wöchentlichen NuGet-Review-Issue (Montag 06:00 UTC). Keine Änderung an `security-scan.yml` oder der Composite Action `security-scan`; die inhaltliche Abgrenzung wird in der Hilfedoku erläutert (Scan = hartes Gate mit Log-Artefakt; Review-Issue = Bearbeitungsauftrag mit Update-Umsetzung per PR).
- **Reale Issues/Test-Issues:** Beim temporären Push-Trigger-Test (siehe Tests) werden echte Issues und Labels im Repository angelegt; die Test-Issues sind anschließend zu schließen. Die Labels selbst (`automated-*`) bleiben dauerhaft — wie die bestehenden `automated-backmerge`-/`automated-promotion`-Labels.
- **Cron-Genauigkeit:** GitHub-`schedule`-Trigger sind best-effort und können bei hoher Last verzögert oder in Repos mit geringer Aktivität deaktiviert werden — in der Hilfedoku als Hinweis aufzunehmen.
- **`@softwarewerkstatt`-Erwähnung:** Die Erwähnung stößt einen externen Prozess an, der außerhalb dieses Repos liegt und hier nicht vollständig verifiziert werden kann. Repo-seitig wird nur die korrekte Textzeile im Issue-Body abgesichert. Der E2E-Test über den temporären `push`-Trigger erzeugt echte Issues mit echter Erwähnung — die externe Integration wird dadurch bewusst ausgelöst (gewollt als End-to-End-Nachweis); die Test-Issues werden unmittelbar geschlossen.
- **Monitoring:** GitHub benachrichtigt bei fehlschlagenden `schedule`-Läufen standardmäßig den letzten Bearbeiter der Cron-Zeile — kein separates Monitoring eingeplant; Hinweis in der Hilfedoku.
- **Bestehende Tests/CI:** Keine bestehenden Workflows, keine C#-Quellen, keine Tests werden verändert; `pr-staging-ci.yml`/`staging-ci.yml` sind unberührt. Keine bekannten Testbrüche.
- **Default-Branch-Abhängigkeit:** `schedule` und `workflow_dispatch` werden erst nach Merge der Workflow-Dateien auf `main` aktiv; auf `staging` allein feuern sie nicht.

## Umsetzungsreihenfolge

1. **Composite Action `create-review-issue` anlegen** (`.github/actions/create-review-issue/action.yml`)
   - Voraussetzungen: Keine (nur Verzeichniskonvention `.github/actions/<name>/action.yml`, die bereits existiert).
   - Beschreibung: `description:`-Block im Stil der bestehenden Actions (Zweck, Wiederverwendung, Verweis auf die drei Workflows); Inputs `label`, `label-color`, `label-description`, `issue-title-prefix`, `issue-body` (alle `required: true`), `cadence` (`required: true`, erlaubte Werte `monthly`/`weekly`/`yearly`); zwei `pwsh`-Schritte mit `env: GH_TOKEN: ${{ github.token }}`: (a) `gh label create ... --force`, (b) Duplikat-Prüfung per `gh issue list`, Titelbildung mit `switch` auf `cadence` (monatlich `Get-Date -Format 'yyyy-MM'`; wöchentlich ISO-Woche über `[System.Globalization.ISOWeek]::GetYear((Get-Date))` für die Jahreszahl und `[System.Globalization.ISOWeek]::GetWeekOfYear((Get-Date))` für die Woche als `yyyy-KWww` — `GetYear` ist zwingend, da an Jahresgrenzen die ISO-Woche zum Folgejahr gehören kann; jährlich `Get-Date -Format 'yyyy'`), `default`-Zweig im `switch`, der bei unbekanntem `cadence`-Wert einen Fehler wirft (`throw`), `issue-body` über `env: ISSUE_BODY` an `gh issue create --body` übergeben, Skip-Pfad mit `Write-Host`. Nach jedem `gh`-Aufruf `$LASTEXITCODE` prüfen und bei Fehler abbrechen (`exit $LASTEXITCODE`); das Listing-Ergebnis robust zählen (`@($existing).Count -eq 0`), damit ein `gh`-Fehlschlag nicht still in den Create-Pfad läuft.

2. **Workflow `monthly-architecture-security-review.yml` anlegen**
   - Voraussetzungen: Schritt 1 (referenziert `./.github/actions/create-review-issue`).
   - Beschreibung: Struktur gemäß Designentscheidungen; Cron `0 5 1 * *` + `workflow_dispatch`; Issue-Body mit korrigiertem deutschem Text gemäß der Vorgabe aus `issue.md` Abschnitt 1 (Architektur-/Sicherheitsreview, komplette Prüfliste inkl. `@softwarewerkstatt`-Zeile — Mojibake dekodieren, siehe Programmablauf-Hinweis); `cadence: monthly`.

3. **Workflow `weekly-nuget-review.yml` anlegen**
   - Voraussetzungen: Schritt 1.
   - Beschreibung: Cron `0 6 * * 1` + `workflow_dispatch`; Issue-Body mit korrigiertem deutschem Text gemäß `issue.md` Abschnitt 2 (NuGet-Prüfung inkl. PR-Umsetzungsanweisung — Mojibake dekodieren); `cadence: weekly`.

4. **Workflow `yearly-compliance-review.yml` anlegen**
   - Voraussetzungen: Schritt 1.
   - Beschreibung: Cron `0 5 1 1 *` + `workflow_dispatch`; Issue-Body mit korrigiertem deutschem Text gemäß `issue.md` Abschnitt 3 (Compliance-Review — Mojibake dekodieren); `cadence: yearly`.

5. **YAML-Validierung und lokaler Trockenlauf der Duplikat-Logik**
   - Voraussetzungen: Schritte 1–4; lokal installierte `gh`-CLI mit Authentifizierung gegen `martin-stromberg/Schnittstellenzentrale` (vorhanden); Python für YAML-Parsing (Voraussetzung laut `docs/help/git-hooks` bereits gegeben).
   - Beschreibung: Alle vier neuen YAML-Dateien parse-bar prüfen; die pwsh-Befehle der Action lokal ausführen (`gh label create automated-nuget-review ... --force` → idempotent; `gh issue list --label automated-nuget-review --state open --json number` → leeres Ergebnis erwartet). Kein Issue anlegen.

6. **Realer E2E-Test über temporären `push`-Trigger**
   - Voraussetzungen: Schritte 1–5; Push-Recht auf den Feature-Branch.
   - Beschreibung: In den drei Workflows temporär `on: push: branches: [task/issue-79-...]` ergänzen, pushen, pro Workflow einen Lauf abwarten und das erzeugte Issue prüfen (Titel, Label, `@softwarewerkstatt`, vollständiger Body, korrekt gerenderte Umlaute); zweiten Push auslösen und prüfen, dass kein zweites Issue entsteht (Skip-Meldung im Job-Log); Test-Issues unmittelbar schließen (die `@softwarewerkstatt`-Erwähnung feuert bewusst die echte externe Integration — gewollter Nebeneffekt des E2E-Tests, der die End-to-End-Kette belegt); temporären Trigger wieder entfernen und **per grep verifizieren, dass in den drei Workflow-Dateien kein `push:`-Trigger mehr enthalten ist**.

7. **Hilfethema `docs/help/review-automatisierung/` anlegen**
   - Voraussetzungen: Schritte 1–6 (dokumentiert wird der verifizierte Endzustand); Muster `docs/help/git-hooks/`.
   - Beschreibung: `index.md`, `beschreibung.md`, `ablauf-technisch.md` auf Deutsch; Inhalte: Zweck, drei Workflows mit Intervallen, `@softwarewerkstatt`, Label-/Duplikat-Mechanik, manuelle Auslösung, Cron-Deaktivierungs-Hinweis, Abgrenzung zu `security-scan.yml`.

8. **`docs/help/index.md`-Eintrag und `README.md`-Abschnitt ergänzen**
   - Voraussetzungen: Schritt 7.
   - Beschreibung: Eintrag im Abschnitt „Qualität und Tests" (`docs/help/index.md`); Kurzabschnitt in `README.md` mit Link auf das Hilfethema.

## Tests

### Neue Tests

Da keine C#-Artefakte entstehen, gibt es keine xUnit-Tests; die Absicherung erfolgt über Workflow-Verifikation:

| Test / Hilfsmethode | „Testklasse" / Ort | Was wird geprüft / bereitgestellt? |
|---------------------|--------------------|-------------------------------------|
| YAML-Parse-Check der vier neuen Dateien | lokal, Python `yaml.safe_load` o. ä. | Syntaktische Validität der Workflows und der Action (Achtung: PyYAML parst `on:` als YAML-1.1-Boolean `True` — der Check muss das tolerieren bzw. darf nur die Parse-Barkeit prüfen, nicht die Schlüsselnamen) |
| Struktur-Check je Workflow | lokal, manuell/Script | `schedule`-Cron korrekt, `workflow_dispatch` vorhanden, `permissions` (`contents: read`, `issues: write`), `concurrency`, `timeout-minutes`, `ubuntu-latest`, Aufruf der Composite Action mit allen Pflicht-Inputs |
| Umlaute-Check der Issue-Bodies | lokal, manuell/Script | Kein Mojibake (`Ã¼`, `Ã¶` etc.) in den drei Workflow-Dateien; deutsche Umlaute korrekt als UTF-8 |
| Trockenlauf Label-Sicherstellung | lokal, `gh label create <label> --force` | Idempotenz; Label wird angelegt bzw. unverändert bestätigt |
| Trockenlauf Duplikat-Prüfung | lokal, `gh issue list --label <label> --state open --json number` | Liefert leeres Array, wenn kein offenes Issue existiert; Befehl fehlerfrei |
| Push-Trigger-Entfernung | lokal, grep nach `push:` in den drei Workflow-Dateien vor dem finalen Commit | Kein temporärer `push:`-Trigger mehr vorhanden |

### Betroffene bestehende Tests

Keine.

### E2E-Tests (Pflicht)

Es gibt keine UI-Interaktion; die Pflicht-E2E-Abdeckung wird durch reale Workflow-Läufe ersetzt. Da `schedule`/`workflow_dispatch` erst auf dem Default-Branch (`main`) feuern, erfolgt der reale Test über einen temporären `push`-Trigger auf den Feature-Branch, der vor dem Abschluss wieder entfernt wird.

| Szenario | Testdatei / Ort | Abgedecktes Akzeptanzkriterium |
|----------|-----------------|-------------------------------|
| Happy Path: Push auf Feature-Branch löst alle drei Workflows aus; je ein Issue wird erzeugt mit korrektem deutschen Titel (Präfix `[Review]` + Periode), zugehörigem `automated-*`-Label, `@softwarewerkstatt` als erster Zeile und vollständiger Prüfliste aus `issue.md` | GitHub Actions, temporärer `push`-Trigger in den drei Workflow-Dateien | Separate Action je Prüfung; aussagekräftige Titel; `@softwarewerkstatt`-Erwähnung |
| Duplikat-Test: Zweiter Push/Lauf desselben Workflows erzeugt kein zweites Issue; Job-Log zeigt die Skip-Meldung mit bestehender Issue-Nummer | GitHub Actions, zweiter Lauf | Duplikat-Vermeidung bei offenem Issue desselben Typs |
| Neuer Zyklus: Nach dem Schließen des Test-Issues erzeugt ein erneuter Lauf wieder ein Issue (Label-Logik greift nur auf offene Issues) | GitHub Actions, dritter Lauf | Korrekte Definition „offenes Issue desselben Typs" |
| Cron-Verifikation: Review der `schedule`-Einträge (Montag 06:00 UTC / 1. des Monats 05:00 UTC / 1. Januar 05:00 UTC) und Kommentar im Workflow | Code-Review der YAML-Dateien | Zeitgesteuerte Ausführung über Cron-Trigger |
| Dokumentations-Verifikation: Hilfethema existiert, ist in `docs/help/index.md` verlinkt, README verweist darauf | manuelle Prüfung / Markdown-Link-Check | Vollständige In-Repo-Dokumentation |

Welche bestehenden E2E-Tests müssen angepasst werden?

Keine (keine Playwright-Tests betroffen).

## Offene Punkte

Keine. Die ursprünglich offenen Punkte wurden entschieden bzw. verifiziert:

- **Assignee/Projekt/Milestone:** Keiner — nur `@softwarewerkstatt`-Erwähnung (siehe Designentscheidungen; Collaborator-Status verifiziert).
- **Verhältnis zu `security-scan.yml`:** Bewusst parallel ohne Verknüpfung; Abgrenzung wird im Hilfethema dokumentiert.
- **Vorab-Abnahme:** Realer E2E-Test über temporären `push`-Trigger (siehe Tests); Nachverifikation per `workflow_dispatch` nach Merge auf `main`.
- **Erstlauf-Verhalten:** Live geprüft — aktuell existiert kein inhaltlich passendes offenes Issue; Hinweis für künftige Fälle wird in `ablauf-technisch.md` dokumentiert.
