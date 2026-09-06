# Git-Hooks - Beschreibung

## Zweck

Die Git-Hooks unter `.githooks/` sichern lokale Qualitätsregeln ab, bevor Änderungen das Repository verlassen. Sie stammen aus der Pattern-Collection und werden unverändert versioniert. Voraussetzung sind Python 3 und das .NET SDK.

## Aktivierung

Einmalig pro Klon ausführen:

```
.githooks\install-hooks.cmd    # Windows
./.githooks/install-hooks.sh   # Linux/macOS
```

Das Skript setzt `core.hooksPath` auf `.githooks` (`git config --local core.hooksPath .githooks`).

## Prüfungen beim Commit (pre-commit)

Blockierend:

- Direkte Commits auf `main`/`staging` werden verweigert.
- `translation-check.py`: resx-Konsistenz (verwendete Schlüssel vorhanden, Sprachpakete vollständig, resx-Header valide).
- `csproj-xmldoc-check.py`: `GenerateDocumentationFile` aktiv, CS1591 als Fehler konfiguriert, kein `#pragma warning disable` für XML-Dokumentationscodes, vollständige `<param>`/`<typeparam>`/`<returns>`/`<response>`-Tags.
- `razor-l10n-check.py`: keine hartcodierten UI-Strings in `.razor`-Dateien.

Warnend (blockieren den Commit nicht):

- `razor-usage-check.py`: verwaiste Razor-Komponenten.
- `no-notimplemented-check.py`: `NotImplementedException` und Throw-only-Stubs.
- `enum-coverage-check.py`: Enum-Werte ohne Testabdeckung.

Optional (werden übersprungen, wenn die Projekte nicht existieren): `SecretScan`, `MarkdownLinkCheck`.

## Prüfungen beim Push (pre-push)

Blockierend, jeweils im Gesamtmodus (`--all --strict`):

- Direkte Pushes auf `main`/`staging` werden verweigert.
- `no-notimplemented-check.py`: keine `NotImplementedException`/Throw-only-Stubs im gesamten Repo.
- `razor-usage-check.py`: keine verwaisten Razor-Komponenten im gesamten Repo.
- `enum-coverage-check.py`: alle `public`/`internal`-Enum-Werte müssen in Testdateien vorkommen.

## Hinweis

Die Checks dürfen nicht abgeschwächt werden; Fehler sind in der Anwendung zu beheben.
