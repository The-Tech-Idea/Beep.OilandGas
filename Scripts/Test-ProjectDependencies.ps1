<#
.SYNOPSIS
Evaluates project references through MSBuild and reports missing dependency checkouts before restore/build.
.DESCRIPTION
Traverses actual evaluated references, including imported props/targets and sibling projects.
Does not restore packages, change source or install dependencies. Missing references fail the gate.
#>
[CmdletBinding()]
param(
    [string]$RootPath = (Split-Path -Parent $PSScriptRoot),
    [string[]]$Projects,
    [string]$Solution = 'Beep.OilandGas.sln',
    [string]$Configuration = 'Release',
    [string]$ReportPath
)

$ErrorActionPreference = 'Stop'
$dependencyRoot = (Resolve-Path -LiteralPath $RootPath).Path
if ([string]::IsNullOrWhiteSpace($Configuration)) { throw 'Specify a build configuration.' }
if (-not $Projects -or $Projects.Count -eq 0) {
    $dependencySolution = [IO.Path]::GetFullPath((Join-Path $dependencyRoot $Solution))
    $dependencySolutionText = Get-Content -LiteralPath $dependencySolution -Raw
    $dependencySolutionDirectory = Split-Path -Parent $dependencySolution
    $Projects = @([regex]::Matches($dependencySolutionText,
        '(?im)^Project\("[^"]+"\)\s*=\s*"[^"]+",\s*"([^"]+\.csproj)",') |
        ForEach-Object { [IO.Path]::GetFullPath((Join-Path $dependencySolutionDirectory $_.Groups[1].Value)) })
    if ($Projects.Count -eq 0) { throw 'The solution contains no C# project declarations.' }
}
$dependencyQueue = [Collections.Generic.Queue[string]]::new()
$dependencySeen = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$dependencyFailures = [Collections.Generic.List[object]]::new()
$dependencyEdges = [Collections.Generic.List[object]]::new()
foreach ($dependencyProject in $Projects) {
    $dependencyEntry = if ([IO.Path]::IsPathRooted($dependencyProject)) { $dependencyProject }
        else { Join-Path $dependencyRoot $dependencyProject }
    $dependencyQueue.Enqueue([IO.Path]::GetFullPath($dependencyEntry))
}
while ($dependencyQueue.Count -gt 0) {
    $dependencyProject = $dependencyQueue.Dequeue()
    if (-not $dependencySeen.Add($dependencyProject)) { continue }
    if (-not (Test-Path -LiteralPath $dependencyProject -PathType Leaf)) {
        $dependencyFailures.Add([pscustomobject]@{ Project = $dependencyProject; Reason = 'Missing project' })
        continue
    }
    $dependencyOutput = & dotnet msbuild $dependencyProject -getItem:ProjectReference "-p:Configuration=$Configuration" -nologo 2>&1
    if ($LASTEXITCODE -ne 0) {
        $dependencyFailures.Add([pscustomobject]@{ Project = $dependencyProject; Reason = "MSBuild evaluation failed: $($dependencyOutput -join [Environment]::NewLine)" })
        continue
    }
    $dependencyEvaluation = ($dependencyOutput -join [Environment]::NewLine) | ConvertFrom-Json
    foreach ($dependencyReference in $dependencyEvaluation.Items.ProjectReference) {
        $dependencyTarget = $dependencyReference.FullPath
        $dependencyEdges.Add([pscustomobject]@{ Project = $dependencyProject; Reference = $dependencyTarget })
        $dependencyQueue.Enqueue($dependencyTarget)
    }
}
$dependencyReport = [pscustomobject]@{
    EvaluatedUtc = [DateTime]::UtcNow.ToString('O')
    Configuration = $Configuration
    EntryProjects = @($Projects)
    Projects = @($dependencySeen | Sort-Object)
    References = @($dependencyEdges.ToArray())
    Failures = @($dependencyFailures.ToArray())
}
if ($ReportPath) {
    $dependencyReport | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $ReportPath -Encoding utf8
}
Write-Output "Dependency preflight: $($dependencySeen.Count) projects, $($dependencyEdges.Count) references, $($dependencyFailures.Count) failures."
foreach ($dependencyFailure in $dependencyFailures) {
    Write-Output "$($dependencyFailure.Project): $($dependencyFailure.Reason)"
}
if ($dependencyFailures.Count -gt 0) { exit 1 }
