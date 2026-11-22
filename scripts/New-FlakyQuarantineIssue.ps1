#Requires -Version 7.0
<#
.SYNOPSIS
    Legt fuer jeden Quarantaene-Kandidaten aus dem Analyzer-JSON ein dedupliziertes
    GitHub-Issue an.

.DESCRIPTION
    Liest die von scripts/Analyze-FlakyTestResults.ps1 -CandidatesFile geschriebene
    JSON-Datei ([{test, executions, failures, flakeEvents, failureRate}]) und legt
    pro Kandidat ein Issue "Quarantaene: <test-fqn>" an — aber nur, wenn kein
    <b>offenes</b> Issue mit diesem Titel existiert (Dedupe primaer ueber die
    Titel-Suche "in:title \"Quarantaene: <fqn>\"" --state open; die Volltext-Suche
    tokenisiert an Satzzeichen und ist dafuer ungeeignet). Ein geschlossenes oder
    nicht existierendes Issue fuehrt zu einem neuen Issue. Der Body traegt
    zusaetzlich den stabilen Marker <!-- flaky-quarantine:<fqn> --> als Beleg
    und als Fallback-Anker.

    Komplett best-effort: Bei PRs aus Forks ist issues: write / GH_TOKEN wirkungslos
    (GITHUB_TOKEN dort read-only) — gh-Fehler werden als Warnung protokolliert und
    das Skript endet mit Exit 0 statt zu werfen, damit Fork-PRs keinen roten Step
    trotz funktionierendem Rest zeigen. Fehlende oder leere Kandidaten-Datei ist
    ein No-op (Exit 0).

.PARAMETER CandidatesFile
    Pfad zur JSON-Datei mit den Quarantaene-Kandidaten.

.EXAMPLE
    ./scripts/New-FlakyQuarantineIssue.ps1 -CandidatesFile flaky-quarantine-candidates.json
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$CandidatesFile
)

$ErrorActionPreference = "Continue"

if (-not (Test-Path $CandidatesFile)) {
    Write-Host "Keine Kandidaten-Datei '$CandidatesFile' — nichts zu tun."
    exit 0
}

$candidates = @()
try {
    $raw = Get-Content -LiteralPath $CandidatesFile -Raw
    if (-not [string]::IsNullOrWhiteSpace($raw)) {
        $parsed = $raw | ConvertFrom-Json
        # Einzelnes Objekt oder Array einheitlich behandeln.
        $candidates = @($parsed)
    }
}
catch {
    Write-Warning "Kandidaten-Datei '$CandidatesFile' nicht auswertbar: $_"
    exit 0
}

if ($candidates.Count -eq 0) {
    Write-Host "Keine Quarantaene-Kandidaten in '$CandidatesFile' — nichts zu tun."
    exit 0
}

Write-Host "==> $($candidates.Count) Quarantaene-Kandidat(en) aus '$CandidatesFile'."

foreach ($candidate in $candidates) {
    $fqn = "$($candidate.test)"
    if ([string]::IsNullOrWhiteSpace($fqn)) { continue }
    $title = "Quarantaene: $fqn"
    $marker = "<!-- flaky-quarantine:$fqn -->"

    # Dedupe: offenes Issue mit identischem Titel vorhanden?
    $existing = @()
    # Kein 2>&1 vor ConvertFrom-Json: gh-Hinweise auf stderr (Update-Notices,
    # Deprecations) wuerden den JSON-String verunreinigen und den Parse brechen.
    # stderr laeuft direkt ins Log; der Fehlerindikator bleibt $LASTEXITCODE.
    $listOutput = & gh issue list --search "in:title `"$title`"" --state open --json number,title
    if ($LASTEXITCODE -ne 0) {
        # Fehlende Rechte (Fork-PR), kein Repo-Kontext, API-Fehler — tolerieren.
        Write-Warning "gh issue list fehlgeschlagen fuer '$title' — Kandidat uebersprungen (best-effort): $listOutput"
        continue
    }
    try {
        $parsedList = $listOutput | ConvertFrom-Json
        $existing = @($parsedList | Where-Object { $_.title -eq $title })
    }
    catch {
        Write-Warning "Antwort von gh issue list nicht auswertbar fuer '$title' — Kandidat uebersprungen: $_"
        continue
    }

    if ($existing.Count -gt 0) {
        Write-Host "    Uebersprungen (offenes Issue #$($existing[0].number) vorhanden): $title"
        continue
    }

    $body = @"
$marker

Der Flake-Analyzer (scripts/Analyze-FlakyTestResults.ps1) hat diesen Test als
Quarantaene-Kandidaten eingestuft.

| Kennzahl | Wert |
|---|---|
| Test | ``$fqn`` |
| Ausfuehrungen | $($candidate.executions) |
| Fehlschlaege | $($candidate.failures) |
| Flake-Ereignisse | $($candidate.flakeEvents) |
| Fehlrate | $([math]::Round([double]$candidate.failureRate * 100, 1)) % |

Empfohlene Schritte:

1. Fehlerbild analysieren (TRX-/Trace-Artefakte des os-interface-tests-Jobs,
   ``npx playwright show-trace`` fuer Playwright-Tests).
2. Ursache beheben oder Test haerten (siehe docs/help/stabilitaet/os-interface-tests.md).
3. Test bei dokumentiertem Flake mit ``[Flaky]`` markieren (nur in Kombination mit
   ``[OsInterface]`` — Vergaberegeln in der Doku) oder das Issue nach dem Fix
   schliessen.

_Automatisch erstellt vom os-interface-tests CI-Job._
"@

    $createOutput = & gh issue create --title $title --body $body 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "    Issue angelegt: $title -> $createOutput"
    }
    else {
        # Fork-PR ohne issues: write o. ae. — tolerieren, kein Step-Rot.
        Write-Warning "gh issue create fehlgeschlagen fuer '$title' (best-effort): $createOutput"
    }
}

exit 0
