# Bestandsaufnahme: Coverage Gate schlägt fehl

Die Analyse fokussiert sich auf die vorhandene Test- und UI-Infrastruktur rund um die Coverage-Sicherung. Im Code ist keine produktive Erweiterung der Geschäftslogik erkennbar; die relevante Fläche liegt in Blazor-Komponenten und den dafür bereits vorhandenen bUnit-/Playwright-Tests.

## Zusammenfassung

- `Schnittstellenzentrale` enthält bereits mehrere bUnit-Tests, die layout- und interaktionsbezogene Blazor-Komponenten absichern, etwa `AppShell`, `EnvironmentSelector` und `ApplicationContentView`.
- Die Testbasis für das Coverage-Gate ist damit nicht „leer“, sondern über unterschiedliche UI- und Integrationstesttypen verteilt.
- Relevante Abhängigkeiten für die vorhandenen Komponententests sind `IApplicationApiClient`, `IApplicationService`, `IActiveEnvironmentService` und `ISystemEnvironmentRepository`.
- Für den Coverage-Gate-Fall ist das wesentliche Problem daher eher die Lücken bei bislang ungetesteten Dialog-/Editor-Komponenten als ein fehlender produktiver Codepfad.
- Die Playwright-Infrastruktur ergänzt die bUnit-Abdeckung mit End-to-End-Flows wie CRUD, Navigation, Layout und Import-Szenarien.

## Details

- [Logik](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
