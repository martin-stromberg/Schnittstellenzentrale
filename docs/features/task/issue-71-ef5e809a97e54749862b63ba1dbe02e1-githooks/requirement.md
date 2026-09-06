# Übersetzte Anforderung – GitHooks

Quelle: `issue.md` (Aufgaben-ID ef5e809a-97e5-4749-862b-63ba1dbe02e1)

## Ausgangslage

Das Repository <https://github.com/martin-stromberg/Pattern-Collection.git> enthält
unter `Git-Hooks/githooks/` einen Satz von Git-Hooks (pre-commit, pre-push und
zugehörige Python-Prüfskripte). Diese Hooks sollen in das aktuelle Repository
(Schnittstellenzentrale) übernommen werden.

## Ziel

1. Die aktuellen Versionen der Git-Hooks aus dem Pattern-Collection-Repository
   werden in dieses Repository übernommen. Eventuell bereits vorhandene ältere
   Versionen werden vollständig ersetzt.
2. Die Hooks werden installiert/aktiviert (`core.hooksPath` auf `.githooks`,
   wie vom mitgelieferten `install-hooks`-Skript vorgesehen).
3. Fehlermeldungen, die die Hooks beim Commit oder Push auslösen, werden durch
   Korrekturen **in der Anwendung** behoben. Die Prüfungen selbst dürfen
   **auf keinen Fall entschärft oder umgangen** werden.
4. Bei großer Fehlerliste wird thematisch aufgeteilt und systematisch
   abgearbeitet (lifecycle-Agenten).

## Enthaltene Prüfungen (aus dem Pattern-Collection-Repo, Stand 9683e0c)

- `pre-commit` (blockierend): Branch-Blocker für `main`/`staging`,
  `translation-check.py` (resx-Konsistenz, Header, verwendete Schlüssel),
  `csproj-xmldoc-check.py` (GenerateDocumentationFile, CS1591 als Fehler,
  kein `#pragma warning disable` für XML-Doc-Codes, vollständige
  `<param>`/`<typeparam>`/`<returns>`/`<response>`-Tags),
  `razor-l10n-check.py` (keine hartcodierten UI-Strings),
  warnend: `razor-usage-check.py`, `no-notimplemented-check.py`,
  `enum-coverage-check.py`; optional `SecretScan`/`MarkdownLinkCheck`,
  falls entsprechende Projekte existieren.
- `pre-push` (blockierend): Push-Blocker für `main`/`staging` sowie
  `no-notimplemented-check.py`, `razor-usage-check.py` und
  `enum-coverage-check.py` jeweils mit `--all --strict` (gesamtes Repo).

## Abnahmekriterien

- `.githooks/` liegt im Repo und ist per `core.hooksPath` aktiviert.
- Alle blockierenden Prüfungen laufen im `--all`-Modus fehlerfrei durch
  (Commit- und Push-Bedingungen erfüllt).
- Keine Prüfskripte wurden verändert oder abgeschwächt.
- Build und bestehende Tests bleiben grün.
