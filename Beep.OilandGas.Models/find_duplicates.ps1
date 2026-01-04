# Script to find duplicate DTOs
$dtoClasses = @(
    "GasCompositionDto", "GasComponentDto", "GasPropertyResultDto",
    "OilCompositionDto", "OilPropertyResultDto"
)

$results = @()

foreach ($dtoName in $dtoClasses) {
    $baseName = $dtoName -replace "Dto$", "" -replace "DTO$", ""
    
    Write-Host "Checking $dtoName -> $baseName"
    
    # Search in Models folder
    $modelsFiles = Get-ChildItem -Path "Models" -Recurse -Filter "*.cs" | Where-Object {
        $content = Get-Content $_.FullName -Raw
        $content -match "public class $baseName\b"
    }
    
    # Search in Data folder
    $dataFiles = Get-ChildItem -Path "Data" -Recurse -Filter "*.cs" | Where-Object {
        $content = Get-Content $_.FullName -Raw
        $content -match "public class $baseName\b"
    }
    
    if ($modelsFiles.Count -gt 0 -or $dataFiles.Count -gt 0) {
        $result = [PSCustomObject]@{
            DtoName = $dtoName
            BaseName = $baseName
            ModelsFiles = $modelsFiles.FullName
            DataFiles = $dataFiles.FullName
        }
        $results += $result
    }
}

$results | Format-List
