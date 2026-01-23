
$interfacesDir = "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\Core\Interfaces"
$dataDir = "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\Data"

Get-ChildItem -Path $interfacesDir -Filter "*.cs" | ForEach-Object {
    $file = $_
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $modified = $false
    
    # Infer Category
    $baseName = $file.BaseName
    $category = "General"
    if ($baseName -match "^I(.+?)Service$") {
        $category = $matches[1]
    } elseif ($baseName -match "^I(.+)$") {
        $category = $matches[1]
    }
    
    # Check if category folder exists in Data
    $nsToAdd = $null
    if (Test-Path (Join-Path $dataDir $category)) {
        $nsToAdd = "Beep.OilandGas.Models.Data.$category"
    } elseif (Test-Path (Join-Path $dataDir "General")) {
        # Maybe it went to General?
        # But we only add General if we suspect it went there.
        # Let's add General anyway if not found matches? 
        # Safer to just add the Category one if it exists.
    }
    
    if ($nsToAdd) {
        if (-not ($content -match "using $nsToAdd;")) {
             if ($content -match "using .*?;") {
                 $content = $content -replace "(using .*?;(?!\r?\nusing))", "`$1`r`nusing $nsToAdd;"
             } else {
                 $content = "using $nsToAdd;`r`n" + $content
             }
             $modified = $true
             Write-Host "Added using $nsToAdd to $($file.Name)"
        }
    }

    if ($modified) {
        Set-Content -Path $file.FullName -Value $content
    }
}
