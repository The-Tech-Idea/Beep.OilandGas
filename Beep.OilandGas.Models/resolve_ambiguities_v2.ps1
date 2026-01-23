
$types = @(
"DatabaseDriverInfo",
"DriverInfo",
"SchemaPrivilegeCheckResult",
"CreateSchemaResult",
"DatabaseConnectionListItem",
"DeleteConnectionResult",
"ProspectRanking",
"ReconciliationResult",
"OilPropertyResult",
"Imbalance"
)

$dataDir = "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\Data"

foreach ($type in $types) {
    # Find files containing "class $type"
    $files = Get-ChildItem -Path $dataDir -Recurse -Filter "*.cs" | Select-String -Pattern "class\s+$type\b" | Select-Object -ExpandProperty Path -Unique
    
    if ($files.Count -gt 1) {
        Write-Host "Ambiguity for $type in:`n$($files -join "`n")"
        
        foreach ($file in $files) {
            # Prefer keeping *Models.cs or similar container files.
            # Delete if filename is exactly [Type].cs (standalone) and another file exists.
            
            if ($file -match "\\$type\.cs$") {
                Write-Host "Deleting duplicate: $file"
                Remove-Item -Path $file -Force
            }
        }
    }
}
