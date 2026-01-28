# Function to convert PascalCase to UPPER_CASE_WITH_UNDERSCORES
function Convert-ToUpperSnakeCase {
    param([string]$text)
    
    # Insert underscore before capital letters (except first)
    $result = $text -creplace '([a-z])([A-Z])', '$1_$2'
    # Insert underscore before numbers
    $result = $result -creplace '([a-zA-Z])(\d)', '$1_$2'
    # Convert to uppercase
    return $result.ToUpper()
}

# Function to normalize property name for comparison (remove _, lowercase)
function Get-NormalizedName {
    param([string]$name)
    return $name.ToLower() -replace '[_\-\s]', ''
}

# Function to merge properties from PascalCase to UPPER_CASE file
function Merge-Properties {
    param(
        [string]$upperCaseFile,
        [string]$pascalCaseFile
    )
    
    Write-Host "`nProcessing: $($upperCaseFile | Split-Path -Leaf)"
    
    # Read both files
    $upperContent = Get-Content $upperCaseFile -Raw
    $pascalContent = Get-Content $pascalCaseFile -Raw
    
    # Extract properties from UPPER_CASE file
    $upperProps = [regex]::Matches($upperContent, 'private\s+(\w+\??)\s+(\w+)Value[^;]*;\s*public\s+\1\s+(\w+)\s*\{[^}]+get[^}]+set[^}]+\}', 
        [System.Text.RegularExpressions.RegexOptions]::Singleline) | ForEach-Object {
        @{
            Type           = $_.Groups[1].Value
            Name           = $_.Groups[3].Value
            NormalizedName = Get-NormalizedName $_.Groups[3].Value
            FullMatch      = $_.Value
        }
    }
    
    # Extract properties from PascalCase file
    $pascalProps = [regex]::Matches($pascalContent, 'private\s+(\w+\??)\s+(\w+)Value[^;]*;\s*(?:///[^\n]*\n\s*)*public\s+\1\s+(\w+)\s*\{[^}]+get[^}]+set[^}]+\}',
        [System.Text.RegularExpressions.RegexOptions]::Singleline) | ForEach-Object {
        @{
            Type           = $_.Groups[1].Value
            Name           = $_.Groups[3].Value
            NormalizedName = Get-NormalizedName $_.Groups[3].Value
            FullMatch      = $_.Value
        }
    }
    
    # Find unique properties in PascalCase
    $upperNormalized = $upperProps | ForEach-Object { $_.NormalizedName }
    $uniqueProps = $pascalProps | Where-Object { $_.NormalizedName -notin $upperNormalized }
    
    if ($uniqueProps.Count -eq 0) {
        Write-Host "  ✅ No unique properties to merge"
        return $false
    }
    
    Write-Host "  ⚠️  Found $($uniqueProps.Count) unique properties to merge:"
    
    # Generate new properties in UPPER_CASE format
    $newProperties = ""
    foreach ($prop in $uniqueProps) {
        $upperName = Convert-ToUpperSnakeCase $prop.Name
        $upperValueName = "${upperName}Value"
        
        Write-Host "    - $($prop.Name) -> $upperName"
        
        # Create property in UPPER_CASE format
        $newProperties += "`r`n        private $($prop.Type) $upperValueName;`r`n"
        $newProperties += "        public $($prop.Type) $upperName`r`n"
        $newProperties += "        {`r`n"
        $newProperties += "            get { return this.$upperValueName; }`r`n"
        $newProperties += "            set { SetProperty(ref $upperValueName, value); }`r`n"
        $newProperties += "        }`r`n"
    }
    
    # Find insertion point (before closing brace of class)
    $insertionPoint = $upperContent.LastIndexOf("    }")
    if ($insertionPoint -eq -1) {
        Write-Host "  ❌ Could not find class closing brace"
        return $false
    }
    
    # Insert new properties
    $newContent = $upperContent.Substring(0, $insertionPoint) + $newProperties + $upperContent.Substring($insertionPoint)
    
    # Write back to file
    Set-Content -Path $upperCaseFile -Value $newContent -NoNewline
    
    Write-Host "  ✅ Merged successfully"
    return $true
}

# Main script
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

$mergedCount = 0
$skippedCount = 0

foreach ($group in $duplicateGroups) {
    $files = $group.Value | Sort-Object Name
    
    # Identify UPPER_CASE and PascalCase files
    $upperFile = $files | Where-Object { $_.Name -cmatch '^[A-Z_]+\.cs$' } | Select-Object -First 1
    $pascalFile = $files | Where-Object { $_.Name -cnotmatch '^[A-Z_]+\.cs$' } | Select-Object -First 1
    
    if (-not $upperFile -or -not $pascalFile) { continue }
    
    $merged = Merge-Properties -upperCaseFile $upperFile.FullName -pascalCaseFile $pascalFile.FullName
    
    if ($merged) {
        $mergedCount++
    }
    else {
        $skippedCount++
    }
}

Write-Host "`n========================================="
Write-Host "Merge Complete!"
Write-Host "Files merged: $mergedCount"
Write-Host "Files skipped (no unique props): $skippedCount"
Write-Host "========================================="
