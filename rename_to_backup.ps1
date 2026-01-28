# Rename PascalCase duplicate files to .bak

Write-Host "========================================="
Write-Host "Renaming PascalCase files to .bak"
Write-Host "========================================="

$allFiles = Get-ChildItem -Path "Beep.OilandGas.Models\Data" -Recurse -File -Filter "*.cs" | 
Where-Object { $_.Name -ne "ModelEntityBase.cs" }

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

$duplicateGroups = $sameDirDupes.GetEnumerator() | 
Where-Object { $_.Value.Count -gt 1 } | 
Sort-Object { $_.Value[0].Directory.Name }

$renamedCount = 0

foreach ($group in $duplicateGroups) {
    $files = $group.Value | Sort-Object Name
    
    # Identify PascalCase file (not UPPER_CASE)
    $pascalFile = $files | Where-Object { $_.Name -cnotmatch '^[A-Z_]+\.cs$' } | Select-Object -First 1
    
    if ($pascalFile) {
        $newName = $pascalFile.FullName + ".bak"
        
        Write-Host "Renaming: $($pascalFile.Name) -> $($pascalFile.Name).bak"
        
        Rename-Item -Path $pascalFile.FullName -NewName $newName -Force
        $renamedCount++
    }
}

Write-Host "`n========================================="
Write-Host "Rename Complete!"
Write-Host "Files renamed to .bak: $renamedCount"
Write-Host "========================================="
