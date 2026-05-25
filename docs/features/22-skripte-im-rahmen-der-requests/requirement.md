# Übersetzte Anforderung: Skripte und Variablen in der Swagger-Definition

## Fachliche Zusammenfassung

Der `SwaggerImportService` soll beim Import einer Swagger/OpenAPI-Definition nicht nur `Name`, `Method` und `RelativePath` der Endpunkte erzeugen, sondern auch `PreRequestScript`, `PostRequestScript` und `AuthenticationType` (inkl. zugehörigem Bearer-Token-Wert) belegen können — sofern die Definition entsprechende Erweiterungsfelder enthält. Konkret soll der `/authenticate`-Endpunkt beim Import mit einem definierten Post-Request-Skript versehen werden; alle übrigen Endpunkte sollen mit `AuthenticationType.BearerToken` und dem Platzhalter-Wert `{{schnittstellenzentrale.authToken}}` als Bearer-Token sowie einem Post-Request-Skript für die Token-Erneuerung angelegt werden. Die Skripte und Token-Konfigurationen müssen dazu aus der Swagger-Definition auslesbar sein, z. B. über OpenAPI-Erweiterungsfelder (`x-sz-post-request-script`, `x-sz-bearer-token` o. ä.).

## Betroffene Klassen und Komponenten

### Datenmodell

- `Endpoint` — bereits vorhanden mit `PreRequestScript` und `PostRequestScript`; `AuthenticationType` bereits vorhanden. **Kein neues Feld erforderlich**, sofern der Bearer-Token-Wert wie bisher über den `ICredentialService` gespeichert wird.
- **Offene Frage:** Der Bearer-Token-Wert wird aktuell im Windows Credential Manager gespeichert (`ICredentialService.SavePassword`), **nicht** im `Endpoint`-Datensatz selbst. Um beim Import einen Platzhalter-Wert (`{{schnittstellenzentrale.authToken}}`) als Token einzutragen, müsste entweder:
  - der `SwaggerImportService` nach dem Anlegen des Endpunkts `ICredentialService.SavePassword` aufrufen, oder
  - das `Endpoint`-Modell um ein Feld `BearerTokenValue` (persistiert, enthält den Rohwert inkl. Platzhalter) erweitert werden, das den Credential-Manager für importierte Endpunkte ergänzt oder ersetzt.

### Logikklassen / Services

- `SwaggerImportService` (`SwaggerImportService.cs`) — muss erweitert werden, um beim Parsen der `OpenApiOperation`-Objekte Erweiterungsfelder auszulesen und die entsprechenden Felder der `Endpoint`-Instanz zu belegen.
- `ISwaggerImportService` — Signatur bleibt voraussichtlich unverändert; ggf. Erweiterung, wenn `ApplyDiffAsync` die Credential-Speicherung übernehmen soll.

### Interfaces

- Kein neues Interface erwartet. Ggf. `ICredentialService` muss aus `SwaggerImportService` aufrufbar sein (aktuell nicht injiziert).

### Enums

- Kein neuer Enum-Wert.

### UI-Komponenten / Controller

- Keine direkten UI-Änderungen erwartet. Der Import-Ablauf (Aufruf von `SwaggerImportService.ImportAsync` und `ApplyDiffAsync`) bleibt unverändert.

### Tests

- `SwaggerImportServiceTests` — neue Testfälle für Endpunkte mit Erweiterungsfeldern (Skript und Bearer-Token-Platzhalter).

## Implementierungsansatz

Die OpenAPI-Spezifikation erlaubt beliebige Erweiterungsfelder mit dem Präfix `x-`. Der `SwaggerImportService` liest diese Felder aus den `OpenApiOperation.Extensions` aus und überträgt sie auf die neu erstellten `Endpoint`-Instanzen:

- `x-sz-post-request-script` → `Endpoint.PostRequestScript`
- `x-sz-pre-request-script` → `Endpoint.PreRequestScript`
- `x-sz-bearer-token` → `Endpoint.AuthenticationType = BearerToken` + Ablage des Token-Werts (siehe offene Frage zur Speicherung)

`ImportDiffCalculator.Calculate` und `ApplyDiffAsync` müssen die neuen Felder bei der Diff-Berechnung und beim Anlegen/Aktualisieren berücksichtigen.

Abhängigkeiten:
- `Microsoft.OpenApi` (bereits vorhanden) — `OpenApiOperation.Extensions` liefert die Erweiterungswerte.
- `ICredentialService` — ggf. neu zu injizieren in `SwaggerImportService`, falls der Bearer-Token-Wert im Credential Manager gespeichert werden soll.
- `IEndpointRepository` (bereits injiziert) — keine Änderung erforderlich, sofern `Endpoint` die Felder bereits trägt.

## Konfiguration

Die Zuordnung von Erweiterungsfeld-Namen zu `Endpoint`-Eigenschaften ist implizit durch die Implementierung festgelegt (keine externe Konfiguration). Die konkret zu verwendenden Feldnamen (`x-sz-post-request-script` etc.) müssen mit dem Kunden abgestimmt werden — sie müssen in der Swagger-Definition eingetragen werden und sind damit Teil der Vereinbarung zwischen API-Anbieter und Schnittstellenzentrale.

## Offene Fragen

1. **Speicherung des Bearer-Token-Werts:** Der Bearer-Token-Wert wird aktuell über den Windows Credential Manager gespeichert, nicht im `Endpoint`-Datensatz. Beim Import ist kein Benutzerdialog möglich, der den Wert abfragt. Zwei Optionen:
   - Option A: `SwaggerImportService` ruft `ICredentialService.SavePassword` mit dem Platzhalter-Wert auf — der Platzhalter wird wie ein echter Token-Wert behandelt.
   - Option B: Das `Endpoint`-Modell wird um ein neues Feld (z. B. `BearerTokenHint`) erweitert, das den Rohwert (inkl. Platzhalter) direkt persistiert; `EndpointExecutionService` liest zuerst den Credential Manager, fällt dann auf dieses Feld zurück.
   - **Entscheidung erforderlich**, bevor die Implementierung beginnt.

2. **Namenskonvention der Erweiterungsfelder:** Wie sollen die OpenAPI-Erweiterungsfelder benannt werden (`x-sz-post-request-script`, `x-sz-bearer-token`, anderes Schema)? Die Konvention muss mit dem API-Anbieter abgestimmt werden.

3. **Verhalten beim Re-Import (Diff):** Wenn ein Endpunkt bereits existiert und ein Re-Import erfolgt, sollen dann Skripte und Bearer-Token überschrieben oder beibehalten werden? Der `ImportDiffCalculator` muss das Verhalten für die neuen Felder festlegen.

4. **Gilt die Anforderung nur für den Swagger-Import oder auch für die manuelle Anlage?** Der Kunde beschreibt ausschließlich den Swagger-Import-Weg — Skripte und Bearer-Token können auch manuell gesetzt werden (bestehende Funktion). Es ist zu klären, ob ein separates UI-Feature zur Vorbelegung beim manuellen Anlegen gewünscht ist.

5. **Pfad-Matching für `/authenticate`:** Der Sonderfall „`/authenticate`-Endpunkt erhält ein anderes Skript als alle übrigen" muss in der Swagger-Definition durch per-Endpunkt-Erweiterungsfelder abgebildet werden — nicht durch eine hartcodierte Pfad-Erkennung im Import-Service. Stimmt der Kunde diesem Ansatz zu?
