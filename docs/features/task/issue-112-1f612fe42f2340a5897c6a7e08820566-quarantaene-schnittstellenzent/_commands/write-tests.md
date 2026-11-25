# Tests schreiben

Dieses Kommando gilt für das Schreiben neuer Testmethoden — sowohl bei der Feature-Implementierung als auch für eigenständige Test-Sessions.

**Ziel:** Kleine, fokussierte Testklassen, die nach Themengruppen aufgeteilt sind, damit Korrekturen minimal invasiv bleiben.

---

## Testklassen-Aufteilung

Testmethoden werden **nach Themengruppen** auf separate Klassen verteilt. Niemals alle Tests einer Klasse in eine einzelne Testklasse schreiben.

**Namensschema:** `{TestedClass}Tests_{Thema}`

Beispiele für `Customer`:
- `CustomerTests_Properties` — Getter/Setter, Validierung von Eigenschaften
- `CustomerTests_PaymentMethods` — Zahlungsarten hinzufügen, entfernen, auswählen
- `CustomerTests_MailSettings` — E-Mail-Konfiguration und -Versand
- `CustomerTests_Lifecycle` — Anlegen, Aktivieren, Deaktivieren, Löschen

**Wann aufteilen:**

Eine neue Testklasse anlegen, wenn:
- mehr als 5 Testmethoden dasselbe Thema (Methoden-Gruppe, Feature-Bereich) abdecken, **oder**
- die Testmethoden unterschiedliche Abhängigkeiten (Mocks, Fixtures) benötigen, **oder**
- das Thema klar abgrenzbar ist — auch wenn es aktuell weniger als 5 Methoden sind

**Dateiname** entspricht dem Klassennamen. Eine Klasse pro Datei.

---

## Testmethoden

**Namensschema:** `MethodName_Szenario_ErwartetesErgebnis`

Beispiele:
- `AddPaymentMethod_WithDuplicateIban_ThrowsInvalidOperationException`
- `SetEmail_WithInvalidFormat_ReturnsFalse`
- `Deactivate_WhenAlreadyInactive_DoesNothing`

**Aufbau:** Arrange → Act → Assert — immer in dieser Reihenfolge, keine Logik im Test.

**Arrange — Domain-treue Testdaten:** Fixtures und Mocks bilden die Wertebereiche und Konventionen der Domain ab, nicht die Annahmen des Implementierers. Vorzeichenbehaftete Felder erhalten auch negative Werte, Grenzwerte werden ausgeschöpft, Enums vollständig abgedeckt, Null-Semantik wird wie im Domain-Modell behandelt. Orientierung sind der Domain-Code und reale Seed-/Demo-Daten. Testdaten, die nur die Annahmen der Implementierung spiegeln, erzeugen selbstreferenzielle Tests: intern konsistent grün, aber inkorrekt gegen den Domain-Vertrag.

Jede Testmethode prüft **genau ein Verhalten**. Kein Verketten von mehreren Assert-Blöcken für unterschiedliche Szenarien in einer Methode.

---

## Ablauf

### 1. Themengruppen bestimmen

Bevor du Code schreibst: Identifiziere alle zu testenden Szenarien und gruppiere sie. Schreibe die Gruppennamen auf — das sind die Testklassen-Suffixe.

### 2. Testklassen anlegen oder zuordnen

- Existiert bereits eine passende Testklasse (`CustomerTests_PaymentMethods`)? Methoden dort einfügen.
- Gibt es noch keine? Neue Datei mit neuer Klasse anlegen.
- Nie Methoden zu einer Klasse hinzufügen, die thematisch nicht passt.

### 3. Testmethoden implementieren

Für jede Testmethode:
- Arrange: Testdaten und Mocks aufbauen
- Act: Die zu testende Methode aufrufen
- Assert: Ergebnis mit FluentAssertions prüfen

### 4. Tests ausführen

```
dotnet test
```

Alle neuen Tests müssen grün sein. Kein bestehender Test darf rot werden.

---

## Integrations- und E2E-Tests

Dasselbe Prinzip gilt für Integrations- und E2E-Tests. Aufteilen nach Ablauf oder Szenario-Gruppe:

- `OrderFlow_PlacementTests`
- `OrderFlow_PaymentTests`
- `OrderFlow_FulfillmentTests`

### Pflichtszenarien pro Themengruppe

Jede E2E-Testklasse deckt ein Thema vollständig ab — Happy Path **und** Fehlerfälle gehören zusammen in dieselbe Klasse.

Pro Themengruppe müssen enthalten sein:
- **Happy Path** — ein vollständiger Durchlauf mit korrekten Daten
- **Fehlkonfigurationen** — fehlende oder ungültige Pflichtfelder, nicht erfüllte Vorbedingungen, externe Fehler (Downstream-Fehler, Timeouts), Konfigurationsfehler die erst zur Laufzeit sichtbar werden

Bei Fehlerfall-Tests wird **sowohl der Fehler als auch die Fehlermeldung** geprüft — nicht nur ob etwas schiefgelaufen ist, sondern ob das System die richtige Fehlermeldung zurückgibt. Das stellt sicher, dass das Fehlerhandling des Ablaufs korrekt implementiert ist.

Beispiel für `OrderFlow_PlacementTests`:
- `PlaceOrder_WithValidData_CreatesOrderSuccessfully` — Happy Path
- `PlaceOrder_WithMissingAddress_ReturnsBadRequest_WithValidationMessage`
- `PlaceOrder_WhenInventoryUnavailable_ReturnsConflict_WithStockErrorMessage`

### Geometrische Assertions bei Dialogen und Overlays

E2E-Tests für Dialoge, Overlays und Panel-Container enthalten eine BoundingBox-Assertion: Der sichtbare Inhalt liegt vollständig innerhalb der Begrenzung des Dialogs bzw. Containers — kein Element ragt über dessen Ränder hinaus. Damit werden Layout-Defekte (Überlauf, abgeschnittene Inhalte) CI-erzwungen abgefangen statt aufmerksamkeitsabhängig in der Abnahme. Referenzimplementierung: `PostingRulesOverlay_OpensAndCloses_ViaRibbon` in FinanceManager (Commit `0a94bb74`).

---

## Hinweise

- Nutze dieselben Test-Frameworks wie im restlichen Projekt (aus Bestandsaufnahme ablesen — typisch: xUnit, FluentAssertions, Moq).
- Keine Kommentare in Testmethoden — der Name erklärt das Szenario.
- Für Regressionstests bei Bugfixes: Ablauf aus `/fix-with-test` folgen (Test zuerst rot machen, dann Fix implementieren).
- Keine spekulativen Tests für Szenarien, die nicht im Plan oder der Anforderung stehen.
