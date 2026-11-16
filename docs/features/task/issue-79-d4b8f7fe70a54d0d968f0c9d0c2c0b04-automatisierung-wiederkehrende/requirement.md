# Übersetzte Anforderungsbeschreibung

Quelle: `issue.md` (GitHub-Issue #79 — „Automatisierung wiederkehrender Architektur-, Sicherheits- und Compliance-Reviews")

## Fachliche Zusammenfassung

Es werden drei eigenständige, zeitgesteuerte GitHub-Actions-Workflows unter `.github/workflows/` eingeführt, die automatisiert GitHub-Issues erzeugen. Jedes Issue enthält eine `@softwarewerkstatt`-Erwähnung, die die automatische Analyse und Bearbeitung anstoßen soll, sowie eine fest definierte Prüfliste (Architektur-/Sicherheitsreview monatlich, NuGet-Paketprüfung wöchentlich, Compliance-/Copyright-Review jährlich). Vor dem Anlegen eines Issues muss jeder Workflow prüfen, ob bereits ein offenes Issue desselben Typs aus einem vorherigen Zyklus existiert, und in diesem Fall kein neues Issue erstellen (Duplikat-Vermeidung).

## Betroffene Klassen und Komponenten

Reine GitHub-Actions-Anforderung — keine C#-Artefakte. Voraussichtlich betroffene bzw. neu zu erstellende Artefakte:

- Datenmodellklassen: Keine
- Logikklassen / Services: Keine
- Interfaces: Keine
- Enums: Keine
- Workflow-Dateien (neu, unter `.github/workflows/`):
  - Ein Workflow für das monatliche Architektur- und Sicherheitsreview (Vorschlag z. B. `monthly-architecture-security-review.yml`)
  - Ein Workflow für die wöchentliche NuGet-Prüfung (Vorschlag z. B. `weekly-nuget-review.yml`)
  - Ein Workflow für das jährliche Compliance- und Copyright-Review (Vorschlag z. B. `yearly-compliance-review.yml`)
- Issue-Texte / -Templates: Die Issue-Bodies sind im Issue #79 wörtlich vorgegeben; sie können inline im Workflow (pwsh-Here-String) oder als Template-Dateien z. B. unter `.github/` abgelegt werden (Annahme, zu entscheiden)
- Dokumentation: neuer Eintrag unter `docs/help/` (Konvention: eigenes Unterverzeichnis mit `index.md`, `beschreibung.md` etc., vergleichbar `docs/help/git-hooks/`) sowie Verlinkung in `docs/help/index.md`; ggf. Ergänzung in `README.md`
- Tests: Keine klassischen Unit-/Integrationstests möglich; Absicherung erfolgt über `workflow_dispatch`-Trigger zum manuellen Testlauf und ggf. YAML-Linting (Annahme)

## Implementierungsansatz

- Trigger: `on.schedule` mit Cron-Ausdruck (UTC):
  - Monatlich am 1. Tag des Monats, z. B. `0 5 1 * *` (Uhrzeit nicht spezifiziert — Vorschlag)
  - Wöchentlich, Wochentag nicht spezifiziert — Vorschlag: Montag, analog `security-scan.yml` (`0 4 * * 1`), ggf. zeitversetzt um Kollisionen zu vermeiden
  - Jährlich am 1. Januar, z. B. `0 5 1 1 *`
  - Zusätzlich `workflow_dispatch`, um einen Lauf manuell auslösen und testen zu können (Konvention aus `security-scan.yml`)
- Issue-Erstellung über die `gh`-CLI mit `env: GH_TOKEN: ${{ github.token }}` und `shell: pwsh` — exakt das in `sync-staging-with-main.yml` etablierte Muster (`gh pr create`, `gh label create --force`)
- Duplikat-Vermeidung: vor `gh issue create` per `gh issue list --state open` auf ein offenes Issue desselben Typs prüfen. Als Identifikationsmerkmal bieten sich dedizierte Labels an (Konvention: Label wird vorab per `gh label create ... --force` sichergestellt, wie `automated-backmerge` in `sync-staging-with-main.yml`) — Alternative: Suche über den Issue-Titel. Annahme: Label-basiert, ein Label pro Prüfungstyp
- Issue-Inhalte (wörtlich aus Issue #79):
  - Architektur-/Sicherheitsreview: `@softwarewerkstatt`-Erwähnung plus Prüfliste (Sicherheitsrisiken in Frameworks/Bibliotheken, CVEs in NuGet-Abhängigkeiten, Empfehlungen zu ASP.NET Core/Blazor/.NET, AuthN/AuthZ-Änderungen, Design Patterns, veraltete APIs, Wartbarkeit/Testbarkeit, Logging/Telemetrie/Monitoring, Community-/Microsoft-Erkenntnisse) und Aufforderung zu einer priorisierten Liste konkreter Handlungsempfehlungen
  - NuGet-Prüfung: `@softwarewerkstatt`-Erwähnung plus Prüfliste (Updates, Security-Updates, Schwachstellen, veraltete/ungepflegte Pakete, Nachfolgepakete, Breaking Changes) sowie Anweisung, unbedenkliche Updates (niedriges bis mittleres Risiko) direkt in einem Pull Request umzusetzen und risikoreiche/aufgeschobene Empfehlungen im PR-Body bzw. als Folge-Issue zu dokumentieren
  - Compliance-Review: `@softwarewerkstatt`-Erwähnung plus Prüfliste (Copyright-Hinweise im Quellcode, Webseiten, About-Dialoge, Impressum, Datenschutzerklärung, Drittanbieter-Lizenzen, Open-Source-Verpflichtungen, Unternehmens-/Kontaktinformationen, App-Store-Metadaten) und Aufforderung zu konkreten Änderungsvorschlägen inkl. Risiken/Handlungsbedarf
- Workflow-Konventionen des Projekts einhalten: englischer Kommentarblock am Dateianfang mit Zweckbeschreibung, expliziter `permissions:`-Block (mindestens `issues: write`, `contents: read`), `concurrency`-Gruppe, `timeout-minutes` am Job, `ubuntu-latest` als Runner (leichtgewichtige `gh`-Aufrufe, kein .NET-Build nötig), `pwsh` als Shell
- Keine Abhängigkeiten zu C#-Code, keine Änderungen an bestehenden Workflows erforderlich; fachliche Überschneidung mit `security-scan.yml` besteht nur thematisch (siehe Offene Fragen)

## Konfiguration

- Kein Konfigurationsbedarf auf Anwendungsebene (keine `appsettings`, keine UI-Einstellung)
- Intervalle sind fest im Cron-Ausdruck der Workflows codiert; eine Anpassung erfolgt über die Workflow-Datei
- Die zu erwähnende Identität `@softwarewerkstatt` ist fester Bestandteil der Issue-Texte (keine Variable erforderlich)
- Optional (Annahme): Repository-Variablen (`vars`) für Labels/Titel-Präfixe, falls gewünscht — nicht gefordert

## Offene Fragen

1. **Uhrzeiten und Wochentag:** Die Cron-Zeitpunkte (Uhrzeit UTC; Wochentag der wöchentlichen Prüfung) sind nicht spezifiziert. Vorschlag: Montag für die Wochenprüfung (analog `security-scan.yml`), jeweils frühe UTC-Morgenstunden.
2. **Duplikat-Erkennung:** Soll die Erkennung „offenes Issue desselben Typs" über ein dediziertes Label pro Prüfungstyp (Vorschlag, entspricht `automated-backmerge`-Konvention) oder über den Issue-Titel erfolgen?
3. **Zuweisung vs. Erwähnung:** Gefordert ist nur die `@softwarewerkstatt`-Erwähnung im Issue-Body. Soll das Issue zusätzlich einem Benutzer/Assignee oder einem Projekt/Milestone zugeordnet werden?
4. **Überschneidung mit `security-scan.yml`:** Es existiert bereits ein wöchentlicher NuGet-Vulnerability-Scan, der Artefakte statt Issues erzeugt. Soll der neue wöchentliche Workflow bewusst parallel dazu laufen (Issue mit `@softwarewerkstatt`-Bearbeitungsauftrag), oder soll er das bestehende Scan-Ergebnis einbeziehen? Annahme: parallel, da das Issue explizit eine Analyse mit PR-Umsetzung fordert.
5. **Issue-Titel:** Aussagekräftige Titel sind gefordert, aber nicht konkret vorgegeben — Vorschlag: feste Titel mit erkennbarem Präfix (z. B. `[Review] Architektur- und Sicherheitsreview <Monat Jahr>`), um sowohl Lesbarkeit als auch ggf. titelbasierte Duplikatsuche zu unterstützen.
6. **Ablage der Issue-Texte:** Inline im Workflow oder als separate Markdown-Templates unter `.github/`? (Annahme: inline per pwsh-Here-String, sofern nicht anders gewünscht.)
