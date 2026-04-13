[CmdletBinding()]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string[]]$Profiles = @('core'),
    [string]$SchemaUri = 'https://github.com/incursa/spec-trace/raw/refs/heads/main/model/model.schema.json',
    [string]$JsonReportPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Add-ValidationError {
    param(
        [Parameter(Mandatory)]
        [object]$Errors,
        [Parameter(Mandatory)]
        [string]$Message
    )

    ([System.Collections.Generic.List[string]]$Errors).Add($Message)
}

function Get-CanonicalJsonPaths {
    param([Parameter(Mandatory)][string]$Root)

    $directories = @(
        (Join-Path $Root 'specs/requirements/qlog'),
        (Join-Path $Root 'specs/architecture/qlog'),
        (Join-Path $Root 'specs/work-items/qlog'),
        (Join-Path $Root 'specs/verification/qlog')
    )

    foreach ($directory in $directories) {
        if (-not (Test-Path -LiteralPath $directory)) {
            continue
        }

        Get-ChildItem -LiteralPath $directory -Filter '*.json' -File
    }
}

function Get-ExpectedArtifactTypeFromRelativePath {
    param([string]$RelativePath)

    if ($RelativePath.StartsWith('specs/requirements/', [System.StringComparison]::OrdinalIgnoreCase)) { return 'specification' }
    if ($RelativePath.StartsWith('specs/architecture/', [System.StringComparison]::OrdinalIgnoreCase)) { return 'architecture' }
    if ($RelativePath.StartsWith('specs/work-items/', [System.StringComparison]::OrdinalIgnoreCase)) { return 'work_item' }
    if ($RelativePath.StartsWith('specs/verification/', [System.StringComparison]::OrdinalIgnoreCase)) { return 'verification' }

    return $null
}

function Get-ExpectedDomainFromRelativePath {
    param([string]$RelativePath)

    $parts = $RelativePath -split '/'
    if ($parts.Count -lt 3) {
        return $null
    }

    return $parts[2]
}

function Get-NonEmptyArray {
    param([object]$Value)

    $values = New-Object System.Collections.Generic.List[object]
    foreach ($item in @($Value)) {
        if ($item -is [System.Collections.IEnumerable] -and -not ($item -is [string]) -and -not ($item -is [System.Collections.IDictionary])) {
            foreach ($nested in @($item)) {
                if ($null -ne $nested -and -not [string]::IsNullOrWhiteSpace($nested.ToString())) {
                    $values.Add($nested)
                }
            }

            continue
        }

        if ($null -ne $item -and -not [string]::IsNullOrWhiteSpace($item.ToString())) {
            $values.Add($item)
        }
    }

    return $values.ToArray()
}

function ConvertTo-RelativePath {
    param(
        [Parameter(Mandatory)][string]$Root,
        [Parameter(Mandatory)][string]$Path
    )

    $rootFullPath = [System.IO.Path]::GetFullPath($Root).TrimEnd('\', '/')
    $pathFullPath = [System.IO.Path]::GetFullPath($Path)

    if ($pathFullPath.StartsWith($rootFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        $relativePath = $pathFullPath.Substring($rootFullPath.Length).TrimStart('\', '/')
    }
    else {
        $relativePath = $pathFullPath
    }

    return $relativePath.Replace('\', '/')
}

function ConvertTo-DeepHashtable {
    param([Parameter(Mandatory)][object]$InputObject)

    if ($null -eq $InputObject) {
        return $null
    }

    if ($InputObject -is [pscustomobject]) {
        $table = @{}
        foreach ($property in $InputObject.PSObject.Properties) {
            $table[$property.Name] = ConvertTo-DeepHashtable -InputObject $property.Value
        }

        return $table
    }

    if ($InputObject -is [System.Collections.IDictionary]) {
        $table = @{}
        foreach ($key in $InputObject.Keys) {
            $table[$key] = ConvertTo-DeepHashtable -InputObject $InputObject[$key]
        }

        return $table
    }

    if ($InputObject -is [System.Collections.IEnumerable] -and -not ($InputObject -is [string])) {
        $items = New-Object System.Collections.Generic.List[object]
        foreach ($item in $InputObject) {
            $items.Add((ConvertTo-DeepHashtable -InputObject $item))
        }

        return $items.ToArray()
    }

    return $InputObject
}

function Test-ArtifactShape {
    param(
        [Parameter(Mandatory)]
        [System.Collections.IDictionary]$Artifact,
        [Parameter(Mandatory)]
        [string]$RelativePath
    )

    $errors = New-Object System.Collections.Generic.List[string]

    function Add-ShapeError {
        param([Parameter(Mandatory)][string]$Message)
        $errors.Add($Message)
    }

    function Test-NonEmptyStringProperty {
        param([Parameter(Mandatory)][string]$PropertyName)

        if (-not $Artifact.Contains($PropertyName) -or [string]::IsNullOrWhiteSpace([string]$Artifact[$PropertyName])) {
            Add-ShapeError "Missing or empty '$PropertyName'."
            return
        }
    }

    function Test-StringListProperty {
        param([Parameter(Mandatory)][string]$PropertyName)

        if (-not $Artifact.Contains($PropertyName)) {
            Add-ShapeError "Missing '$PropertyName'."
            return
        }

        $values = @(Get-NonEmptyArray -Value $Artifact[$PropertyName])
        if ($values.Count -eq 0) {
            Add-ShapeError "'$PropertyName' must contain at least one non-empty string."
            return
        }

        foreach ($value in $values) {
            if ($value -isnot [string]) {
                Add-ShapeError "'$PropertyName' must contain strings."
                break
            }
        }
    }

    function Test-RequirementListProperty {
        param([Parameter(Mandatory)][string]$PropertyName)

        if (-not $Artifact.Contains($PropertyName)) {
            Add-ShapeError "Missing '$PropertyName'."
            return
        }

        $values = @(Get-NonEmptyArray -Value $Artifact[$PropertyName])
        if ($values.Count -eq 0) {
            Add-ShapeError "'$PropertyName' must contain at least one requirement object."
            return
        }

        foreach ($value in $values) {
            if ($value -isnot [System.Collections.IDictionary]) {
                Add-ShapeError "'$PropertyName' must contain requirement objects."
                break
            }
        }
    }

    function Test-Status {
        param(
            [Parameter(Mandatory)][string]$PropertyName,
            [Parameter(Mandatory)][string[]]$AllowedValues
        )

        if (-not $Artifact.Contains($PropertyName) -or [string]::IsNullOrWhiteSpace([string]$Artifact[$PropertyName])) {
            Add-ShapeError "Missing or empty '$PropertyName'."
            return
        }

        if ($AllowedValues -notcontains [string]$Artifact[$PropertyName]) {
            Add-ShapeError "'$PropertyName' has invalid value '$($Artifact[$PropertyName])'."
        }
    }

    function Test-RequirementStatement {
        param(
            [Parameter(Mandatory)]
            [string]$Statement,
            [Parameter(Mandatory)]
            [string]$RequirementId
        )

        $matches = [regex]::Matches($Statement, '\b(?:MUST NOT|SHALL NOT|SHOULD NOT|MUST|SHALL|SHOULD|MAY)\b')
        if ($matches.Count -ne 1) {
            Add-ShapeError "Requirement '$RequirementId' statement must contain exactly one normative keyword."
        }
    }

    function Test-Coverage {
        param(
            [Parameter(Mandatory)]
            [object]$Coverage,
            [Parameter(Mandatory)]
            [string]$RequirementId
        )

        if ($Coverage -isnot [System.Collections.IDictionary]) {
            Add-ShapeError "Requirement '$RequirementId' coverage must be an object."
            return
        }

        foreach ($field in @('positive', 'negative', 'edge', 'fuzz')) {
            if (-not $Coverage.Contains($field)) {
                Add-ShapeError "Requirement '$RequirementId' coverage is missing '$field'."
                continue
            }

            $value = [string]$Coverage[$field]
            if (@('required', 'optional', 'not_applicable', 'deferred') -notcontains $value) {
                Add-ShapeError "Requirement '$RequirementId' coverage field '$field' has invalid value '$value'."
            }
        }
    }

    function Test-RequirementTrace {
        param(
            [Parameter(Mandatory)]
            [object]$Trace,
            [Parameter(Mandatory)]
            [string]$RequirementId
        )

        if ($Trace -isnot [System.Collections.IDictionary]) {
            Add-ShapeError "Requirement '$RequirementId' trace must be an object."
            return
        }
    }

    function Test-RequirementItem {
        param([Parameter(Mandatory)][object]$RequirementItem)

        if ($RequirementItem -isnot [System.Collections.IDictionary]) {
            Add-ShapeError "Requirements entries must be objects."
            return
        }

        foreach ($field in @('id', 'title', 'statement')) {
            if (-not $RequirementItem.Contains($field) -or [string]::IsNullOrWhiteSpace([string]$RequirementItem[$field])) {
                Add-ShapeError "Requirement is missing or has an empty '$field'."
            }
        }

        if ($RequirementItem.Contains('statement')) {
            Test-RequirementStatement -Statement ([string]$RequirementItem['statement']) -RequirementId ([string]$RequirementItem['id'])
        }

        if ($RequirementItem.Contains('coverage')) {
            Test-Coverage -Coverage $RequirementItem['coverage'] -RequirementId ([string]$RequirementItem['id'])
        }

        if ($RequirementItem.Contains('trace')) {
            Test-RequirementTrace -Trace $RequirementItem['trace'] -RequirementId ([string]$RequirementItem['id'])
        }

        if ($RequirementItem.Contains('notes')) {
            $notes = @(Get-NonEmptyArray -Value $RequirementItem['notes'])
            foreach ($note in $notes) {
                if ($note -isnot [string]) {
                    Add-ShapeError "Requirement notes must be strings."
                    break
                }
            }
        }
    }

    Test-NonEmptyStringProperty -PropertyName 'artifact_id'
    Test-NonEmptyStringProperty -PropertyName 'artifact_type'
    Test-NonEmptyStringProperty -PropertyName 'title'
    Test-NonEmptyStringProperty -PropertyName 'domain'
    Test-NonEmptyStringProperty -PropertyName 'owner'

    if ($Artifact.Contains('domain') -and [string]$Artifact['domain'] -notmatch '^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$') {
        Add-ShapeError "Domain value '$($Artifact['domain'])' is invalid."
    }

    switch ([string]$Artifact['artifact_type']) {
        'specification' {
            Test-Status -PropertyName 'status' -AllowedValues @('draft', 'proposed', 'approved', 'implemented', 'verified', 'superseded', 'retired')
            Test-NonEmptyStringProperty -PropertyName 'capability'
            Test-NonEmptyStringProperty -PropertyName 'purpose'
            Test-RequirementListProperty -PropertyName 'requirements'

            foreach ($requirement in @(Get-NonEmptyArray -Value $Artifact['requirements'])) {
                Test-RequirementItem -RequirementItem $requirement
            }
        }
        'architecture' {
            Test-Status -PropertyName 'status' -AllowedValues @('draft', 'proposed', 'approved', 'implemented', 'verified', 'superseded', 'retired')
            Test-StringListProperty -PropertyName 'satisfies'
            Test-NonEmptyStringProperty -PropertyName 'purpose'
            Test-NonEmptyStringProperty -PropertyName 'design_summary'
        }
        'work_item' {
            Test-Status -PropertyName 'status' -AllowedValues @('planned', 'in_progress', 'blocked', 'complete', 'cancelled', 'superseded')
            Test-StringListProperty -PropertyName 'addresses'
            Test-StringListProperty -PropertyName 'design_links'
            Test-StringListProperty -PropertyName 'verification_links'
            Test-NonEmptyStringProperty -PropertyName 'summary'
            Test-NonEmptyStringProperty -PropertyName 'planned_changes'
            Test-NonEmptyStringProperty -PropertyName 'verification_plan'
        }
        'verification' {
            Test-Status -PropertyName 'status' -AllowedValues @('planned', 'passed', 'failed', 'blocked', 'waived', 'obsolete')
            Test-StringListProperty -PropertyName 'verifies'
            Test-NonEmptyStringProperty -PropertyName 'scope'
            Test-NonEmptyStringProperty -PropertyName 'verification_method'
            Test-StringListProperty -PropertyName 'procedure'
            Test-NonEmptyStringProperty -PropertyName 'expected_result'
        }
        default {
            Add-ShapeError "Unsupported artifact_type '$($Artifact['artifact_type'])'."
        }
    }

    return $errors.ToArray()
}

function Get-SchemaText {
    param(
        [Parameter(Mandatory)][string]$Uri,
        [string]$LocalPath
    )

    if (-not [string]::IsNullOrWhiteSpace($LocalPath) -and (Test-Path -LiteralPath $LocalPath)) {
        return Get-Content -LiteralPath $LocalPath -Raw
    }

    try {
        $response = Invoke-WebRequest -Uri $Uri -MaximumRedirection 5 -ErrorAction Stop
    }
    catch {
        throw "Could not download SpecTrace schema from '$Uri': $($_.Exception.Message)"
    }

    if ($null -eq $response -or [string]::IsNullOrWhiteSpace($response.Content)) {
        throw "Downloaded empty SpecTrace schema content from '$Uri'."
    }

    return $response.Content
}

function Test-ReferenceExists {
    param(
        [Parameter(Mandatory)][string]$Value,
        [Parameter(Mandatory)][hashtable]$ArtifactById,
        [Parameter(Mandatory)][hashtable]$RequirementById
    )

    if ($Value -match '^REQ-[A-Z0-9-]+$') {
        return $RequirementById.Contains($Value)
    }

    if ($Value -match '^(SPEC|ARC|WI|VER)-[A-Z0-9-]+$') {
        return $ArtifactById.Contains($Value)
    }

    return $true
}

$resolvedRepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
$localSchemaPath = $null
$candidateSchemaPath = Join-Path (Split-Path -Parent $resolvedRepoRoot) 'spec-trace/model/model.schema.json'
if (Test-Path -LiteralPath $candidateSchemaPath) {
    $localSchemaPath = (Resolve-Path -LiteralPath $candidateSchemaPath).Path
}

$schemaValidationAvailable = [bool](Get-Command Test-Json -ErrorAction SilentlyContinue)
$schemaText = $null
if ($schemaValidationAvailable) {
    $schemaText = Get-SchemaText -Uri $SchemaUri -LocalPath $localSchemaPath
}
$profiles = @($Profiles | ForEach-Object { $_.Trim().ToLowerInvariant() } | Where-Object { $_ })
if ($profiles.Count -eq 0) {
    $profiles = @('core')
}

$jsonPaths = @(Get-CanonicalJsonPaths -Root $resolvedRepoRoot)
if ($jsonPaths.Count -eq 0) {
    throw "No canonical SpecTrace JSON artifacts were found under '$resolvedRepoRoot'."
}

$errors = New-Object System.Collections.Generic.List[string]
$artifactById = @{}
$requirementById = @{}
$artifactRecords = New-Object System.Collections.Generic.List[object]

foreach ($jsonPath in $jsonPaths) {
    $relativePath = ConvertTo-RelativePath -Root $resolvedRepoRoot -Path $jsonPath.FullName
    $jsonText = Get-Content -LiteralPath $jsonPath.FullName -Raw

    try {
        $artifact = ConvertTo-DeepHashtable -InputObject ($jsonText | ConvertFrom-Json)
    }
    catch {
        Add-ValidationError -Errors $errors -Message "JSON parsing failed for '$relativePath': $($_.Exception.Message)"
        continue
    }

    if ($schemaValidationAvailable) {
        try {
            $null = Test-Json -Json $jsonText -Schema $schemaText -ErrorAction Stop
        }
        catch {
            Add-ValidationError -Errors $errors -Message "Schema validation failed for '$relativePath': $($_.Exception.Message)"
            continue
        }
    }
    else {
        foreach ($shapeError in (Test-ArtifactShape -Artifact $artifact -RelativePath $relativePath)) {
            Add-ValidationError -Errors $errors -Message "Shape validation failed for '$relativePath': $shapeError"
        }
    }
    $expectedArtifactType = Get-ExpectedArtifactTypeFromRelativePath -RelativePath $relativePath
    $expectedDomain = Get-ExpectedDomainFromRelativePath -RelativePath $relativePath
    $expectedFileStem = [System.IO.Path]::GetFileNameWithoutExtension($jsonPath.FullName)
    $markdownCompanion = [System.IO.Path]::ChangeExtension($jsonPath.FullName, '.md')

    if ($artifact['artifact_id'] -ne $expectedFileStem) {
        Add-ValidationError -Errors $errors -Message "Artifact id/file mismatch in '$relativePath': expected '$expectedFileStem' but found '$($artifact['artifact_id'])'."
    }

    if ($artifact['artifact_type'] -ne $expectedArtifactType) {
        Add-ValidationError -Errors $errors -Message "Artifact type/path mismatch in '$relativePath': expected '$expectedArtifactType' but found '$($artifact['artifact_type'])'."
    }

    if ($artifact['domain'] -ne $expectedDomain) {
        Add-ValidationError -Errors $errors -Message "Domain/path mismatch in '$relativePath': expected '$expectedDomain' but found '$($artifact['domain'])'."
    }

    if (Test-Path -LiteralPath $markdownCompanion) {
        Add-ValidationError -Errors $errors -Message "Residual canonical Markdown artifact '$(ConvertTo-RelativePath -Root $resolvedRepoRoot -Path $markdownCompanion)' exists for '$relativePath'. Remove the sibling '.md' file."
    }

    $artifactId = $artifact['artifact_id']
    if ($artifactById.Contains($artifactId)) {
        Add-ValidationError -Errors $errors -Message "Duplicate artifact id '$artifactId' in '$relativePath' and '$($artifactById[$artifactId].path)'."
        continue
    }

    $record = [ordered]@{
        id       = $artifactId
        type     = $artifact['artifact_type']
        path     = $relativePath
        artifact = $artifact
    }

    $artifactById[$artifactId] = $record
    $artifactRecords.Add($record)

    if ($artifact['artifact_type'] -eq 'specification') {
        foreach ($requirement in @($artifact['requirements'])) {
            $requirementId = $requirement['id']
            if ($requirementById.Contains($requirementId)) {
                Add-ValidationError -Errors $errors -Message "Duplicate requirement id '$requirementId' in '$relativePath' and '$($requirementById[$requirementId].path)'."
                continue
            }

            $requirementById[$requirementId] = [ordered]@{
                id          = $requirementId
                path        = $relativePath
                artifact_id = $artifactId
                requirement = $requirement
            }
        }
    }
}

$downstreamRefs = @{
    architecture = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparer]::Ordinal)
    work_item    = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparer]::Ordinal)
    verification = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparer]::Ordinal)
}

foreach ($record in $artifactRecords) {
    $artifact = $record.artifact
    $relativePath = $record.path

    foreach ($relatedArtifact in (Get-NonEmptyArray -Value $artifact['related_artifacts'])) {
        if (-not (Test-ReferenceExists -Value $relatedArtifact -ArtifactById $artifactById -RequirementById $requirementById)) {
            Add-ValidationError -Errors $errors -Message "Unresolved related artifact '$relatedArtifact' in '$relativePath'."
        }
    }

    switch ($artifact['artifact_type']) {
        'architecture' {
            foreach ($requirementId in (Get-NonEmptyArray -Value $artifact['satisfies'])) {
                if (-not $requirementById.Contains($requirementId)) {
                    Add-ValidationError -Errors $errors -Message "Unresolved requirement '$requirementId' in satisfies for '$relativePath'."
                }
            }
        }
        'work_item' {
            foreach ($requirementId in (Get-NonEmptyArray -Value $artifact['addresses'])) {
                if (-not $requirementById.Contains($requirementId)) {
                    Add-ValidationError -Errors $errors -Message "Unresolved requirement '$requirementId' in addresses for '$relativePath'."
                }
            }

            foreach ($architectureId in (Get-NonEmptyArray -Value $artifact['design_links'])) {
                if (-not ($artifactById.Contains($architectureId) -and $artifactById[$architectureId].type -eq 'architecture')) {
                    Add-ValidationError -Errors $errors -Message "Unresolved architecture '$architectureId' in design_links for '$relativePath'."
                }
            }

            foreach ($verificationId in (Get-NonEmptyArray -Value $artifact['verification_links'])) {
                if (-not ($artifactById.Contains($verificationId) -and $artifactById[$verificationId].type -eq 'verification')) {
                    Add-ValidationError -Errors $errors -Message "Unresolved verification '$verificationId' in verification_links for '$relativePath'."
                }
            }
        }
        'verification' {
            foreach ($requirementId in (Get-NonEmptyArray -Value $artifact['verifies'])) {
                if (-not $requirementById.Contains($requirementId)) {
                    Add-ValidationError -Errors $errors -Message "Unresolved requirement '$requirementId' in verifies for '$relativePath'."
                }
            }
        }
        'specification' {
            foreach ($requirement in @($artifact['requirements'])) {
                $trace = if ($requirement.Contains('trace')) { $requirement['trace'] } else { @{} }
                foreach ($traceKey in @('satisfied_by', 'implemented_by', 'verified_by', 'derived_from', 'supersedes', 'related')) {
                    foreach ($value in (Get-NonEmptyArray -Value $trace[$traceKey])) {
                        if (-not (Test-ReferenceExists -Value $value -ArtifactById $artifactById -RequirementById $requirementById)) {
                            Add-ValidationError -Errors $errors -Message "Unresolved '$traceKey' reference '$value' in '$relativePath' ($($requirement['id']))."
                        }
                    }
                }

                foreach ($architectureId in (Get-NonEmptyArray -Value $trace['satisfied_by'])) {
                    [void]$downstreamRefs['architecture'].Add($architectureId)
                    if ($artifactById.Contains($architectureId) -and @($artifactById[$architectureId].artifact['satisfies']) -notcontains $requirement['id']) {
                        Add-ValidationError -Errors $errors -Message "Missing reciprocal architecture trace from '$architectureId' back to '$($requirement['id'])'."
                    }
                }

                foreach ($workItemId in (Get-NonEmptyArray -Value $trace['implemented_by'])) {
                    [void]$downstreamRefs['work_item'].Add($workItemId)
                    if ($artifactById.Contains($workItemId) -and @($artifactById[$workItemId].artifact['addresses']) -notcontains $requirement['id']) {
                        Add-ValidationError -Errors $errors -Message "Missing reciprocal work-item trace from '$workItemId' back to '$($requirement['id'])'."
                    }
                }

                foreach ($verificationId in (Get-NonEmptyArray -Value $trace['verified_by'])) {
                    [void]$downstreamRefs['verification'].Add($verificationId)
                    if ($artifactById.Contains($verificationId) -and @($artifactById[$verificationId].artifact['verifies']) -notcontains $requirement['id']) {
                        Add-ValidationError -Errors $errors -Message "Missing reciprocal verification trace from '$verificationId' back to '$($requirement['id'])'."
                    }
                }
            }
        }
    }
}

if ($profiles -contains 'traceable') {
    foreach ($requirementRecord in $requirementById.Values) {
        $trace = if ($requirementRecord.requirement.Contains('trace')) { $requirementRecord.requirement['trace'] } else { @{} }
        $downstreamCount = (Get-NonEmptyArray -Value $trace['satisfied_by']).Count + (Get-NonEmptyArray -Value $trace['implemented_by']).Count + (Get-NonEmptyArray -Value $trace['verified_by']).Count
        if ($downstreamCount -eq 0) {
            Add-ValidationError -Errors $errors -Message "Requirement '$($requirementRecord.id)' is missing downstream trace links."
        }
    }
}

$report = [ordered]@{
    repo_root      = $resolvedRepoRoot
    schema_uri     = $SchemaUri
    profiles       = $profiles
    artifact_count = $jsonPaths.Count
    errors         = @($errors)
}

if (-not [string]::IsNullOrWhiteSpace($JsonReportPath)) {
    $resolvedReportPath = if ([System.IO.Path]::IsPathRooted($JsonReportPath)) { $JsonReportPath } else { Join-Path $resolvedRepoRoot $JsonReportPath }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $resolvedReportPath) | Out-Null
    $report | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $resolvedReportPath
}

if ($errors.Count -gt 0) {
    throw "SpecTrace JSON validation failed with $($errors.Count) error(s).`n$($errors -join [Environment]::NewLine)"
}

Write-Output "Validated $($jsonPaths.Count) SpecTrace JSON artifact(s) for profile(s): $($profiles -join ', ')."
