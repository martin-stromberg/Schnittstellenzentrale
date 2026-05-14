# Schnittstellenzentrale — Beschreibung

## Zweck

Die Schnittstellenzentrale ist eine zentrale Verwaltungsoberfläche für Webservice-Endpunkte der lokalen Servermaschine. Sie löst das Problem, dass Entwickler und Administratoren Webservice-Endpunkte bisher manuell in Textdateien oder Drittanbieter-Werkzeugen verwalten mussten. Mit der Schnittstellenzentrale können Endpunkte direkt aus der Anwendung heraus aufgerufen, dokumentiert und — im Team-Modus — gemeinsam gepflegt werden.

## Funktionsweise

Die Anwendung läuft als Blazor Server App unter IIS und sichert den Zugriff ausschließlich über Windows-Authentifizierung. Alle Daten werden in einer relationalen Datenbank (SQLite oder SQL Server) gespeichert.

**Anwendungen und Gruppen:** Webservice-Anwendungen werden mit Name, Beschreibung, Basis-URL und optionalen Swagger- bzw. OData-Metadata-URLs erfasst. Anwendungen können in `ApplicationGroup`s organisiert werden. Die Seitenleiste zeigt alle Gruppen und Anwendungen als zugeklappten Baum; ein Klick auf eine Anwendung öffnet die Detailansicht.

**Endpunkte:** Jede Anwendung kann beliebig viele HTTP-Endpunkte besitzen. Endpunkte werden durch HTTP-Methode, relativen Pfad, optionale Header, Query-Parameter, Body und Authentifizierungstyp beschrieben. Sie können in `EndpointGroup`s zusammengefasst werden.

**Ausführung:** Endpunkte werden direkt aus der UI heraus ausgeführt. Request-Details und Antwort werden in der Oberfläche angezeigt. Bei einem Verbindungsfehler (kein HTTP-Status) wird automatisch ein Health-Check der zugehörigen Anwendung ausgelöst.

**Import:** Endpunkte können aus einer Swagger/OpenAPI-Definition oder einem OData-`$metadata`-Dokument importiert werden. Der Import zeigt zunächst eine Diff-Vorschau (neue, geänderte, entfernte Endpunkte); der Benutzer wählt selektiv aus, welche Änderungen übernommen werden sollen.

**Speichermodi:** Der Modus „Team" zeigt alle Anwendungen und Endpunkte für alle Benutzer. Im Modus „Benutzer" sieht jeder Benutzer nur seine eigenen Datensätze (gefiltert nach Windows-Benutzername). Der aktive Modus wird in der Kopfzeile umgeschaltet.

**Echtzeit-Synchronisation:** Im Team-Modus werden Änderungen über SignalR (`EndpointHub`) sofort an alle verbundenen Sitzungen gebroadcastet. Clients abonnieren dabei gezielt die Gruppen ihrer aktuell angezeigten Anwendungen und Gruppen.

**Authentifizierung der Endpunkte:** Beim Ausführen eines Endpunkts wählt der `EndpointExecutionService` die passende Authentifizierungsstrategie anhand des gespeicherten `AuthenticationType`. Passwörter und Tokens werden sicher im Windows Credential Manager (DPAPI, maschinengebunden) gespeichert.

## Beispiele

**Endpunkt anlegen:** Benutzer öffnet eine Anwendung, klickt „Bearbeiten" an einem Endpunkt (oder legt einen neuen an), trägt Methode, Pfad, ggf. Header und Authentifizierungstyp ein und speichert.

**Swagger-Import:** Benutzer öffnet eine Anwendung mit konfigurierter Swagger-URL und klickt „Swagger-Import". Es erscheint eine Vorschau mit neuen, geänderten und entfernten Endpunkten. Benutzer wählt gewünschte Änderungen und klickt „Übernehmen".

**Health-Check:** Benutzer klickt in der Anwendungsdetailansicht auf „Health-Check". Der Dialog zeigt, ob die Anwendung erreichbar ist. Bei Nichterreichbarkeit kann die Anwendung direkt entfernt werden.

## Einschränkungen

- Die Anwendung läuft ausschließlich unter Windows mit aktivierter Windows-Authentifizierung (IIS Negotiate). Ein Betrieb ohne IIS oder unter Linux ist ohne Anpassungen nicht möglich.
- Credentials werden maschinengebunden (DPAPI `CRED_PERSIST_LOCAL_MACHINE`) gespeichert; eine Übertragung auf andere Maschinen ist nicht vorgesehen.
- Konfigurationsänderungen (`DatabaseProvider`, Verbindungszeichenfolge, Log-Level) erfordern einen Neustart der Anwendung.
- Der Health-Check-Cooldown verhindert wiederholte Prüfungen innerhalb eines konfigurierbaren Zeitfensters (Standard: 60 Sekunden); in dieser Zeit gibt `CheckAsync` `null` zurück.
- OData-Import erzeugt nur Endpunkte für EntitySets (GET/POST) sowie Aktionen und Funktionen, keine weiteren OData-Operationstypen.
