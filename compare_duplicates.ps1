$allFiles = Get-ChildItem -Path "Beep.OilandGas.Models\Data" -Recurse -File -Filter "*.cs" | Where-Object { $_.Name -ne "ModelEntityBase.cs" }

# Group by directory + normalized name
$sameDirDupes = @{}
foreach ($file in $allFiles) {
    $normalized = $file.Name.ToLower() -replace '[_\-\s]', ''
    $key = "$($file.DirectoryName)|$normalized"
    if (-not $sameDirDupes.ContainsKey($key)) {
        $sameDirDupes[$key] = @()
    }
    $sameDirDupes[$key] += $file
}

$duplicateGroups = $sameDirDupes.GetEnumerator() | Where-Object { $_.Value.Count -gt 1 } | Sort-Object { $_.Value[0].Directory.Name }

$report = "# Property Comparison Report`n"
$report += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n"
$report += "Total Duplicate Groups: $($duplicateGroups.Count)`n`n"

$needsMerging = 0
$identicalCount = 0

foreach ($group in $duplicateGroups) {
    $files = $group.Value | Sort-Object Name
    
    # Identify UPPER_CASE and PascalCase files
    $upperFile = $files | Where-Object { $_.Name -cmatch '^[A-Z_]+\.cs$' } | Select-Object -First 1
    $pascalFile = $files | Where-Object { $_.Name -cnotmatch '^[A-Z_]+\.cs$' } | Select-Object -First 1
    
    if (-not $upperFile -or -not $pascalFile) { continue }
    
    $report += "## $($upperFile.Directory.Name) / $($upperFile.BaseName)`n`n"
    
    # Extract properties
    $upperContent = Get-Content $upperFile.FullName -Raw
    $pascalContent = Get-Content $pascalFile.FullName -Raw
    
    $upperProps = [regex]::Matches($upperContent, 'public\s+(\w+\??)\s+(\w+)\s*\{') | ForEach-Object { 
        @{Type=$_.Groups[1].Value; Name=$_.Groups[2].Value}
    }
    
    $pascalProps = [regex]::Matches($pascalContent, 'public\s+(\w+\??)\s+(\w+)\s*\{') | ForEach-Object { 
        @{Type=$_.Groups[1].Value; Name=$_.Groups[2].Value}
    }
    
    $report += "- **UPPER_CASE**: ``$($upperFile.Name)`` ($($upperProps.Count) properties)`n"
    $report += "- **PascalCase**: ``$($pascalFile.Name)`` ($($pascalProps.Count) properties)`n`n"
    
    # Normalize for comparison
    $upperNorm = $upperProps | ForEach-Object { $_.Name.ToLower() -replace '_', '' }
    $pascalNorm = $pascalProps | ForEach-Object { $_.Name.ToLower() -replace '_', '' }
    
    $uniqueToPascal = @()
    for ($i = 0; $i -lt $pascalProps.Count; $i++) {
        $normName = $pascalProps[$i].Name.ToLower() -replace '_', ''
        if ($normName -notin $upperNorm) {
            $uniqueToPascal += $pascalProps[$i]
        }
    }
    
    if ($uniqueToPascal.Count -gt 0) {
        $report += "**⚠️ NEEDS MERGING**: $($uniqueToPascal.Count) properties only in PascalCase:`n"
        foreach ($prop in $uniqueToPascal) {
            $report += "  - ``$($prop.Type) $($prop.Name)```n"
        }
        $needsMerging++
    } else {
        $report += "**✅ IDENTICAL**: All properties present in UPPER_CASE version`n"
        $identicalCount++
    }
    
    $report += "`n"
}

$report += "`n---`n`n"
$report += "## Summary`n`n"
$report += "- **Total Groups**: $($duplicateGroups.Count)`n"
$report += "- **Identical** (safe to delete): $identicalCount`n"
$report += "- **Needs Merging**: $needsMerging`n"

$report | Out-File "property_comparison_report.md" -Encoding UTF8
Write-Host "Report generated: property_comparison_report.md"
Write-Host "Total groups: $($duplicateGroups.Count)"
Write-Host "Identical: $identicalCount"
Write-Host "Needs merging: $needsMerging"
