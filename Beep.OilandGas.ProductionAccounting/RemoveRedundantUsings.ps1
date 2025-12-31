# Script to remove redundant using statements that are now in Globalusings.cs
$globalUsings = @(
    "using System;",
    "using System.Collections.Generic;",
    "using System.Linq;",
    "using System.Threading.Tasks;",
    "using Beep.OilandGas.Models.Core.Interfaces;",
    "using Beep.OilandGas.Models.Data;",
    "using Beep.OilandGas.PPDM39.Core.Metadata;",
    "using Beep.OilandGas.PPDM39.DataManagement.Core;",
    "using Beep.OilandGas.PPDM39.DataManagement.Core.Common;",
    "using Beep.OilandGas.PPDM39.Repositories;",
    "using Beep.OilandGas.ProductionAccounting.Exceptions;",
    "using Microsoft.Extensions.Logging;",
    "using TheTechIdea.Beep.Editor;",
    "using TheTechIdea.Beep.Report;"
)

$files = Get-ChildItem -Path $PSScriptRoot -Recurse -Filter *.cs | Where-Object { $_.Name -ne "Globalusings.cs" }

foreach ($file in $files) {
    $lines = Get-Content $file.FullName
    $newLines = New-Object System.Collections.ArrayList
    $inUsingBlock = $true
    $modified = $false
    $removedConsecutive = $false
    
    foreach ($line in $lines) {
        $trimmedLine = $line.Trim()
        
        if ($inUsingBlock -and $trimmedLine.StartsWith("using ") -and $trimmedLine.EndsWith(";")) {
            $shouldRemove = $false
            foreach ($globalUsing in $globalUsings) {
                if ($trimmedLine -eq $globalUsing.Trim()) {
                    $shouldRemove = $true
                    $modified = $true
                    $removedConsecutive = $true
                    break
                }
            }
            
            if (-not $shouldRemove) {
                $inUsingBlock = $false
                if ($removedConsecutive) {
                    # Add a blank line before the first non-removed using if we removed some
                    [void]$newLines.Add("")
                    $removedConsecutive = $false
                }
                [void]$newLines.Add($line)
            }
        }
        else {
            if ($trimmedLine -eq "" -and $inUsingBlock -and $removedConsecutive) {
                # Skip blank lines between removed usings
                continue
            }
            else {
                $inUsingBlock = $false
                $removedConsecutive = $false
                [void]$newLines.Add($line)
            }
        }
    }
    
    if ($modified) {
        # Remove trailing blank lines from using block
        while ($newLines.Count -gt 0 -and $newLines[$newLines.Count - 1].Trim() -eq "") {
            $newLines.RemoveAt($newLines.Count - 1)
        }
        $newLines | Set-Content $file.FullName
        Write-Host "Updated: $($file.Name)"
    }
}

Write-Host "Done!"
