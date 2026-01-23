
param(
    [string]$Path
)

$files = Get-ChildItem -Path $Path -Filter *.cs -Recurse

foreach ($file in $files) {
    Write-Host "Processing $($file.Name)..."
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content

    # Regex to find auto-properties
    # Matches: [Attribute] public Type Name { get; set; } (= val)?;
    # We need to be careful with matching strict auto-props to avoid messing up existing full props
    
    # Pattern explanation:
    # (\s*) - capture indentation
    # ((?:\[[^\]]+\]\s*)*) - capture attributes (allow multiple lines?) - simplified for now to same-line or strictly preceding
    # public\s+
    # ([^\s]+(?:\s*<[^>]+>)?(?:\?+)?) - capture type (including generics and nullables) - this is tricky
    # \s+
    # (\w+) - capture name
    # \s*\{\s*get;\s*set;\s*\} - capture auto-prop body
    # \s*(=\s*[^;]+)? - capture initializer
    # ; - end
    
    # Simplified Regex for mostly standard cases
    # Note: escaping { and } with \
    # Regex for auto-properties
    # Corrected to handle cases with and without initializers
    # Pattern:
    # 1. Indentation
    # 2. Attributes (optional)
    # 3. Type
    # 4. Name
    # 5. Initializer (optional, includes semicolon if present)
    
    $pattern = '(\s*)((?:\[[^\]]+\]\s*)*)public\s+([\w\.<>\[\]\?]+)\s+(\w+)\s*\{\s*get;\s*set;\s*\}(\s*=\s*[^;]+;)?'
    
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, $pattern, {
        param($match)
        $indent = $match.Groups[1].Value
        $attrs = $match.Groups[2].Value
        $type = $match.Groups[3].Value
        $name = $match.Groups[4].Value
        $init = $match.Groups[5].Value 
        
        # Remove trailing semicolon from init if present (we will re-add it in the field definition)
        if ($init -match ';$') {
            $init = $init.TrimEnd(';')
        }
        
        # remove = from init if present for the field (keep it)
        # but for List initialization, we want it on the field.
        
        $fieldInit = ""
        if ($init) {
            $fieldInit = " $init"
        }
        
        # Construct new property
        $field = "${indent}private $type ${name}Value$fieldInit;"
        $prop = "${indent}${attrs}public $type $name"
        $getter = "${indent}{"
        $getterBody = "${indent}    get { return ${name}Value; }"
        $setterBody = "${indent}    set { SetProperty(ref ${name}Value, value); }"
        $closer = "${indent}}"
        
        # Using the requested block style (approximately)
        return "$field`n$prop`n$indent{`n$indent    get { return this.${name}Value; }`n$indent    set { SetProperty(ref ${name}Value, value); }`n$indent}"
    })
    
    # Ensure ModelEntityBase inheritance
    # Look for: public class Name : Base
    # If : Base is missing, add : ModelEntityBase
    # If : Base is separate, leave it (assume it inherits ModelEntityBase safely? or check?)
    # Warn if not inheriting proper base
    
    if (-not ($content -match ":\s*ModelEntityBase") -and -not ($content -match ":\s*Beep.OilandGas.Models.Data.ModelEntityBase")) {
       # simple check: public class ClassName\s*$
       $content = $content -replace '(public\s+class\s+\w+)(\s*)$', '$1 : ModelEntityBase$2'
    }

    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "Updated $($file.Name)"
    }
}
