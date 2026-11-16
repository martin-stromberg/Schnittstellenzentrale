# Automatisierte Review-Workflows — Beschreibung

## Zweck

Wiederkehrende Qualitäts- und Rechtsprüfungen sollen nicht von Hand angestoßen werden müssen. Drei zeitgesteuerte GitHub-Actions-Workflows unter `.github/workflows/` legen dafür automatisch GitHub-Issues mit einer fest definierten Prüfliste an. Jedes Issue beginnt mit einer `@softwarewerkstatt`-Erwähnung, die die externe automatische Analyse und Bearbeitung des Prüfauftrags auslöst.

## Die drei Workflows

| Workflow | Intervall | Label | Issue-Titel |
|----------|-----------|-------|-------------|
| `monthly-architecture-security-review.yml` | Monatlich, am 1. des Monats | `automated-architecture-review` | `[Review] Architektur- und Sicherheitsreview <yyyy-MM>` |
| `weekly-nuget-review.yml` | Wöchentlich, montags | `automated-nuget-review` | `[Review] NuGet-Paketprüfung <yyyy-KWww>` |
| `yearly-compliance-review.yml` | Jährlich, am 1. Januar | `automated-compliance-review` | `[Review] Compliance- und Copyright-Review <yyyy>` |

Jeder Workflow erzeugt ein Issue mit einer fachlich vorgegebenen Prüfliste:

- **Architektur- und Sicherheitsreview:** Sicherheitsrisiken in Frameworks und Bibliotheken, CVEs in NuGet-Abhängigkeiten, Empfehlungen zu ASP.NET Core / Blazor / .NET, Authentifizierung und Autorisierung, Design Patterns, veraltete APIs, Wartbarkeit und Testbarkeit, Logging / Telemetrie / Monitoring sowie Community- und Microsoft-Erkenntnisse. Ergebnis soll eine priorisierte Liste konkreter Handlungsempfehlungen sein.
- **NuGet-Paketprüfung:** verfügbare Updates, Security-Updates, bekannt gewordene Schwachstellen, veraltete oder ungepflegte Pakete, Nachfolgepakete und Breaking Changes. Unbedenkliche Empfehlungen (niedriges bis mittleres Risiko) sollen direkt in einem Pull Request umgesetzt, risikoreiche im PR-Body oder als Folge-Issue dokumentiert werden.
- **Compliance- und Copyright-Review:** Copyright-Hinweise im Quellcode, auf Webseiten und in About-Dialogen, Impressum, Datenschutzerklärung, Drittanbieter-Lizenzen, Open-Source-Verpflichtungen, Unternehmens- und Kontaktinformationen sowie App-Store-Metadaten. Ergebnis sollen konkrete Änderungsvorschläge samt Risiken und Handlungsbedarfen sein.

## Duplikat-Vermeidung

Vor dem Anlegen eines Issues prüft jeder Workflow, ob bereits ein offenes Issue mit dem Label des jeweiligen Prüfungstyps existiert. Ist das der Fall, wird kein neues Issue erstellt — ein nicht abgearbeitetes Review aus einem früheren Zyklus blockiert also den Folgezyklus, statt Duplikate zu erzeugen. Geschlossene Issues blockieren nicht. Manuell angelegte Issues ohne das `automated-*`-Label werden nicht als Duplikat erkannt.

## Issue-Titel

Alle Review-Issues tragen den Präfix `[Review]` gefolgt vom Prüfungsnamen und einem Perioden-Suffix, das den Zyklus erkennbar macht: Jahr-Monat (`yyyy-MM`) beim monatlichen Lauf, ISO-Kalenderwoche (`yyyy-KWww`) beim wöchentlichen Lauf und das Jahr (`yyyy`) beim jährlichen Lauf.

## Manuelle Auslösung

Zusätzlich zum Zeitplan kann jeder der drei Workflows über `workflow_dispatch` manuell gestartet werden (GitHub-UI: *Actions → Workflow auswählen → Run workflow*). Die Duplikat-Prüfung greift auch hier — ein Testlauf bei bereits offenem Review-Issue erzeugt kein Duplikat.

## Abgrenzung zum Security-Scan

Der bestehende Workflow `security-scan.yml` läuft ebenfalls wöchentlich (montags 04:00 UTC) und prüft NuGet-Abhängigkeiten auf bekannte Schwachstellen. Er ist ein **hartes CI-Gate**: Bei einem Befund schlägt der Lauf fehl und legt ein Log-Artefakt ab — es wird kein Issue erzeugt und niemand zur Bearbeitung aufgefordert.

Die Review-Workflows dagegen erzeugen einen **Bearbeitungsauftrag**: Das Issue enthält die `@softwarewerkstatt`-Erwähnung und eine Prüfliste, deren Ergebnis Analysen, Handlungsempfehlungen und — bei der NuGet-Prüfung — direkt umgesetzte Updates per Pull Request sind. Beide Mechanismen laufen bewusst parallel; das wöchentliche NuGet-Review startet zeitversetzt um 06:00 UTC.

## Einschränkungen

- Die Zeitsteuerung wird erst aktiv, sobald die Workflow-Dateien auf dem Default-Branch (`main`) liegen.
- Manuell angelegte oder ältere Issues ohne das jeweilige `automated-*`-Label werden von der Duplikat-Erkennung nicht erfasst.
- Intervalle, Label, Titel und Issue-Texte stehen fest in den Workflow-Dateien; eine Anpassung erfolgt durch Änderung der YAML-Datei.
