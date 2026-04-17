<#
.SYNOPSIS
Validates that canonical qlog requirement IDs have owning requirement-home test traits.

.DESCRIPTION
Scans `specs/requirements/qlog/*.json` for requirement identifiers and verifies that
each selected requirement appears in at least one `[Trait("Requirement", "...")]`
usage under `tests/`. This is a lightweight audit for requirement-home coverage,
not a substitute for assessing proof quality.
#>

param(
    [string]$RepoRoot = "",
    [string]$RequirementsRoot = "",
    [string]$TestsRoot = "",
    [string[]]$RequirementFamilies = @("REQ-QLOG")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-ExistingPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Path does not exist: $Path"
    }

    return (Resolve-Path -LiteralPath $Path).Path
}

function Get-NormalizedStringList {
    param([AllowNull()][string[]]$Items = @())

    if ($null -eq $Items) {
        return @()
    }

    return @(
        $Items |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object { $_ -split ',' } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object { $_.Trim() } |
        Select-Object -Unique
    )
}

function Test-RequirementFamilyMatch {
    param(
        [Parameter(Mandatory = $true)][string]$RequirementId,
        [AllowNull()][string[]]$Families = @()
    )

    $normalizedFamilies = @(Get-NormalizedStringList -Items $Families)
    if ($normalizedFamilies.Count -eq 0) {
        return $true
    }

    foreach ($family in $normalizedFamilies) {
        if ($RequirementId.StartsWith($family, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Path $PSScriptRoot -Parent
}

if ([string]::IsNullOrWhiteSpace($RequirementsRoot)) {
    $RequirementsRoot = Join-Path $RepoRoot "specs\requirements\qlog"
}

if ([string]::IsNullOrWhiteSpace($TestsRoot)) {
    $TestsRoot = Join-Path $RepoRoot "tests"
}

$resolvedRequirementsRoot = Resolve-ExistingPath -Path $RequirementsRoot
$resolvedTestsRoot = Resolve-ExistingPath -Path $TestsRoot
$selectedFamilies = @(Get-NormalizedStringList -Items $RequirementFamilies)

$requirementIds = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
$requirementFiles = Get-ChildItem -LiteralPath $resolvedRequirementsRoot -Filter *.json -File | Sort-Object Name
foreach ($file in $requirementFiles) {
    $artifact = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json -Depth 100
    foreach ($requirement in @($artifact.requirements)) {
        $requirementId = [string]$requirement.id
        if ([string]::IsNullOrWhiteSpace($requirementId)) {
            continue
        }

        if (Test-RequirementFamilyMatch -RequirementId $requirementId -Families $selectedFamilies) {
            [void]$requirementIds.Add($requirementId)
        }
    }
}

$traitReferences = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
$traitPattern = 'Trait\("Requirement",\s*"([^"]+)"\)'
$testFiles = Get-ChildItem -LiteralPath $resolvedTestsRoot -Recurse -Filter *.cs -File
foreach ($file in $testFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    foreach ($match in [regex]::Matches($content, $traitPattern)) {
        $requirementId = $match.Groups[1].Value
        if (Test-RequirementFamilyMatch -RequirementId $requirementId -Families $selectedFamilies) {
            [void]$traitReferences.Add($requirementId)
        }
    }
}

$selectedRequirementIds = @($requirementIds | Sort-Object)
$selectedTraitReferences = @($traitReferences | Sort-Object)
$missingRequirementIds = @($selectedRequirementIds | Where-Object { $traitReferences -notcontains $_ })
$orphanTraitReferences = @($selectedTraitReferences | Where-Object { $requirementIds -notcontains $_ })

Write-Host "Requirement families: $([string]::Join(', ', $selectedFamilies))"
Write-Host "Canonical requirements scanned: $($selectedRequirementIds.Count)"
Write-Host "Requirement-home references found: $($selectedTraitReferences.Count)"

if ($orphanTraitReferences.Count -gt 0) {
    Write-Warning "Requirement-home references with no matching canonical requirement:"
    foreach ($requirementId in $orphanTraitReferences) {
        Write-Warning "  - $requirementId"
    }
}

if ($missingRequirementIds.Count -gt 0) {
    Write-Error ("Missing requirement-home coverage for {0} canonical requirement(s):{1}{2}" -f $missingRequirementIds.Count, [Environment]::NewLine, (($missingRequirementIds | ForEach-Object { "  - $_" }) -join [Environment]::NewLine))
    exit 1
}

Write-Host "Requirement-home coverage passed." -ForegroundColor Green
