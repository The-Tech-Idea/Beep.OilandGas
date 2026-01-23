
$folder = "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\Data\WellTestAnalysis"
$container = Join-Path $folder "WellTestAnalysisSupportModels.cs"
$content = Get-Content $container -Raw

$files = Get-ChildItem -Path $folder -Filter "*.cs" | Where-Object { $_.Name -ne "WellTestAnalysisSupportModels.cs" }

foreach ($file in $files) {
    $className = $file.BaseName
    # Check if class definition exists in container
    if ($content -match "class\s+$className\b") {
        Write-Host "Duplicate found: $className in container. Deleting $($file.Name)"
        Remove-Item -Path $file.FullName -Force
    }
}
