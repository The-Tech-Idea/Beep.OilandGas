<#
.SYNOPSIS
Exports declared Razor routes and controller HTTP attributes for the company coverage audit.
.DESCRIPTION
Source inventory only. Does not infer runtime authorization, composed routes, data access or feature completeness.
#>
[CmdletBinding()]
param(
    [string]$RootPath = (Split-Path -Parent $PSScriptRoot),
    [Parameter(Mandatory)][string]$OutputPath
)
$ErrorActionPreference = 'Stop'
$surfaceRoot = (Resolve-Path -LiteralPath $RootPath).Path
$surfaceRecords = [Collections.Generic.List[object]]::new()
foreach ($surfaceArea in @('Beep.OilandGas.Web', 'Beep.OilandGas.ApiService/Controllers')) {
    $surfaceDirectory = Join-Path $surfaceRoot $surfaceArea
    foreach ($surfaceFile in (Get-ChildItem -LiteralPath $surfaceDirectory -Recurse -File | Sort-Object FullName)) {
        if ($surfaceFile.FullName -match '[\\/](bin|obj)[\\/]' -or $surfaceFile.Extension -notin @('.cs', '.razor')) { continue }
        $surfaceRelative = [IO.Path]::GetRelativePath($surfaceRoot, $surfaceFile.FullName).Replace('\', '/')
        $surfaceLines = Get-Content -LiteralPath $surfaceFile.FullName
        for ($surfaceIndex = 0; $surfaceIndex -lt $surfaceLines.Count; $surfaceIndex++) {
            $surfaceLine = $surfaceLines[$surfaceIndex]
            $surfaceMatches = if ($surfaceFile.Extension -eq '.razor') {
                [regex]::Matches($surfaceLine, '^\s*@page\s+"([^"]+)"')
            } else {
                [regex]::Matches($surfaceLine, '\[Http(Get|Post|Put|Patch|Delete|Head|Options)\s*(?:\(\s*"([^"]*)"\s*\))?\s*\]')
            }
            foreach ($surfaceMatch in $surfaceMatches) {
                $surfaceIsPage = $surfaceFile.Extension -eq '.razor'
                $surfaceRecords.Add([pscustomobject]@{
                    Kind = $(if ($surfaceIsPage) { 'Page' } else { 'HttpAttribute' })
                    Method = $(if ($surfaceIsPage) { '' } else { $surfaceMatch.Groups[1].Value.ToUpperInvariant() })
                    DeclaredTemplate = $(if ($surfaceIsPage) { $surfaceMatch.Groups[1].Value } else { $surfaceMatch.Groups[2].Value })
                    File = $surfaceRelative
                    Line = $surfaceIndex + 1
                    Verification = 'Source declaration only; owner, authorization, persistence and runtime behavior require review'
                })
            }
        }
    }
}
$surfaceRecords.ToArray() | Export-Csv -LiteralPath $OutputPath -NoTypeInformation -Encoding utf8
Write-Output "Exported $($surfaceRecords.Count) source declarations to $OutputPath."
