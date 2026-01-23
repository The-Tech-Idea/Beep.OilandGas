
$files = Get-ChildItem -Path "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas" -Recurse -Filter "*.csproj"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $modified = $false

    # Regex to replace TargetFramework
    # <TargetFramework>net8.0</TargetFramework> or similar
    # Also handle multiple frameworks if necessary, but request was "net10.0" implying single target or all of them.
    # We will replace any netX.X with net10.0 inside TargetFramework tags.
    
    $regex = "<TargetFramework>.*?</TargetFramework>"
    if ($content -match $regex) {
        $content = $content -replace $regex, "<TargetFramework>net10.0</TargetFramework>"
        $modified = $true
    }
    
    # Also handle plural <TargetFrameworks> if present
    $regexs = "<TargetFrameworks>.*?</TargetFrameworks>"
    if ($content -match $regexs) {
        $content = $content -replace $regexs, "<TargetFrameworks>net10.0</TargetFrameworks>"
        # Note: usually TargetFrameworks is used fo multi-targeting. Changing to single net10.0 might break things if they needed others, but user said "all project should net10.0"
        $modified = $true
    }

    if ($modified -and $content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content
        Write-Host "Updated $($file.Name) to net10.0"
    }
}
