# Comprehensive duplicate detection script for Beep.OilandGas.Models
# Handles case-insensitive matching (UPPER_CASE vs PascalCase)

function Normalize-ClassName {
    param([string]$Name)
    # Remove Dto/DTO suffix
    $name = $Name -replace '(?i)Dto$|DTO$', ''
    # Convert UPPER_CASE to PascalCase equivalent
    if ($name -match '_') {
        $parts = $name.ToLower() -split '_'
        $normalized = ($parts | ForEach-Object { 
            $_.Substring(0,1).ToUpper() + $_.Substring(1) 
        }) -join ''
        return $normalized
    }
    return $name.ToLower()
}

function Normalize-PropertyName {
    param([string]$Name)
    if ($Name -match '_') {
        $parts = $Name.ToLower() -split '_'
        return ($parts | ForEach-Object { 
            $_.Substring(0,1).ToUpper() + $_.Substring(1) 
        }) -join ''
    }
    return $Name
}

function Normalize-Type {
    param([string]$Type)
    $type = $Type -replace '\?', '' -replace 'System\.', ''
    $type = $type -replace '\bstring\b', 'String'
    $type = $type -replace '\bint\b', 'Int32'
    $type = $type -replace '\bdecimal\b', 'Decimal'
    $type = $type -replace '\bDateTime\b', 'DateTime'
    $type = $type -replace '\bbool\b', 'Boolean'
    $type = $type -replace '\bdouble\b', 'Double'
    $type = $type -replace '\bfloat\b', 'Single'
    # Remove generic parameters
    $type = $type -replace '<[^>]+>', ''
    $type = $type -replace '\bList\b', '' -replace '\bIEnumerable\b', ''
    return $type.Trim()
}

function Extract-ClassInfo {
    param([string]$FilePath)
    
    try {
        $content = Get-Content $FilePath -Raw -ErrorAction Stop
        
        # Find class definition
        if ($content -notmatch 'public\s+(?:partial\s+)?class\s+(\w+)') {
            return $null
        }
        
        $className = $matches[1]
        
        # Extract namespace
        $namespace = if ($content -match 'namespace\s+([^\s{]+)') { 
            $matches[1] 
        } else { 
            'Unknown' 
        }
        
        # Extract properties - Pattern 1: Auto-properties
        $properties = @()
        $autoPropPattern = 'public\s+([\w\.<>,\s\[\]?]+)\s+(\w+)\s*\{[^}]*get[^}]*set[^}]*\}'
        $autoProps = [regex]::Matches($content, $autoPropPattern)
        
        foreach ($match in $autoProps) {
            $propType = $match.Groups[1].Value.Trim()
            $propName = $match.Groups[2].Value.Trim()
            
            # Skip PPDM standard columns
            $ppdmColumns = @('ACTIVE_IND', 'PPDM_GUID', 'REMARK', 'SOURCE', 
                            'ROW_CREATED_DATE', 'ROW_CREATED_BY', 'ROW_CHANGED_DATE', 
                            'ROW_CHANGED_BY', 'ROW_EFFECTIVE_DATE', 'ROW_EXPIRY_DATE', 
                            'ROW_QUALITY', 'AREA_ID', 'AREA_TYPE', 'BUSINESS_ASSOCIATE_ID',
                            'EFFECTIVE_DATE', 'EXPIRY_DATE')
            
            if ($ppdmColumns -notcontains $propName) {
                $properties += @{
                    Name = $propName
                    Type = $propType
                }
            }
        }
        
        # Extract properties - Pattern 2: SetProperty style (Data folder)
        $setPropPattern = 'private\s+([\w\.<>,\s\[\]?]+)\s+(\w+)Value;.*?public\s+([\w\.<>,\s\[\]?]+)\s+(\w+)\s*\{[^}]*get[^}]*SetProperty'
        $setProps = [regex]::Matches($content, $setPropPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
        
        foreach ($match in $setProps) {
            $propType = $match.Groups[3].Value.Trim()
            $propName = $match.Groups[4].Value.Trim()
            
            # Skip PPDM standard columns
            $ppdmColumns = @('ACTIVE_IND', 'PPDM_GUID', 'REMARK', 'SOURCE', 
                            'ROW_CREATED_DATE', 'ROW_CREATED_BY', 'ROW_CHANGED_DATE', 
                            'ROW_CHANGED_BY', 'ROW_EFFECTIVE_DATE', 'ROW_EXPIRY_DATE', 
                            'ROW_QUALITY', 'AREA_ID', 'AREA_TYPE', 'BUSINESS_ASSOCIATE_ID',
                            'EFFECTIVE_DATE', 'EXPIRY_DATE')
            
            if ($ppdmColumns -notcontains $propName) {
                $properties += @{
                    Name = $propName
                    Type = $propType
                }
            }
        }
        
        # Determine folder and type
        $folder = if ($FilePath -match '\\Data\\') { 'Data' }
                  elseif ($FilePath -match '\\Models\\') { 'Models' }
                  else { 'DTOs' }
        
        $isPpdm = $content -match 'IPPDMEntity|:\s*Entity'
        $isDto = $className -match '(?i)Dto$|DTO$'
        
        return @{
            ClassName = $className
            NormalizedName = Normalize-ClassName $className
            Namespace = $namespace
            FilePath = $FilePath
            Properties = $properties
            IsPpdm = $isPpdm
            IsDto = $isDto
            Folder = $folder
        }
    }
    catch {
        Write-Warning "Error processing $FilePath : $_"
        return $null
    }
}

function Compare-Properties {
    param(
        [array]$Props1,
        [array]$Props2
    )
    
    # Normalize both property lists
    $normalized1 = @{}
    foreach ($prop in $Props1) {
        $normName = Normalize-PropertyName $prop.Name
        $normType = Normalize-Type $prop.Type
        $normalized1[$normName] = $normType
    }
    
    $normalized2 = @{}
    foreach ($prop in $Props2) {
        $normName = Normalize-PropertyName $prop.Name
        $normType = Normalize-Type $prop.Type
        $normalized2[$normName] = $normType
    }
    
    # Find common properties
    $common = $normalized1.Keys | Where-Object { $normalized2.ContainsKey($_) }
    $only1 = $normalized1.Keys | Where-Object { -not $normalized2.ContainsKey($_) }
    $only2 = $normalized2.Keys | Where-Object { -not $normalized1.ContainsKey($_) }
    
    # Check type matches
    $typeMatches = 0
    $typeMismatches = @()
    foreach ($prop in $common) {
        if ($normalized1[$prop] -eq $normalized2[$prop]) {
            $typeMatches++
        } else {
            $typeMismatches += $prop
        }
    }
    
    $maxProps = [Math]::Max($normalized1.Count, $normalized2.Count)
    $similarity = if ($maxProps -gt 0) { $common.Count / $maxProps } else { 0 }
    
    return @{
        CommonCount = $common.Count
        OnlyInFirst = $only1.Count
        OnlyInSecond = $only2.Count
        TypeMatches = $typeMatches
        TypeMismatches = $typeMismatches
        Similarity = $similarity
    }
}

function Classify-Duplicate {
    param(
        [hashtable]$Class1,
        [hashtable]$Class2,
        [hashtable]$Comparison
    )
    
    $totalProps1 = $Class1.Properties.Count
    $totalProps2 = $Class2.Properties.Count
    $common = $Comparison.CommonCount
    
    # True duplicate: same number of properties, all match
    if ($totalProps1 -eq $totalProps2 -and $totalProps1 -eq $common -and $Comparison.TypeMatches -eq $common) {
        return 'true_duplicate'
    }
    
    # Structural duplicate: same core properties but different metadata
    $minProps = [Math]::Min($totalProps1, $totalProps2)
    if ($common -ge ($minProps * 0.8) -and $Comparison.TypeMatches -ge ($common * 0.9)) {
        return 'structural_duplicate'
    }
    
    # Semantic duplicate: same concept but different representation
    if ($common -ge ($minProps * 0.5)) {
        return 'semantic_duplicate'
    }
    
    # False positive
    return 'false_positive'
}

# Main execution
Write-Host "Scanning for duplicate classes..." -ForegroundColor Cyan

$dataFiles = Get-ChildItem -Path "Data" -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
$modelsFiles = Get-ChildItem -Path "Models" -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
$dtoFiles = Get-ChildItem -Path "DTOs" -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue

Write-Host "Found $($dataFiles.Count) Data files, $($modelsFiles.Count) Models files, $($dtoFiles.Count) DTO files"

# Extract all class information
$allClasses = @()
foreach ($file in $dataFiles + $modelsFiles + $dtoFiles) {
    $classInfo = Extract-ClassInfo $file.FullName
    if ($classInfo) {
        $allClasses += $classInfo
    }
}

Write-Host "Extracted $($allClasses.Count) classes"

# Group classes by normalized name
$classesByName = @{}
foreach ($cls in $allClasses) {
    $normName = $cls.NormalizedName
    if (-not $classesByName.ContainsKey($normName)) {
        $classesByName[$normName] = @()
    }
    $classesByName[$normName] += $cls
}

# Find potential duplicates
$duplicates = @()
foreach ($normName in $classesByName.Keys) {
    $classes = $classesByName[$normName]
    if ($classes.Count -gt 1) {
        # Compare all pairs
        for ($i = 0; $i -lt $classes.Count; $i++) {
            for ($j = $i + 1; $j -lt $classes.Count; $j++) {
                $class1 = $classes[$i]
                $class2 = $classes[$j]
                
                # Skip if same file
                if ($class1.FilePath -eq $class2.FilePath) {
                    continue
                }
                
                $comparison = Compare-Properties $class1.Properties $class2.Properties
                $duplicateType = Classify-Duplicate $class1 $class2 $comparison
                
                if ($duplicateType -ne 'false_positive') {
                    $duplicates += @{
                        Class1 = $class1
                        Class2 = $class2
                        Comparison = $comparison
                        DuplicateType = $duplicateType
                    }
                }
            }
        }
    }
}

# Sort by duplicate type and similarity
$duplicates = $duplicates | Sort-Object @{
    Expression = {
        switch ($_.DuplicateType) {
            'true_duplicate' { 0 }
            'structural_duplicate' { 1 }
            'semantic_duplicate' { 2 }
            default { 3 }
        }
    }
}, @{ Expression = { -$_.Comparison.Similarity } }

# Generate report
Write-Host "`n$('='*80)" -ForegroundColor Green
Write-Host "Found $($duplicates.Count) potential duplicate pairs" -ForegroundColor Green
Write-Host "$('='*80)`n" -ForegroundColor Green

$reportData = @()
foreach ($dup in $duplicates) {
    $c1 = $dup.Class1
    $c2 = $dup.Class2
    $comp = $dup.Comparison
    
    Write-Host "Type: $($dup.DuplicateType.ToUpper())" -ForegroundColor Yellow
    Write-Host "  Class 1: $($c1.ClassName) ($($c1.Folder))"
    Write-Host "    File: $($c1.FilePath)"
    Write-Host "    Properties: $($c1.Properties.Count)"
    Write-Host "  Class 2: $($c2.ClassName) ($($c2.Folder))"
    Write-Host "    File: $($c2.FilePath)"
    Write-Host "    Properties: $($c2.Properties.Count)"
    Write-Host "  Comparison:"
    Write-Host "    Common properties: $($comp.CommonCount)"
    Write-Host "    Type matches: $($comp.TypeMatches)"
    Write-Host "    Similarity: $([math]::Round($comp.Similarity * 100, 2))%"
    Write-Host "    Only in $($c1.ClassName): $($comp.OnlyInFirst)"
    Write-Host "    Only in $($c2.ClassName): $($comp.OnlyInSecond)"
    Write-Host ""
    
    $reportData += [PSCustomObject]@{
        Type = $dup.DuplicateType
        Class1Name = $c1.ClassName
        Class1Folder = $c1.Folder
        Class1File = $c1.FilePath
        Class1Props = $c1.Properties.Count
        Class2Name = $c2.ClassName
        Class2Folder = $c2.Folder
        Class2File = $c2.FilePath
        Class2Props = $c2.Properties.Count
        CommonProps = $comp.CommonCount
        TypeMatches = $comp.TypeMatches
        Similarity = [math]::Round($comp.Similarity * 100, 2)
    }
}

# Save to CSV
$reportData | Export-Csv -Path "duplicate_report.csv" -NoTypeInformation
Write-Host "Detailed report saved to duplicate_report.csv" -ForegroundColor Green

