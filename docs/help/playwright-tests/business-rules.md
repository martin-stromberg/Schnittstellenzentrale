# Playwright-Tests — Business Rules

## Coverage-Gate mit 70 % Line Coverage

**Beschreibung:** Das bestehende CI-Gate verlangt eine globale Line-Coverage von mindestens `70 %`. Es ist kein Produkt-Feature, sondern eine Qualitätsvorgabe für die Testbasis.

**Bedingungen:**
- Die Auswertung erfolgt über `dotnet test Schnittstellenzentrale.slnx --collect:"XPlat Code Coverage"`.
- Die Schwelle bleibt unverändert; sie wird nicht durch Anpassung der Metrik gelöst.
- Die sichere Methode ist die Ergänzung fehlender UI- und State-Tests.

**Verhalten:**
- Wenn die Line-Coverage unter `70 %` liegt, schlägt der CI-Lauf fehl.
- Wenn gezielte bUnit-Tests für bislang unabgedeckte Pfade ergänzt werden, steigt die Abdeckung und der Lauf bleibt grün.
- Playwright-Tests dienen als Ergänzung, aber nicht als alleinige Begründung für das Qualitätsziel.

**Umsetzung:** `TestMockFactory.CreateCoverageScenarioDependencies`, `ApplicationContentViewTests`, `EnvironmentSelectorTests`, `AppShellTests`, `MainLayoutTests` — die ergänzten Tests decken kritische Branches, Fehlerpfade und Wiederherstellungszustände ab, die bisher für die Coverage-Schwelle fehlten.

---

## bUnit vor Playwright für UI-Pfade

**Beschreibung:** Für Blazor-Komponenten ist `bUnit` die bevorzugte Teststrategie, wenn die Abdeckung eines einzelnen Zustands oder Dialogpfads erhöht werden soll.

**Bedingungen:**
- Die Komponente ist UI-lastig, aber als isolierte Einheit testbar.
- Die Logik lässt sich durch Mock-Services auf die relevante Sichtbarkeit und State-Änderung reduzieren.

**Verhalten:**
- Fehlerzustände, lokale Speicherung und Render-Branches werden schnell und reproduzierbar validiert.
- Die Testlaufzeit bleibt kurz und stabil, wodurch das Coverage-Gate ohne Excessive E2E-Ausführung erreicht werden kann.

**Umsetzung:** Die Tests `ApplicationContentViewTests`, `EnvironmentSelectorTests`, `AppShellTests` und `MainLayoutTests` verwenden `BunitContext` und gezielte Mocks, um UI-State und Interaktion isoliert zu prüfen.

---

## Keine unkontrollierten Produktänderungen

**Beschreibung:** Die Aufgabe ist testorientiert; der produktive Code wird nicht aus freien Stücken erweitert oder refaktoriert, sondern nur so weit angepasst, wie die Testbarkeit es erfordert.

**Verhalten:**
- Die neuen Tests decken echte Zustandswechsel und Fehlerpfade ab.
- Geänderte Hilfsmethoden sind auf gemeinsame Mock-/Fixture-Szenarien begrenzt.
- Für die Coverage-Stabilisierung werden keine funktionalen Änderungen an der Systemlogik erwartet.

**Umsetzung:** `TestMockFactory` und `CoverageTestFactory` bündeln gemeinsame Testdaten und Mocks; sie ersetzen keine Produktlogik, sondern stellen einen stabilen Testkontext für die Coverage-Ergänzungen bereit.
