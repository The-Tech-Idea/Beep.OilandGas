
$interfacesDir = "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\Core\Interfaces"
$dataDir = "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\Data"

Get-ChildItem -Path $interfacesDir -Filter "*.cs" | ForEach-Object {
    $file = $_
    $content = Get-Content $file.FullName
    $newLines = @()
    $modified = $false
    
    foreach ($line in $content) {
        if ($line -match "using Beep.OilandGas.Models.Data\.(\w+);") {
            $category = $matches[1]
            $folderPath = Join-Path $dataDir $category
            
            # Check if folder exists and has files
            if ((Test-Path $folderPath) -and (Get-ChildItem $folderPath -Filter "*.cs")) {
                $newLines += $line
            } else {
                # Invalid or empty folder, remove using
                $modified = $true
                Write-Host "Removing invalid using $category from $($file.Name)"
            }
        } else {
            $newLines += $line
        }
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $newLines
    }
}
