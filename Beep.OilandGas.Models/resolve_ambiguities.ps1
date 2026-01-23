
$types = @(
"DatabaseDriverInfo",
"DriverInfo",
"SchemaPrivilegeCheckResult",
"CreateSchemaResult",
"DatabaseConnectionListItem",
"DeleteConnectionResult",
"ProspectRanking",
"ReconciliationResult"
)

$dataDir = "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\Data"

foreach ($type in $types) {
    # Find files containing "class $type"
    $files = Get-ChildItem -Path $dataDir -Recurse -Filter "*.cs" | Select-String -Pattern "class\s+$type\b" | Select-Object -ExpandProperty Path -Unique
    
    if ($files.Count -gt 1) {
        Write-Host "Ambiguity for $type in:`n$($files -join "`n")"
        
        # Heuristic: Delete the one that matches explicit [Type].cs in a subfolder, 
        # preferring to keep the one in a larger container or root.
        
        foreach ($file in $files) {
            # If filename is exactly [Type].cs AND it is in a subdirectory of Data (Data\Sub\Type.cs)
            # And there is another file
            
            # Check if this file was likely created by us (standalone file)
            if ($file -match "\\$type\.cs$") {
                # Candidate for deletion if another exists
                Write-Host "Deleting duplicate: $file"
                Remove-Item -Path $file -Force
            }
        }
    } else {
        Write-Host "No ambiguity found for $type (or grep failed)"
    }
}
