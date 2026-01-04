# Script to compare all DTOs with actual classes
$dtoFiles = Get-ChildItem -Path "DTOs" -Recurse -Filter "*.cs"
$results = @()

foreach ($dtoFile in $dtoFiles) {
    $content = Get-Content $dtoFile.FullName -Raw
    $namespace = if ($content -match 'namespace\s+([^\s{]+)') { $matches = [regex]::Match($content, 'namespace\s+([^\s{]+)'); $matches2.Groups[1].Value } else { 'Unknown' }
    
    # Find all DTO classes in this file
    $dtoMatches = [regex]::Matches($content, 'public class (\w+Dto|\w+DTO)')
    
    foreach ($match in $dtoMatches) {
        $dtoName = $match.Groups[1].Value
        $baseName = $dtoName -replace "Dto$", "" -replace "DTO$", ""
        
        # Search in Models folder
        $modelsFiles = Get-ChildItem -Path "Models" -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue | Where-Object {
            $fileContent = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
            if ($fileContent) {
                $fileContent -match "public class $baseName\b"
            } else {
                $false
            }
        }
        
        # Search in Data folder  
        $dataFiles = Get-ChildItem -Path "Data" -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue | Where-Object {
            $fileContent = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
            if ($fileContent) {
                $fileContent -match "public class $baseName\b"
            } else {
                $false
            }
        }
        
        if ($modelsFiles.Count -gt 0 -or $dataFiles.Count -gt 0) {
            $result = [PSCustomObject]@{
                DtoName = $dtoName
                DtoNamespace = $namespace
                DtoFile = $dtoFile.FullName
                BaseName = $baseName
                ModelsFiles = ($modelsFiles | ForEach-Object { $_.FullName }) -join ";"
                DataFiles = ($dataFiles | ForEach-Object { $_.FullName }) -join ";"
            }
            $results += $result
        }
    }
}

# Export to CSV for analysis
$results | Export-Csv -Path "dto_matches.csv" -NoTypeInformation
Write-Host "Found $($results.Count) potential matches. Results saved to dto_matches.csv"
$results | Select-Object DtoName, BaseName, ModelsFiles, DataFiles | Format-Table -AutoSize
