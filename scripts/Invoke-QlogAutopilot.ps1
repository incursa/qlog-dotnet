<#
.SYNOPSIS
Plans and runs local multi-lane Codex autopilot work for qlog-dotnet.

.DESCRIPTION
Creates bounded worker lanes in separate git worktrees, starts the generic
Codex autopilot runner for each lane, and can merge or clean up the results.
By default, paths are resolved relative to this repository instead of a single
machine-specific checkout root.
#>

param(
    [ValidateSet("plan", "prepare", "run", "merge", "cleanup")]
    [string]$Mode = "plan",

    [string]$RepoRoot = "",
    [string]$RunnerScriptPath = "",
    [string]$MissionPromptFile = "",
    [string]$WorktreeRoot = "",
    [string]$StateDirectory = "",
    [string[]]$LaneIds = @(),
    [string]$TargetBranch = "main",
    [string]$CodexCommand = "codex",
    [string]$Sandbox = "danger-full-access",
    [string]$WorkerModel = "gpt-5.4-mini",
    [string]$WorkerReasoningEffort = "xhigh",
    [int]$ParallelLanes = 2,
    [int]$WorkerMaxIterations = 6,
    [int]$WorkerMaxRescueAttemptsPerTurn = 1,
    [switch]$AutoMerge,
    [switch]$CleanupAfterMerge,
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Path $PSScriptRoot -Parent
}

if ([string]::IsNullOrWhiteSpace($RunnerScriptPath)) {
    $RunnerScriptPath = Join-Path $PSScriptRoot "Run-CodexAutopilot.ps1"
}

if ([string]::IsNullOrWhiteSpace($MissionPromptFile)) {
    $MissionPromptFile = Join-Path (Join-Path $RepoRoot "prompts") "mission.md"
}

if ([string]::IsNullOrWhiteSpace($WorktreeRoot)) {
    $repoParent = Split-Path -Path $RepoRoot -Parent
    $repoLeaf = Split-Path -Path $RepoRoot -Leaf
    $WorktreeRoot = Join-Path (Join-Path $repoParent ($repoLeaf + ".worktrees")) "qlog-autopilot"
}

if ([string]::IsNullOrWhiteSpace($StateDirectory)) {
    $StateDirectory = Join-Path $RepoRoot ".artifacts\qlog-autopilot"
}

function Resolve-ExistingPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Path does not exist: $Path"
    }

    return (Resolve-Path -LiteralPath $Path).Path
}

function Ensure-Directory {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }

    return (Resolve-Path -LiteralPath $Path).Path
}

function Get-ExceptionDetail {
    param([Parameter(Mandatory = $true)][System.Exception]$Exception)

    $messages = New-Object System.Collections.Generic.List[string]
    $current = $Exception
    while ($null -ne $current) {
        if (-not [string]::IsNullOrWhiteSpace($current.Message)) {
            $messages.Add($current.Message.Trim())
        }

        $current = $current.InnerException
    }

    if ($messages.Count -eq 0) {
        return $Exception.ToString()
    }

    return ($messages -join " | Inner: ")
}

function Resolve-CommandPath {
    param(
        [Parameter(Mandatory = $true)][string]$Command,
        [string[]]$Candidates = @()
    )

    if (-not [string]::IsNullOrWhiteSpace($Command)) {
        $resolved = Get-Command -Name $Command -ErrorAction SilentlyContinue
        if ($resolved -and -not [string]::IsNullOrWhiteSpace($resolved.Path)) {
            return $resolved.Path
        }
    }

    foreach ($candidate in $Candidates) {
        $resolved = Get-Command -Name $candidate -ErrorAction SilentlyContinue
        if ($resolved -and -not [string]::IsNullOrWhiteSpace($resolved.Path)) {
            return $resolved.Path
        }
    }

    throw "Unable to resolve a runnable command for '$Command'."
}

function Get-NormalizedStringList {
    param([AllowNull()][string[]]$Items = @())

    if ($null -eq $Items) {
        return @()
    }

    return @(
        $Items |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object { $_.Trim() } |
        Select-Object -Unique
    )
}

function Invoke-NativeCapture {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$ArgumentList,
        [string]$WorkingDirectory = ""
    )

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $FilePath
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    if (-not [string]::IsNullOrWhiteSpace($WorkingDirectory)) {
        $psi.WorkingDirectory = $WorkingDirectory
    }

    foreach ($arg in $ArgumentList) {
        [void]$psi.ArgumentList.Add($arg)
    }

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $psi
    [void]$process.Start()
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    return [pscustomobject]@{
        ExitCode = $process.ExitCode
        StdOut = $stdout
        StdErr = $stderr
    }
}

function Get-GitHead {
    param(
        [Parameter(Mandatory = $true)][string]$GitExecutable,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [string]$Ref = "HEAD"
    )

    $result = Invoke-NativeCapture -FilePath $GitExecutable -ArgumentList @("-C", $RepositoryRoot, "rev-parse", $Ref)
    if ($result.ExitCode -ne 0) {
        throw "git rev-parse failed for ${Ref}: $($result.StdErr.Trim())"
    }

    return $result.StdOut.Trim()
}

function Get-GitCurrentBranch {
    param(
        [Parameter(Mandatory = $true)][string]$GitExecutable,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot
    )

    $result = Invoke-NativeCapture -FilePath $GitExecutable -ArgumentList @("-C", $RepositoryRoot, "rev-parse", "--abbrev-ref", "HEAD")
    if ($result.ExitCode -ne 0) {
        throw "git rev-parse --abbrev-ref failed: $($result.StdErr.Trim())"
    }

    return $result.StdOut.Trim()
}

function Test-GitClean {
    param(
        [Parameter(Mandatory = $true)][string]$GitExecutable,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot
    )

    $result = Invoke-NativeCapture -FilePath $GitExecutable -ArgumentList @("-C", $RepositoryRoot, "status", "--porcelain")
    if ($result.ExitCode -ne 0) {
        throw "git status --porcelain failed: $($result.StdErr.Trim())"
    }

    return [string]::IsNullOrWhiteSpace($result.StdOut)
}

function Get-CommitRange {
    param(
        [Parameter(Mandatory = $true)][string]$GitExecutable,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$FromRef,
        [Parameter(Mandatory = $true)][string]$ToRef
    )

    $result = Invoke-NativeCapture -FilePath $GitExecutable -ArgumentList @("-C", $RepositoryRoot, "rev-list", "--reverse", "$FromRef..$ToRef")
    if ($result.ExitCode -ne 0) {
        throw "git rev-list failed: $($result.StdErr.Trim())"
    }

    return @(
        $result.StdOut -split '\r?\n' |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Ensure-GitWorktree {
    param(
        [Parameter(Mandatory = $true)][string]$GitExecutable,
        [Parameter(Mandatory = $true)][string]$RepoRoot,
        [Parameter(Mandatory = $true)][string]$WorktreePath,
        [Parameter(Mandatory = $true)][string]$BranchName,
        [Parameter(Mandatory = $true)][string]$BaseRef
    )

    $parent = Split-Path -Path $WorktreePath -Parent
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        Ensure-Directory -Path $parent | Out-Null
    }

    & $GitExecutable -C $RepoRoot worktree add -b $BranchName $WorktreePath $BaseRef | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "git worktree add failed for $WorktreePath ($BranchName)."
    }
}

function Remove-GitWorktreeAndBranch {
    param(
        [Parameter(Mandatory = $true)][string]$GitExecutable,
        [Parameter(Mandatory = $true)][string]$RepoRoot,
        [Parameter(Mandatory = $true)][string]$WorktreePath,
        [Parameter(Mandatory = $true)][string]$BranchName
    )

    if (Test-Path -LiteralPath $WorktreePath) {
        & $GitExecutable -C $RepoRoot worktree remove --force $WorktreePath | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "git worktree remove failed for $WorktreePath."
        }
    }

    & $GitExecutable -C $RepoRoot show-ref --verify --quiet "refs/heads/$BranchName"
    if ($LASTEXITCODE -eq 0) {
        & $GitExecutable -C $RepoRoot branch -D $BranchName | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "git branch -D failed for $BranchName."
        }
    }
}

function Invoke-CommandBatch {
    param(
        [Parameter(Mandatory = $true)][string]$WorkingDirectory,
        [Parameter(Mandatory = $true)][string[]]$Commands
    )

    $commands = @(Get-NormalizedStringList -Items $Commands)
    if ($commands.Count -eq 0) {
        return @()
    }

    $pwsh = Resolve-CommandPath -Command "pwsh" -Candidates @("pwsh.exe", "pwsh", "powershell.exe")
    $results = New-Object System.Collections.Generic.List[object]
    foreach ($commandText in $commands) {
        $argumentList = @("-NoProfile")
        if ($env:OS -eq "Windows_NT") {
            $argumentList += @("-ExecutionPolicy", "Bypass")
        }

        $argumentList += @("-Command", $commandText)
        $result = Invoke-NativeCapture -FilePath $pwsh -ArgumentList $argumentList -WorkingDirectory $WorkingDirectory
        [void]$results.Add([pscustomobject]@{
            CommandText = $commandText
            ExitCode = $result.ExitCode
            StdOut = $result.StdOut
            StdErr = $result.StdErr
        })

        if ($result.ExitCode -ne 0) {
            break
        }
    }

    return $results.ToArray()
}

function Get-LaneDefinitions {
    return @(
        [pscustomobject]@{
            lane_id = "core-capture-runtime"
            priority = 1
            group = "core"
            prerequisite_lane_ids = @()
            objective = "Land the append-oriented capture/session core with immutable snapshots and off-thread dispatch."
            allowed_path_prefixes = @("src/Incursa.Qlog", "tests/Incursa.Qlog.Tests", "specs/requirements/qlog", "specs/architecture/qlog", "specs/work-items/qlog", "specs/verification/qlog", "Incursa.Qlog.slnx", "README.md")
            forbidden_path_prefixes = @("specs/generated")
            requirement_families = @("REQ-QLOG-CAPTURE", "REQ-QLOG-MAIN")
            verification_commands = @('pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Test-RequirementHomeCoverage.ps1 -RequirementFamilies REQ-QLOG-CAPTURE', 'dotnet test tests/Incursa.Qlog.Tests/Incursa.Qlog.Tests.csproj')
            merge_check_commands = @('dotnet test tests/Incursa.Qlog.Tests/Incursa.Qlog.Tests.csproj')
        }
        [pscustomobject]@{
            lane_id = "sink-adapters-file-and-stream"
            priority = 2
            group = "sinks"
            prerequisite_lane_ids = @("core-capture-runtime")
            objective = "Land out-of-the-box file and streaming sinks as separate adapters over the capture core."
            allowed_path_prefixes = @("src/Incursa.Qlog", "src/Incursa.Qlog.Sinks", "tests/Incursa.Qlog.Tests", "tests/Incursa.Qlog.Sinks.Tests", "benchmarks", "specs/requirements/qlog", "specs/architecture/qlog", "specs/work-items/qlog", "specs/verification/qlog", "Incursa.Qlog.slnx", "README.md")
            forbidden_path_prefixes = @("specs/generated")
            requirement_families = @("REQ-QLOG-CAPTURE", "REQ-QLOG-SINKS")
            verification_commands = @('pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Test-RequirementHomeCoverage.ps1 -RequirementFamilies REQ-QLOG-SINKS', 'dotnet test tests/Incursa.Qlog.Tests/Incursa.Qlog.Tests.csproj', 'dotnet run -c Release --project benchmarks/Incursa.Qlog.Benchmarks.csproj -- --job Dry --filter "*Qlog*"')
            merge_check_commands = @('dotnet test tests/Incursa.Qlog.Tests/Incursa.Qlog.Tests.csproj')
        }
        [pscustomobject]@{
            lane_id = "quic-adapter-follow-through"
            priority = 3
            group = "quic"
            prerequisite_lane_ids = @("core-capture-runtime")
            objective = "Keep the QUIC event builders and requirement homes aligned with the capture changes."
            allowed_path_prefixes = @("src/Incursa.Qlog.Quic", "tests/Incursa.Qlog.Quic.Tests", "specs/requirements/qlog", "specs/architecture/qlog", "specs/work-items/qlog", "specs/verification/qlog", "Incursa.Qlog.slnx", "README.md")
            forbidden_path_prefixes = @("specs/generated")
            requirement_families = @("REQ-QLOG-QUIC")
            verification_commands = @('pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Test-RequirementHomeCoverage.ps1 -RequirementFamilies REQ-QLOG-QUIC', 'dotnet test tests/Incursa.Qlog.Quic.Tests/Incursa.Qlog.Quic.Tests.csproj')
            merge_check_commands = @('dotnet test tests/Incursa.Qlog.Quic.Tests/Incursa.Qlog.Quic.Tests.csproj')
        }
        [pscustomobject]@{
            lane_id = "proof-fuzz-mutation-benchmarks"
            priority = 4
            group = "quality"
            prerequisite_lane_ids = @()
            objective = "Expand proof depth across positive, negative, edge, fuzz, mutation-style, and benchmark coverage."
            allowed_path_prefixes = @("tests/Incursa.Qlog.Tests", "tests/Incursa.Qlog.Quic.Tests", "benchmarks", "docs", "scripts", "specs/requirements/qlog", "specs/work-items/qlog", "specs/verification/qlog", "README.md")
            forbidden_path_prefixes = @("specs/generated")
            requirement_families = @("REQ-QLOG")
            verification_commands = @('pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Test-RequirementHomeCoverage.ps1 -RequirementFamilies REQ-QLOG-MAIN,REQ-QLOG-QUIC', 'dotnet test Incursa.Qlog.slnx', 'dotnet run -c Release --project benchmarks/Incursa.Qlog.Benchmarks.csproj -- --job Dry --filter "*Qlog*"')
            merge_check_commands = @('dotnet test Incursa.Qlog.slnx')
        }
    )
}

function Get-StatePath {
    param([Parameter(Mandatory = $true)][string]$ResolvedStateDirectory)

    return Join-Path $ResolvedStateDirectory "orchestration-state.json"
}

function Get-OrchestrationState {
    param([Parameter(Mandatory = $true)][string]$ResolvedStateDirectory)

    $statePath = Get-StatePath -ResolvedStateDirectory $ResolvedStateDirectory
    if (-not (Test-Path -LiteralPath $statePath)) {
        return [pscustomobject]@{
            schema_version = 1
            last_updated = ""
            completed_lane_ids = @()
            active_lanes = @()
        }
    }

    return Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json -Depth 100
}

function Save-OrchestrationState {
    param(
        [Parameter(Mandatory = $true)][string]$ResolvedStateDirectory,
        [Parameter(Mandatory = $true)]$State
    )

    $statePath = Get-StatePath -ResolvedStateDirectory $ResolvedStateDirectory
    $State.last_updated = (Get-Date).ToString("o")
    $State.completed_lane_ids = @(Get-NormalizedStringList -Items $State.completed_lane_ids)
    ($State | ConvertTo-Json -Depth 100) | Set-Content -LiteralPath $statePath -Encoding utf8
}

function Get-EligibleLanes {
    param(
        [Parameter(Mandatory = $true)]$State,
        [Parameter(Mandatory = $true)][int]$ParallelLanes
    )

    $completed = @(Get-NormalizedStringList -Items $State.completed_lane_ids)
    $eligible = @(
        Get-LaneDefinitions |
        Where-Object {
            $lane = $_
            @($lane.prerequisite_lane_ids | Where-Object { $completed -notcontains $_ }).Count -eq 0
        } |
        Sort-Object priority, lane_id
    )

    $selected = New-Object System.Collections.Generic.List[object]
    $usedGroups = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($lane in $eligible) {
        if ($selected.Count -ge $ParallelLanes) {
            break
        }

        if ($usedGroups.Add([string]$lane.group)) {
            [void]$selected.Add($lane)
        }
    }

    return @($selected.ToArray())
}

function New-LaneRecord {
    param(
        [Parameter(Mandatory = $true)]$Lane,
        [Parameter(Mandatory = $true)][string]$ResolvedRepoRoot,
        [Parameter(Mandatory = $true)][string]$ResolvedStateDirectory,
        [Parameter(Mandatory = $true)][string]$ResolvedWorktreeRoot,
        [Parameter(Mandatory = $true)][string]$TargetBranch,
        [Parameter(Mandatory = $true)][string]$GitExecutable,
        [Parameter(Mandatory = $true)][int]$Index
    )

    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $branchName = "codex/$($Lane.lane_id)-$timestamp-$Index"
    $worktreePath = Join-Path $ResolvedWorktreeRoot "$($Lane.lane_id)-$timestamp-$Index"
    $outputDirectory = Join-Path $ResolvedStateDirectory ("runs\" + $Lane.lane_id + "-" + $timestamp + "-" + $Index)
    Ensure-Directory -Path $outputDirectory | Out-Null
    $baseRef = Get-GitHead -GitExecutable $GitExecutable -RepositoryRoot $ResolvedRepoRoot -Ref $TargetBranch
    Ensure-GitWorktree -GitExecutable $GitExecutable -RepoRoot $ResolvedRepoRoot -WorktreePath $worktreePath -BranchName $branchName -BaseRef $baseRef

    return [pscustomobject]@{
        lane_id = $Lane.lane_id
        objective = $Lane.objective
        priority = $Lane.priority
        branch_name = $branchName
        worktree_path = $worktreePath
        output_directory = $outputDirectory
        base_ref = $baseRef
        target_branch = $TargetBranch
        allowed_path_prefixes = @($Lane.allowed_path_prefixes)
        forbidden_path_prefixes = @($Lane.forbidden_path_prefixes)
        requirement_families = @($Lane.requirement_families)
        verification_commands = @($Lane.verification_commands)
        merge_check_commands = @($Lane.merge_check_commands)
    }
}

function Start-LaneWorker {
    param(
        [Parameter(Mandatory = $true)]$LaneRecord,
        [Parameter(Mandatory = $true)][string]$PowerShellExecutable,
        [Parameter(Mandatory = $true)][string]$ResolvedRunnerScriptPath,
        [Parameter(Mandatory = $true)][string]$ResolvedMissionPromptFile,
        [Parameter(Mandatory = $true)][string]$CodexCommand,
        [Parameter(Mandatory = $true)][string]$Sandbox,
        [Parameter(Mandatory = $true)][string]$WorkerModel,
        [Parameter(Mandatory = $true)][string]$WorkerReasoningEffort,
        [Parameter(Mandatory = $true)][int]$WorkerMaxIterations,
        [Parameter(Mandatory = $true)][int]$WorkerMaxRescueAttemptsPerTurn
    )

    $runnerParameters = [ordered]@{
        WorkingDirectory = $LaneRecord.worktree_path
        InitialPromptFile = $ResolvedMissionPromptFile
        OutputDirectory = $LaneRecord.output_directory
        CodexCommand = $CodexCommand
        Sandbox = $Sandbox
        Model = $WorkerModel
        ReasoningEffort = $WorkerReasoningEffort
        MissionPromptStyle = "always_digest"
        MaxIterations = $WorkerMaxIterations
        MaxRescueAttemptsPerTurn = $WorkerMaxRescueAttemptsPerTurn
        TargetLaneId = $LaneRecord.lane_id
        TargetScope = $LaneRecord.objective
        AllowedPathPrefixes = @($LaneRecord.allowed_path_prefixes)
        ForbiddenPathPrefixes = @($LaneRecord.forbidden_path_prefixes)
        RequirementFamilies = @($LaneRecord.requirement_families)
        VerificationCommands = @($LaneRecord.verification_commands)
        MergeCheckCommands = @($LaneRecord.merge_check_commands)
        RequirementGapsPath = (Join-Path $LaneRecord.worktree_path "specs/requirements/qlog/REQUIREMENT-GAPS.md")
        StopOnPathViolation = $true
    }

    $parametersPath = Join-Path $LaneRecord.output_directory "worker.parameters.json"
    ($runnerParameters | ConvertTo-Json -Depth 100) | Set-Content -LiteralPath $parametersPath -Encoding utf8

    $bootstrapPath = Join-Path $LaneRecord.output_directory "worker.bootstrap.ps1"
    $script = @"
Set-StrictMode -Version Latest
`$runnerParameters = Get-Content -LiteralPath '$($parametersPath.Replace("'", "''"))' -Raw | ConvertFrom-Json -AsHashtable
foreach (`$arrayKey in @('AllowedPathPrefixes', 'ForbiddenPathPrefixes', 'RequirementFamilies', 'VerificationCommands', 'MergeCheckCommands')) {
    if (`$runnerParameters.ContainsKey(`$arrayKey)) { `$runnerParameters[`$arrayKey] = @(`$runnerParameters[`$arrayKey]) }
}
`$runnerParameters['StopOnPathViolation'] = [bool]`$runnerParameters['StopOnPathViolation']
& '$($ResolvedRunnerScriptPath.Replace("'", "''"))' @runnerParameters
"@
    Set-Content -LiteralPath $bootstrapPath -Value $script -Encoding utf8

    $args = @("-NoProfile")
    if ($env:OS -eq "Windows_NT") {
        $args += @("-ExecutionPolicy", "Bypass")
    }

    $args += @("-File", $bootstrapPath)
    $stdoutPath = Join-Path $LaneRecord.output_directory "worker-host.stdout.log"
    $stderrPath = Join-Path $LaneRecord.output_directory "worker-host.stderr.log"
    $process = Start-Process -FilePath $PowerShellExecutable -ArgumentList $args -WorkingDirectory $LaneRecord.worktree_path -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath -PassThru

    return [pscustomobject]@{
        lane = $LaneRecord
        process = $process
    }
}

function Get-WorkerFinalDecision {
    param([Parameter(Mandatory = $true)][string]$OutputDirectory)

    $summaryPath = Join-Path $OutputDirectory "autopilot-summary.csv"
    if (-not (Test-Path -LiteralPath $summaryPath)) {
        return $null
    }

    $rows = Import-Csv -LiteralPath $summaryPath
    if ($null -eq $rows -or @($rows).Count -eq 0) {
        return $null
    }

    return @($rows)[-1]
}

function Invoke-PreflightMerge {
    param(
        [Parameter(Mandatory = $true)][string]$GitExecutable,
        [Parameter(Mandatory = $true)][string]$ResolvedRepoRoot,
        [Parameter(Mandatory = $true)][string]$ResolvedStateDirectory,
        [Parameter(Mandatory = $true)]$LaneRecord,
        [Parameter(Mandatory = $true)][string[]]$CommitShas
    )

    $integrationRoot = Ensure-Directory -Path (Join-Path $ResolvedStateDirectory "integration")
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $branchName = "codex/integration-$($LaneRecord.lane_id)-$timestamp"
    $worktreePath = Join-Path $integrationRoot "$($LaneRecord.lane_id)-$timestamp"
    $targetHead = Get-GitHead -GitExecutable $GitExecutable -RepositoryRoot $ResolvedRepoRoot -Ref $LaneRecord.target_branch
    Ensure-GitWorktree -GitExecutable $GitExecutable -RepoRoot $ResolvedRepoRoot -WorktreePath $worktreePath -BranchName $branchName -BaseRef $targetHead

    try {
        foreach ($commitSha in $CommitShas) {
            $result = Invoke-NativeCapture -FilePath $GitExecutable -ArgumentList @("-C", $worktreePath, "cherry-pick", $commitSha)
            if ($result.ExitCode -ne 0) {
                throw "Preflight cherry-pick failed for ${commitSha}: $($result.StdErr.Trim())"
            }
        }

        $mergeChecks = Invoke-CommandBatch -WorkingDirectory $worktreePath -Commands $LaneRecord.merge_check_commands
        $failed = @($mergeChecks | Where-Object { $_.ExitCode -ne 0 } | Select-Object -First 1)
        if ($failed.Count -gt 0) {
            throw "Preflight merge checks failed: $($failed[0].CommandText)"
        }
    }
    finally {
        Remove-GitWorktreeAndBranch -GitExecutable $GitExecutable -RepoRoot $ResolvedRepoRoot -WorktreePath $worktreePath -BranchName $branchName
    }
}

function Merge-Lane {
    param(
        [Parameter(Mandatory = $true)][string]$GitExecutable,
        [Parameter(Mandatory = $true)][string]$ResolvedRepoRoot,
        [Parameter(Mandatory = $true)][string]$ResolvedStateDirectory,
        [Parameter(Mandatory = $true)]$LaneRecord
    )

    $workerHead = Get-GitHead -GitExecutable $GitExecutable -RepositoryRoot $LaneRecord.worktree_path -Ref "HEAD"
    $commitShas = @(Get-CommitRange -GitExecutable $GitExecutable -RepositoryRoot $LaneRecord.worktree_path -FromRef $LaneRecord.base_ref -ToRef $workerHead)
    if ($commitShas.Count -eq 0) {
        return [pscustomobject]@{ merged = $false; reason = "no commits" }
    }

    Invoke-PreflightMerge -GitExecutable $GitExecutable -ResolvedRepoRoot $ResolvedRepoRoot -ResolvedStateDirectory $ResolvedStateDirectory -LaneRecord $LaneRecord -CommitShas $commitShas
    foreach ($commitSha in $commitShas) {
        $result = Invoke-NativeCapture -FilePath $GitExecutable -ArgumentList @("-C", $ResolvedRepoRoot, "cherry-pick", $commitSha)
        if ($result.ExitCode -ne 0) {
            throw "Cherry-pick failed for ${commitSha}: $($result.StdErr.Trim())"
        }
    }

    $mergeChecks = Invoke-CommandBatch -WorkingDirectory $ResolvedRepoRoot -Commands $LaneRecord.merge_check_commands
    $failed = @($mergeChecks | Where-Object { $_.ExitCode -ne 0 } | Select-Object -First 1)
    if ($failed.Count -gt 0) {
        throw "Merge checks failed: $($failed[0].CommandText)"
    }

    return [pscustomobject]@{ merged = $true; reason = "" }
}

try {
    $resolvedRepoRoot = Resolve-ExistingPath -Path $RepoRoot
    $resolvedStateDirectory = Ensure-Directory -Path $StateDirectory
    $resolvedWorktreeRoot = Ensure-Directory -Path $WorktreeRoot
    $gitExecutable = Resolve-CommandPath -Command "git" -Candidates @("git.exe", "git")
    $powerShellExecutable = Resolve-CommandPath -Command "pwsh" -Candidates @("pwsh.exe", "pwsh", "powershell.exe")
    $resolvedRunnerScriptPath = if ($Mode -in @("prepare", "run")) { Resolve-ExistingPath -Path $RunnerScriptPath } else { $RunnerScriptPath }
    $resolvedMissionPromptFile = if ($Mode -in @("prepare", "run")) { Resolve-ExistingPath -Path $MissionPromptFile } else { $MissionPromptFile }
    $state = Get-OrchestrationState -ResolvedStateDirectory $resolvedStateDirectory

    switch ($Mode) {
        "plan" {
            $eligible = Get-EligibleLanes -State $state -ParallelLanes ([Math]::Max(1, $ParallelLanes))
            Write-Host "Recommended lanes: $([string]::Join(', ', @($eligible | ForEach-Object { $_.lane_id })))" -ForegroundColor Green
            $completed = @(Get-NormalizedStringList -Items $state.completed_lane_ids)
            foreach ($lane in Get-LaneDefinitions) {
                if ($completed -contains $lane.lane_id) {
                    $status = "completed"
                }
                elseif (@($lane.prerequisite_lane_ids | Where-Object { $completed -notcontains $_ }).Count -gt 0) {
                    $status = "blocked_prerequisite"
                }
                else {
                    $status = "available"
                }

                Write-Host "  $($lane.lane_id) [$status] - $($lane.objective)"
            }

            break
        }

        "prepare" {
            if (@($state.active_lanes).Count -gt 0 -and -not $Force) {
                throw "Active lanes already exist. Use -Force or run cleanup first."
            }

            if (-not (Test-GitClean -GitExecutable $gitExecutable -RepositoryRoot $resolvedRepoRoot) -and -not $Force) {
                throw "Repository must be clean before preparing worker lanes."
            }

            $currentBranch = Get-GitCurrentBranch -GitExecutable $gitExecutable -RepositoryRoot $resolvedRepoRoot
            if ($currentBranch -ne $TargetBranch -and -not $Force) {
                throw "Repository must be on '$TargetBranch' before preparing worker lanes. Current branch: $currentBranch"
            }

            $selected = if (@(Get-NormalizedStringList -Items $LaneIds).Count -gt 0) {
                @(Get-LaneDefinitions | Where-Object { (Get-NormalizedStringList -Items $LaneIds) -contains $_.lane_id } | Sort-Object priority, lane_id)
            }
            else {
                @(Get-EligibleLanes -State $state -ParallelLanes ([Math]::Max(1, $ParallelLanes)))
            }

            if ($selected.Count -eq 0) {
                throw "No lanes were selected."
            }

            $prepared = New-Object System.Collections.Generic.List[object]
            $index = 0
            $completed = @(Get-NormalizedStringList -Items $state.completed_lane_ids)
            foreach ($lane in $selected) {
                $missingPrereqs = @($lane.prerequisite_lane_ids | Where-Object { $completed -notcontains $_ })
                if ($missingPrereqs.Count -gt 0 -and -not $Force) {
                    throw "Lane '$($lane.lane_id)' is blocked by prerequisites: $($missingPrereqs -join ', ')"
                }

                $index++
                [void]$prepared.Add((New-LaneRecord -Lane $lane -ResolvedRepoRoot $resolvedRepoRoot -ResolvedStateDirectory $resolvedStateDirectory -ResolvedWorktreeRoot $resolvedWorktreeRoot -TargetBranch $TargetBranch -GitExecutable $gitExecutable -Index $index))
            }

            $state.active_lanes = @($prepared.ToArray())
            Save-OrchestrationState -ResolvedStateDirectory $resolvedStateDirectory -State $state
            foreach ($lane in $state.active_lanes) {
                Write-Host "Prepared lane '$($lane.lane_id)' at $($lane.worktree_path)" -ForegroundColor Green
            }

            break
        }

        "run" {
            if (@($state.active_lanes).Count -eq 0) {
                & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -Mode prepare -RepoRoot $resolvedRepoRoot -RunnerScriptPath $resolvedRunnerScriptPath -MissionPromptFile $resolvedMissionPromptFile -WorktreeRoot $resolvedWorktreeRoot -StateDirectory $resolvedStateDirectory -LaneIds $LaneIds -TargetBranch $TargetBranch -ParallelLanes ([Math]::Max(1, $ParallelLanes)) -Force:$Force
                if ($LASTEXITCODE -ne 0) {
                    throw "Failed to prepare worker lanes."
                }

                $state = Get-OrchestrationState -ResolvedStateDirectory $resolvedStateDirectory
            }

            $runs = New-Object System.Collections.Generic.List[object]
            foreach ($lane in @($state.active_lanes)) {
                [void]$runs.Add((Start-LaneWorker -LaneRecord $lane -PowerShellExecutable $powerShellExecutable -ResolvedRunnerScriptPath $resolvedRunnerScriptPath -ResolvedMissionPromptFile $resolvedMissionPromptFile -CodexCommand $CodexCommand -Sandbox $Sandbox -WorkerModel $WorkerModel -WorkerReasoningEffort $WorkerReasoningEffort -WorkerMaxIterations $WorkerMaxIterations -WorkerMaxRescueAttemptsPerTurn $WorkerMaxRescueAttemptsPerTurn))
                Write-Host "Started lane '$($lane.lane_id)'." -ForegroundColor Green
            }

            foreach ($run in $runs) {
                $run.process.WaitForExit()
                Write-Host "Lane '$($run.lane.lane_id)' exited with code $($run.process.ExitCode)." -ForegroundColor Green
            }

            $mergeCandidates = New-Object System.Collections.Generic.List[string]
            foreach ($run in @($runs.ToArray() | Sort-Object { $_.lane.priority })) {
                $decision = Get-WorkerFinalDecision -OutputDirectory $run.lane.output_directory
                $workerHead = Get-GitHead -GitExecutable $gitExecutable -RepositoryRoot $run.lane.worktree_path -Ref "HEAD"
                $commitShas = @(Get-CommitRange -GitExecutable $gitExecutable -RepositoryRoot $run.lane.worktree_path -FromRef $run.lane.base_ref -ToRef $workerHead)
                if ($null -ne $decision) {
                    Write-Host "  $($run.lane.lane_id): state=$($decision.State), commits=$($commitShas.Count)"
                }
                else {
                    Write-Host "  $($run.lane.lane_id): commits=$($commitShas.Count)"
                }

                if ($run.process.ExitCode -eq 0 -and $commitShas.Count -gt 0 -and ($null -eq $decision -or $decision.State -notin @("pause_manual", "stuck"))) {
                    [void]$mergeCandidates.Add($run.lane.lane_id)
                }
            }

            $shouldMerge = if ($PSBoundParameters.ContainsKey("AutoMerge")) { [bool]$AutoMerge } else { $true }
            if ($shouldMerge -and $mergeCandidates.Count -gt 0) {
                & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -Mode merge -RepoRoot $resolvedRepoRoot -StateDirectory $resolvedStateDirectory -WorktreeRoot $resolvedWorktreeRoot -LaneIds $mergeCandidates.ToArray() -TargetBranch $TargetBranch -Force:$Force
                if ($LASTEXITCODE -ne 0) {
                    throw "Merge step failed."
                }

                $shouldCleanup = if ($PSBoundParameters.ContainsKey("CleanupAfterMerge")) { [bool]$CleanupAfterMerge } else { $true }
                if ($shouldCleanup) {
                    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -Mode cleanup -RepoRoot $resolvedRepoRoot -StateDirectory $resolvedStateDirectory -WorktreeRoot $resolvedWorktreeRoot -LaneIds $mergeCandidates.ToArray() -Force:$Force
                    if ($LASTEXITCODE -ne 0) {
                        throw "Cleanup step failed."
                    }
                }
            }

            break
        }

        "merge" {
            if (-not (Test-GitClean -GitExecutable $gitExecutable -RepositoryRoot $resolvedRepoRoot) -and -not $Force) {
                throw "Repository must be clean before merging."
            }

            $currentBranch = Get-GitCurrentBranch -GitExecutable $gitExecutable -RepositoryRoot $resolvedRepoRoot
            if ($currentBranch -ne $TargetBranch -and -not $Force) {
                throw "Repository must be on '$TargetBranch' before merging. Current branch: $currentBranch"
            }

            $requested = @(Get-NormalizedStringList -Items $LaneIds)
            $lanesToMerge = if ($requested.Count -gt 0) { @($state.active_lanes | Where-Object { $requested -contains [string]$_.lane_id }) } else { @($state.active_lanes) }
            foreach ($lane in @($lanesToMerge | Sort-Object priority, lane_id)) {
                $result = Merge-Lane -GitExecutable $gitExecutable -ResolvedRepoRoot $resolvedRepoRoot -ResolvedStateDirectory $resolvedStateDirectory -LaneRecord $lane
                if ($result.merged) {
                    $state.completed_lane_ids = @($state.completed_lane_ids + @($lane.lane_id))
                    $state.active_lanes = @($state.active_lanes | Where-Object { [string]$_.lane_id -ne $lane.lane_id })
                    Save-OrchestrationState -ResolvedStateDirectory $resolvedStateDirectory -State $state
                    Write-Host "Merged lane '$($lane.lane_id)'." -ForegroundColor Green
                }
                else {
                    Write-Warning "Lane '$($lane.lane_id)' was not merged: $($result.reason)"
                }
            }

            break
        }

        "cleanup" {
            $requested = @(Get-NormalizedStringList -Items $LaneIds)
            $lanesToClean = if ($requested.Count -gt 0) { @($state.active_lanes | Where-Object { $requested -contains [string]$_.lane_id }) } else { @($state.active_lanes) }
            foreach ($lane in $lanesToClean) {
                Remove-GitWorktreeAndBranch -GitExecutable $gitExecutable -RepoRoot $resolvedRepoRoot -WorktreePath $lane.worktree_path -BranchName $lane.branch_name
                $state.active_lanes = @($state.active_lanes | Where-Object { [string]$_.lane_id -ne $lane.lane_id })
                Write-Host "Removed lane '$($lane.lane_id)'." -ForegroundColor Green
            }

            Save-OrchestrationState -ResolvedStateDirectory $resolvedStateDirectory -State $state
            break
        }
    }
}
catch {
    Write-Error (Get-ExceptionDetail -Exception $_.Exception)
    exit 1
}
