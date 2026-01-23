
param(
    [string]$Path
)

$files = Get-ChildItem -Path $Path -Filter *.cs -Recurse

$propertiesToRepair = @(
    "ACTIVE_IND",
    "ROW_CREATED_BY",
    "ROW_CREATED_DATE",
    "ROW_CHANGED_BY",
    "ROW_CHANGED_DATE",
    "ROW_EFFECTIVE_DATE",
    "ROW_EXPIRY_DATE",
    "ROW_QUALITY",
    "PPDM_GUID",
    "REMARK",
    "SOURCE",
    "EFFECTIVE_DATE",
    "EXPIRY_DATE"
)

foreach ($file in $files) {
    # Write-Host "Checking $($file.Name)..."
    $content = Get-Content -Path $file.FullName -Raw
    $modified = $false

    foreach ($prop in $propertiesToRepair) {
        # Regex to remove the specific dangling set block pattern
        # Matches:
        # set { SetProperty(ref REMARKValue, value); }
        # }
        
        # We need to be careful with whitespace.
        # Pattern:
        # \s*set\s*\{\s*SetProperty\(\s*ref\s+PropValue,\s*value\);\s*\}\s*\}
        
        $repairRegex = "(?ms)^\s*set\s*\{\s*SetProperty\(\s*ref\s+${prop}Value,\s*value\);\s*\}\s*\}\s*$"
        
        if ($content -match $repairRegex) {
            Write-Host "Repairing $prop in $($file.Name)"
            $content = $content -replace $repairRegex, ""
            $modified = $true
        }
    }

    if ($modified) {
        # Clean up excessive newlines again
        $content = $content -replace '(\r?\n){3,}', "`r`n`r`n"
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
    }
}
