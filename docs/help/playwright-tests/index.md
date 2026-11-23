# Playwright-Tests

Die Playwright-Testinfrastruktur ergänzt das Testprojekt `Schnittstellenzentrale.Tests` um End-to-End-UI-Tests. Sie prüft die wichtigsten Benutzerabläufe in einem echten Chromium-Browser. Alle Playwright-Tests tragen den Trait `Category=OsInterface` und laufen in der CI als eigener best-effort-Job `os-interface-tests` — ihre Coverage fließt **nicht** mehr in das 70-%-Line-Coverage-Gate ein (das speist sich ausschließlich aus der regulären Lane mit gezielten bUnit-Tests, siehe [OS-Schnittstellen-Tests](../stabilitaet/os-interface-tests.md)).

## Inhalt

- [Beschreibung](beschreibung.md)
- [Technischer Ablauf](ablauf-technisch.md)
- [Installation & Konfiguration](installation.md)
- [Business Rules](business-rules.md)
