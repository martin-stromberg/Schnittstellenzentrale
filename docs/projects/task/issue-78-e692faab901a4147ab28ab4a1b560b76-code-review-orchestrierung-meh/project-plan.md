# Projektplan: Code-Review-Orchestrierung – Behebung der Befunde aus Issue #78 (Issues #87–#92)

## Übersicht

Das Projekt behebt die acht im Code-Review (Issue #78) festgestellten Befunde, die über die
Issues #87 bis #92 verteilt sind. Kern der Maßnahme ist die Verlagerung mehrstufiger fachlicher
Geschäftsvorfälle aus aufrufenden Komponenten (Blazor-UI, Hintergrunddienst beim App-Start,
Controller) in jeweils genau einen serverseitigen Ausführungspfad. Betroffen sind die Bereiche
Swagger-Import inkl. Startabgleich der Systemanwendung, REST- und OData-API, das atomare
Löschen von Anwendungsgruppen mit Inhalt, die Update-Semantik des Endpunkt-Aggregats sowie die
SignalR-Benachrichtigungspolitik. Ergebnis ist ein gemeinsamer Pull Request gegen `staging`
vom Projekt-Basisbranch aus.

## Grobe Vorgehensentscheidungen

| Bereich | Entscheidung | Begründung |
|---------|-------------|------------|
| Reihenfolge | Umsetzung in der Reihenfolge #87 → #88 → #90 → #89 → #92 → #91 (vom Projektleiter bestätigt) | Serverseitige Grundlagen und neue API-Verträge werden zuerst geschaffen; die UI-Umstellung (#89) setzt direkt auf den finalen Endpunkten auf; die zentralisierte Benachrichtigungspolitik (#91) kommt zuletzt, damit alle mutierenden Pfade bereits ihre finale Form haben. |
| Endpunkt-Aggregat | Kinder des Endpunkts (Header, Query-Parameter) werden auf allen mutierenden Pfaden (REST, OData, Import) über denselben Aggregat-Update-Pfad gepflegt; der REST-Vertrag mit Vollmengen-Ersetzung der Kinder bleibt unverändert, es werden keine Flags eingeführt | Einheitliche Semantik für alle Pfade verhindert stilles Ignorieren und inkonsistente Zwischenstände; der bestehende Vertrag bleibt stabil. |
| API-Kompatibilität | `DELETE /api/application-groups/{id}` bleibt abwärtskompatibel: ohne `content`-Parameter gilt das bisherige Verhalten (= `detach`), ein ungültiger Wert wird mit `400` abgelehnt | Bestehende API-Nutzer bleiben unverändert lauffähig. |
| Import-Apply-Absicherung | Beim Anwenden eines Swagger-Import-Diffs wird geprüft, dass alle Diff-Endpunkte dieselbe Anwendungs-ID wie die Zielanwendung tragen; bei Verletzung wird nicht angewendet und ein verständlicher Fehler zurückgegeben | Schützt vor fälschlich auf eine fremde Anwendung angewendeten Diffs. |
| Benachrichtigungspolitik | Alle mutierenden Operationen aller vier OData-Entity-Sets (Endpunkte, Endpunktgruppen, Anwendungen, Anwendungsgruppen) benachrichtigen im Team-Modus mit derselben Benachrichtigungsart wie ihre REST-Pendants | Eine einheitliche Auslösepolitik für alle API-Pfade statt nur für eine Teilmenge. |
| Aktivitätsprotokoll | Das UI-seitige Aktivitätsprotokoll bleibt in der Workspace-Komponente; entfernt werden nur die direkten Repository-Zugriffe und die redundanten Benachrichtigungs-Entscheidungen | Das Protokoll ist eine UI-Eigenheit und kein Teil des Controller-Pfads. |
| Zerlegung | Jeder der sechs Issues wird als ein eigener Entwicklungsschritt umgesetzt — keine Zusammenlegung, keine weitere Aufteilung | Die Issues sind fachlich sauber voneinander abgrenzbar und jeweils eigenständig liefer- und testbar; fachliche Überschneidungen (gleiche UI-Komponenten/Controller) werden über die Reihenfolge und Abhängigkeiten aufgelöst. |
| Auslieferung | Ein gemeinsamer Pull Request gegen `staging` vom Projekt-Basisbranch; jede Schritt-Umsetzung committet mit Closing-Keyword (`Closes #N`) auf den Basisbranch | Issues sollen erst bei der Promotion nach main geschlossen werden (Staging-Workflow); ein PR hält die zusammengehörige Review-Einheit zusammen. |
| UI-Umstellung | Die UI wird erst umgestellt, nachdem der jeweilige serverseitige Endpunkt existiert | Verhindert Zwischenstände, in denen die Oberfläche auf nicht vorhandene API-Verträge zeigt. |

## Entwicklungsschritte

### Schritt 1: Einheitlicher Swagger-Import-Ausführungspfad inklusive Startabgleich (Issue #87)

**Beschreibung:**
Der automatische Endpunktabgleich der Systemanwendung beim App-Start soll denselben
serverseitigen Ausführungspfad nutzen wie der manuelle Swagger-Import — von einem bereits
vorliegenden OpenAPI-Dokument bis zur persistierten Endpunktmenge. Dazu wird der
Swagger-Import-Service um einen Einstieg erweitert, der ein OpenAPI-Dokument entgegennimmt,
die Endpunkte daraus ableitet, den Bestand der Zielanwendung lädt, die Differenz berechnet
(neu, geändert, entfernt) und sie anwendet — einschließlich der Übernahme der im Dokument
mitgelieferten Bearer-Tokens und der Erkennung von Duplikatschlüsseln. Der Hintergrunddienst
für den Startabgleich wird entsprechend vereinfacht: Er löst nur noch Systemgruppe und
Systemanwendung auf, beschafft das Dokument über die bestehende Swagger-Bereitstellung und
ruft den neuen Service-Einstieg auf; die bisherige handgebaute Differenzbildung entfällt.
Fachliche Wirkung: Geänderte System-Endpunkte (Name, Body, Authentifizierungstyp,
Vor-/Nachher-Skripte) werden künftig beim Start aktualisiert — bisher wurden nur neue und
entfernte Endpunkte verarbeitet; Duplikatschlüssel im Dokument werden erkannt und als Fehler
behandelt statt still ignoriert zu werden. Ein Dokument mit Fehler führt wie im bisherigen
Import-Pfad zu keiner Anwendung der Änderungen; Fehler werden geloggt, der Start läuft weiter.
Mit der Verlagerung der Abgleichslogik in den gemeinsamen Service-Pfad ist außerdem die in
den Entwicklungskonventionen dokumentierte Ausnahme vom API-First-Prinzip (direkter
Repository-Zugriff beim Startabgleich) zu prüfen und zu pflegen: Sie gilt weiterhin nur für
den Startabgleich und wandert mit der Logik an den neuen Ausführungsort; greift der
Hintergrunddienst künftig gar nicht mehr direkt auf die Persistenz zu, ist die dokumentierte
Ausnahme entsprechend zu streichen oder auf den neuen Ort umzuschreiben, damit die Konvention
und der Code übereinstimmen.
Abdeckung durch angepasste und neue Unit-Tests für den Startabgleich (neue Semantik:
geänderte Endpunkte, Service-Pfad statt eigener Differenzbildung) und den Import-Service;
bestehende Unit-, Integrations- und Playwright-Tests bleiben grün.

**Abhängigkeiten:** Keine

**Betroffene Bereiche:** Swagger-Import, automatischer Startabgleich der Systemanwendung, Persistenz der System-Endpunkte, Entwicklungskonventionen (API-First-Dokumentation)

### Schritt 2: API-Endpunkt für den Swagger-Import-Apply und Umstellung des Import-Dialogs (Issue #88)

**Beschreibung:**
Das Anwenden eines berechneten Swagger-Import-Diffs soll über einen eigenen,
validierten REST-Endpunkt laufen — analog zum bestehenden OData-Import-Apply. Neu ist der
Endpunkt `POST /api/applications/{id}/swagger-import/apply`: Er validiert das Bearer-Token
(`401` bei ungültigem Token), prüft die Existenz der Zielanwendung (`404`), erzwingt den
Schnittstellentyp REST der Anwendung (`422` bei abweichendem Typ) und wendet anschließend den
übergebenen Diff an (`204`). Zusätzlich wird die Apply-Logik des Swagger-Imports so
abgesichert, dass beim Anwenden eines Diffs geprüft wird, dass alle Endpunkte des Diffs
dieselbe Anwendungs-ID tragen wie die Zielanwendung des Aufrufs (Aggregat-Validierung);
bei Verletzung wird der Diff nicht angewendet und ein verständlicher Fehler
zurückgegeben. Der API-Client der Anwendung wird um eine
Aufrufmethode für diesen Endpunkt erweitert (Fehler werden wie im OData-Pendant als
verständliche Fehlermeldung durchgereicht), und der Swagger-Import-Dialog in der Oberfläche
ruft künftig diesen API-Pfad statt den Import-Service direkt — damit gelten für den
UI-Import dieselben Nebenbedingungen (Token-Validierung, Existenz- und Typprüfung) wie für
externe Aufrufer. Abdeckung durch Unit-Tests mit gemocktem HTTP-Handler für den neuen
Client-Aufruf sowie Integrationstests für den Endpunkt (204/401/404/422); bestehende
Suiten inkl. Playwright bleiben grün. Die REST-API-Dokumentation wird um den neuen
Endpunkt ergänzt. Neue UI-Texte (falls z. B. neue Fehlermeldungen im Dialog entstehen)
werden über das zentrale Ressourcen-Paket nach dem vereinbarten Schlüsselschema mit
Kontext-Kommentar je Sprache gepflegt.

**Abhängigkeiten:** 1

**Betroffene Bereiche:** REST-API für Importe, Swagger-Import, API-Client, Import-Dialog der Oberfläche, API-Dokumentation, Lokalisierung

### Schritt 3: Atomares Löschen von Anwendungsgruppen mit Inhalt (Issue #90)

**Beschreibung:**
Das Löschen einer Anwendungsgruppe samt enthaltener Anwendungen soll serverseitig als eine
atomare Einheit ausgeführt werden — statt als mehrstufige, bei Teilerfolg inkonsistente
Abfolge aus der Oberfläche. Der bestehende Endpunkt `DELETE /api/application-groups/{id}`
wird um einen Auswahlparameter `content` erweitert: `content=delete` löscht die Gruppe
inklusive aller zugeordneten Anwendungen (mit deren Endpunkten und Endpunktgruppen
kaskadierend) in einer Transaktion, `content=detach` hängt die enthaltenen Anwendungen
explizit in die ungruppierte Menge um und löscht dann die Gruppe — ebenfalls atomar. Bei
Fehlschlag bleibt der Bestand vollständig erhalten (kein Teilerfolg). Fehlt die Gruppe, wird
`404` zurückgegeben; Systemgruppen bleiben wie bisher geschützt (`403`); eine bestehende
Änderungs-Benachrichtigung bei Erfolg bleibt unverändert. Der Parameter ist
abwärtskompatibel ausgelegt: Wird `content` nicht übergeben, gilt das bisherige Verhalten
(Gruppe löschen, enthaltene Anwendungen werden ungruppiert — entspricht faktisch
`detach`); ein ungültiger Wert wird mit `400` abgelehnt. Die Oberfläche soll diesen
einen Endpunkt für beide Bestätigungsvarianten des Löschdialogs nutzen („Alles löschen" →
`delete`, „Nur Gruppe löschen" → `detach`); die bisherigen Mehrschritt-Aufrufe entfallen
(die eigentliche UI-Umstellung des Löschdialogs erfolgt im separaten Schritt zur
API-Umstellung der Workspace-Oberfläche — der Endpunkt muss in jedem Fall zuerst
existieren). Der API-Client wird um die parametrisierte Lösch-Variante erweitert.
Abdeckung durch Unit-Tests des API-Clients (korrekte URL inkl. Parameter), Integrations-
und Repository-Tests für beide Varianten inkl. Atomarität und Rollback; bestehende Suiten
bleiben grün. Die REST-API-Dokumentation wird um den neuen Parameter ergänzt.

**Abhängigkeiten:** Keine

**Betroffene Bereiche:** Anwendungsgruppen-Verwaltung, REST-API, Datenpersistenz (Transaktion), API-Client, Löschdialog der Oberfläche, API-Dokumentation

### Schritt 4: Workspace-Oberfläche vollständig auf den API-Pfad umstellen (Issue #89)

**Beschreibung:**
Der Workspace-Bereich der Oberfläche soll für alle Endpunkt- und Endpunktgruppen-Operationen
(Anlegen, Umbenennen, Löschen sowie die zugehörigen Bestätigungs- und Zählabfragen)
ausschließlich über den bestehenden API-Client gehen — also über den REST-Controller-Pfad
mit Kontext-Headern, Team-Benachrichtigung und konsistenten Nebenbedingungen. Die direkte
Nutzung des Endpunkt-Repositories in dieser Komponente entfällt vollständig; ebenso entfallen
die komponenteneigenen Entscheidungen, ob eine Änderungs-Benachrichtigung auszulösen ist —
diese Politik liegt einheitlich im Controller-Pfad. Auch das Löschen einer Anwendungsgruppe
mit Inhalt wird über den einzigen neuen Gruppen-Lösch-Endpunkt mit Inhalts-Parameter
abgewickelt (aus Schritt 3): Beide Bestätigungsvarianten des Löschdialogs rufen denselben
Endpunkt mit unterschiedlichem Parameterwert; die bisherigen mehrstufigen UI-Abläufe
entfallen. Das UI-seitige Protokollieren der Nutzeraktionen (Aktivitätsprotokoll für
Ordner-/Endpunkt-Aktionen) bleibt als UI-Eigenheit in der Komponente erhalten; entfernt
werden ausschließlich die direkten Repository-Zugriffe und die redundanten
Benachrichtigungs-Entscheidungen. Reine Lesezugriffe anderer Komponenten auf Repositories außerhalb dieser
Umstellung bleiben unberührt. Fachliche Wirkung: Es existiert nur noch ein produktiver
Ausführungspfad für Endpunkt-/Endpunktgruppen-CRUD aus der UI — mit einheitlichen
Nebenbedingungen und ohne Doppelbenachrichtigungen. Abdeckung durch die bestehenden
Komponenten-, Integrations- und Playwright-Tests (Gruppen-CRUD, Endpunktgruppen-CRUD,
SignalR-Synchronisation, Layout-Prüfung), die ohne neue Tests grün bleiben müssen; bei
Bedarf Anpassung bestehender Komponenten-Tests an die geänderten Abhängigkeiten.

**Abhängigkeiten:** 3

**Betroffene Bereiche:** Workspace-Oberfläche, Endpunkt- und Endpunktgruppen-Verwaltung, Anwendungsgruppen-Löschung, API-First-Einhaltung, Aktivitätsprotokoll

### Schritt 5: Einheitliche Update-Semantik für das Endpunkt-Aggregat (Issue #92)

**Beschreibung:**
Das vollständige Ersetzen eines Endpunkts — Skalarwerte (Name, Methode, Pfad, Body,
Authentifizierung, Skripte, Gruppenzugehörigkeit, Version) plus die Kind-Mengen Header und
Query-Parameter — soll als genau eine gemeinsame serverseitige Operation mit einer
eindeutigen Semantik bereitstehen und von allen mutierenden Pfaden genutzt werden: dem
REST-Update-Endpunkt, den OData-Update-Operationen auf Endpunkte und den Import-Apply-Pfaden.
Heute komponiert der REST-Endpunkt das Aggregat-Update noch redundan­terweise aus einem
Skalar-Update und manuellen Einzel-Lösch-/Anlege-Aufrufen pro Kind — diese Doppelarbeit
entfällt zugunsten der einen Operation. Auf der OData-Seite pflegen die
Update-Operationen mitgelieferte Header/Query-Parameter über denselben
Aggregat-Update-Pfad — damit gilt für REST, OData und Import eine einheitliche Semantik,
und mitgelieferte Kinder werden nicht mehr still ignoriert. Der REST-Vertrag bleibt
unverändert: Die Vollmengen-Ersetzung der Kinder ist bereits Teil des bestehenden
Vertrags, es werden keine Flags für optionale Kind-Pflege eingeführt. Die redundante
Sofort-Kinder-Pflege in der Endpunkt-Detailseite der Oberfläche (parallele Persistenz pro
Panel-Änderung zusätzlich zur Vollmengen-Ersetzung beim Speichern) darf entfallen,
solange das Speichern weiterhin die Vollmenge korrekt übermittelt. Das Optimistic-Concurrency-
Verhalten über das Versionsfeld bleibt unverändert. Abdeckung durch Integrations-Tests der
Aggregat-Operation (Skalare + Kinder in einem Schritt, Versions-Konflikt) sowie OData-
und REST-Update-Tests der einheitlichen Semantik; bestehende Suiten bleiben grün. Die
einheitliche Aggregat-Semantik ist in der API- bzw. OData-API-Dokumentation
festzuhalten.

**Abhängigkeiten:** 1, 2

**Betroffene Bereiche:** Endpunkt-Persistenz, REST-API, OData-API, Import-Apply-Pfade, Endpunkt-Detailseite der Oberfläche, API-Dokumentation

### Schritt 6: Zentralisierte Änderungs-Benachrichtigungen inklusive OData (Issue #91)

**Beschreibung:**
Die Entscheidung, ob eine SignalR-Änderungs-Benachrichtigung ausgelöst wird — genau eine
Nachricht pro mutierendem Vorgang und nur im Team-Speichermodus — soll an einer zentralen
Stelle definiert und einheitlich in allen mutierenden API-Pfaden angewendet werden, statt
als dezentral wiederholte Prüfung in jedem Controller und in UI-Komponenten. Konkret: Alle
mutierenden Operationen aller vier OData-Entity-Sets (Endpunkte, Endpunktgruppen,
Anwendungen, Anwendungsgruppen) lösen künftig im Team-Modus dieselbe Benachrichtigungsart
aus wie ihre REST-Pendants — Baum-Benachrichtigung für Anwendungen und
Anwendungsgruppen, entitätsspezifische Benachrichtigung für Endpunkte und
Endpunktgruppen; die heute gar nicht benachrichtigenden OData-Pfade werden entsprechend
nachgerüstet. Die doppelte Benachrichtigung beim Speichern eines
Endpunkts aus der Oberfläche (einmal im Controller, einmal zusätzlich auf der
Endpunkt-Detailseite) entfällt — die Seite feuert nach erfolgreichem Speichern keine eigene
Nachricht mehr. Der Benachrichtigungs-Transport selbst (SignalR-Hub, Gruppen,
Ereignisnamen) bleibt unverändert; es wird ausschließlich die Auslösepolitik vereinheitlicht.
Fachliche Wirkung: Jede mutierende Änderung — egal ob über REST-API, OData-API, Oberfläche,
Import oder Startabgleich — löst im Team-Modus genau eine Benachrichtigung aus, im
Benutzermodus keine. Abdeckung durch Integrations-Tests, die die Benachrichtigung auf den
mutierenden OData-Pfaden verifizieren (bisher nur technisch abgefangen, nicht inhaltlich
geprüft), sowie durch die bestehenden Playwright-SignalR-Tests; bestehende Suiten bleiben
grün.

**Abhängigkeiten:** 5

**Betroffene Bereiche:** Änderungs-Benachrichtigungen (SignalR-Politik), OData-API, REST-API, Endpunkt-Detailseite der Oberfläche

## Übersichtstabelle

| # | Titel | Abhängigkeiten | Betroffene Bereiche |
|---|-------|-----------------|----------------------|
| 1 | Einheitlicher Swagger-Import-Ausführungspfad inkl. Startabgleich (#87) | Keine | Swagger-Import, Startabgleich, System-Endpunkte, Konventions-Doku |
| 2 | API-Endpunkt für Swagger-Import-Apply und Umstellung des Import-Dialogs (#88) | 1 | REST-API, Swagger-Import, API-Client, Import-Dialog, API-Doku, Lokalisierung |
| 3 | Atomares Löschen von Anwendungsgruppen mit Inhalt (#90) | Keine | Anwendungsgruppen, REST-API, Transaktion, API-Client, Löschdialog, API-Doku |
| 4 | Workspace-Oberfläche vollständig auf den API-Pfad umstellen (#89) | 3 | Workspace-UI, Endpunkt-/Gruppen-CRUD, API-First, Aktivitätsprotokoll |
| 5 | Einheitliche Update-Semantik für das Endpunkt-Aggregat (#92) | 1, 2 | Endpunkt-Persistenz, REST/OData-API, Import-Pfade, Endpunkt-Detailseite, API-Doku |
| 6 | Zentralisierte Änderungs-Benachrichtigungen inkl. OData (#91) | 5 | Benachrichtigungspolitik, OData-/REST-API, Endpunkt-Detailseite |

## Offene Punkte

Keine. Alle zuvor offenen Punkte wurden vom Projektleiter entschieden und sind in den
Vorgehensentscheidungen sowie den betroffenen Schrittbeschreibungen eingearbeitet.
