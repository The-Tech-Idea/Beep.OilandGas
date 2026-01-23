
# 1. Build a map of existing classes in Data
$dataDir = "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\Data"
$interfacesDir = "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\Core\Interfaces"

$existingClasses = @{}
Get-ChildItem -Path $dataDir -Recurse -Filter "*.cs" | ForEach-Object {
    $existingClasses[$_.Name] = $_.FullName
    # Also parse content for class names if filenames don't match? 
    # For now assume filename matches classname or close enough, or just searching by name 
    # to avoid complex parsing of every file.
    # Actually, a better check is "Does 'class X' exist?"
    # We'll just check filenames first, but precise class duplicate checking is better done by
    # "If I move X, will it conflict?"
}

# 2. Process Interface files
Get-ChildItem -Path $interfacesDir -Filter "*.cs" | ForEach-Object {
    $file = $_
    $content = Get-Content $file.FullName -Raw
    
    # Heuristic for destination
    $baseName = $file.BaseName
    if ($baseName -match "^I(.+?)Service$") {
        $category = $matches[1]
    } elseif ($baseName -match "^I(.+)$") {
        $category = $matches[1]
    } else {
        $category = "General"
    }
    
    # Try to find matching folder
    $destDir = Join-Path $dataDir $category
    if (-not (Test-Path $destDir)) {
        # Try to find specific folder?
        $found = Get-ChildItem $dataDir -Directory | Where-Object { $_.Name -eq $category }
        if ($found) {
            $destDir = $found.FullName
        } else {
            $destDir = Join-Path $dataDir "General"
        }
    }
    if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }

    # Parse for classes
    # We are looking for "public class ClassName { ... }" (generic handles are hard)
    # We will use a state machine approach in C# embedded script or just splitting by tokens?
    # Simple regex based extraction:
    # Match "public class Name :? ..." 
    
    # Regex to capture the whole class block is tricky with nested braces.
    # Strategy: Read content, find start index of "public class", count braces.
    
    $modified = $false
    $newContent = $content
    
    # Find all class starts
    $matches = [regex]::Matches($content, "public class\s+(\w+)(?:[^\{]*)\{")
    
    # We process in reverse order to not mess up indices? No, string replacement is better.
    # Actually, we should iterate and extract.
    
    foreach ($match in $matches) {
        $className = $match.Groups[1].Value
        $startIndex = $match.Index
        
        # Find ending brace
        $braceCount = 0
        $endIndex = -1
        $foundStart = $false
        
        for ($i = $startIndex; $i -lt $content.Length; $i++) {
            if ($content[$i] -eq '{') {
                $braceCount++
                $foundStart = $true
            }
            elseif ($content[$i] -eq '}') {
                $braceCount--
            }
            
            if ($foundStart -and $braceCount -eq 0) {
                $endIndex = $i
                break
            }
        }
        
        if ($endIndex -ne -1) {
            $fullClassBlock = $content.Substring($startIndex, $endIndex - $startIndex + 1)
            
            # Check if duplicate in Data
            # Simple check: Does a file ClassName.cs exist?
            # Or scan all files?
            
            $isDuplicate = $false
             if ($existingClasses.ContainsKey("$className.cs")) {
                 $isDuplicate = $true
                 Write-Host "Duplicate found: $className in $($file.Name)"
             } else {
                 # Deeper check: grep in Data?
                 $grep = Select-String -Path "$dataDir\*.cs" -Pattern "class\s+$className\b" -Recurse
                 if ($grep) { 
                    $isDuplicate = $true 
                    Write-Host "Duplicate found (grep): $className in $($file.Name)"
                 }
             }
             
             if ($isDuplicate) {
                 # Remove from interface file
                 $newContent = $newContent.Replace($fullClassBlock, "")
                 $modified = $true
             } else {
                 # Move to new file
                 $destPath = Join-Path $destDir "$className.cs"
                 
                 # Prepare content
                 # Fix inheritance
                 $classDef = $fullClassBlock
                 if ($classDef -match ":\s*ModelEntityBase") {
                     # Already has it
                 } elseif ($classDef -match ":") {
                     # Has other inheritance?
                     # User said "make implement ModelEntityBase". 
                     # If inherits something else, we might break it. 
                     # Assumption: DTOs in interfaces usually usually POCOs.
                     # Replace ": Base" with ": ModelEntityBase, Base"? No.
                     # Replace first inheritance?
                     # Let's simple append or replace.
                     # If no inheritance: "public class X" -> "public class X : ModelEntityBase"
                     $classDef = $classDef -replace "class $className\s*\{", "class $className : ModelEntityBase {"
                     $classDef = $classDef -replace "class $className\s*:\s*(\w+)", "class $className : ModelEntityBase, `$1"
                     # Cleanup double inheritance if ModelEntityBase was already there? regex above handles 'no brace'.
                 } else {
                     # No inheritance
                     $classDef = $classDef -replace "class $className", "class $className : ModelEntityBase"
                 }
                 
                 # Namespace
                 $ns = "Beep.OilandGas.Models.Data.$($destDir.Split('\')[-1])"
                 if ($destDir.EndsWith("Data")) { $ns = "Beep.OilandGas.Models.Data" }
                 
                 $fileContent = @"
using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Beep.Editor;

namespace $ns
{
    $classDef
}
"@
                 
                 Set-Content -Path $destPath -Value $fileContent
                 Write-Host "Moved $className to $destPath"
                 
                 # Remove from interface
                 $newContent = $newContent.Replace($fullClassBlock, "")
                 $modified = $true
                 
                 # Add using to interface file if needed?
                 # Handled later or assume global usings?
             }
        }
    }
    
    if ($modified) {
        # Check and add using statement
        if (-not ($newContent -match "using Beep.OilandGas.Models.Data;")) {
             if ($newContent -match "using .*?;") {
                 $newContent = $newContent -replace "(using .*?;(?!\r?\nusing))", "`$1`r`nusing Beep.OilandGas.Models.Data;"
             } else {
                 $newContent = "using Beep.OilandGas.Models.Data;`r`n" + $newContent
             }
        }
        
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Updated $($file.Name)"
    }
}
