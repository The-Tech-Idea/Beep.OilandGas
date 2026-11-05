function Add-OilGasRestoredOwnerReferences {
    param([Parameter(Mandatory)][xml]$Project, [Parameter(Mandatory)][string]$RepositoryRoot)
    $manifestPath = Join-Path $RepositoryRoot 'Plans/FrameworkAppEnhancement/Module-Data-Ownership/restoration-progress.json'
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    if ($Project.SelectSingleNode('/Project/ItemGroup/Compile[contains(@Include,"ModuleSchemaBoundary.cs")]') -and
        -not $Project.SelectSingleNode('/Project/ItemGroup/Compile[contains(@Include,"ApplicationContractSchemaPolicy.cs")]')) {
        $policyGroup = $Project.CreateElement('ItemGroup')
        $policySource = $Project.CreateElement('Compile')
        $policySource.SetAttribute('Include', (Join-Path $RepositoryRoot 'Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ApplicationContractSchemaPolicy.cs'))
        [void]$policyGroup.AppendChild($policySource); [void]$Project.DocumentElement.AppendChild($policyGroup)
    }
    $definitions = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($move in $manifest.moves) {
        [void]$definitions.Add([IO.Path]::GetFullPath((Join-Path $RepositoryRoot $move.destination)))
        foreach ($merged in $move.merged_into) { [void]$definitions.Add([IO.Path]::GetFullPath((Join-Path $RepositoryRoot $merged))) }
    }
    foreach ($source in @($Project.SelectNodes('/Project/ItemGroup/Compile'))) {
        $include = $source.GetAttribute('Include')
        if ([IO.Path]::IsPathRooted($include) -and $definitions.Contains([IO.Path]::GetFullPath($include))) {
            [void]$source.ParentNode.RemoveChild($source)
        }
    }
    $owners = @($manifest.moves.owner | Sort-Object -Unique) + @('Beep.OilandGas.PPDM.Models')
    $group = $Project.CreateElement('ItemGroup')
    [void]$Project.DocumentElement.AppendChild($group)
    foreach ($owner in $owners) {
        $assemblyPath = Join-Path $RepositoryRoot "$owner/bin/Debug/net10.0/$owner.dll"
        if (-not (Test-Path -LiteralPath $assemblyPath)) { throw "Build the actual restored owner first: $assemblyPath" }
        $aliases = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        [void]$aliases.Add('global'); [void]$aliases.Add('actualOwners')
        foreach ($reference in @($Project.SelectNodes('/Project/ItemGroup/Reference'))) {
            $hint = $reference.SelectSingleNode('HintPath')
            if ($reference.GetAttribute('Include') -eq $owner -or ($hint -and [IO.Path]::GetFileNameWithoutExtension($hint.InnerText) -eq $owner)) {
                $oldAliases = $reference.SelectSingleNode('Aliases')
                if ($oldAliases) { foreach ($alias in $oldAliases.InnerText.Split(',')) { [void]$aliases.Add($alias.Trim()) } }
                [void]$reference.ParentNode.RemoveChild($reference)
            }
        }
        $reference = $Project.CreateElement('Reference'); $reference.SetAttribute('Include', $owner)
        $hint = $Project.CreateElement('HintPath'); $hint.InnerText = $assemblyPath; [void]$reference.AppendChild($hint)
        $aliasNode = $Project.CreateElement('Aliases'); $aliasNode.InnerText = (@($aliases) -join ','); [void]$reference.AppendChild($aliasNode)
        [void]$group.AppendChild($reference)
    }
    # The real stable failure vocabulary is required by restored owner signatures.
    if (-not $Project.SelectSingleNode('/Project/ItemGroup/Reference[@Include="TheTechIdeaWeb.Diagnostics.Abstractions"]')) {
        $diagnosticsPath = Join-Path $RepositoryRoot '../Beep.Web/TheTechIdeaWeb.Diagnostics.Abstractions/bin/Debug/net10.0/TheTechIdeaWeb.Diagnostics.Abstractions.dll'
        if (-not (Test-Path -LiteralPath $diagnosticsPath)) { throw 'Build the canonical Diagnostics.Abstractions project first.' }
        $reference = $Project.CreateElement('Reference'); $reference.SetAttribute('Include', 'TheTechIdeaWeb.Diagnostics.Abstractions')
        $hint = $Project.CreateElement('HintPath'); $hint.InnerText = $diagnosticsPath; [void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
    }
    $beepPath = Join-Path $RepositoryRoot 'Beep.OilandGas.Models/bin/Debug/net10.0/DataManagementModels.dll'
    if (-not $Project.SelectSingleNode('/Project/ItemGroup/Reference[@Include="DataManagementModels"]') -and (Test-Path -LiteralPath $beepPath)) {
        $reference = $Project.CreateElement('Reference'); $reference.SetAttribute('Include', 'DataManagementModels')
        $hint = $Project.CreateElement('HintPath'); $hint.InnerText = $beepPath; [void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
    }
}
function Update-OilGasRestoredFixtureProjectFile {
    param([Parameter(Mandatory)][string]$ProjectPath, [Parameter(Mandatory)][string]$RepositoryRoot)
    [xml]$fixtureProject = Get-Content -LiteralPath $ProjectPath -Raw
    Add-OilGasRestoredOwnerReferences -Project $fixtureProject -RepositoryRoot $RepositoryRoot
    $fixtureProject.Save($ProjectPath)
}
