
$files = Get-ChildItem -Path "c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models" -Recurse -Filter "*.cs"

$propertiesToRemove = @(
    "ACTIVE_IND",
    "PPDM_GUID",
    "ROW_CREATED_BY",
    "ROW_CREATED_DATE",
    "ROW_CHANGED_BY",
    "ROW_CHANGED_DATE",
    "ROW_EFFECTIVE_DATE",
    "ROW_EXPIRY_DATE",
    "ROW_QUALITY"
)

foreach ($file in $files) {
    if ($file.Name -eq "ModelEntityBase.cs") { continue }

    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $modified = $false

    # 1. Update Inheritance
    # Regex to find class declaration inheriting from Entity and/or IPPDMEntity
    # We want to catch:
    # 1. : Entity, IPPDMEntity
    # 2. : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    # 3. : IPPDMEntity
    
    $inheritanceRegex = "class\s+(\w+)\s*:\s*(?:Entity\s*,\s*)?(?:Beep\.OilandGas\.PPDM\.Models\.)?IPPDMEntity"
    
    if ($content -match $inheritanceRegex) {
        $content = $content -replace $inheritanceRegex, 'class $1 : ModelEntityBase'
        $modified = $true
        Write-Host "Updated inheritance in $($file.Name)"
    } elseif ($content -match "class\s+(\w+)\s*:\s*ModelEntityBase") {
        # Already inherits ModelEntityBase, continue to check for properties
    } else {
        # Check if it has the properties but weird inheritance?
        # For now, rely on inheritance check.
        continue 
    }

    # 2. Remove Redundant Properties
    foreach ($prop in $propertiesToRemove) {
        # Regex matches property block. 
        # CAUTION: Backing fields might be _activeIndValue, ACTIVE_INDValue, etc.
        # "private System.String ACTIVE_INDValue;"
        # "public System.String ACTIVE_IND { ... }"
        # Use single-line mode (?s) or multi-line (?m) carefully.
        
        # Pattern: 
        # Any whitespace
        # Access modifier (private/protected/public)
        # Type
        # Name (Value or _Value)
        # Optional initialization
        # Semicolon
        # Any whitespace
        # public
        # Type
        # Name
        # { get/set block }
        
        # We try to match the backing field AND the property.
        
        $escapedProp = [Regex]::Escape($prop)
        
        # Backing field variants:
        # ACTIVE_INDValue
        # _activeIndValue (camel cased? usually generated code uses UpperCaseValue)
        # Let's assume the generated naming convention seen so far: PROPERTYValue or _propertyNameValue
        
        $propRegex = "(?s)\s*(private|protected|public)\s+[\w\?\.]+\s+(?:${escapedProp}Value|_${escapedProp}Value|_?${escapedProp})\s*(?:=.*?)?;\s*" +
                     "public\s+[\w\?\.]+\s+${escapedProp}\s*\{\s*get\s*\{.*?\}\s*set\s*\{.*?\}\s*\}"
        
        if ($content -match $propRegex) {
            $content = $content -replace $propRegex, ""
            $modified = $true
            # Write-Host "   Removed property $prop"
        }
    }

    # 3. Add using statement if missing
    # Match namespace inside namespace block or at top?
    # Simple check: if ModelEntityBase is used, make sure namespace is available.
    
    if ($content -match "ModelEntityBase") {
        if (-not ($content -match "using Beep.OilandGas.Models.Data;") -and -not ($content -match "namespace Beep.OilandGas.Models.Data\s*\{")) {
             # Add after last using
             # If namespaces match Beep.OilandGas.Models.Data.*, they usually have access, but explicit using is safer if in sub-namespace
             
             # Check if we are logically in a sub-namespace of Data. 
             # Actually, if we are in Beep.OilandGas.Models.Data.Accounting, we can access Beep.OilandGas.Models.Data classes directly? 
             # No, sibling namespaces don't automatically import parent namespace types if not hierarchical.
             
             if ($content -match "using .*?;") {
                 # Find the last using statement
                 $regex = "(using [^;]+;(\r?\n)?)+"
                 $match = [regex]::match($content, $regex)
                 if ($match.Success) {
                    $lastUsing = $match.Value
                    $newUsing = "$lastUsing`r`nusing Beep.OilandGas.Models.Data;"
                    $content = $content.Replace($lastUsing, $newUsing)
                    $modified = $true
                 }
             }
        }
    }

    if ($modified) {
        Set-Content -Path $file.FullName -Value $content
        Write-Host "Saved changes to $($file.Name)"
    }
}
