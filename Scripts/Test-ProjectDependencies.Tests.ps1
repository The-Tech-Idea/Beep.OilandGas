[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$fixtureParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$fixtureRoot = Join-Path $fixtureParent ('oilgas-dependency-tests-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixtureRoot | Out-Null
$fixtureGate = Join-Path $PSScriptRoot 'Test-ProjectDependencies.ps1'
try {
    '<Project />' | Set-Content -LiteralPath (Join-Path $fixtureRoot 'leaf.csproj')
    '<Project><ItemGroup><ProjectReference Include="leaf.csproj" /></ItemGroup></Project>' |
        Set-Content -LiteralPath (Join-Path $fixtureRoot 'root.csproj')
    $fixtureReport = Join-Path $fixtureRoot 'report.json'
    & pwsh -NoProfile -File $fixtureGate -RootPath $fixtureRoot -Projects root.csproj -ReportPath $fixtureReport
    if ($LASTEXITCODE -ne 0) { throw 'Valid dependency graph failed.' }
    $fixtureResult = Get-Content -LiteralPath $fixtureReport -Raw | ConvertFrom-Json
    if ($fixtureResult.Projects.Count -ne 2 -or $fixtureResult.References.Count -ne 1) { throw 'Dependency traversal omitted a project.' }

    '<Project><Import Project="references.props" /></Project>' | Set-Content -LiteralPath (Join-Path $fixtureRoot 'root.csproj')
    '<Project><ItemGroup><ProjectReference Include="missing.csproj" /></ItemGroup></Project>' |
        Set-Content -LiteralPath (Join-Path $fixtureRoot 'references.props')
    & pwsh -NoProfile -File $fixtureGate -RootPath $fixtureRoot -Projects root.csproj -ReportPath $fixtureReport
    if ($LASTEXITCODE -ne 1) { throw 'Missing imported dependency did not fail closed.' }
    $fixtureResult = Get-Content -LiteralPath $fixtureReport -Raw | ConvertFrom-Json
    if ($fixtureResult.Failures.Count -ne 1 -or $fixtureResult.Failures[0].Reason -ne 'Missing project') { throw 'Missing dependency report was incorrect.' }

    '<Project><ItemGroup><ProjectReference Include="leaf.csproj" /></ItemGroup></Project>' |
        Set-Content -LiteralPath (Join-Path $fixtureRoot 'root.csproj')
    '<Project><ItemGroup><ProjectReference Include="root.csproj" /></ItemGroup></Project>' |
        Set-Content -LiteralPath (Join-Path $fixtureRoot 'leaf.csproj')
    & pwsh -NoProfile -File $fixtureGate -RootPath $fixtureRoot -Projects root.csproj -ReportPath $fixtureReport
    if ($LASTEXITCODE -ne 0) { throw 'Reference traversal failed to terminate for a cycle.' }
    $fixtureResult = Get-Content -LiteralPath $fixtureReport -Raw | ConvertFrom-Json
    if ($fixtureResult.Projects.Count -ne 2 -or $fixtureResult.References.Count -ne 2) { throw 'Cycle traversal was incorrect.' }

    '<Project />' | Set-Content -LiteralPath (Join-Path $fixtureRoot 'root.csproj')
    '<Project><ItemGroup Condition="''$(Configuration)'' == ''Release''"><ProjectReference Include="release-only-missing.csproj" /></ItemGroup></Project>' |
        Set-Content -LiteralPath (Join-Path $fixtureRoot 'tests.csproj')
    @'
Microsoft Visual Studio Solution File, Format Version 12.00
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "App", "root.csproj", "{1B2DF795-B5D8-49F9-BBDC-2A161A561345}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Tests", "tests.csproj", "{DAFF1BB5-049E-4AB2-B5CD-D51302977560}"
EndProject
'@ | Set-Content -LiteralPath (Join-Path $fixtureRoot 'fixture.sln')
    & pwsh -NoProfile -File $fixtureGate -RootPath $fixtureRoot -Solution fixture.sln -ReportPath $fixtureReport
    if ($LASTEXITCODE -ne 1) { throw 'Solution preflight missed a Release-only test dependency.' }
    $fixtureResult = Get-Content -LiteralPath $fixtureReport -Raw | ConvertFrom-Json
    if ($fixtureResult.EntryProjects.Count -ne 2 -or $fixtureResult.Failures.Count -ne 1 -or
        $fixtureResult.Configuration -ne 'Release') { throw 'Solution/configuration evidence was incorrect.' }
    & pwsh -NoProfile -File $fixtureGate -RootPath $fixtureRoot -Solution fixture.sln -Configuration Debug -ReportPath $fixtureReport
    if ($LASTEXITCODE -ne 0) { throw 'Debug preflight incorrectly included the Release-only reference.' }
    Write-Output 'PASS: valid graph, imported missing reference, cycle traversal and solution/configuration coverage.'
}
finally {
    $fixtureResolved = [IO.Path]::GetFullPath($fixtureRoot)
    if ([IO.Path]::GetDirectoryName($fixtureResolved) -ne $fixtureParent.TrimEnd([IO.Path]::DirectorySeparatorChar) -or
        [IO.Path]::GetFileName($fixtureResolved) -notlike 'oilgas-dependency-tests-*') {
        throw 'Refusing cleanup outside the dependency test fixture directory.'
    }
    Remove-Item -LiteralPath $fixtureResolved -Recurse -Force
}
