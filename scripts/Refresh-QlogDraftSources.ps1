[CmdletBinding()]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$SnapshotDirectory = '',
    [string]$ManifestPath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($SnapshotDirectory)) {
    $SnapshotDirectory = Join-Path $RepoRoot 'specs/generated/qlog/source-docs'
}

if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    $ManifestPath = Join-Path $RepoRoot 'specs/generated/qlog/source-manifest.json'
}

function ConvertTo-RepoRelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $relativePath = [System.IO.Path]::GetRelativePath($Root, $Path)
    return $relativePath.Replace('\', '/')
}

function Ensure-Directory {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }

    return (Resolve-Path -LiteralPath $Path).Path
}

function Get-QlogDraftSources {
    return @(
        [pscustomobject]@{
            draft_name      = 'draft-ietf-quic-qlog-main-schema'
            revision        = 13
            datatracker_url = 'https://datatracker.ietf.org/doc/html/draft-ietf-quic-qlog-main-schema-13'
            archive_txt_url = 'https://www.ietf.org/archive/id/draft-ietf-quic-qlog-main-schema-13.txt'
            artifact_id     = 'SPEC-QLOG-MAIN'
            purpose         = 'qlog core model and serialization envelope'
        },
        [pscustomobject]@{
            draft_name      = 'draft-ietf-quic-qlog-quic-events'
            revision        = 12
            datatracker_url = 'https://datatracker.ietf.org/doc/html/draft-ietf-quic-qlog-quic-events-12'
            archive_txt_url = 'https://www.ietf.org/archive/id/draft-ietf-quic-qlog-quic-events-12.txt'
            artifact_id     = 'SPEC-QLOG-QUIC'
            purpose         = 'qlog QUIC event vocabulary'
        }
    )
}

$snapshotRoot = Ensure-Directory -Path $SnapshotDirectory
$resolvedRepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
$generatedAt = (Get-Date).ToUniversalTime().ToString('o')
$documents = @()

foreach ($source in Get-QlogDraftSources) {
    $fileName = '{0}-{1}.txt' -f $source.draft_name, $source.revision
    $snapshotPath = Join-Path $snapshotRoot $fileName

    $response = Invoke-WebRequest -Uri $source.archive_txt_url -UseBasicParsing
    $content = $response.Content

    [System.IO.File]::WriteAllText($snapshotPath, $content)

    $hash = (Get-FileHash -LiteralPath $snapshotPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $documents += [pscustomobject]@{
            draft_name        = $source.draft_name
            revision          = $source.revision
            artifact_id       = $source.artifact_id
            purpose           = $source.purpose
            datatracker_url   = $source.datatracker_url
            archive_txt_url   = $source.archive_txt_url
            local_snapshot    = ConvertTo-RepoRelativePath -Root $resolvedRepoRoot -Path (Resolve-Path -LiteralPath $snapshotPath).Path
            sha256            = $hash
            retrieved_utc     = $generatedAt
        }
}

$manifest = [pscustomobject]@{
    generated_utc = $generatedAt
    repo_root     = $resolvedRepoRoot.Replace('\', '/')
    documents     = @($documents)
}

$manifestDirectory = Split-Path -Path $ManifestPath -Parent
if (-not [string]::IsNullOrWhiteSpace($manifestDirectory)) {
    Ensure-Directory -Path $manifestDirectory | Out-Null
}

$json = $manifest | ConvertTo-Json -Depth 10
[System.IO.File]::WriteAllText($ManifestPath, $json + [Environment]::NewLine)

Write-Host "Wrote qlog draft snapshots to $snapshotRoot"
Write-Host "Wrote qlog source manifest to $ManifestPath"
