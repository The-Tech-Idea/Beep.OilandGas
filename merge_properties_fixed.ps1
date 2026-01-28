# Function to convert PascalCase to UPPER_CASE_WITH_UNDERSCORES
function Convert-ToUpperSnakeCase {
    param([string]$text)
    
    # Insert underscore before capital letters (except first) and before numbers
    $result = $text -creplace '([a-z0-9])([A-Z])', '$1_$2'
    $result = $result -creplace '([a-zA-Z])(\d)', '$1_$2'
    return $result.ToUpper()
}

# Function to normalize property name for comparison
function Get-NormalizedName {
    param([string]$name)
    return $name.ToLower() -replace '[_\-\s]', ''
}

# Function to extract properties from a C# file
function Get-Properties {
    param([string]$filePath)
    
    $content = Get-Content $filePath -Raw
    $properties = @()
    
    # Match property pattern
    $matches = [regex]::Matches($content, 'private\s+(\w+\??)\s+(\w+)Value[^;]*;[^p]*public\s+\1\s+(\w+)\s*\{', 
        [System.Text.RegularExpressions.RegexOptions]::Singleline)
    
    foreach ($match in $matches) {
        $properties += @{
            Type           = $match.Groups[1].Value
            Name           = $match.Groups[3].Value
            NormalizedName = Get-NormalizedName $match.Groups[3].Value
        }
    }
    
    return $properties
}

# Function to merge properties
function Merge-PropertiesFixed {
    param(
        [string]$upperCaseFile,
        [string]$pascalCaseFile
    )
    
    $fileName = Split-Path $upperCaseFile -Leaf
    Write-Host "`nProcessing: $fileName"
    
    # Extract properties
    $upperProps = Get-Properties $upperCaseFile
    $pascalProps = Get-Properties $pascalCaseFile
    
    if ($upperProps.Count -eq 0 -and $pascalProps.Count -eq 0) {
        Write-Host "  WARNING: No properties found in either file"
        return $false
    }
    
    # Find unique properties in PascalCase
    $upperNormalized = $upperProps | ForEach-Object { $_.NormalizedName }
    $uniqueProps = $pascalProps | Where-Object { $_.NormalizedName -notin $upperNormalized }
    
    if ($uniqueProps.Count -eq 0) {
        Write-Host "  OK: No unique properties to merge"
        return $false
    }
    
    Write-Host "  Found $($uniqueProps.Count) unique properties:"
    
    # Read file content
    $content = Get-Content $upperCaseFile -Raw
    
    # Generate new properties
    $newProperties = ""
    foreach ($prop in $uniqueProps) {
        $upperName = Convert-ToUpperSnakeCase $prop.Name
        $upperValueName = "${upperName}Value"
        
        Write-Host "    + $($prop.Name) ($($prop.Type)) -> $upperName"
        
        # Create property in UPPER_CASE format
        $newProperties += "`r`n        private $($prop.Type) $upperValueName;`r`n"
        $newProperties += "        public $($prop.Type) $upperName`r`n"
        $newProperties += "        {`r`n"
        $newProperties += "            get { return this.$upperValueName; }`r`n"
        $newProperties += "            set { SetProperty(ref $upperValueName, value); }`r`n"
        $newProperties += "        }`r`n"
    }
    
    # Find insertion point (before last closing brace)
    $insertionPoint = $content.LastIndexOf("    }")
    if ($insertionPoint -eq -1) {
        Write-Host "  ERROR: Could not find class closing brace"
        return $false
    }
    
    # Insert new properties
    $newContent = $content.Substring(0, $insertionPoint) + $newProperties + $content.Substring($insertionPoint)
    
    # Write back
    Set-Content -Path $upperCaseFile -Value $newContent -NoNewline -Encoding UTF8
    
    Write-Host "  SUCCESS: Merged"
    return $true
}

# Main execution
Write-Host "========================================="
Write-Host "Property Merge Script"
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

$mergedCount = 0
$skippedCount = 0
$errorCount = 0

foreach ($group in $duplicateGroups) {
    $files = $group.Value | Sort-Object Name
    
    # Identify UPPER_CASE and PascalCase files
    $upperFile = $files | Where-Object { $_.Name -cmatch '^[A-Z_]+\.cs$' } | Select-Object -First 1
    $pascalFile = $files | Where-Object { $_.Name -cnotmatch '^[A-Z_]+\.cs$' } | Select-Object -First 1
    
    if (-not $upperFile -or -not $pascalFile) { 
        continue 
    }
    
    try {
        $merged = Merge-PropertiesFixed -upperCaseFile $upperFile.FullName -pascalCaseFile $pascalFile.FullName
        
        if ($merged) {
            $mergedCount++
        }
        else {
            $skippedCount++
        }
    }
    catch {
        Write-Host "  ERROR: $_"
        $errorCount++
    }
}

Write-Host "`n========================================="
Write-Host "Merge Complete!"
Write-Host "Files merged: $mergedCount"
Write-Host "Files skipped: $skippedCount"
Write-Host "Errors: $errorCount"
Write-Host "========================================="
