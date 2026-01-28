# Script to replace class and property references
# 1. Generates mapping of PascalCase -> UPPER_CASE classes
# 2. Generates mapping of PascalCase -> UPPER_CASE properties for each class
# 3. Replaces references in all .cs files

$solutionDir = "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas"

# --- 1. Generate Class Mapping ---
Write-Host "Generating class mapping..."
$allFiles = Get-ChildItem -Path "$solutionDir\Beep.OilandGas.Models\Data" -Recurse -File -Filter "*.cs" | 
Where-Object { $_.Name -ne "ModelEntityBase.cs" }

$classMap = @{}
$sameDirDupes = @{}

# Group by normalized name to find pairs
foreach ($file in $allFiles) {
    $normalized = $file.Name.ToLower() -replace '[_\-\s]', ''
    $key = "$($file.DirectoryName)|$normalized"
    if (-not $sameDirDupes.ContainsKey($key)) { $sameDirDupes[$key] = @() }
    $sameDirDupes[$key] += $file
}

# Also check for .bak files (since we renamed them)
$bakFiles = Get-ChildItem -Path "$solutionDir\Beep.OilandGas.Models\Data" -Recurse -File -Filter "*.cs.bak"
foreach ($file in $bakFiles) {
    $normalized = $file.Name.ToLower().Replace('.bak', '') -replace '[_\-\s]', ''
    $key = "$($file.DirectoryName)|$normalized"
    if (-not $sameDirDupes.ContainsKey($key)) { $sameDirDupes[$key] = @() }
    $sameDirDupes[$key] += $file
}

$duplicateGroups = $sameDirDupes.GetEnumerator() | Where-Object { $_.Value.Count -gt 1 }

$replacements = @()

foreach ($group in $duplicateGroups) {
    $files = $group.Value
    
    # helper to check name pattern
    $isUpper = { param($f) $f.Name -cmatch '^[A-Z_]+\.cs$' }
    $isPascal = { param($f) $f.Name -cnotmatch '^[A-Z_]+\.cs' }
    
    $upperFile = $files | Where-Object { &$isUpper $_ } | Select-Object -First 1
    $pascalFile = $files | Where-Object { &$isPascal $_ } | Select-Object -First 1
    
    if ($upperFile -and $pascalFile) {
        $upperName = $upperFile.BaseName
        $pascalName = $pascalFile.Name.Replace('.bak', '').Replace('.cs', '')
        
        # Don't map if they are already the same (case insensitive match but same string)
        if ($upperName -cne $pascalName) {
            # Get property mapping
            $propMap = @{}
            if (Test-Path $pascalFile.FullName) {
                $content = Get-Content $pascalFile.FullName -Raw
                # Regex to find properties
                [regex]::Matches($content, 'public\s+\w+\??\s+(\w+)\s*\{') | ForEach-Object {
                    $pName = $_.Groups[1].Value
                    # Convert to snake case
                    $upperPName = $pName -creplace '([a-z0-9])([A-Z])', '$1_$2' -creplace '([a-zA-Z])(\d)', '$1_$2' 
                    $upperPName = $upperPName.ToUpper()
                    $propMap[$pName] = $upperPName
                }
            }

            $replacements += @{
                Pascal     = $pascalName
                Upper      = $upperName
                Properties = $propMap
            }
        }
    }
}

Write-Host "Found $($replacements.Count) classes to map."

# --- 2. Perform Replacements ---
$targetFiles = Get-ChildItem -Path $solutionDir -Recurse -File -Filter "*.cs" | 
Where-Object { 
    $_.FullName -notmatch "\\obj\\" -and 
    $_.FullName -notmatch "\\bin\\" -and
    $_.Name -notmatch "\.bak$" -and
    $_.Name -ne "GlobalUsings.cs"
}

Write-Host "Processing $($targetFiles.Count) files..."

foreach ($file in $targetFiles) {
    $content = Get-Content $file.FullName -Raw
    $modified = $false
    
    foreach ($map in $replacements) {
        $pascal = $map.Pascal
        $upper = $map.Upper
        
        # Simple regex for class name replacement (whole word)
        # Avoid partial matches
        if ($content -match "\b$pascal\b") {
            
            # Replace class usage
            $content = $content -replace "\b$pascal\b", $upper
            $modified = $true
            
            # Replace properties for this class
            # This is tricky because we don't have semantic analysis to know if ".Values" belongs to THIS class
            # But we can try a best effort: variable.Property
            
            # For now, let's just do Class Name replacement mostly, and property replacement where obvious
            foreach ($prop in $map.Properties.Keys) {
                $upperProp = $map.Properties[$prop]
                if ($prop -cne $upperProp) {
                    # Conservative property replacement: matching typical usage
                    # We can't easily distinguish MyClass.Name vs OtherClass.Name without Roslyn
                    # But for PPDM, the property names (like DRILLING_OPERATION_ID) are distinct enough 
                    # that generally replacing .OperationId with .OPERATION_ID might be safe IF the property name is unique enough.
                    # Common names like "Name", "Id", "Status" are DANGEROUS to replace globally.
                     
                    # SKIP common property names for global replacement
                    if ($prop -in @("Id", "Name", "Status", "Description", "Value", "Code", "Type", "Date")) { continue }
                     
                    # Replace .Property
                    $content = $content -replace "\.$prop\b", ".$upperProp"
                }
            }
        }
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -NoNewline -Encoding UTF8
        Write-Host "Updated: $($file.Name)"
    }
}

Write-Host "Done Replacement."
