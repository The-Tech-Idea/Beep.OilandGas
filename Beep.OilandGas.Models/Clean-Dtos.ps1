
param(
    [string]$Path
)

$files = Get-ChildItem -Path $Path -Filter *.cs -Recurse

# List of properties to remove because they are in ModelEntityBase
# We match "public Type Name { get; set; }" (auto-property) OR the new pattern "public Type Name { get ... }"
# Actually, we should just remove the PROPERTY definitions.
# The backing fields (private ... NameValue) might be left behind as "unused fields" (CS0169), which is another warning.
# So we should remove backing fields too if possible.

$propertiesToRemove = @(
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
    Write-Host "Scanning $($file.Name)..."
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    $modified = $false

    foreach ($prop in $propertiesToRemove) {
        # Regex to remove backing field:
        # private [Type] [PropName]Value ... ;
        # Case insensitive match for property name
        
        $fieldRegex = "(?mi)^\s*private\s+[\w\?]+\s+${prop}Value\s*(?:=.*?)?;\s*$"
        if ($content -match $fieldRegex) {
            $content = $content -replace $fieldRegex, ""
            $modified = $true
        }

        # Regex to remove property block (The new pattern we just added)
        # public [Type] [PropName] \n { ... }
        # This is tricky across multiple lines.
        # Pattern:
        # \s*public\s+[\w\?]+\s+${prop}\s*\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}
        # Balanced group matching or just simple multi-line matching if structure is predictable.
        # Our script generated:
        # indent public Type Name
        # indent {
        # ...
        # indent }
        
        # We can try to match the specific structure we generated:
        # \s*public\s+[\w\?]+\s+${prop}\s*\{\s*get\s*\{\s*return\s*this\.${prop}Value;\s*\}\s*set\s*\{\s*SetProperty.*;\s*\}\s*\}
        
        # Using a slightly generous regex:
        # (?s) enables dot matching newlines.
        
        $propRegex = "(?mi)^\s*public\s+[\w\?]+\s+${prop}\s*\{[^}]*?\}"
        # This matches one line? No, `[^}]*?` matches content until closing brace.
        # What if braces are nested? (They aren't in these simple properties).
        # But wait, our properties span multiple lines.
        # (?s) is vital.
        # We need to construct the regex carefully.
        
        # Match: whitespace, public, type, PropName, whitespace, {, content non-greedy, }
        $blockRegex = "(?ms)^\s*public\s+[\w\.<>\[\]\?]+\s+${prop}\s*\{.*?\}\s*$"
        
        if ($content -match $blockRegex) {
            # Write-Host "Removing $prop from $($file.Name)"
            $content = $content -replace $blockRegex, ""
            $modified = $true
        }
        
        # Also remove old auto-properties if any survived (unlikely but possible)
        $autoPropRegex = "(?mi)^\s*public\s+[\w\.<>\[\]\?]+\s+${prop}\s*\{\s*get;\s*set;\s*\}\s*(?:=.*?)?;\s*$"
        if ($content -match $autoPropRegex) {
             # Write-Host "Removing auto-prop $prop from $($file.Name)"
             $content = $content -replace $autoPropRegex, ""
             $modified = $true
        }
    }

    if ($modified) {
        # Remove multiple empty lines resulting from deletions
        # Replace 3 or more newlines with 2
        $content = $content -replace '(\r?\n){3,}', "`r`n`r`n"
        
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "Cleaned $($file.Name)"
    }
}
