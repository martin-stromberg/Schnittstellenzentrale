#Requires -Version 7.0
<#
.SYNOPSIS
    Laedt die test-results-os-interface-TRX-Artefakte der letzten CI-Laeufe desselben
    Workflows herunter (Persistenz der Flake-Statistik ueber Artefakt-Kette).

.DESCRIPTION
    Ermittelt per "gh run list" die letzten -MaxRuns abgeschlossenen Laeufe des
    angegebenen Workflows und laedt pro Lauf das Artefakt -ArtifactName nach
    <Destination>/<run-id>/ herunter. Der Analyzer
    (scripts/Analyze-FlakyTestResults.ps1) wertet dieses Verzeichnis gemeinsam mit dem
    aktuellen TestResults-Verzeichnis aus, damit die Flake-Statistik ueber Laeufe
    hinweg persistiert, ohne eine committed Datei zu brauchen.

    Komplett best-effort: fehlende Artefakte (aeltere Runs ohne OS-Artefakt,
    Retention 14 d), API-Fehler oder fehlende Rechte (Fork-PRs) werden pro Lauf
    toleriert und nur als Warnung protokolliert — der Exit-Code ist immer 0.

.PARAMETER WorkflowFile
    Dateiname des Workflows (z. B. "pr-staging-ci.yml", "staging-ci.yml"). Der
    History-Download darf nur Artefakte desselben Workflows holen — artefakt-
    uebergreifende Downloads ueber Workflow-Grenzen waeren unsauber.

.PARAMETER ArtifactName
    Name des TRX-Artefakts (z. B. "test-results-os-interface-pr" bzw.
    "test-results-os-interface-staging").

.PARAMETER Destination
    Zielverzeichnis fuer die heruntergeladenen Artefakte. Standard: "trx-history".

.PARAMETER MaxRuns
    Maximale Anzahl historischer Laeufe. Standard: 15.

.EXAMPLE
    ./scripts/Download-OsInterfaceTrxHistory.ps1 -WorkflowFile pr-staging-ci.yml -ArtifactName test-results-os-interface-pr
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$WorkflowFile,
    [Parameter(Mandatory = $true)]
    [string]$ArtifactName,
    [string]$Destination = "trx-history",
    [int]$MaxRuns = 15
)

# Best-effort: Einzelne Fehler duerfen den Schritt nie brechen.
$ErrorActionPreference = "Continue"

$runIds = @()
try {
    # Kein 2>&1 vor ConvertFrom-Json: gh-Hinweise auf stderr (Update-Notices,
    # Deprecations) wuerden den JSON-String verunreinigen und den Parse brechen.
    # stderr laeuft direkt ins Log; der Fehlerindikator bleibt $LASTEXITCODE.
    $runListJson = & gh run list --workflow $WorkflowFile --limit $MaxRuns --status completed --json databaseId
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "gh run list fuer Workflow '$WorkflowFile' fehlgeschlagen: $runListJson"
        exit 0
    }
    $runIds = @($runListJson | ConvertFrom-Json | ForEach-Object { $_.databaseId })
}
catch {
    Write-Warning "Lauf-Liste fuer Workflow '$WorkflowFile' nicht auswertbar: $_"
    exit 0
}

Write-Host "==> $($runIds.Count) abgeschlossene Lauf/Laeufe von '$WorkflowFile' gefunden (MaxRuns=$MaxRuns)."

$downloaded = 0
foreach ($runId in $runIds) {
    $target = Join-Path $Destination "$runId"
    try {
        & gh run download $runId -n $ArtifactName -D $target 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $downloaded++
        }
        else {
            # Kein Artefakt in diesem Lauf (aelter als die Lane, Retention abgelaufen,
            # fehlgeschlagener Upload) — normal, kein Fehler des Skripts.
            Write-Host "    Lauf ${runId}: kein Artefakt '$ArtifactName' verfuegbar — uebersprungen."
        }
    }
    catch {
        Write-Warning "Download von Artefakt '$ArtifactName' aus Lauf $runId fehlgeschlagen: $_"
    }
}

Write-Host "==> $downloaded von $($runIds.Count) Laeufen lieferten Artefakt '$ArtifactName' nach '$Destination'."
exit 0
